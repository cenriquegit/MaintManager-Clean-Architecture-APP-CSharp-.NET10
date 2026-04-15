// MaintManager.MAUI/Views/Inventory/InventoryListPage.xaml.cs
using MaintManager.MAUI.ViewModels.Inventory;
namespace MaintManager.MAUI.Views.Inventory;

public partial class InventoryListPage : ContentPage
{
    public InventoryListPage(InventoryListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as InventoryListViewModel)?.LoadCommand.Execute(null);
}