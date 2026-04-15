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
