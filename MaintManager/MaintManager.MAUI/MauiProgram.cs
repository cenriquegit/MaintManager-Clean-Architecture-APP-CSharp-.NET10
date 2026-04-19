using CommunityToolkit.Maui;
using MaintManager.MAUI.Services;
using MaintManager.MAUI.ViewModels.Alerts;
using MaintManager.MAUI.ViewModels.Auth;
using MaintManager.MAUI.ViewModels.Inventory;
using MaintManager.MAUI.Views.Alerts;
using MaintManager.MAUI.Views.Auth;
using MaintManager.MAUI.Views.Inventory;
using Microsoft.Extensions.Logging;

namespace MaintManager.MAUI;

public static class MauiProgram
{
    public static IServiceProvider? Services { get; private set; }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // 🔥 Registrar HttpClient para ApiService
        builder.Services.AddHttpClient<ApiService>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5056");
        });

        builder.Services.AddSingleton<AuthService>();

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<AlertListViewModel>();
        builder.Services.AddTransient<InventoryListViewModel>();
        builder.Services.AddTransient<LotCreateViewModel>();

        // Páginas
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AlertListPage>();
        builder.Services.AddTransient<InventoryListPage>();
        builder.Services.AddTransient<LotCreatePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        Services = app.Services;
        return app;
    }
}