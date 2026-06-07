using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.VehicleManagement;

public partial class VehicleManagementViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public VehicleManagementViewModel(ApiService apiService)
    {
        _apiService = apiService;
        Title = "Vehículos";
    }

    [ObservableProperty]
    private ObservableCollection<ManagedVehicleItem> _vehicles = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedSource = "all";

    partial void OnSelectedSourceChanged(string value) => LoadVehiclesCommand.Execute(null);

    public List<string> SourceFilterOptions { get; } = ["all", "legacy", "managed"];
    public Dictionary<string, string> SourceFilterLabels { get; } = new()
    {
        ["all"] = "Todos",
        ["legacy"] = "Flota (Legacy)",
        ["managed"] = "Externos (Nuevos)"
    };

    [RelayCommand]
    private async Task LoadVehicles()
    {
        await ExecuteAsync(async () =>
        {
            var source = SelectedSource == "all" ? null : SelectedSource;
            var search = string.IsNullOrWhiteSpace(SearchText) ? null : SearchText;
            var url = $"api/v1/vehicles/managed?source={source}&search={search}";
            var response = await _apiService.GetAsync<ApiResponse<List<ManagedVehicleItem>>>(url);
            if (response?.Success == true && response.Data is not null)
                Vehicles = new ObservableCollection<ManagedVehicleItem>(response.Data);
            else
            {
                HasError = true;
                ErrorMessage = "Error al cargar vehículos.";
            }
        });
    }

    [RelayCommand]
    private void Search() => LoadVehiclesCommand.Execute(null);

    [RelayCommand]
    private void ApplyFilter() => LoadVehiclesCommand.Execute(null);

    [RelayCommand]
    private async Task CreateVehicle()
    {
        try { await Shell.Current.GoToAsync("///Vehicles/CreateVehicle"); }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error al navegar: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task ConfigVehicle(ManagedVehicleItem? vehicle)
    {
        if (vehicle is null) return;
        try
        {
            var isManaged = vehicle.Source == "managed";
            await Shell.Current.GoToAsync(isManaged
                ? $"///ConfigVehicle?mvId={vehicle.MvId}"
                : $"///ConfigVehicle?vehicleId={vehicle.Prcoid ?? vehicle.MvId}");
        }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error al navegar: {ex.Message}"; }
    }

    [RelayCommand]
    private async Task EditVehicle(ManagedVehicleItem? vehicle)
    {
        if (vehicle is null) return;
        try { await Shell.Current.GoToAsync($"///Vehicles/CreateVehicle?mvId={vehicle.MvId}"); }
        catch (Exception ex) { HasError = true; ErrorMessage = $"Error al navegar: {ex.Message}"; }
    }

    public void OnAppearing()
    {
        LoadVehiclesCommand.Execute(null);
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }
}

public class ManagedVehicleItem
{
    public int MvId { get; set; }
    public int? Prcoid { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public short? Year { get; set; }
    public string? Color { get; set; }
    public string? Vin { get; set; }
    public string? EngineNumber { get; set; }
    public string Source { get; set; } = "managed";
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }

    public string SourceLabel => Source == "legacy" ? "FLOTA" : "EXTERNO";
    public string SourceColor => Source == "legacy" ? "#1565C0" : "#2E7D32";
    public string Display => $"{VehicleName} — {LicensePlate}";
    public string DetailLine => $"{(Brand is not null ? Brand + " " : "")}{(Model is not null ? Model + " " : "")}{(Year.HasValue ? "(" + Year + ")" : "")}".Trim();
    public string BrandModelDetail => $"{(Brand is not null ? Brand : "")}{(Model is not null ? " · " + Model : "")}".Trim();
}
