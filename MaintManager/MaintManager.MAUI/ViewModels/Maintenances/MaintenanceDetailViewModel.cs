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
    private int _mainid;

    public MaintenanceDetailViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Detalle de Orden";
    }

    [ObservableProperty]
    private MaintenanceDetailResponse? _maintenanceDetail;

    partial void OnMaintenanceDetailChanged(MaintenanceDetailResponse? value)
    {
        OnPropertyChanged(nameof(IsReadOnly));
    }

    public bool IsReadOnly => MaintenanceDetail?.Status == "FI";

    [ObservableProperty]
    private ObservableCollection<ActionDetailItem> _actionDetails = new();

    [ObservableProperty]
    private DiagnosisResponse? _diagnosis;

    [ObservableProperty]
    private bool _diagnosisSaved;

    [ObservableProperty]
    private bool _canClose;

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

    // ── Action catalog for Add Action picker ────────────────────
    [ObservableProperty]
    private ObservableCollection<ActionCatalogOption> _actionCatalogItems = new();

    [ObservableProperty]
    private ActionCatalogOption? _selectedActionCatalog;

    [RelayCommand]
    private void AddAction()
    {
        if (SelectedActionCatalog is null) return;
        ActionDetails.Add(new ActionDetailItem
        {
            ActionId = SelectedActionCatalog.Acatid,
            Name = SelectedActionCatalog.Name,
            Description = SelectedActionCatalog.Category ?? string.Empty,
            IsCompleted = false,
            IsPending = true
        });
        SelectedActionCatalog = null;
    }

    [RelayCommand]
    private void RemoveAction(ActionDetailItem action)
    {
        ActionDetails.Remove(action);
    }

    // ── Consumed materials local list ─────────────────────────
    [ObservableProperty]
    private ObservableCollection<ConsumedMaterialItem> _consumedMaterials = new();

    [RelayCommand]
    private void RemoveConsumedMaterial(ConsumedMaterialItem item)
    {
        ConsumedMaterials.Remove(item);
    }

    // ── Remove component locally ──────────────────────────────
    [RelayCommand]
    private void RemoveComponentLocal(ComponentItem component)
    {
        Components.Remove(component);
    }

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
            // 1. Persist pending actions first
            await PersistPendingActionsAsync();

            // 2. Save diagnosis with all fields
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

    private async Task PersistPendingActionsAsync()
    {
        var pending = ActionDetails.Where(a => a.IsPending).ToList();
        foreach (var action in pending)
        {
            try
            {
                var actionsEndpoint = ApiRoutes.Maintenances.CreateAction
                    .Replace("{id}", _mainid.ToString());
                await _apiService.PostAsync<object>(actionsEndpoint, new
                {
                    ActionCatalogId = action.ActionId
                });
            }
            catch
            {
            }
        }

        // Persist pending ratings (materials consumed with local ratings)
        foreach (var item in ConsumedMaterials.Where(c => c.Rating > 0))
        {
            try
            {
                // Get the mateid from the current available materials
                var material = AvailableMaterials.FirstOrDefault(m =>
                    m.Name == item.MaterialName);
                if (material is null) continue;

                var rateEndpoint = ApiRoutes.Inventory.RateMaterial
                    .Replace("{mateid}", material.Mateid.ToString());
                await _apiService.PostAsync<object>(rateEndpoint, new
                {
                    Mateid = material.Mateid,
                    Mainid = _mainid,
                    Rating = (short)item.Rating,
                    Observation = item.RatingObservation
                });
            }
            catch
            {
            }
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
                ActionDetails = new ObservableCollection<ActionDetailItem>(
                    detail.Actions?.Select(a => { a.IsPending = false; return a; }) ?? new List<ActionDetailItem>());
                Diagnosis = detail.Diagnosis;
                DiagnosisSaved = detail.Diagnosis is not null;
                CanClose = detail.Status == "AC" && DiagnosisSaved;
                Components = new ObservableCollection<ComponentItem>(
                    detail.Components ?? new List<ComponentItem>());
                IsEmpty = false;
            }
            else
            {
                throw new Exception("No se encontró la orden de mantenimiento.");
            }

            await LoadMaterialsAsync();
            await LoadTechniciansAsync();
            await LoadComponentActionsAsync();
        });
    }

    private async Task LoadMaterialsAsync()
    {
        try
        {
            var prcoid = MaintenanceDetail?.Prcoid ?? 0;
            var allowedIds = new HashSet<int>();

            if (prcoid > 0)
            {
                var configUrl = $"{ApiRoutes.Vehicles.Base}/{prcoid}/config";
                var config = await _apiService.GetAsync<ApiResponse<VehicleConfigRaw>>(configUrl);
                if (config?.Success == true && config.Data?.AllowedMaterials != null)
                    allowedIds = config.Data.AllowedMaterials.Select(m => m.Mateid).ToHashSet();
            }

            var raw = await _apiService.GetAsync<ApiResponse<List<MaterialItemDto>>>(ApiRoutes.Inventory.GetMaterials);
            var materials = raw?.Data ?? new List<MaterialItemDto>();

            if (allowedIds.Count > 0)
                materials = materials.Where(m => allowedIds.Contains(m.Mateid)).ToList();

            AvailableMaterials = new ObservableCollection<MaterialOption>(
                materials.Select(m => new MaterialOption { Mateid = m.Mateid, Name = m.Name, UnitOfMeasure = m.UnitOfMeasure }));
        }
        catch { }
    }

    private async Task LoadComponentActionsAsync()
    {
        try
        {
            var prcoid = MaintenanceDetail?.Prcoid ?? 0;
            var allowedActionIds = new HashSet<int>();
            var allowedComponentIds = new HashSet<int>();

            if (prcoid > 0)
            {
                var configUrl = $"{ApiRoutes.Vehicles.Base}/{prcoid}/config";
                var config = await _apiService.GetAsync<ApiResponse<VehicleConfigRaw>>(configUrl);
                if (config?.Success == true && config.Data != null)
                {
                    if (config.Data.AllowedActions != null)
                        allowedActionIds = config.Data.AllowedActions.Select(a => a.Acatid).ToHashSet();
                    if (config.Data.AllowedComponents != null)
                        allowedComponentIds = config.Data.AllowedComponents.Select(c => c.Acatid).ToHashSet();
                }
            }

            var raw = await _apiService.GetAsync<ApiResponse<List<ActionCatalogOption>>>(ApiRoutes.Maintenances.ActionCatalog);
            var all = raw?.Data ?? new List<ActionCatalogOption>();

            if (allowedActionIds.Count > 0 || allowedComponentIds.Count > 0)
            {
                all = all.Where(a =>
                {
                    var isComp = a.Category is not null && a.Category.Contains("Componente");
                    return isComp ? allowedComponentIds.Contains(a.Acatid) : allowedActionIds.Contains(a.Acatid);
                }).ToList();
            }

            ComponentActions = new ObservableCollection<ActionCatalogOption>(
                all.Where(a => a.Category is not null && a.Category.Contains("Componente")));
            ActionCatalogItems = new ObservableCollection<ActionCatalogOption>(
                all.Where(a => a.Category is not null && a.Category.Contains("Acción")));
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

    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }

    public sealed class MaterialOption
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public override string ToString() => $"{Name} ({UnitOfMeasure})";
    }

    [RelayCommand]
    private async Task CompleteAction(ActionDetailItem action)
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Maintenances.CompleteAction
                .Replace("{id}", _mainid.ToString())
                .Replace("{actionId}", action.ActionId.ToString());
            await _apiService.PutAsync<object>(endpoint);
            action.IsCompleted = true;
        });
    }

    [ObservableProperty]
    private ObservableCollection<MaterialOption> _availableMaterials = new();

    [ObservableProperty]
    private MaterialOption? _selectedMaterial;

    partial void OnSelectedMaterialChanged(MaterialOption? value)
    {
        _ = LoadSelectedMaterialLotInfo(value);
    }

    [ObservableProperty]
    private string _consumeQuantity = string.Empty;

    // FIFO lot info for selected material
    [ObservableProperty]
    private bool _showLotInfo;

    [ObservableProperty]
    private string _lotNumberDisplay = string.Empty;

    [ObservableProperty]
    private string _lotExpiryDisplay = string.Empty;

    [ObservableProperty]
    private string _lotCostDisplay = string.Empty;

    private async Task LoadSelectedMaterialLotInfo(MaterialOption? material)
    {
        if (material is null)
        {
            ShowLotInfo = false;
            return;
        }
        try
        {
            var endpoint = ApiRoutes.Inventory.GetMaterialLots.Replace("{id}", material.Mateid.ToString());
            var response = await _apiService.GetAsync<ApiResponse<List<LotInfo>>>(endpoint);
            if (response?.Success == true && response.Data is not null && response.Data.Count > 0)
            {
                var firstLot = response.Data[0]; // FIFO: primero que vence
                LotNumberDisplay = string.IsNullOrEmpty(firstLot.SupplierLotNumber) ? "S/N" : firstLot.SupplierLotNumber;
                LotExpiryDisplay = firstLot.ExpirationDate ?? "Sin vencimiento";
                LotCostDisplay = $"S/ {firstLot.UnitCost:N2}";
                ShowLotInfo = true;
            }
            else
            {
                ShowLotInfo = false;
            }
        }
        catch
        {
            ShowLotInfo = false;
        }
    }

    [ObservableProperty]
    private ObservableCollection<TechnicianOption> _technicians = new();

    [ObservableProperty]
    private TechnicianOption? _selectedTechnician;

    [RelayCommand]
    private async Task ConsumeMaterial()
    {
        if (SelectedMaterial is null || !decimal.TryParse(ConsumeQuantity, out var qty) || qty <= 0)
        {
            ErrorMessage = "Selecciona un material y una cantidad válida.";
            HasError = true;
            return;
        }

        var mateConsumed = SelectedMaterial.Mateid;
        var nameConsumed = SelectedMaterial.Name;

        await ExecuteAsync(async () =>
        {
            var request = new { Mateid = mateConsumed, Quantity = qty };
            var endpoint = ApiRoutes.Maintenances.ConsumeMaterial.Replace("{id}", _mainid.ToString());
            await _apiService.PostAsync<object>(endpoint, request);

            ConsumedMaterials.Add(new ConsumedMaterialItem
            {
                MaterialName = nameConsumed,
                LotNumber = LotNumberDisplay,
                UnitCost = decimal.TryParse(LotCostDisplay.Replace("S/ ", ""), out var c) ? c : 0,
                Quantity = qty
            });

            ConsumeQuantity = string.Empty;
            SelectedMaterial = null;
        });
        if (!HasError) await Load();

        // Preguntar si quiere calificar el material recién consumido
        var rate = await Shell.Current.DisplayAlert("Calificar material",
            $"¿Deseas calificar {nameConsumed}?", "Sí, calificar", "No, gracias");

        if (rate)
        {
            var rating = await Shell.Current.DisplayActionSheet(
                $"Califica {nameConsumed} (1-5 estrellas)", "Cancelar", null,
                "⭐ 1 - Malo", "⭐⭐ 2 - Regular", "⭐⭐⭐ 3 - Bueno",
                "⭐⭐⭐⭐ 4 - Muy bueno", "⭐⭐⭐⭐⭐ 5 - Excelente");

            if (rating is not null && rating != "Cancelar")
            {
                var stars = rating.Count(c => c == '⭐');
                if (stars > 0)
                {
                    var observation = await Shell.Current.DisplayPromptAsync(
                        "Observación (opcional)",
                        "Agrega un comentario sobre el material:",
                        "Guardar", "Omitir",
                        placeholder: "ej: Buen rendimiento, llegó en buen estado...");

                    // Guardar rating LOCALMENTE
                    var lastConsumed = ConsumedMaterials.LastOrDefault();
                    if (lastConsumed is not null)
                    {
                        lastConsumed.Rating = stars;
                        lastConsumed.RatingObservation = string.IsNullOrWhiteSpace(observation)
                            ? null : observation;
                    }
                }
            }
        }
    }

    [RelayCommand]
    private async Task CloseOrder()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Maintenances.Close.Replace("{id}", _mainid.ToString());
            await _apiService.PutAsync<object>(endpoint, new { IsEmergencyComplete = false });
            await Shell.Current.GoToAsync("..");
        });
    }

    // ── Installed Components ──────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<ComponentItem> _components = new();

    // ── Install Component ──────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<ActionCatalogOption> _componentActions = new();

    [ObservableProperty]
    private ActionCatalogOption? _selectedComponent;

    partial void OnSelectedComponentChanged(ActionCatalogOption? value)
    {
        OnPropertyChanged(nameof(ComponentHelpText));
    }

    public string? ComponentHelpText => SelectedComponent switch
    {
        { } c when c.Name is not null && c.Category is not null =>
            $"📌 {c.Name} ({c.Category}). Al instalarlo se registrará su vida útil para generar alertas de caducidad.",
        _ => null
    };

    [RelayCommand]
    private async Task InstallComponent()
    {
        await ExecuteAsync(async () =>
        {
            if (SelectedComponent is null) return;
            var endpoint = ApiRoutes.Maintenances.InstallComponent.Replace("{id}", _mainid.ToString());
            await _apiService.PostAsync<object>(endpoint, new
            {
                ActionCatalogId = SelectedComponent.Acatid,
                LotId = (int?)null,
                UsefulLifeDays = (int?)null
            });
            SelectedComponent = null;
        });
        if (!HasError) await Load();
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

    public sealed class LotInfo
    {
        public decimal UnitCost { get; set; }
        public string? ExpirationDate { get; set; }
        public string? SupplierLotNumber { get; set; }
    }

    public class ActionCatalogOption
    {
        public int Acatid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
        public override string ToString() => Name;
    }

    public class ConsumedMaterialItem
    {
        public string MaterialName { get; set; } = string.Empty;
        public string LotNumber { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalCost => UnitCost * Quantity;
        public int Rating { get; set; }
        public string? RatingObservation { get; set; }
        public override string ToString() => $"{MaterialName} ({LotNumber})";
    }

    public class VehicleConfigRaw
    {
        public int Prcoid { get; set; }
        public List<ActionConfigRaw>? AllowedActions { get; set; }
        public List<MaterialConfigRaw>? AllowedMaterials { get; set; }
        public List<ActionConfigRaw>? AllowedComponents { get; set; }
    }

    public class ActionConfigRaw
    {
        public int Acatid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Category { get; set; }
    }

    public class MaterialConfigRaw
    {
        public int Mateid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? UnitOfMeasure { get; set; }
    }
}
