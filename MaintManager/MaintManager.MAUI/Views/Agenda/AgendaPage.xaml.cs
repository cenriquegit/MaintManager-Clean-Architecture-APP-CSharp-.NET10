using MaintManager.MAUI.ViewModels.Agenda;

namespace MaintManager.MAUI.Views.Agenda;

public partial class AgendaPage : ContentPage
{
    private readonly AgendaViewModel _viewModel;

    public AgendaPage(AgendaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Overdue.Count == 0 && _viewModel.Upcoming.Count == 0 &&
            _viewModel.InService.Count == 0 && _viewModel.Ok.Count == 0)
            _viewModel.LoadCommand.Execute(null);
    }
}
