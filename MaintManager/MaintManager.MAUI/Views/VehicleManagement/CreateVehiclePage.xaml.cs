using MaintManager.MAUI.ViewModels.VehicleManagement;

namespace MaintManager.MAUI.Views.VehicleManagement;

public partial class CreateVehiclePage : ContentPage, IQueryAttributable
{
    private readonly CreateVehicleViewModel _viewModel;

    public CreateVehiclePage(CreateVehicleViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _viewModel.ApplyQueryAttributes(query);
    }
}
