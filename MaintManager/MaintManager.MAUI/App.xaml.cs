using MaintManager.MAUI.Services;

namespace MaintManager.MAUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Created += async (s, e) =>
        {
            var apiService = MauiProgram.Services?.GetService<ApiService>();
            if (apiService is not null)
            {
                var restored = await apiService.TryRestoreSessionAsync();
                if (restored && Shell.Current is not null)
                {
                    await Shell.Current.GoToAsync("//Dashboard");
                }
            }
        };

        return window;
    }
}
