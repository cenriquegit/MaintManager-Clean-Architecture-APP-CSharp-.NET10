using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaintManager.MAUI.Services;
using MaintManager.Shared.Constants;
using System.Collections.ObjectModel;

namespace MaintManager.MAUI.ViewModels.Maintenances;

public partial class MaintenanceWizardViewModel : BaseViewModel
{
    private readonly ApiService _apiService;

    public MaintenanceWizardViewModel(ApiService apiService)
    {
        _apiService = apiService;
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
    }

    // Step 1: Vehicle selection + mileage
    [ObservableProperty]
    private ObservableCollection<VehicleOption> _vehicles = new();

    [ObservableProperty]
    private VehicleOption? _selectedVehicle;

    [ObservableProperty]
    private string _mileageText = string.Empty;

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

    // Step 6: Next service planning
    [ObservableProperty]
    private int _nextServiceKm;

    [ObservableProperty]
    private DateTime _nextServiceDate = DateTime.Today.AddMonths(3);

    public DateTime NextServiceMinDate => DateTime.Today;

    [ObservableProperty]
    private string _nextServiceNotes = string.Empty;

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

    private sealed class VehicleListRaw
    {
        public int Prcoid { get; init; }
        public string LicensePlate { get; init; } = string.Empty;
        public string VehicleName { get; init; } = string.Empty;
    }

    [RelayCommand]
    private async Task LoadVehicles()
    {
        await ExecuteAsync(async () =>
        {
            var response = await _apiService.GetAsync<ApiResponse<List<VehicleListRaw>>>(ApiRoutes.Vehicles.GetAll);
            if (response?.Success == true && response.Data is not null)
            {
                Vehicles = new ObservableCollection<VehicleOption>(
                    response.Data.Select(v => new VehicleOption
                    {
                        VehicleId = v.Prcoid,
                        LicensePlate = v.LicensePlate,
                        Name = v.VehicleName,
                    }));
            }
            else
            {
                HasError = true;
                ErrorMessage = "Error al cargar vehículos. Verifica la conexión e intenta nuevamente.";
            }
        });
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }

    [RelayCommand]
    private async Task Save()
    {
        IsSaving = true;
        HasError = false;
        try
        {
            var request = new
            {
                VehicleId = SelectedVehicle?.VehicleId,
                Mileage = int.TryParse(MileageText, out var km) ? km : 0,
                ServiceType = SelectedServiceType,
                Diagnosis = new
                {
                    Code = DiagnosisCode,
                    Description = DiagnosisDescription,
                    RecommendedAction = string.Empty,
                    Severity = string.Empty
                },
                NextServiceKm = NextServiceKm,
                NextServiceDate = NextServiceDate.ToString("yyyy-MM-dd"),
                NextServiceNotes = NextServiceNotes,
                Materials = Materials.Select(m => new { m.MaterialId, m.Quantity }),
                Operations = Operations.Select(o => new { OperationId = o.OperationId, IsCompleted = o.IsCompleted })
            };

            await _apiService.PostAsync<object>(ApiRoutes.Maintenances.Create, request);
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
