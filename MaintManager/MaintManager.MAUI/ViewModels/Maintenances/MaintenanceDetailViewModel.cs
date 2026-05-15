using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
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
    private bool _canClose;

    [ObservableProperty]
    private bool _isOilInfoExpanded;

    [RelayCommand]
    private void ToggleOilInfo()
    {
        IsOilInfoExpanded = !IsOilInfoExpanded;
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
                CanClose = detail.Status is "En progreso" or "Pendiente";
                IsEmpty = false;
            }
            else
            {
                throw new Exception("No se encontró la orden de mantenimiento.");
            }
        });
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
        public OilInfo? OilInfo { get; set; }
        public List<ActionDetailItem>? Actions { get; set; }
        public DiagnosisResponse? Diagnosis { get; set; }
    }

    public class OilInfo
    {
        public string OilBrand { get; set; } = string.Empty;
        public string OilViscosity { get; set; } = string.Empty;
        public decimal OilQuantity { get; set; }
        public int OilFilterChanged { get; set; }
    }

    public class ActionDetailItem
    {
        public int ActionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class DiagnosisResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RecommendedAction { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
    }
}
