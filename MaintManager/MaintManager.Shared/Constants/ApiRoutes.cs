
namespace MaintManager.Shared.Constants;

/// <summary>
/// Rutas de la API. Centraliza las URLs para que MAUI y la API usen las mismas constantes.
/// Versión: v1.
/// </summary>
public static class ApiRoutes
{
    private const string Base = "api/v1";

    public static class Auth
    {
        public const string Login = $"{Base}/auth/login";
    }

    public static class Vehicles
    {
        public const string Base = $"{ApiRoutes.Base}/vehicles";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string GetCurrentKm = $"{Base}/{{id}}/current-km";
        public const string GetSchedule = $"{Base}/{{id}}/schedule";
    }

    public static class Maintenances
    {
        public const string Base = $"{ApiRoutes.Base}/maintenances";
        public const string GetAll = Base;
        public const string GetById = $"{Base}/{{id}}";
        public const string Create = Base;
        public const string CompleteAction = $"{Base}/{{id}}/actions/{{actionId}}/complete";
        public const string SaveDiagnosis = $"{Base}/{{id}}/diagnosis";
        public const string Close = $"{Base}/{{id}}/close";
        public const string GetByVehicle = $"{ApiRoutes.Base}/vehicles/{{vehicleId}}/maintenances";
    }

    public static class Inventory
    {
        public const string Base = $"{ApiRoutes.Base}/inventory";
        public const string GetMaterials = $"{Base}/materials";
        public const string GetMaterialById = $"{Base}/materials/{{id}}";
        public const string CreateMaterial = $"{Base}/materials";
        public const string RegisterLot = $"{Base}/materials/{{id}}/lots";
        public const string DiscardLot = $"{Base}/lots/{{lotId}}/discard";
        public const string RateMaterial = $"{Base}/materials/{{id}}/ratings";
        public const string GetLowStock = $"{Base}/low-stock";
        public const string GetExpiringLots = $"{Base}/expiring-lots";
    }

    public static class Alerts
    {
        public const string Base = $"{ApiRoutes.Base}/alerts";
        public const string GetUnresolved = Base;
        public const string MarkRead = $"{Base}/{{id}}/read";
        public const string Resolve = $"{Base}/{{id}}/resolve";
    }

    public static class Reports
    {
        public const string Base = $"{ApiRoutes.Base}/reports";
        public const string Dashboard = $"{Base}/dashboard";
        public const string CostPerKm = $"{Base}/cost-per-km";
        public const string EmergencyRate = $"{Base}/emergency-rate";
        public const string MonthlyCost = $"{Base}/monthly-cost";
        public const string CalendarCompliance = $"{Base}/calendar-compliance";
        public const string ExportMaintenancePdf = $"{Base}/maintenances/{{id}}/pdf";
        public const string ExportCostExcel = $"{Base}/cost-excel";
    }
}

// ─────────────────────────────────────────────────────────────────────────────

