// MaintManager.MAUI/Views/BiDashboard/BiDashboardPage.xaml.cs
using MaintManager.MAUI.ViewModels.BiDashboard;
namespace MaintManager.MAUI.Views.BiDashboard;

public partial class BiDashboardPage : ContentPage
{
    public BiDashboardPage(BiDashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as BiDashboardViewModel)?.LoadCommand.Execute(null);
}
