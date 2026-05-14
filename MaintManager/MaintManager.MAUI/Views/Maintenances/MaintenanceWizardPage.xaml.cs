using MaintManager.MAUI.ViewModels.Maintenances;

namespace MaintManager.MAUI.Views.Maintenances;

public partial class MaintenanceWizardPage : ContentPage
{
    public MaintenanceWizardPage(MaintenanceWizardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as MaintenanceWizardViewModel;
        if (vm?.Vehicles.Count == 0)
            vm.LoadVehiclesCommand.Execute(null);
    }
}
