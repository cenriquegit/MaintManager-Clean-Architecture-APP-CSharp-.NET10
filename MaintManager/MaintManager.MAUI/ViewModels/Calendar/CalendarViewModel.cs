using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Models;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Calendar;

public partial class CalendarViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public CalendarViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Calendario";
    }

    [ObservableProperty]
    private ObservableCollection<MaintenanceCalendarItem> _maintenances = new();

    [ObservableProperty]
    private ObservableCollection<VehicleOption> _vehicleOptions = new();

    [ObservableProperty]
    private string? _filterByVehicle;

    [ObservableProperty]
    private int _filterByType;

    [ObservableProperty]
    private string? _filterByStatus;

    [ObservableProperty]
    private MaintenanceCalendarItem? _selectedMaintenance;

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<MaintenanceCalendarItem>>>(ApiRoutes.Maintenances.GetAll);
            if (response?.Success == true)
            {
                Maintenances = new ObservableCollection<MaintenanceCalendarItem>(response.Data ?? []);
            }
            else
            {
                Maintenances =
                [
                    new MaintenanceCalendarItem { Id = 1, LicensePlate = "ABC-123", VehicleName = "Toyota Hilux", Type = "Preventivo", Status = "Programado", ScheduledDate = DateTime.Today.AddDays(3), AssignedTo = "Carlos" },
                    new MaintenanceCalendarItem { Id = 2, LicensePlate = "DEF-456", VehicleName = "Ford Ranger", Type = "Correctivo", Status = "En Progreso", ScheduledDate = DateTime.Today, AssignedTo = "Miguel" },
                    new MaintenanceCalendarItem { Id = 3, LicensePlate = "GHI-789", VehicleName = "Nissan Navara", Type = "Emergencia", Status = "Pendiente", ScheduledDate = DateTime.Today.AddDays(-1), AssignedTo = "Carlos" },
                    new MaintenanceCalendarItem { Id = 4, LicensePlate = "ABC-123", VehicleName = "Toyota Hilux", Type = "Preventivo", Status = "Completado", ScheduledDate = DateTime.Today.AddDays(-5), AssignedTo = "Luis" },
                ];
            }

            IsEmpty = Maintenances.Count == 0;

            VehicleOptions = new ObservableCollection<VehicleOption>(
                Maintenances
                    .Select(m => m.LicensePlate)
                    .Distinct()
                    .Select(p => new VehicleOption { LicensePlate = p })
                    .Prepend(new VehicleOption { LicensePlate = "Todos" })
                    .ToList());
        });
    }

    [RelayCommand]
    private async Task Filter()
    {
        await ExecuteAsync(async () =>
        {
            var all = new List<MaintenanceCalendarItem>();

            var response = await _apiService.GetAsync<ApiResponse<List<MaintenanceCalendarItem>>>(ApiRoutes.Maintenances.GetAll);
            if (response?.Success == true)
            {
                all = response.Data ?? [];
            }

            var filtered = all.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterByVehicle) && FilterByVehicle != "Todos")
                filtered = filtered.Where(m => m.LicensePlate == FilterByVehicle);

            if (FilterByType > 0)
            {
                var typeMap = FilterByType switch
                {
                    1 => "Preventivo",
                    2 => "Correctivo",
                    3 => "Emergencia",
                    4 => "Programado",
                    _ => null,
                };
                if (typeMap is not null)
                    filtered = filtered.Where(m => m.Type == typeMap);
            }

            if (!string.IsNullOrWhiteSpace(FilterByStatus))
                filtered = filtered.Where(m => m.Status == FilterByStatus);

            Maintenances = new ObservableCollection<MaintenanceCalendarItem>(filtered);
            IsEmpty = Maintenances.Count == 0;
        });
    }

    [RelayCommand]
    private async Task ViewDetail(MaintenanceCalendarItem? item)
    {
        if (item is null) return;
        await Shell.Current.GoToAsync($"//Maintenances/Detail?id={item.Id}");
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
