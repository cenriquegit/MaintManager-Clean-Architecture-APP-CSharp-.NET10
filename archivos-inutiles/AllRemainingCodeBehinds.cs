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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.MAUI/Views/Inventory/LotCreatePage.xaml.cs
namespace MaintManager.MAUI.Views.Inventory;
using MaintManager.MAUI.ViewModels.Inventory;

public partial class LotCreatePage : ContentPage
{
    public LotCreatePage(LotCreateViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as LotCreateViewModel)?.LoadMaterialsCommand.Execute(null);

    private async void OnCancelClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.MAUI/Views/Alerts/AlertListPage.xaml.cs
namespace MaintManager.MAUI.Views.Alerts;
using MaintManager.MAUI.ViewModels.Alerts;

public partial class AlertListPage : ContentPage
{
    public AlertListPage(AlertListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as AlertListViewModel)?.LoadCommand.Execute(null);
}
