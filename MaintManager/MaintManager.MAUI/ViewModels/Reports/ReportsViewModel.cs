using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
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
                Route = "cost-per-km"
            },
            new()
            {
                Title = "Órdenes de Mantenimiento",
                Description = "Exportar órdenes de mantenimiento en formato PDF.",
                Icon = "🔧",
                Route = "maintenance-orders"
            },
            new()
            {
                Title = "Alertas",
                Description = "Exportar resumen de alertas del sistema.",
                Icon = "⚠️",
                Route = "alerts"
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

        await ExecuteAsync(async () =>
        {
            switch (reportType)
            {
                case "cost-per-km":
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
                    break;
                default:
                    HasError = true;
                    ErrorMessage = "Este reporte aún no está disponible.";
                    return;
            }
        });
    }

    public partial class ReportItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
    }
}
