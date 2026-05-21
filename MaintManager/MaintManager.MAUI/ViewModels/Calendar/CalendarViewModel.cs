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
    private List<MaintenanceCalendarItem> _allMaintenances = [];

    public CalendarViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Calendario";
    }

    private sealed class MaintenanceListRaw
    {
        public int Mainid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
        public string MaintenanceType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime MaintenanceDate { get; init; }
        public string AssignedToName { get; init; } = string.Empty;
    }

    private sealed class PagedResponse<T>
    {
        public List<T> Items { get; init; } = [];
        public int TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
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
            var response = await _apiService.GetAsync<ApiResponse<PagedResponse<MaintenanceListRaw>>>(ApiRoutes.Maintenances.GetAll);
            if (response?.Success == true && response.Data is not null)
            {
                _allMaintenances = response.Data.Items.Select(raw => new MaintenanceCalendarItem
                {
                    Id = raw.Mainid,
                    LicensePlate = raw.LicensePlate,
                    VehicleName = raw.VehicleName,
                    Type = raw.MaintenanceType,
                    Status = raw.Status,
                    ScheduledDate = raw.MaintenanceDate,
                    AssignedTo = raw.AssignedToName,
                }).ToList();
                Maintenances = new ObservableCollection<MaintenanceCalendarItem>(_allMaintenances);
            }
            else
            {
                HasError = true;
                IsEmpty = false;
                ErrorMessage = "No se pudieron cargar los datos del calendario.";
                return;
            }

            IsEmpty = Maintenances.Count == 0;

            VehicleOptions = new ObservableCollection<VehicleOption>(
                _allMaintenances
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
        if (_allMaintenances.Count == 0)
        {
            await Load();
            return;
        }

        await ExecuteAsync(async () =>
        {
            var filtered = _allMaintenances.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(FilterByVehicle) && FilterByVehicle != "Todos")
                filtered = filtered.Where(m => m.LicensePlate == FilterByVehicle);

            if (FilterByType > 0)
            {
                var typeMap = FilterByType switch
                {
                    1 => "Calendarizado",
                    2 => "Emergencia",
                    _ => null,
                };
                if (typeMap is not null)
                    filtered = filtered.Where(m => m.Type == typeMap);
            }

            if (!string.IsNullOrWhiteSpace(FilterByStatus))
            {
                var statusCode = FilterByStatus switch
                {
                    "Programado" or "En Progreso" or "Pendiente" => "AC",
                    "Completado" => "FI",
                    _ => null
                };
                if (statusCode is not null)
                    filtered = filtered.Where(m => m.Status == statusCode);
            }

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
