using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
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
    private ISeries[]? _costPerKmSeries;

    [ObservableProperty]
    private Axis[]? _costPerKmXAxes;

    [ObservableProperty]
    private Axis[]? _costPerKmYAxes;

    // Emergency rate chart
    [ObservableProperty]
    private ISeries[]? _emergencyRateSeries;

    [ObservableProperty]
    private Axis[]? _emergencyRateXAxes;

    [ObservableProperty]
    private Axis[]? _emergencyRateYAxes;

    // Monthly cost chart
    [ObservableProperty]
    private ISeries[]? _monthlyCostSeries;

    [ObservableProperty]
    private Axis[]? _monthlyCostXAxes;

    [ObservableProperty]
    private Axis[]? _monthlyCostYAxes;

    // Expiring lots chart
    [ObservableProperty]
    private ISeries[]? _expiringLotsSeries;

    // Calendar compliance chart
    [ObservableProperty]
    private ISeries[]? _complianceSeries;

    [ObservableProperty]
    private Axis[]? _complianceXAxes;

    [ObservableProperty]
    private Axis[]? _complianceYAxes;

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
            var hasData = false;

            // Dashboard summary
            try
            {
                var summary = await _apiService.GetAsync<ApiResponse<DashboardSummaryDto>>(ApiRoutes.Reports.Dashboard);
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Dashboard summary error: {ex.Message}");
            }

            // Cost per km
            try
            {
                var cost = await _apiService.GetAsync<ApiResponse<List<CostPerKmDto>>>(ApiRoutes.Reports.CostPerKm);
                if (cost?.Success == true && cost.Data is { Count: > 0 })
                {
                    BuildCostPerKmChart(cost.Data);
                    hasData = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Cost per km error: {ex.Message}");
            }

            // Emergency rate
            try
            {
                var emergency = await _apiService.GetAsync<ApiResponse<List<EmergencyRateDto>>>(ApiRoutes.Reports.EmergencyRate);
                if (emergency?.Success == true && emergency.Data is { Count: > 0 })
                {
                    BuildEmergencyRateChart(emergency.Data);
                    hasData = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Emergency rate error: {ex.Message}");
            }

            // Monthly cost
            try
            {
                var monthly = await _apiService.GetAsync<ApiResponse<List<MonthlyCostDto>>>(ApiRoutes.Reports.MonthlyCost + "?months=6");
                if (monthly?.Success == true && monthly.Data is { Count: > 0 })
                {
                    BuildMonthlyCostChart(monthly.Data);
                    hasData = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Monthly cost error: {ex.Message}");
            }

            // Expiring lots
            try
            {
                var lots = await _apiService.GetAsync<ApiResponse<List<ExpiringLotDto>>>(ApiRoutes.Inventory.GetExpiringLots + "?days=60");
                if (lots?.Success == true && lots.Data is { Count: > 0 })
                {
                    BuildExpiringLotsChart(lots.Data);
                    hasData = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Expiring lots error: {ex.Message}");
            }

            // Calendar compliance
            try
            {
                var compliance = await _apiService.GetAsync<ApiResponse<List<ComplianceDto>>>(ApiRoutes.Reports.CalendarCompliance);
                if (compliance?.Success == true && compliance.Data is { Count: > 0 })
                {
                    BuildComplianceChart(compliance.Data);
                    hasData = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[BiDashboard] Calendar compliance error: {ex.Message}");
            }

            IsEmpty = !hasData;
            if (IsEmpty)
            {
                ErrorMessage = "No se encontraron datos para los gráficos. Verifica que haya mantenimientos finalizados.";
                HasError = true;
            }
        });
    }

    private void BuildCostPerKmChart(List<CostPerKmDto> data)
    {
        var ordered = data.OrderByDescending(d => d.CostPerKm).Take(10).ToList();
        var labels = ordered.Select(d => d.LicensePlate).ToArray();
        var values = ordered.Select(d => (double)(d.CostPerKm * 1000m)).ToArray();

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
                Name = "$/1000km",
                TextSize = 11,
                Labeler = v => $"${v:F2}",
            }
        ];
    }

    private void BuildEmergencyRateChart(List<EmergencyRateDto> data)
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

    private void BuildMonthlyCostChart(List<MonthlyCostDto> data)
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

    private void BuildExpiringLotsChart(List<ExpiringLotDto> data)
    {
        var critical = data.Where(l => l.DaysUntilExpiry.HasValue && l.DaysUntilExpiry <= 7).Sum(l => (double)l.CurrentQuantity);
        var warning = data.Where(l => l.DaysUntilExpiry.HasValue && l.DaysUntilExpiry > 7 && l.DaysUntilExpiry <= 30).Sum(l => (double)l.CurrentQuantity);
        var normal = data.Where(l => !l.DaysUntilExpiry.HasValue || l.DaysUntilExpiry > 30).Sum(l => (double)l.CurrentQuantity);

        ExpiringLotsSeries =
        [
            new PieSeries<double>
            {
                Values = [critical],
                Name = $"Crítico (=7d) — {critical:F0}",
                Stroke = new SolidColorPaint(Red),
                Fill = new SolidColorPaint(Red),
                HoverPushout = 4,
            },
            new PieSeries<double>
            {
                Values = [warning],
                Name = $"Próximo (=30d) — {warning:F0}",
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

    private void BuildComplianceChart(List<ComplianceDto> data)
    {
        var topDeviations = data.OrderByDescending(d => Math.Abs(d.KmDeviation)).Take(10).ToList();
        var labels = topDeviations.Select(d => d.LicensePlate).ToArray();

        var seriesList = new List<ISeries>();
        for (int i = 0; i < topDeviations.Count; i++)
        {
            var item = topDeviations[i];
            var color = item.ComplianceStatus switch
            {
                "Puntual" => Green,
                "Anticipado" => Orange,
                "Tardio" => Red,
                _ => Red
            };

            var values = new double[topDeviations.Count];
            values[i] = item.KmDeviation;

            seriesList.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = item.LicensePlate,
                Stroke = new SolidColorPaint(color),
                Fill = new SolidColorPaint(color),
                MaxBarWidth = 24,
            });
        }

        ComplianceSeries = seriesList.ToArray();

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
                Name = "Km de desviación (±)",
                TextSize = 11,
            }
        ];
    }

    [RelayCommand]
    private async Task Refresh() => await Load();

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
