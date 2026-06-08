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
        _viewModel.LoadCommand.Execute(null);
    }

    private void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bo && bo.BindingContext is AgendaViewModel.AgendaItem item)
            _viewModel.OpenItemCommand.Execute(item);
    }
}
