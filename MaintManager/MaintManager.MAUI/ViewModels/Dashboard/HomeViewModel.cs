using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Dashboard;

public partial class HomeViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public HomeViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Panel Principal";
    }

    [ObservableProperty]
    private ObservableCollection<VehicleCard> _vehicles = new();

    [ObservableProperty]
    private ObservableCollection<KpiItem> _kpiItems =
    [
        new KpiItem("Cargando...", "-", ""),
        new KpiItem("Cargando...", "-", ""),
        new KpiItem("Cargando...", "-", ""),
        new KpiItem("Cargando...", "-", ""),
    ];

    [ObservableProperty]
    private int _scheduledCount;

    [ObservableProperty]
    private int _inProgressCount;

    [ObservableProperty]
    private int _completedThisMonth;

    [ObservableProperty]
    private int _emergencyThisMonth;

    [ObservableProperty]
    private ObservableCollection<QuickAction> _quickActions = new();

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            QuickActions =
            [
                new QuickAction("🚗 Nuevo Mantenimiento", nameof(NavigateToMaintenance)),
                new QuickAction("📅 Calendario", nameof(NavigateToCalendar)),
                new QuickAction("📦 Inventario", nameof(NavigateToInventory)),
                new QuickAction("📊 BI Dashboard", nameof(NavigateToBiDashboard)),
            ];

            var dashResponse = await _apiService.GetAsync<ApiResponse<DashboardData>>(ApiRoutes.Reports.Dashboard);
            if (dashResponse?.Success == true && dashResponse.Data is not null)
            {
                var data = dashResponse.Data;
                KpiItems =
                [
                    new KpiItem("Vehículos", data.TotalVehicles.ToString(), "🚗"),
                    new KpiItem("Servicios del Mes", data.ServicesThisMonth.ToString(), "🔧"),
                    new KpiItem("Stock Bajo", data.LowStockMaterials.ToString(), "📦"),
                    new KpiItem("Alertas", data.UnresolvedAlerts.ToString(), "⚠️"),
                ];
            }
            else
            {
                HasError = true;
                IsEmpty = false;
                ErrorMessage = "No se pudieron cargar los datos del Dashboard.";
                return;
            }

            // Cargar estadísticas de mantenimientos
            var statsResponse = await _apiService.GetAsync<ApiResponse<MaintenanceStats>>(
                $"{ApiRoutes.Maintenances.Base}/stats");
            if (statsResponse?.Success == true && statsResponse.Data is not null)
            {
                var stats = statsResponse.Data;
                ScheduledCount = stats.Scheduled;
                InProgressCount = stats.InProgress;
                CompletedThisMonth = stats.CompletedThisMonth;
                EmergencyThisMonth = stats.EmergencyThisMonth;
            }

            Vehicles =
            [
                new VehicleCard { LicensePlate = "ABC-123", Brand = "Toyota", Model = "Hilux", Year = 2020, CurrentKm = 45000, NextServiceKm = 50000, Status = "Operativo" },
                new VehicleCard { LicensePlate = "DEF-456", Brand = "Ford", Model = "Ranger", Year = 2021, CurrentKm = 32000, NextServiceKm = 40000, Status = "En Mantención" },
                new VehicleCard { LicensePlate = "GHI-789", Brand = "Nissan", Model = "Navara", Year = 2019, CurrentKm = 68000, NextServiceKm = 70000, Status = "Operativo" },
            ];

            IsEmpty = false;
        });
    }

    [RelayCommand]
    private async Task NavigateToMaintenance()
    {
        await Shell.Current.GoToAsync("//Maintenances");
    }

    [RelayCommand]
    private async Task NavigateToCalendar()
    {
        await Shell.Current.GoToAsync("//Calendar");
    }

    [RelayCommand]
    private async Task NavigateToInventory()
    {
        await Shell.Current.GoToAsync("//Inventory");
    }

    [RelayCommand]
    private async Task NavigateToBiDashboard()
    {
        await Shell.Current.GoToAsync("//BiDashboard");
    }

    public partial class VehicleCard
    {
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public double CurrentKm { get; set; }
        public double NextServiceKm { get; set; }
        public string Status { get; set; } = string.Empty;
        public double KmUntilService => NextServiceKm - CurrentKm;
        public string KmProgress => $"{CurrentKm:N0} / {NextServiceKm:N0} km";
        public double Progress => NextServiceKm > 0 ? CurrentKm / NextServiceKm : 0;
    }

    public partial class KpiItem
    {
        public KpiItem() { }
        public KpiItem(string label, string value, string icon)
        {
            Label = label;
            Value = value;
            Icon = icon;
        }
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public partial class QuickAction
    {
        public QuickAction() { }
        public QuickAction(string label, string commandName)
        {
            Label = label;
            CommandName = commandName;
        }
        public string Label { get; set; } = string.Empty;
        public string CommandName { get; set; } = string.Empty;
    }

    public class DashboardData
    {
        public int TotalVehicles { get; set; }
        public int ServicesThisMonth { get; set; }
        public decimal GlobalEmergencyRatePercent { get; set; }
        public int LowStockMaterials { get; set; }
        public int UnresolvedAlerts { get; set; }
        public int ExpiringLots { get; set; }
        public decimal FleetAvgCostPerKm { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    public class MaintenanceStats
    {
        public int Scheduled { get; set; }
        public int InProgress { get; set; }
        public int CompletedThisMonth { get; set; }
        public int EmergencyThisMonth { get; set; }
    }
}
