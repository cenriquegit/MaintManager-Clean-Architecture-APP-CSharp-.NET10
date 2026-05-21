using MaintManager.MAUI.ViewModels.Maintenances;

namespace MaintManager.MAUI.Views.Maintenances;

public partial class VehicleHistoryPage : ContentPage
{
    public VehicleHistoryPage(VehicleHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
