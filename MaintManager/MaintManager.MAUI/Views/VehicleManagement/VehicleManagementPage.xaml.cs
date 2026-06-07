using MaintManager.MAUI.ViewModels.VehicleManagement;

namespace MaintManager.MAUI.Views.VehicleManagement;

public partial class VehicleManagementPage : ContentPage
{
    private readonly VehicleManagementViewModel _viewModel;

    public VehicleManagementPage(VehicleManagementViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing();
    }

    private void OnEditTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is ManagedVehicleItem item)
            _viewModel.EditVehicleCommand.Execute(item);
    }

    private void OnConfigTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is ManagedVehicleItem item)
            _viewModel.ConfigVehicleCommand.Execute(item);
    }
}
