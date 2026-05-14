using MaintManager.MAUI.ViewModels.Maintenances;

namespace MaintManager.MAUI.Views.Maintenances;

public partial class MaintenanceDetailPage : ContentPage
{
    public MaintenanceDetailPage(MaintenanceDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var vm = BindingContext as MaintenanceDetailViewModel;
        if (vm?.IsLoading == false && vm?.MaintenanceDetail == null && !vm.HasError)
            vm.LoadCommand.Execute(null);
    }
}
