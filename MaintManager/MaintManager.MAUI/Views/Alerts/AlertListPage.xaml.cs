using MaintManager.MAUI.ViewModels.Alerts;

namespace MaintManager.MAUI.Views.Alerts;

public partial class AlertListPage : ContentPage
{
    private readonly AlertListViewModel _viewModel;

    public AlertListPage(AlertListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing() =>
        _viewModel.LoadCommand.Execute(null);
}
