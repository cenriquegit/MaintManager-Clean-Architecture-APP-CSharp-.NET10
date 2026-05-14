using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
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

    [ObservableProperty]
    private DashboardSummaryItem _dashboardSummary = new();

    [ObservableProperty]
    private ObservableCollection<CostPerKmItem> _costPerKmData = new();

    [ObservableProperty]
    private ObservableCollection<EmergencyRateItem> _emergencyRateData = new();

    [ObservableProperty]
    private ObservableCollection<MonthlyCostItem> _monthlyCostData = new();

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var summaryTask = _apiService.GetAsync<ApiResponse<DashboardSummaryItem>>(ApiRoutes.Reports.Dashboard);
            var costTask = _apiService.GetAsync<ApiResponse<List<CostPerKmItem>>>(ApiRoutes.Reports.CostPerKm);
            var emergencyTask = _apiService.GetAsync<ApiResponse<List<EmergencyRateItem>>>(ApiRoutes.Reports.EmergencyRate);
            var monthlyTask = _apiService.GetAsync<ApiResponse<List<MonthlyCostItem>>>(ApiRoutes.Reports.MonthlyCost);

            await Task.WhenAll(summaryTask, costTask, emergencyTask, monthlyTask);

            if (summaryTask.Result?.Success == true && summaryTask.Result.Data is not null)
                DashboardSummary = summaryTask.Result.Data;

            if (costTask.Result?.Success == true && costTask.Result.Data is not null)
                CostPerKmData = new ObservableCollection<CostPerKmItem>(costTask.Result.Data);

            if (emergencyTask.Result?.Success == true && emergencyTask.Result.Data is not null)
                EmergencyRateData = new ObservableCollection<EmergencyRateItem>(emergencyTask.Result.Data);

            if (monthlyTask.Result?.Success == true && monthlyTask.Result.Data is not null)
                MonthlyCostData = new ObservableCollection<MonthlyCostItem>(monthlyTask.Result.Data);

            IsEmpty = CostPerKmData.Count == 0 && EmergencyRateData.Count == 0 && MonthlyCostData.Count == 0;
        });
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await Load();
    }

    public record DashboardSummaryItem
    {
        public int TotalVehicles { get; init; }
        public int ServicesThisMonth { get; init; }
        public decimal EmergencyRate { get; init; }
        public int LowStockMaterials { get; init; }
        public int UnresolvedAlerts { get; init; }
        public int ExpiringLots { get; init; }
        public decimal FleetAvgCostPerKm { get; init; }
    }

    public partial class CostPerKmItem
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public partial class EmergencyRateItem
    {
        public string Period { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public int TotalServices { get; set; }
        public int EmergencyCount { get; set; }
    }

    public partial class MonthlyCostItem
    {
        public string Month { get; set; } = string.Empty;
        public decimal Cost { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
