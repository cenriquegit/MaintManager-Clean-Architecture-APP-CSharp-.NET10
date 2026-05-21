using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using MaintManager.Shared.Models;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Maintenances;

public partial class MaintenanceWizardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    public MaintenanceWizardViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
        Title = "Nueva Orden de Mantenimiento";
    }

    public const int MaxSteps = 4;

    [ObservableProperty]
    private int _currentStep = 1;

    public bool IsFirstStep => CurrentStep == 1;
    public bool IsLastStep => CurrentStep == MaxSteps;

    partial void OnCurrentStepChanged(int value)
    {
        OnPropertyChanged(nameof(IsFirstStep));
        OnPropertyChanged(nameof(IsLastStep));
        OnPropertyChanged(nameof(IsNextButtonVisible));
        OnPropertyChanged(nameof(IsSaveButtonVisible));
    }

    public bool IsNextButtonVisible => !IsLastStep;
    public bool IsSaveButtonVisible => IsLastStep;

    // Step 1: Vehicle selection + mileage
    [ObservableProperty]
    private ObservableCollection<VehicleOption> _vehicles = new();

    [ObservableProperty]
    private VehicleOption? _selectedVehicle;

    partial void OnSelectedVehicleChanged(VehicleOption? value)
    {
        if (value is not null)
            _ = LoadVehicleCurrentKmAsync(value);
        else
        {
            VehicleLastKm = 0;
            OnPropertyChanged(nameof(ShowMileageWarning));
            OnPropertyChanged(nameof(MileageWarning));
        }
    }

    private async Task LoadVehicleCurrentKmAsync(VehicleOption vehicle)
    {
        try
        {
            var endpoint = ApiRoutes.Vehicles.GetCurrentKm.Replace("{id}", vehicle.VehicleId.ToString());
            var response = await _apiService.GetAsync<ApiResponse<int>>(endpoint);
            VehicleLastKm = response?.Data ?? vehicle.CurrentKm;
        }
        catch
        {
            VehicleLastKm = vehicle.CurrentKm;
        }
        OnPropertyChanged(nameof(ShowMileageWarning));
        OnPropertyChanged(nameof(MileageWarning));
    }

    [ObservableProperty]
    private string _mileageText = string.Empty;

    partial void OnMileageTextChanged(string value)
    {
        OnPropertyChanged(nameof(ShowMileageWarning));
        OnPropertyChanged(nameof(MileageWarning));
    }

    public int VehicleLastKm { get; private set; }

    public bool ShowMileageWarning =>
        SelectedVehicle is not null
        && int.TryParse(MileageText, out var km)
        && km > 0
        && km < VehicleLastKm;

    public string MileageWarning => ShowMileageWarning
        ? $"⚠ El kilometraje ingresado ({MileageText} km) es menor al último registrado ({VehicleLastKm} km). Verifica."
        : string.Empty;

    // Step 2: Service type selection
    [ObservableProperty]
    private string _selectedServiceType = string.Empty;

    public List<string> ServiceTypes { get; } = new() { "Servicio A", "Servicio B", "Emergencia" };

    // Step 3: Technician assignment
    [ObservableProperty]
    private ObservableCollection<TechnicianOption> _technicians = new();

    [ObservableProperty]
    private TechnicianOption? _selectedTechnician;

    [ObservableProperty]
    private string _mechanicNote = string.Empty;

    // Step 4: Confirmation
    [ObservableProperty]
    private bool _isSaving;

    [RelayCommand]
    private void NextStep()
    {
        if (CurrentStep < MaxSteps)
            CurrentStep++;
    }

    [RelayCommand]
    private void PreviousStep()
    {
        if (CurrentStep > 1)
            CurrentStep--;
    }

    [RelayCommand]
    private async Task LoadVehicles()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<VehicleListItemDto>>>(ApiRoutes.Vehicles.GetAll);
            if (response?.Success == true && response.Data is not null)
            {
                Vehicles = new ObservableCollection<VehicleOption>(
                    response.Data.Select(v => new VehicleOption
                    {
                        VehicleId = v.Prcoid,
                        LicensePlate = v.LicensePlate,
                        Name = v.VehicleName,
                        CurrentKm = v.CurrentKm,
                    }));
            }
            else
            {
                HasError = true;
                ErrorMessage = "Error al cargar vehículos. Verifica la conexión e intenta nuevamente.";
            }
        });

        await LoadTechniciansAsync();
    }

    private async Task LoadTechniciansAsync()
    {
        try
        {
            var raw = await _apiService.GetAsync<TechnicianListResponse>(ApiRoutes.Workers.GetTechnicians);
            if (raw?.Success == true && raw.Data is not null)
            {
                Technicians = new ObservableCollection<TechnicianOption>(
                    raw.Data.Select(t => new TechnicianOption
                    {
                        Workid = t.Workid,
                        FullName = t.FullName,
                    }));
            }
        }
        catch { }
    }

    [RelayCommand]
    private async Task Save()
    {
        IsSaving = true;
        HasError = false;
        try
        {
            var isEmergency = SelectedServiceType == "Emergencia";
            short matyid = (short)(isEmergency ? 2 : 1);
            short? setyid = isEmergency ? null : (short?)(SelectedServiceType == "Servicio B" ? 2 : 1);
            var assignedWorkid = SelectedTechnician?.Workid ?? _authService.GetWorkid();

            var request = new MaintenanceCreateRequest(
                Prcoid: SelectedVehicle?.VehicleId ?? 0,
                Matyid: matyid,
                Mileage: int.TryParse(MileageText, out var km) ? km : 0,
                AssignedTo: assignedWorkid,
                Setyid: setyid,
                Note: string.IsNullOrWhiteSpace(MechanicNote) ? null : MechanicNote,
                OriginService: "Taller propio"
            );

            var mainid = await _apiService.PostAndUnwrapAsync<int>(ApiRoutes.Maintenances.Create, request);
            if (mainid > 0)
                await Shell.Current.GoToAsync($"///Maintenances/Detail?mainid={mainid}");
            else
                await Shell.Current.GoToAsync("..");
        }
        catch (Exception)
        {
            ErrorMessage = "Error al guardar el mantenimiento. Verifica los datos e intenta nuevamente.";
            HasError = true;
        }
        finally
        {
            IsSaving = false;
        }
    }

    public class VehicleOption
    {
        public int VehicleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public int CurrentKm { get; set; }
        public override string ToString() => $"{Name} - {LicensePlate}";
    }

    public class TechnicianOption
    {
        public int Workid { get; set; }
        public string FullName { get; set; } = string.Empty;
        public override string ToString() => FullName;
    }

    public class TechnicianListResponse
    {
        public bool Success { get; set; }
        public List<TechnicianDto>? Data { get; set; }
    }

    public class TechnicianDto
    {
        public int Workid { get; set; }
        public string FullName { get; set; } = string.Empty;
    }

    private sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

}
