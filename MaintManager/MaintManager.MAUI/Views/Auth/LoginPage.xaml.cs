using MaintManager.MAUI.Services;
using MaintManager.MAUI.ViewModels.Auth;

namespace MaintManager.MAUI.Views.Auth;

public partial class LoginPage : ContentPage
{
    private bool _sessionChecked;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

        if (_sessionChecked) return;
        _sessionChecked = true;

        try
        {
            var apiService = IPlatformApplication.Current?.Services.GetService<ApiService>();
            if (apiService is not null && await apiService.TryRestoreSessionAsync())
            {
                await Shell.Current.GoToAsync("///Dashboard");
            }
        }
        catch { }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Shell.SetFlyoutBehavior(this, FlyoutBehavior.Flyout);
    }
}
