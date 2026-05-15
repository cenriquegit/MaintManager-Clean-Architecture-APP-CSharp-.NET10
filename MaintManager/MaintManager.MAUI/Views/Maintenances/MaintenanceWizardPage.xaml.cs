using MaintManager.MAUI.ViewModels.Maintenances;

namespace MaintManager.MAUI.Views.Maintenances;

public partial class MaintenanceWizardPage : ContentPage
{
    private readonly MaintenanceWizardViewModel _vm;
    private bool _initialized;

    public MaintenanceWizardPage(MaintenanceWizardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized) return;
        _initialized = true;

        try
        {
            if (_vm.Vehicles.Count == 0)
                await _vm.LoadVehiclesCommand.ExecuteAsync(null);
        }
        catch
        {
            // Evita crash en primera carga
        }
    }
}
