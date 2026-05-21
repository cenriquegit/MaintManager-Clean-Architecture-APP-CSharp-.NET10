using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;

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

    [RelayCommand]
    private void ToggleOilInfo()
    {
        IsOilInfoExpanded = !IsOilInfoExpanded;
    }

    [RelayCommand]
    private async Task SaveDiagnosis()
    {
        await ExecuteAsync(async () =>
        {
            var request = new
            {
                GeneralStatus = "Bueno",
                VehicleOperative = true,
                Observations = DiagnosisObservations,
                FutureRecommendations = (string?)null
            };
            var endpoint = ApiRoutes.Maintenances.SaveDiagnosis.Replace("{id}", _mainid.ToString());
            await _apiService.PostAsync<object>(endpoint, request);
            DiagnosisSaved = true;
            CanClose = true;
            await Load();
        });
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
            var detail = await _apiService.GetAsync<MaintenanceDetailResponse>(endpoint);
            if (detail is not null)
            {
                MaintenanceDetail = detail;
                ActionDetails = new ObservableCollection<ActionDetailItem>(
                    detail.Actions ?? new List<ActionDetailItem>());
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
        });
    }

    private async Task LoadMaterialsAsync()
    {
        try
        {
            var raw = await _apiService.GetAsync<ApiResponse<List<MaterialItemDto>>>(ApiRoutes.Inventory.GetMaterials);
            if (raw?.Success == true && raw.Data is not null)
            {
                AvailableMaterials = new ObservableCollection<MaterialOption>(
                    raw.Data.Select(m => new MaterialOption
                    {
                        Mateid = m.Mateid,
                        Name = m.Name,
                        UnitOfMeasure = m.UnitOfMeasure,
                    }));
            }
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
            await Load();
        });
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

    [ObservableProperty]
    private string _consumeQuantity = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TechnicianOption> _technicians = new();

    [ObservableProperty]
    private TechnicianOption? _selectedTechnician;

    [RelayCommand]
    private async Task ConsumeMaterial()
    {
        await ExecuteAsync(async () =>
        {
            if (SelectedMaterial is null || !decimal.TryParse(ConsumeQuantity, out var qty) || qty <= 0)
            {
                ErrorMessage = "Selecciona un material y una cantidad válida.";
                HasError = true;
                return;
            }
            var request = new { Mateid = SelectedMaterial.Mateid, Quantity = qty };
            var endpoint = ApiRoutes.Maintenances.ConsumeMaterial.Replace("{id}", _mainid.ToString());
            await _apiService.PostAsync<object>(endpoint, request);
            ConsumeQuantity = string.Empty;
            SelectedMaterial = null;
            await Load();
        });
    }

    [RelayCommand]
    private async Task CloseOrder()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Maintenances.Close.Replace("{id}", _mainid.ToString());
            await _apiService.PutAsync<object>(endpoint);
            await Shell.Current.GoToAsync("..");
        });
    }

    // ── Installed Components ──────────────────────────────────────

    [ObservableProperty]
    private ObservableCollection<ComponentItem> _components = new();

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
        public string OrderNumber { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public int Mileage { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public string RegisteredByName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusName => Status switch
        {
            "AC" => "Activo",
            "FI" => "Finalizado",
            "CA" => "Cancelado",
            _ => Status
        };
        public OilInfo? OilInfo { get; set; }
        public List<ActionDetailItem>? Actions { get; set; }
        public DiagnosisResponse? Diagnosis { get; set; }
        public List<ComponentItem>? Components { get; set; }
    }

    public class ActionDetailItem
    {
        public int ActionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class OilInfo
    {
        public string OilBrand { get; set; } = string.Empty;
        public string OilViscosity { get; set; } = string.Empty;
        public decimal OilQuantity { get; set; }
        public int OilFilterChanged { get; set; }
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
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
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
}
