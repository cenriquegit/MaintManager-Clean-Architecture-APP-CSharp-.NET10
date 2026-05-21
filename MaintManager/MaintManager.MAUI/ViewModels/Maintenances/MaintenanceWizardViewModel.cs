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

    public const int MaxSteps = 7;

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
        {
            _ = LoadVehicleCurrentKmAsync(value);
        }
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

    // Step 3: Components and materials
    [ObservableProperty]
    private ObservableCollection<MaterialLine> _materials = new();

    // Step 4: Operations checklist
    [ObservableProperty]
    private ObservableCollection<OperationItem> _operations = new();

    // Step 5: Diagnosis
    [ObservableProperty]
    private string _diagnosisDescription = string.Empty;

    [ObservableProperty]
    private string _diagnosisCode = string.Empty;

    // Step 7: Confirmation
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

    private sealed class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    private sealed class ApiCreateResponse
    {
        public bool Success { get; set; }
        public int Data { get; set; }
        public string? Message { get; set; }
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

        await LoadMaterialsAsync();
        LoadDefaultOperations();
    }

    private async Task LoadMaterialsAsync()
    {
        try
        {
            var raw = await _apiService.GetAsync<ApiResponse<List<MaterialItemDto>>>(ApiRoutes.Inventory.GetMaterials);
            if (raw?.Success == true && raw.Data is not null)
            {
                Materials = new ObservableCollection<MaterialLine>(
                    raw.Data.Select(m => new MaterialLine
                    {
                        MaterialId = m.Mateid,
                        Name = m.Name,
                        UnitOfMeasure = m.UnitOfMeasure,
                    }));
            }
        }
        catch
        {
        }
    }

    private void LoadDefaultOperations()
    {
        Operations = new ObservableCollection<OperationItem>
        {
            new() { OperationId = 1, Name = "Cambio de aceite y filtro" },
            new() { OperationId = 2, Name = "Revisión de frenos" },
            new() { OperationId = 3, Name = "Revisión de neumáticos" },
            new() { OperationId = 4, Name = "Revisión de sistema eléctrico" },
            new() { OperationId = 5, Name = "Lubricación general" },
            new() { OperationId = 6, Name = "Revisión de suspensión y dirección" },
        };
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

            var request = new MaintenanceCreateRequest(
                Prcoid: SelectedVehicle?.VehicleId ?? 0,
                Matyid: matyid,
                Mileage: int.TryParse(MileageText, out var km) ? km : 0,
                AssignedTo: _authService.GetWorkid(),
                Setyid: setyid,
                Note: string.IsNullOrWhiteSpace(DiagnosisDescription) ? null : DiagnosisDescription,
                OriginService: "Taller propio"
            );

            var response = await _apiService.PostAsync<ApiCreateResponse>(ApiRoutes.Maintenances.Create, request);
            var mainid = response?.Data ?? 0;
            if (mainid > 0)
                await Shell.Current.GoToAsync($"Maintenances/Detail?mainid={mainid}");
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

    public class MaterialLine
    {
        public int MaterialId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
    }

    public class OperationItem
    {
        public int OperationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
