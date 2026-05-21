using MaintManager.MAUI.ViewModels.Settings;

namespace MaintManager.MAUI.Views.Settings;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _vm;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _vm = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadConfigCommand.ExecuteAsync(null);
    }
}
