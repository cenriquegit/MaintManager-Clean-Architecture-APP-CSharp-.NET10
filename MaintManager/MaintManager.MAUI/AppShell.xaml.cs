namespace MaintManager.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("Maintenances/Detail", typeof(Views.Maintenances.MaintenanceDetailPage));
        Routing.RegisterRoute("Maintenances/Create", typeof(Views.Maintenances.MaintenanceWizardPage));
        Routing.RegisterRoute("Inventory/CreateLot", typeof(Views.Inventory.LotCreatePage));
    }

    private async void OnFlyoutItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Microsoft.Maui.Controls.Border border && border.ClassId is string route)
        {
            // Cierra el flyout antes de navegar
            Shell.Current.FlyoutIsPresented = false;
            await Task.Delay(100); // espera a que se cierre la animación
            await Shell.Current.GoToAsync(route);
        }
    }
}
