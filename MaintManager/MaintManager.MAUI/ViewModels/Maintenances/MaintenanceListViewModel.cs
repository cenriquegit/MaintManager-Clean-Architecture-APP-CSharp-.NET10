using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Maintenances;

public partial class MaintenanceListViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public MaintenanceListViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Órdenes de Mantenimiento";
    }

    private sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    [ObservableProperty]
    private ObservableCollection<MaintenanceItem> _maintenances = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedFilter = string.Empty;

    public List<string> FilterOptions { get; } = new() { "Todas", "Pendientes", "En progreso", "Completadas", "Canceladas" };

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<PagedResult<MaintenanceListItemDto>>>(ApiRoutes.Maintenances.GetAll + "?page=1&pageSize=50");
            var items = response?.Data?.Items?.ToList() ?? new List<MaintenanceListItemDto>();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                items = items.Where(m =>
                    (m.VehicleName?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.LicensePlate?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SelectedFilter) && SelectedFilter != "Todas")
            {
                items = items.Where(m => m.Status.Equals(SelectedFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            Maintenances = new ObservableCollection<MaintenanceItem>(
                items.Select(i => new MaintenanceItem
                {
                    Mainid = i.Mainid,
                    LicensePlate = i.LicensePlate ?? string.Empty,
                    VehicleName = i.VehicleName,
                    MaintenanceType = i.MaintenanceType,
                    ServiceType = i.ServiceType ?? string.Empty,
                    MaintenanceDate = i.MaintenanceDate,
                    Mileage = i.Mileage,
                    AssignedToName = i.AssignedToName,
                    Status = i.Status
                }));

            IsEmpty = Maintenances.Count == 0;
        });
    }

    partial void OnSearchTextChanged(string value) => LoadCommand.Execute(null);

    partial void OnSelectedFilterChanged(string value) => LoadCommand.Execute(null);

    [RelayCommand]
    private async Task ViewDetail(int mainid)
    {
        await Shell.Current.GoToAsync($"Maintenances/Detail?mainid={mainid}");
    }

    [RelayCommand]
    private async Task CreateNew()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Task.Delay(200);

        try { await Shell.Current.GoToAsync("Maintenances/Create"); }
        catch { try { await Shell.Current.GoToAsync("Create"); } catch { } }
    }

    public record MaintenanceItem
    {
        public int Mainid { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime MaintenanceDate { get; set; }
        public int Mileage { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
