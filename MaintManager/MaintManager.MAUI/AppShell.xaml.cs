using MaintManager.MAUI.Views.Inventory;
using MaintManager.MAUI.Views.Maintenances;

namespace MaintManager.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("Inventory/LotCreate", typeof(LotCreatePage));
        Routing.RegisterRoute("Maintenances/Detail", typeof(MaintenanceDetailPage));
        Routing.RegisterRoute("Maintenances/Create", typeof(MaintenanceWizardPage));
    }
}
