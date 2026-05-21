using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Maintenances;

public partial class VehicleHistoryViewModel : BaseViewModel, IQueryAttributable
{
    private readonly ApiService _apiService;
    private int _vehicleId;

    public VehicleHistoryViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Historial de Mantenimientos";
    }

    [ObservableProperty]
    private string _vehicleInfo = string.Empty;

    [ObservableProperty]
    private ObservableCollection<MaintenanceListItemDto> _maintenances = new();

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("vehicleId", out var id))
            _vehicleId = Convert.ToInt32(id);
        if (query.TryGetValue("vehicleName", out var name))
            VehicleInfo = name?.ToString() ?? "Historial";
        LoadCommand.Execute(null);
    }

    [RelayCommand]
    private async Task Load()
    {
        await ExecuteAsync(async () =>
        {
            var endpoint = ApiRoutes.Maintenances.GetByVehicle
                .Replace("{vehicleId}", _vehicleId.ToString());
            var response = await _apiService.GetAsync<ApiResponse<List<MaintenanceListItemDto>>>(endpoint);
            if (response?.Success == true)
            {
                Maintenances = new ObservableCollection<MaintenanceListItemDto>(
                    response.Data ?? new List<MaintenanceListItemDto>());
                IsEmpty = Maintenances.Count == 0;
                if (string.IsNullOrEmpty(VehicleInfo) && Maintenances.Count > 0)
                    VehicleInfo = $"{Maintenances[0].VehicleName} - {Maintenances[0].LicensePlate}";
            }
            else
            {
                throw new Exception(response?.Message ?? "Error al cargar historial");
            }
        });
    }

    [RelayCommand]
    private async Task ViewDetail(MaintenanceListItemDto item)
    {
        if (item is null) return;
        await Shell.Current.GoToAsync($"//Maintenances/Detail?mainid={item.Mainid}");
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
