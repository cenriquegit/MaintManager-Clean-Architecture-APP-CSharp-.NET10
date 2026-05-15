using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.BiDashboard;

public partial class BiDashboardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public BiDashboardViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Dashboard";
    }

    // KPI values
    [ObservableProperty]
    private int _totalVehicles;

    [ObservableProperty]
    private int _servicesThisMonth;

    [ObservableProperty]
    private int _lowStockMaterials;

    [ObservableProperty]
    private int _unresolvedAlerts;

    [ObservableProperty]
    private string _fleetAvgCostPerKm = "-";

    [ObservableProperty]
    private string _emergencyRatePercent = "-";

    // Cost per Km chart
    [ObservableProperty]
    private ISeries[] _costPerKmSeries = [];

    [ObservableProperty]
    private Axis[] _costPerKmXAxes = [];

    [ObservableProperty]
    private Axis[] _costPerKmYAxes = [];

    // Emergency rate chart
    [ObservableProperty]
    private ISeries[] _emergencyRateSeries = [];

    [ObservableProperty]
    private Axis[] _emergencyRateXAxes = [];

    [ObservableProperty]
    private Axis[] _emergencyRateYAxes = [];

    // Monthly cost chart
    [ObservableProperty]
    private ISeries[] _monthlyCostSeries = [];

    [ObservableProperty]
    private Axis[] _monthlyCostXAxes = [];

    [ObservableProperty]
    private Axis[] _monthlyCostYAxes = [];

    // Expiring lots chart
    [ObservableProperty]
    private ISeries[] _expiringLotsSeries = [];

    // Calendar compliance chart
    [ObservableProperty]
    private ISeries[] _complianceSeries = [];

    [ObservableProperty]
    private Axis[] _complianceXAxes = [];

    [ObservableProperty]
    private Axis[] _complianceYAxes = [];

    private static readonly SKColor Blue = SKColor.Parse("#1565C0");
    private static readonly SKColor Green = SKColor.Parse("#2E7D32");
    private static readonly SKColor Orange = SKColor.Parse("#F57C00");
    private static readonly SKColor Red = SKColor.Parse("#C62828");
    private static readonly SKColor Teal = SKColor.Parse("#00897B");
    private static readonly SKColor Purple = SKColor.Parse("#7B1FA2");
    private static readonly SKColor Yellow = SKColor.Parse("#FDD835");
    private static readonly SKColor[] Palette = [Blue, Green, Orange, Teal, Purple, Red];

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            try
            {
                var summary = await _apiService.GetAsync<ApiResponse<DashboardSummaryRaw>>(ApiRoutes.Reports.Dashboard);
                if (summary?.Success == true && summary.Data is not null)
                {
                    var s = summary.Data;
                    TotalVehicles = s.TotalVehicles;
                    ServicesThisMonth = s.ServicesThisMonth;
                    LowStockMaterials = s.LowStockMaterials;
                    UnresolvedAlerts = s.UnresolvedAlerts;
                    FleetAvgCostPerKm = $"${s.FleetAvgCostPerKm:F4}/km";
                    EmergencyRatePercent = $"{s.GlobalEmergencyRatePercent:F1}%";
                }
            }
            catch { /* endpoint no disponible, se muestran KPIs en 0 */ }

            try
            {
                var cost = await _apiService.GetAsync<ApiResponse<List<CostPerKmRaw>>>(ApiRoutes.Reports.CostPerKm);
                if (cost?.Success == true && cost.Data is not null)
                    BuildCostPerKmChart(cost.Data);
            }
            catch { }

            try
            {
                var emergency = await _apiService.GetAsync<ApiResponse<List<EmergencyRateRaw>>>(ApiRoutes.Reports.EmergencyRate);
                if (emergency?.Success == true && emergency.Data is not null)
                    BuildEmergencyRateChart(emergency.Data);
            }
            catch { }

            try
            {
                var monthly = await _apiService.GetAsync<ApiResponse<List<MonthlyCostRaw>>>(ApiRoutes.Reports.MonthlyCost + "?months=6");
                if (monthly?.Success == true && monthly.Data is not null)
                    BuildMonthlyCostChart(monthly.Data);
            }
            catch { }

            try
            {
                var lots = await _apiService.GetAsync<ApiResponse<List<ExpiringLotRaw>>>(ApiRoutes.Inventory.GetExpiringLots + "?days=60");
                if (lots?.Success == true && lots.Data is not null)
                    BuildExpiringLotsChart(lots.Data);
            }
            catch { }

            try
            {
                var compliance = await _apiService.GetAsync<ApiResponse<List<ComplianceRaw>>>(ApiRoutes.Reports.CalendarCompliance);
                if (compliance?.Success == true && compliance.Data is not null)
                    BuildComplianceChart(compliance.Data);
            }
            catch { }

            IsEmpty = CostPerKmSeries.Length == 0 && MonthlyCostSeries.Length == 0;
        });
    }

    private void BuildCostPerKmChart(List<CostPerKmRaw> data)
    {
        var ordered = data.OrderByDescending(d => d.CostPerKm).Take(10).ToList();
        var labels = ordered.Select(d => d.LicensePlate).ToArray();
        var values = ordered.Select(d => (double)d.CostPerKm).ToArray();

        CostPerKmSeries =
        [
            new ColumnSeries<double>
            {
                Values = values,
                Name = "Costo por Km",
                Stroke = new SolidColorPaint(Blue),
                Fill = new SolidColorPaint(Blue),
                MaxBarWidth = 24,
            }
        ];

        CostPerKmXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -20,
                TextSize = 10,
            }
        ];

        CostPerKmYAxes =
        [
            new Axis
            {
                Name = "$/km",
                TextSize = 11,
                Labeler = v => $"${v:F2}",
            }
        ];
    }

    private void BuildEmergencyRateChart(List<EmergencyRateRaw> data)
    {
        var ordered = data.OrderByDescending(d => d.EmergencyRatePercent).Take(8).ToList();
        var labels = ordered.Select(d => d.LicensePlate).ToArray();
        var values = ordered.Select(d => (double)d.EmergencyRatePercent).ToArray();

        EmergencyRateSeries =
        [
            new RowSeries<double>
            {
                Values = values,
                Name = "Tasa de Emergencia",
                Stroke = new SolidColorPaint(Orange),
                Fill = new SolidColorPaint(Orange),
                MaxBarWidth = 20,
            }
        ];

        EmergencyRateXAxes =
        [
            new Axis
            {
                Name = "%",
                TextSize = 10,
                Labeler = v => $"{v:F0}%",
            }
        ];

        EmergencyRateYAxes =
        [
            new Axis
            {
                Labels = labels,
                TextSize = 10,
            }
        ];
    }

    private void BuildMonthlyCostChart(List<MonthlyCostRaw> data)
    {
        var ordered = data.OrderBy(d => d.Month).ToList();
        var labels = ordered.Select(d => d.Month.ToString("MMM yy")).ToArray();
        var values = ordered.Select(d => (double)d.MonthlyCost).ToArray();

        MonthlyCostSeries =
        [
            new LineSeries<double>
            {
                Values = values,
                Name = "Costo Mensual",
                Stroke = new SolidColorPaint(Green) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(Green.WithAlpha(30)),
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(Green) { StrokeThickness = 2 },
            }
        ];

        MonthlyCostXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -15,
                TextSize = 11,
            }
        ];

        MonthlyCostYAxes =
        [
            new Axis
            {
                Name = "Costo ($)",
                TextSize = 11,
                Labeler = v => $"${v:N0}",
            }
        ];
    }

    private void BuildExpiringLotsChart(List<ExpiringLotRaw> data)
    {
        var critical = data.Where(l => l.DaysUntilExpiry <= 7).Sum(l => (double)l.CurrentQuantity);
        var warning = data.Where(l => l.DaysUntilExpiry > 7 && l.DaysUntilExpiry <= 30).Sum(l => (double)l.CurrentQuantity);
        var normal = data.Where(l => l.DaysUntilExpiry > 30).Sum(l => (double)l.CurrentQuantity);

        ExpiringLotsSeries =
        [
            new PieSeries<double>
            {
                Values = [critical],
                Name = $"Crítico (≤7d) — {critical:F0}",
                Stroke = new SolidColorPaint(Red),
                Fill = new SolidColorPaint(Red),
                HoverPushout = 4,
            },
            new PieSeries<double>
            {
                Values = [warning],
                Name = $"Próximo (≤30d) — {warning:F0}",
                Stroke = new SolidColorPaint(Orange),
                Fill = new SolidColorPaint(Orange),
                HoverPushout = 4,
            },
            new PieSeries<double>
            {
                Values = [normal],
                Name = $"Normal (>30d) — {normal:F0}",
                Stroke = new SolidColorPaint(Green),
                Fill = new SolidColorPaint(Green),
                HoverPushout = 4,
            },
        ];
    }

    private void BuildComplianceChart(List<ComplianceRaw> data)
    {
        var topDeviations = data.OrderByDescending(d => d.KmDeviation).Take(10).ToList();
        var labels = topDeviations.Select(d => d.LicensePlate).ToArray();
        var deviations = topDeviations.Select(d => d.KmDeviation).ToArray();

        ComplianceSeries =
        [
            new ColumnSeries<int>
            {
                Values = deviations,
                Name = "Desviación Km",
                Stroke = new SolidColorPaint(Red),
                Fill = new SolidColorPaint(Red),
                MaxBarWidth = 24,
            }
        ];

        ComplianceXAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -20,
                TextSize = 10,
            }
        ];

        ComplianceYAxes =
        [
            new Axis
            {
                Name = "Km de desviación",
                TextSize = 11,
            }
        ];
    }

    [RelayCommand]
    private async Task Refresh() => await Load();

    // ─── Raw DTOs matching API response structure ───

    private sealed record DashboardSummaryRaw(
        int TotalVehicles, int ServicesThisMonth, decimal GlobalEmergencyRatePercent,
        int LowStockMaterials, int UnresolvedAlerts, int ExpiringLots,
        decimal FleetAvgCostPerKm);

    private sealed class CostPerKmRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int TotalServices { get; init; }
        public decimal TotalMaterialCost { get; init; }
        public int CurrentKm { get; init; }
        public decimal CostPerKm { get; init; }
    }

    private sealed class EmergencyRateRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int ScheduledCount { get; init; }
        public int EmergencyCount { get; init; }
        public int TotalCount { get; init; }
        public decimal EmergencyRatePercent { get; init; }
    }

    private sealed class MonthlyCostRaw
    {
        public DateTime Month { get; init; }
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public int ServicesCount { get; init; }
        public decimal MonthlyCost { get; init; }
    }

    private sealed class ExpiringLotRaw
    {
        public string MaterialName { get; init; } = string.Empty;
        public decimal CurrentQuantity { get; init; }
        public int DaysUntilExpiry { get; init; }
        public decimal AtRiskCost { get; init; }
    }

    private sealed class ComplianceRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public int KmDeviation { get; init; }
        public string ComplianceStatus { get; init; } = string.Empty;
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
