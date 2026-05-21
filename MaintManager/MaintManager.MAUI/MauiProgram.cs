using LiveChartsCore.SkiaSharpView.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;
using MaintManager.MAUI.Services;
using MaintManager.MAUI.ViewModels.Alerts;
using MaintManager.MAUI.ViewModels.Auth;
using MaintManager.MAUI.ViewModels.BiDashboard;
using MaintManager.MAUI.ViewModels.Calendar;
using MaintManager.MAUI.ViewModels.Dashboard;
using MaintManager.MAUI.ViewModels.Inventory;
using MaintManager.MAUI.ViewModels.Maintenances;
using MaintManager.MAUI.ViewModels.Profile;
using MaintManager.MAUI.ViewModels.Reports;
using MaintManager.MAUI.ViewModels.Settings;
using MaintManager.MAUI.Views.Alerts;
using MaintManager.MAUI.Views.Auth;
using MaintManager.MAUI.Views.BiDashboard;
using MaintManager.MAUI.Views.Calendar;
using MaintManager.MAUI.Views.Dashboard;
using MaintManager.MAUI.Views.Inventory;
using MaintManager.MAUI.Views.Maintenances;
using MaintManager.MAUI.Views.Profile;
using MaintManager.MAUI.Views.Reports;
using MaintManager.MAUI.Views.Settings;
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
            .UseSkiaSharp()
            .UseLiveCharts()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<ApiService>();

        builder.Services.AddSingleton<AuthService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<AlertListViewModel>();
        builder.Services.AddTransient<InventoryListViewModel>();
        builder.Services.AddTransient<LotCreateViewModel>();
        builder.Services.AddTransient<CreateMaterialViewModel>();
        builder.Services.AddTransient<LotListViewModel>();
        builder.Services.AddTransient<MaintenanceListViewModel>();
        builder.Services.AddTransient<MaintenanceDetailViewModel>();
        builder.Services.AddTransient<MaintenanceWizardViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CalendarViewModel>();
        builder.Services.AddTransient<BiDashboardViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<VehicleHistoryViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<UserProfileViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<AlertListPage>();
        builder.Services.AddTransient<InventoryListPage>();
        builder.Services.AddTransient<LotCreatePage>();
        builder.Services.AddTransient<CreateMaterialPage>();
        builder.Services.AddTransient<LotListPage>();
        builder.Services.AddTransient<MaintenanceListPage>();
        builder.Services.AddTransient<MaintenanceDetailPage>();
        builder.Services.AddTransient<MaintenanceWizardPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<VehicleHistoryPage>();
        builder.Services.AddTransient<CalendarPage>();
        builder.Services.AddTransient<BiDashboardPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<UserProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        Services = app.Services;
        return app;
    }
}
