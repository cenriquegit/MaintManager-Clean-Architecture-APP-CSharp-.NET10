// MaintManager.MAUI/Views/Reports/ReportsPage.xaml.cs
using MaintManager.MAUI.ViewModels.Reports;
namespace MaintManager.MAUI.Views.Reports;

public partial class ReportsPage : ContentPage
{
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
