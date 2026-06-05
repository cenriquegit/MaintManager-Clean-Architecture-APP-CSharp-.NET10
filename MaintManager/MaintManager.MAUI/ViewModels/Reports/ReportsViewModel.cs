using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Reports;

public partial class ReportsViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public ReportsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Reportes";

        AvailableReports = new ObservableCollection<ReportItem>
        {
            new()
            {
                Title = "Costo por Km",
                Description = "Exportar reporte de costo por kilómetro en formato Excel.",
                Icon = "💰",
                Type = "cost-per-km"
            },
            new()
            {
                Title = "Órdenes de Mantenimiento",
                Description = "Exportar órdenes de mantenimiento en formato PDF con filtros.",
                Icon = "🔧",
                Type = "maintenance-orders"
            },
            new()
            {
                Title = "Alertas",
                Description = "Exportar resumen de alertas del sistema en PDF con filtros.",
                Icon = "⚠️",
                Type = "alerts"
            },
            new()
            {
                Title = "Historial por Vehículo",
                Description = "Exportar historial completo de mantenimiento de un vehículo en PDF.",
                Icon = "📋",
                Type = "vehicle-history"
            }
        };
        IsEmpty = AvailableReports.Count == 0;
    }

    [ObservableProperty]
    private ObservableCollection<ReportItem> _availableReports = new();

    [RelayCommand]
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task GenerateReport(string reportType)
    {
        if (string.IsNullOrWhiteSpace(reportType)) return;

        if (reportType == "cost-per-km")
        {
            await ExecuteAsync(async () =>
            {
                var excelBytes = await _apiService.GetByteArrayAsync("api/v1/reports/cost-excel");
                if (excelBytes is not null && excelBytes.Length > 0)
                {
                    var filePath = Path.Combine(FileSystem.CacheDirectory, $"costo_por_km_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
                    await File.WriteAllBytesAsync(filePath, excelBytes);
                    await Share.Default.RequestAsync(new ShareFileRequest
                    {
                        Title = "Exportar Costo por Km",
                        File = new ShareFile(filePath),
                    });
                }
            });
            return;
        }

        await ExecuteAsync(async () =>
        {
            string endpoint;
            string fileName;

            switch (reportType)
            {
                case "maintenance-orders":
                    endpoint = ApiRoutes.Reports.MaintenanceOrders;
                    fileName = $"ordenes_mantenimiento_{DateTime.Now:yyyyMMdd}.pdf";
                    break;
                case "alerts":
                    endpoint = ApiRoutes.Reports.Alerts;
                    fileName = $"alertas_{DateTime.Now:yyyyMMdd}.pdf";
                    break;
                case "vehicle-history":
                    // Needs vehicleId - prompt for it
                    var vehicles = await _apiService.GetAsync<ApiResponse<List<VehicleOption>>>(ApiRoutes.Vehicles.GetAll);
                    if (vehicles?.Data is null || vehicles.Data.Count == 0) return;

                    var selected = await Shell.Current.DisplayActionSheet("Seleccionar vehículo", "Cancelar", null,
                        vehicles.Data.Select(v => v.ToString()).ToArray());
                    if (string.IsNullOrWhiteSpace(selected) || selected == "Cancelar") return;

                    var vehicle = vehicles.Data.FirstOrDefault(v => v.ToString() == selected);
                    if (vehicle is null) return;

                    endpoint = ApiRoutes.Reports.VehicleHistory;
                    fileName = $"historial_{vehicle.LicensePlate}_{DateTime.Now:yyyyMMdd}.pdf";
                    var body = new { prcoid = vehicle.Prcoid, dateFrom = (DateOnly?)null, dateTo = (DateOnly?)null };
                    var pdf = await _apiService.PostByteArrayAsync(endpoint, body);
                    if (pdf is not null && pdf.Length > 0)
                    {
                        var pdfPath = Path.Combine(FileSystem.CacheDirectory, fileName);
                        await File.WriteAllBytesAsync(pdfPath, pdf);
                        await Share.Default.RequestAsync(new ShareFileRequest
                        {
                            Title = "Historial del Vehículo",
                            File = new ShareFile(pdfPath),
                        });
                    }
                    return;
                default:
                    return;
            }

            // Maintenance orders and alerts: send with no filters
            var pdfBytes = await _apiService.PostByteArrayAsync(endpoint, new { });
            if (pdfBytes is not null && pdfBytes.Length > 0)
            {
                var path = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllBytesAsync(path, pdfBytes);
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = Title,
                    File = new ShareFile(path),
                });
            }
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
    }

    public partial class ReportItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
