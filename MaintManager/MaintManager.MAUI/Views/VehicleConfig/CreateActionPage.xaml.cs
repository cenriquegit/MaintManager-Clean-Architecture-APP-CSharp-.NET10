using MaintManager.MAUI.ViewModels.VehicleConfig;

namespace MaintManager.MAUI.Views.VehicleConfig;

public partial class CreateActionPage : ContentPage
{
    public CreateActionPage(CreateActionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
