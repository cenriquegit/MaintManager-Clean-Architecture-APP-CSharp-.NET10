using MaintManager.MAUI.ViewModels.Calendar;

namespace MaintManager.MAUI.Views.Calendar;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing() =>
        (BindingContext as CalendarViewModel)?.LoadCommand.Execute(null);
}
