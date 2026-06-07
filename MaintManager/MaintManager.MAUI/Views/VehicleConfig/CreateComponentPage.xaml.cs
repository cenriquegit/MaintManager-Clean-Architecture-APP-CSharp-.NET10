using MaintManager.MAUI.ViewModels.VehicleConfig;

namespace MaintManager.MAUI.Views.VehicleConfig;

public partial class CreateComponentPage : ContentPage
{
    public CreateComponentPage(CreateComponentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
