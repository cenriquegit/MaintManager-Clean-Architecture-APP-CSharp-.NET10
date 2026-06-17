using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace MaintManager.MAUI.ViewModels.Maintenances;

public partial class MaintenanceDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private int _mainid;

    public MaintenanceDetailViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Detalle de Orden";
    }

    public bool IsAdmin => _authService.IsAdmin();

    [ObservableProperty]
    private MaintenanceDetailResponse? _maintenanceDetail;

    partial void OnMaintenanceDetailChanged(MaintenanceDetailResponse? value)
    {
        OnPropertyChanged(nameof(IsReadOnly));
    }

    public bool IsReadOnly => MaintenanceDetail?.Status == "FI";

    [ObservableProperty]
    private DiagnosisResponse? _diagnosis;

    [ObservableProperty]
    private bool _diagnosisSaved;

    [ObservableProperty]
    private bool _canClose;

    [ObservableProperty]
    private bool _canCancel;

    [ObservableProperty]
    private bool _isOilInfoExpanded;

    [ObservableProperty]
    private string _diagnosisObservations = string.Empty;

    // ── Diagnosis form fields ──────────────────────────────────
    [ObservableProperty]
    private ObservableCollection<string> _generalStatusOptions = new()
    {
        "Excelente", "Bueno", "Regular", "Reparado", "Malo"
    };

    [ObservableProperty]
    private string _selectedGeneralStatus = "Bueno";

    [ObservableProperty]
    private bool _isVehicleOperative = true;

    [ObservableProperty]
    private string _futureRecommendations = string.Empty;

    // ═══════════════════════════════════════════════════════════════
    // CHECKLISTS (reemplazan los Pickers + botón "+")
    // ═══════════════════════════════════════════════════════════════

    [ObservableProperty]
    private ObservableCollection<ActionChecklistItem> _actionChecklist = new();

    [ObservableProperty]
    private ObservableCollection<MaterialChecklistItem> _materialChecklist = new();

    [ObservableProperty]
    private ObservableCollection<ComponentChecklistItem> _componentChecklist = new();

    [RelayCommand]
    private void ToggleOilInfo()
    {
        IsOilInfoExpanded = !IsOilInfoExpanded;
    }

    [RelayCommand]
    private async Task ViewVehicleHistory()
    {
        if (MaintenanceDetail is null) return;
        var parameters = new Dictionary<string, object>
        {
            { "vehicleId", MaintenanceDetail.Prcoid },
            { "vehicleName", $"{MaintenanceDetail.VehicleName} - {MaintenanceDetail.LicensePlate}" }
        };
        await Shell.Current.GoToAsync("///Maintenances/VehicleHistory", parameters);
    }

    [RelayCommand]
    private async Task SaveDiagnosis()
    {
        await ExecuteAsync(async () =>
        {
            var request = new
            {
                GeneralStatus = SelectedGeneralStatus,
                VehicleOperative = IsVehicleOperative,
                Observations = DiagnosisObservations,
                FutureRecommendations = string.IsNullOrWhiteSpace(FutureRecommendations)
                    ? null : FutureRecommendations
            };
            var endpoint = ApiRoutes.Maintenances.SaveDiagnosis.Replace("{id}", _mainid.ToString());
            await _apiService.PostAsync<object>(endpoint, request);
            DiagnosisSaved = true;
            CanClose = true;
        });
        if (!HasError) await Load();
    }

    private async Task PersistChecklistItemsAsync()
    {
        // Acciones marcadas como "Realizado"
        foreach (var item in ActionChecklist.Where(a => a.IsDone && !a.WasAlreadyDone))
        {
            try
            {
                var endpoint = ApiRoutes.Maintenances.CreateAction.Replace("{id}", _mainid.ToString());
                await _apiService.PostAsync<object>(endpoint, new { ActionCatalogId = item.Acatid });
            }
            catch { }
        }

        // Materiales marcados como "Usado" con cantidad > 0
        foreach (var item in MaterialChecklist.Where(m => m.IsDone && m.Quantity > 0))
        {
            try
            {
                var endpoint = ApiRoutes.Maintenances.ConsumeMaterial.Replace("{id}", _mainid.ToString());
                await _apiService.PostAsync<object>(endpoint, new
                {
                    Mateid = item.Mateid,
                    Quantity = item.Quantity,
                    Origin = item.Origin
                });
            }
            catch { }
        }

        // Componentes marcados como "Instalado"
        foreach (var item in ComponentChecklist.Where(c => c.IsDone && !c.WasAlreadyInstalled))
        {
            try
            {
                var endpoint = ApiRoutes.Maintenances.InstallComponent.Replace("{id}", _mainid.ToString());
                await _apiService.PostAsync<object>(endpoint, new
                {
                    ActionCatalogId = item.Acatid,
                    Quantity = item.Quantity > 0 ? item.Quantity : 1,
                    LotId = (int?)null,
                    UsefulLifeDays = (int?)null
                });
            }
            catch { }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("mainid", out var id) && id is string idStr && int.TryParse(idStr, out var mainid))
        {
            _mainid = mainid;
            Title = $"Orden #{mainid}";
            LoadCommand.Execute(null);
        }
    }

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Maintenances.GetById.Replace("{id}", _mainid.ToString());
            var raw = await _apiService.GetAsync<ApiResponse<MaintenanceDetailResponse>>(endpoint);
            var detail = raw?.Data;
            if (detail is not null)
            {
                MaintenanceDetail = detail;
                Diagnosis = detail.Diagnosis;
                DiagnosisSaved = detail.Diagnosis is not null;
                CanClose = detail.Status == "AC" && DiagnosisSaved;
                CanCancel = detail.Status == "AC";
                IsEmpty = false;
            }
            else
            {
                throw new Exception("No se encontró la orden de mantenimiento.");
            }

            await LoadActionChecklistAsync();
            await LoadMaterialChecklistAsync();
            await LoadComponentChecklistAsync();
            await LoadTechniciansAsync();
        });
    }

    private async Task LoadActionChecklistAsync()
    {
        try
        {
            var allowedIds = new HashSet<int>(MaintenanceDetail?.AllowedActionIds ?? new List<int>());
            var existingIds = new HashSet<int>(MaintenanceDetail?.Actions?.Select(a => a.ActionId) ?? new List<int>());
            var raw = await _apiService.GetAsync<ApiResponse<List<ActionCatalogOption>>>(ApiRoutes.Maintenances.ActionCatalog);
            var all = raw?.Data ?? new List<ActionCatalogOption>();
            var actionsOnly = all.Where(a => a.Category is not null && a.Category.Contains("Acción")).ToList();

            if (allowedIds.Count > 0)
                actionsOnly = actionsOnly.Where(a => allowedIds.Contains(a.Acatid)).ToList();

            ActionChecklist = new ObservableCollection<ActionChecklistItem>(
                actionsOnly.Select(a => new ActionChecklistItem
                {
                    Acatid = a.Acatid,
                    GroupKey = $"action_{a.Acatid}",
                    Name = a.Name,
                    Detail = a.Category,
                    IsDone = existingIds.Contains(a.Acatid),
                    WasAlreadyDone = existingIds.Contains(a.Acatid)
                }));
        }
        catch { }
    }

    private async Task LoadMaterialChecklistAsync()
    {
        try
        {
            var allowedIds = new HashSet<int>(MaintenanceDetail?.AllowedMaterialIds ?? new List<int>());
            var raw = await _apiService.GetAsync<ApiResponse<List<MaterialItemDto>>>(ApiRoutes.Inventory.GetMaterials);
            var materials = raw?.Data ?? new List<MaterialItemDto>();
            if (allowedIds.Count > 0)
                materials = materials.Where(m => allowedIds.Contains(m.Mateid)).ToList();

            MaterialChecklist = new ObservableCollection<MaterialChecklistItem>(
                materials.Select(m => new MaterialChecklistItem
                {
                    Mateid = m.Mateid,
                    GroupKey = $"mat_{m.Mateid}",
                    Name = m.Name,
                    Detail = m.UnitOfMeasure,
                    StockAvailable = m.StockTotal,
                    Quantity = 0
                }));
        }
        catch { }
    }

    private async Task LoadComponentChecklistAsync()
    {
        try
        {
            var allowedIds = new HashSet<int>(MaintenanceDetail?.AllowedComponentIds ?? new List<int>());
            var existingNames = new HashSet<string>(MaintenanceDetail?.Components?.Select(c => c.ComponentName) ?? new List<string>());

            var raw = await _apiService.GetAsync<ApiResponse<List<ActionCatalogOption>>>(ApiRoutes.Maintenances.ActionCatalog);
            var all = raw?.Data ?? new List<ActionCatalogOption>();
            var compsOnly = all.Where(a => a.Category is not null && a.Category.Contains("Componente")).ToList();

            if (allowedIds.Count > 0)
                compsOnly = compsOnly.Where(a => allowedIds.Contains(a.Acatid)).ToList();

            ComponentChecklist = new ObservableCollection<ComponentChecklistItem>(
                compsOnly.Select(a =>
                {
                    var alreadyInstalled = existingNames.Contains(a.Name);
                    return new ComponentChecklistItem
                    {
                        Acatid = a.Acatid,
                        GroupKey = $"comp_{a.Acatid}",
                        Name = a.Name,
                        Detail = a.Category,
                        IsDone = alreadyInstalled,
                        WasAlreadyInstalled = alreadyInstalled
                    };
                }));
        }
        catch { }
    }

    private async Task LoadTechniciansAsync()
    {
        try
        {
            var raw = await _apiService.GetAsync<TechnicianListResponse>(ApiRoutes.Workers.GetTechnicians);
            if (raw?.Success == true && raw.Data is not null)
            {
                Technicians = new ObservableCollection<TechnicianOption>(
                    raw.Data.Select(t => new TechnicianOption
                    {
                        Workid = t.Workid,
                        FullName = t.FullName,
                    }));
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task AssignTechnician()
    {
        await ExecuteAsync(async () =>
        {
            if (SelectedTechnician is null) return;
            var request = new { Workid = SelectedTechnician.Workid };
            var endpoint = ApiRoutes.Maintenances.AssignTechnician.Replace("{id}", _mainid.ToString());
            await _apiService.PutAsync<object>(endpoint, request);
        });
        if (!HasError) await Load();
    }

    [RelayCommand]
    private async Task CloseOrder()
    {
        bool? isEmergencyComplete = null;

        var isEmergency = MaintenanceDetail?.MaintenanceType == "Emergencia";
        if (isEmergency)
        {
            var choice = await Shell.Current.DisplayActionSheet(
                "¿La emergencia fue completa o parcial?",
                "Cancelar", null,
                "Completa (recalendariza próximo servicio)",
                "Parcial (solo lo urgente, no recalendariza)");

            if (choice is null || choice == "Cancelar") return;
            isEmergencyComplete = choice.Contains("Completa");
        }

        await ExecuteAsync(async () =>
        {
            await PersistChecklistItemsAsync();
            var endpoint = ApiRoutes.Maintenances.Close.Replace("{id}", _mainid.ToString());
            await _apiService.PutAsync<object>(endpoint, new { IsEmergencyComplete = isEmergencyComplete });
            await Shell.Current.GoToAsync("..");
        });
    }

    [RelayCommand]
    private async Task CancelOrder()
    {
        var confirm = await Shell.Current.DisplayAlert("Cancelar orden",
            "¿Estás seguro de cancelar esta orden? Los datos registrados se conservarán con estado Cancelado.", "Sí, cancelar", "No");
        if (!confirm) return;

        await ExecuteAsync(async () =>
        {
            var endpoint = $"{ApiRoutes.Maintenances.Base}/{_mainid}/cancel";
            await _apiService.PutAsync<object>(endpoint);
            await Shell.Current.GoToAsync("..");
        });
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Reports.ExportMaintenancePdf.Replace("{id}", _mainid.ToString());
            var pdfBytes = await _apiService.GetByteArrayAsync(endpoint);
            if (pdfBytes is null || pdfBytes.Length == 0)
            {
                ErrorMessage = "El PDF se generó vacío.";
                HasError = true;
                return;
            }

            var fileName = $"mantenimiento_{_mainid}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(filePath, pdfBytes);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Exportar Orden de Mantenimiento",
                File = new ShareFile(filePath),
            });
        });
    }

    [ObservableProperty]
    private ObservableCollection<TechnicianOption> _technicians = new();

    [ObservableProperty]
    private TechnicianOption? _selectedTechnician;

    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // CHECKLIST ITEM CLASSES
    // ═══════════════════════════════════════════════════════════════

    public partial class ActionChecklistItem : ObservableObject
    {
        public int Acatid { get; set; }
        public string GroupKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Detail { get; set; }

        [ObservableProperty]
        private bool _isDone;

        [ObservableProperty]
        private bool _isNotDone = true;

        public string StatusIcon => IsDone ? "✅" : "—";

        public bool WasAlreadyDone { get; set; }

        public bool IsReadOnly { get; set; }
    }

    public partial class MaterialChecklistItem : ObservableObject
    {
        public int Mateid { get; set; }
        public string GroupKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Detail { get; set; }

        [ObservableProperty]
        private bool _isDone;

        [ObservableProperty]
        private bool _isNotDone = true;

        [ObservableProperty]
        private decimal _quantity;

        [ObservableProperty]
        private string _origin = "Stock propio";

        [ObservableProperty]
        private decimal _stockAvailable;

        [ObservableProperty]
        private string _stockNote = string.Empty;

        public List<string> OriginOptions { get; } = new() { "Stock propio", "Externo" };

        public string StatusIcon => IsDone ? "✅" : "—";

        public string StatusDetail => IsDone
            ? (Quantity > 0 ? $"✅ {Quantity} {Detail ?? ""} ({Origin})" : "✅")
            : "—";

        partial void OnIsDoneChanged(bool value)
        {
            OnPropertyChanged(nameof(StatusDetail));
            UpdateStockNote();
        }

        partial void OnOriginChanged(string value)
        {
            OnPropertyChanged(nameof(StatusDetail));
            UpdateStockNote();
        }

        partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(StatusDetail));

        private void UpdateStockNote()
        {
            if (IsDone && StockAvailable > 0)
                StockNote = $"📦 Stock: {StockAvailable} {Detail ?? ""}";
            else if (IsDone && StockAvailable <= 0 && Origin == "Stock propio")
                StockNote = "⚠ Sin stock disponible";
            else if (IsDone && Origin == "Externo")
                StockNote = "📦 Material externo (no descuenta stock)";
            else
                StockNote = string.Empty;
        }

        public bool IsReadOnly { get; set; }
    }

    public partial class ComponentChecklistItem : ObservableObject
    {
        public int Acatid { get; set; }
        public string GroupKey { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Detail { get; set; }

        [ObservableProperty]
        private bool _isDone;

        [ObservableProperty]
        private bool _isNotDone = true;

        [ObservableProperty]
        private decimal _quantity;

        public string StatusIcon => IsDone ? "✅" : "—";

        public string StatusDetail => IsDone
            ? (Quantity > 0 ? $"✅ {Quantity} unid." : "✅")
            : "—";

        partial void OnIsDoneChanged(bool value) => OnPropertyChanged(nameof(StatusDetail));
        partial void OnQuantityChanged(decimal value) => OnPropertyChanged(nameof(StatusDetail));

        public bool WasAlreadyInstalled { get; set; }

        public bool IsReadOnly { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // DTOs / RESPONSE CLASSES
    // ═══════════════════════════════════════════════════════════════

    public class MaintenanceDetailResponse
    {
        public int Mainid { get; set; }
        public int Prcoid { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public int Mileage { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public string RegisteredByName { get; set; } = string.Empty;
        public string OriginService { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName => Status switch
        {
            "AC" => "Activo",
            "FI" => "Finalizado",
            "CA" => "Cancelado",
            _ => Status
        };
        public string? OilBrand { get; set; }
        public string? OilViscositySae { get; set; }
        public bool ShowOilInNextMaintenance { get; set; }
        [JsonPropertyName("actionDetails")]
        public List<ActionDetailItem>? Actions { get; set; }
        public DiagnosisResponse? Diagnosis { get; set; }
        public List<ComponentItem>? Components { get; set; }
        public List<int>? AllowedActionIds { get; set; }
        public List<int>? AllowedMaterialIds { get; set; }
        public List<int>? AllowedComponentIds { get; set; }
    }

    public class ActionDetailItem
    {
        public int ActionId { get; set; }
        [JsonPropertyName("actionName")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("actionCategory")]
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public bool IsPending { get; set; }
    }

    public class ComponentItem
    {
        public int Incoid { get; set; }
        public string ComponentName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime InstallationDate { get; set; }
        public int InstallationKm { get; set; }
        public DateOnly? ExpirationDate { get; set; }
        public bool Active { get; set; }
    }

    public class DiagnosisResponse
    {
        public string GeneralStatus { get; set; } = string.Empty;
        public bool VehicleOperative { get; set; }
        public string? Observations { get; set; }
        public string? FutureRecommendations { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class TechnicianOption
    {
        public int Workid { get; set; }
        public string FullName { get; set; } = string.Empty;
        public override string ToString() => FullName;
    }

    public sealed class TechnicianListResponse
    {
        public bool Success { get; set; }
        public List<TechnicianDto>? Data { get; set; }
    }

    public sealed class TechnicianDto
    {
        public int Workid { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    public class ActionCatalogOption
    {
        public int Acatid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public override string ToString() => Name;
    }
}
