using System.Windows.Input;

namespace MaintManager.MAUI;

public partial class AppShell : Shell
{
    public static ICommand? NavigateCommand { get; private set; }

    public AppShell()
    {
        InitializeComponent();
        NavigateCommand = new Command<string>(async (route) =>
        {
            if (!string.IsNullOrWhiteSpace(route))
                await Shell.Current.GoToAsync(route);
        });

        Routing.RegisterRoute("Maintenances/Detail", typeof(Views.Maintenances.MaintenanceDetailPage));
        Routing.RegisterRoute("Maintenances/Create", typeof(Views.Maintenances.MaintenanceWizardPage));
        Routing.RegisterRoute("Inventory/CreateLot", typeof(Views.Inventory.LotCreatePage));
    }

    // Exponemos un comando estático para usarlo desde el FlyoutContentTemplate
    public static ICommand FlyoutNavigateCommand => NavigateCommand ??= new Command<string>(async (route) =>
    {
        if (!string.IsNullOrWhiteSpace(route))
            await Shell.Current.GoToAsync(route);
    });
}
