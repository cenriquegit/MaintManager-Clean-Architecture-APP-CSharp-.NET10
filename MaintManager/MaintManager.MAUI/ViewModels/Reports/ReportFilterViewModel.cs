using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.MAUI.Models;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Reports;

public partial class ReportFilterViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private string _reportType = string.Empty;

    public ReportFilterViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Filtros de Reporte";
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("type", out var type))
        {
            _reportType = type?.ToString() ?? "";
            Title = _reportType switch
            {
                "maintenance-orders" => "Filtros - Órdenes",
                "alerts" => "Filtros - Alertas",
                "vehicle-history" => "Filtros - Historial",
                _ => "Filtros de Reporte"
            };
            OnReportTypeChanged();
        }
    }

    // ── Filters ─────────────────────────────────────────────────────
    [ObservableProperty]
    private bool _showVehiclePicker;

    [ObservableProperty]
    private bool _showStatusPicker;

    [ObservableProperty]
    private bool _showTypePicker;

    [ObservableProperty]
    private bool _vehicleRequired;

    [ObservableProperty]
    private ObservableCollection<VehicleOption> _vehicles = new();

    [ObservableProperty]
    private VehicleOption? _selectedVehicle;

    [ObservableProperty]
    private ObservableCollection<string> _statusOptions = new()
    { "Todas", "Activas (AC)", "Finalizadas (FI)" };

    [ObservableProperty]
    private string _selectedStatus = "Todas";

    [ObservableProperty]
    private ObservableCollection<string> _maintenanceTypeOptions = new()
    { "Todos", "Calendarizado", "Emergencia" };

    [ObservableProperty]
    private string _selectedMaintenanceType = "Todos";

    [ObservableProperty]
    private ObservableCollection<string> _alertStatusOptions = new()
    { "No resueltas", "Resueltas", "Todas" };

    [ObservableProperty]
    private string _selectedAlertStatus = "No resueltas";

    [ObservableProperty]
    private DateTime _dateFrom = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _dateTo = DateTime.Today;

    [ObservableProperty]
    private string _actionButtonText = "Generar Reporte";

    private void OnReportTypeChanged()
    {
        ShowVehiclePicker = _reportType != "alerts";
        ShowStatusPicker = _reportType == "maintenance-orders";
        ShowTypePicker = _reportType == "maintenance-orders";
        VehicleRequired = _reportType == "vehicle-history";
        ActionButtonText = "Generar PDF";
        SelectedStatus = "Todas";
        SelectedMaintenanceType = "Todos";
        SelectedAlertStatus = _reportType == "alerts" ? "No resueltas" : "Todas";
        _ = LoadVehiclesAsync();
    }

    private async Task LoadVehiclesAsync()
    {
        try
        {
            var raw = await _apiService.GetAsync<ApiResponse<List<VehicleOption>>>(
                ApiRoutes.Vehicles.GetAll);
            if (raw?.Success == true && raw.Data is not null)
            {
                if (!VehicleRequired)
                    raw.Data.Insert(0, new VehicleOption { Prcoid = 0, Name = "Todos los vehículos", LicensePlate = "" });
                Vehicles = new ObservableCollection<VehicleOption>(raw.Data);
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task Generate()
    {
        if (VehicleRequired && SelectedVehicle is null)
        {
            ErrorMessage = "Debes seleccionar un vehículo.";
            HasError = true;
            return;
        }

        await ExecuteAsync(async () =>
        {
            switch (_reportType)
            {
                case "maintenance-orders":
                    await GenerateMaintenanceOrders();
                    break;
                case "alerts":
                    await GenerateAlerts();
                    break;
                case "vehicle-history":
                    await GenerateVehicleHistory();
                    break;
            }
        });
    }

    private async Task GenerateMaintenanceOrders()
    {
        var body = new
        {
            dateFrom = (DateOnly?)DateOnly.FromDateTime(DateFrom),
            dateTo = (DateOnly?)DateOnly.FromDateTime(DateTo),
            prcoid = SelectedVehicle?.Prcoid > 0 ? SelectedVehicle.Prcoid : (int?)null,
            status = SelectedStatus switch
            {
                "Activas (AC)" => "AC",
                "Finalizadas (FI)" => "FI",
                _ => (string?)null
            },
            matyid = SelectedMaintenanceType switch
            {
                "Calendarizado" => (short?)1,
                "Emergencia" => (short?)2,
                _ => (short?)null
            }
        };

        var pdfBytes = await _apiService.PostByteArrayAsync(ApiRoutes.Reports.MaintenanceOrders, body);
        if (pdfBytes is not null && pdfBytes.Length > 0)
            await SharePdf(pdfBytes, $"ordenes_mantenimiento_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task GenerateAlerts()
    {
        var body = new
        {
            dateFrom = (DateOnly?)DateOnly.FromDateTime(DateFrom),
            dateTo = (DateOnly?)DateOnly.FromDateTime(DateTo),
            resolved = SelectedAlertStatus switch
            {
                "No resueltas" => (bool?)false,
                "Resueltas" => (bool?)true,
                _ => (bool?)null
            },
            alertType = (string?)null
        };

        var pdfBytes = await _apiService.PostByteArrayAsync(ApiRoutes.Reports.Alerts, body);
        if (pdfBytes is not null && pdfBytes.Length > 0)
            await SharePdf(pdfBytes, $"alertas_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task GenerateVehicleHistory()
    {
        if (SelectedVehicle is null) return;

        var body = new
        {
            prcoid = SelectedVehicle.Prcoid,
            dateFrom = (DateOnly?)DateOnly.FromDateTime(DateFrom),
            dateTo = (DateOnly?)DateOnly.FromDateTime(DateTo)
        };

        var pdfBytes = await _apiService.PostByteArrayAsync(ApiRoutes.Reports.VehicleHistory, body);
        if (pdfBytes is not null && pdfBytes.Length > 0)
            await SharePdf(pdfBytes, $"historial_{SelectedVehicle.LicensePlate}_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task SharePdf(byte[] bytes, string fileName)
    {
        var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(filePath, bytes);
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = Title,
            File = new ShareFile(filePath),
        });
    }

    public class VehicleOption
    {
        public int Prcoid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public override string ToString() => $"{Name} — {LicensePlate}";
    }

    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
