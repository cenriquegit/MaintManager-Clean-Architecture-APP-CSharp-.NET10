using MaintManager.MAUI.ViewModels.Reports;

namespace MaintManager.MAUI.Views.Reports;

public partial class ReportFilterPage : ContentPage
{
    public ReportFilterPage(ReportFilterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
