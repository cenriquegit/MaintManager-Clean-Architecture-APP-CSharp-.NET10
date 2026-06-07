using MaintManager.MAUI.ViewModels.VehicleConfig;
using ActionOption = MaintManager.MAUI.ViewModels.VehicleConfig.VehicleConfigViewModel.ActionOption;
using MaterialOptionItem = MaintManager.MAUI.ViewModels.VehicleConfig.VehicleConfigViewModel.MaterialOptionItem;

namespace MaintManager.MAUI.Views.VehicleConfig;

public partial class VehicleConfigPage : ContentPage, IQueryAttributable
{
    private readonly VehicleConfigViewModel _viewModel;
    private bool _initialized;

    public VehicleConfigPage(VehicleConfigViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.ApplyQueryAttributes(query);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_initialized)
        {
            _initialized = true;
            _viewModel.LoadVehiclesCommand.Execute(null);
        }
        else if (_viewModel.SelectedVehicle is not null)
        {
            _viewModel.LoadConfigCommand.Execute(null);
        }
    }

    private void OnRemoveActionTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is ActionOption item)
            _viewModel.RemoveActionCommand.Execute(item);
    }

    private void OnRemoveMaterialTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is MaterialOptionItem item)
            _viewModel.RemoveMaterialCommand.Execute(item);
    }

    private void OnRemoveComponentTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is ActionOption item)
            _viewModel.RemoveComponentCommand.Execute(item);
    }
}
