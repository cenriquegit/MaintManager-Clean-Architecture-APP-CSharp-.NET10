using MaintManager.MAUI.ViewModels.Maintenances;

namespace MaintManager.MAUI.Views.Maintenances;

public partial class MaintenanceListPage : ContentPage
{
    public MaintenanceListPage(MaintenanceListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as MaintenanceListViewModel)?.LoadCommand.Execute(null);
}
