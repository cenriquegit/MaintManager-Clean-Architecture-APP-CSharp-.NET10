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
