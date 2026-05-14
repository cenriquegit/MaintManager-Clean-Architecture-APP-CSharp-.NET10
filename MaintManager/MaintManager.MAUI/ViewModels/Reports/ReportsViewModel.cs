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
    }

    [ObservableProperty]
    private ObservableCollection<ReportItem> _availableReports = new();

    [RelayCommand]
    private async Task GenerateReport(string reportType)
    {
        if (string.IsNullOrWhiteSpace(reportType)) return;

        await ExecuteAsync(async () =>
        {
            switch (reportType)
            {
                case "cost-per-km":
                    await _apiService.GetAsync<object>("api/v1/reports/cost-excel");
                    break;
                case "maintenance-orders":
                    await _apiService.GetAsync<object>("api/v1/reports/maintenances/pdf");
                    break;
                case "alerts":
                    await _apiService.GetAsync<object>("api/v1/reports/alerts-summary");
                    break;
            }

            await Shell.Current.DisplayAlert("Exportación", "Reporte generado correctamente.", "Aceptar");
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
