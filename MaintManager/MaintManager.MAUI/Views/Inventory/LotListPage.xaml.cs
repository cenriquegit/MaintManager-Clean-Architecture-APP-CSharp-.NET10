using MaintManager.MAUI.ViewModels.Inventory;

namespace MaintManager.MAUI.Views.Inventory;

public partial class LotListPage : ContentPage
{
    public LotListPage(LotListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
