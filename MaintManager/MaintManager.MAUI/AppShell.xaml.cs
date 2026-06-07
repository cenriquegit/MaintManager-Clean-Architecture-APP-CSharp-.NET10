using MaintManager.MAUI.Services;

namespace MaintManager.MAUI;

public partial class AppShell : Shell
{
    private AuthService? _authService;

    public AppShell()
    {
        InitializeComponent();
        _authService = IPlatformApplication.Current?.Services.GetRequiredService<AuthService>();

        Routing.RegisterRoute("Maintenances/Detail", typeof(Views.Maintenances.MaintenanceDetailPage));
        Routing.RegisterRoute("Maintenances/Create", typeof(Views.Maintenances.MaintenanceWizardPage));
        Routing.RegisterRoute("Inventory/CreateMaterial", typeof(Views.Inventory.CreateMaterialPage));
        Routing.RegisterRoute("Inventory/CreateLot", typeof(Views.Inventory.LotCreatePage));
        Routing.RegisterRoute("Inventory/LotList", typeof(Views.Inventory.LotListPage));
        Routing.RegisterRoute("Maintenances/VehicleHistory", typeof(Views.Maintenances.VehicleHistoryPage));
        Routing.RegisterRoute("Reports/Filter", typeof(Views.Reports.ReportFilterPage));
        Routing.RegisterRoute("Vehicles/CreateVehicle", typeof(Views.VehicleManagement.CreateVehiclePage));
        Routing.RegisterRoute("ConfigVehicle/CreateAction", typeof(Views.VehicleConfig.CreateActionPage));
        Routing.RegisterRoute("ConfigVehicle/CreateComponent", typeof(Views.VehicleConfig.CreateComponentPage));
        Routing.RegisterRoute("ConfigVehicle/CreateMaterial", typeof(Views.Inventory.CreateMaterialPage));
    }

    private async void OnFlyoutItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Border border && border.ClassId is string route)
        {
            var isAdmin = _authService?.IsAdmin() ?? false;
            if (!isAdmin && (route == "//BiDashboard" || route == "//Settings" || route == "//Vehicles"))
            {
                Shell.Current.FlyoutIsPresented = false;
                await Task.Delay(100);
                await DisplayAlert("Acceso denegado",
                    "Solo el Jefe de Mantenimiento puede acceder a esta sección.",
                    "Entendido");
                return;
            }

            Shell.Current.FlyoutIsPresented = false;
            await Task.Delay(100);
            await Shell.Current.GoToAsync(route);
        }
    }
}
