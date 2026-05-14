using MaintManager.MAUI.ViewModels.Dashboard;

namespace MaintManager.MAUI.Views.Dashboard;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as HomeViewModel)?.LoadCommand.Execute(null);
}
