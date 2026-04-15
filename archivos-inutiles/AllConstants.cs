// MaintManager.Shared/Constants/RoleNames.cs
namespace MaintManager.Shared.Constants;

/// <summary>Nombres de roles JWT del sistema. Deben coincidir con los claims del token.</summary>
public static class RoleNames
{
    public const string Admin = "Admin";
    public const string Tecnico = "Tecnico";
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Shared/Constants/ApiRoutes.cs
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

// MaintManager.Shared/Constants/ErrorMessages.cs
namespace MaintManager.Shared.Constants;

/// <summary>Mensajes de error legibles para el usuario final (jefe y técnicos).</summary>
public static class ErrorMessages
{
    public static class Auth
    {
        public const string InvalidCredentials = "Usuario o contraseña incorrectos.";
        public const string UserLocked = "Tu cuenta está bloqueada. Contacta al administrador.";
        public const string Unauthorized = "No tienes permiso para realizar esta acción.";
        public const string SessionExpired = "Tu sesión ha expirado. Por favor vuelve a iniciar sesión.";
    }

    public static class Vehicle
    {
        public const string NotFound = "El vehículo no fue encontrado.";
        public const string Inactive = "El vehículo está inactivo y no puede recibir mantenimientos.";
    }

    public static class Maintenance
    {
        public const string NotFound = "La orden de mantenimiento no fue encontrada.";
        public const string AlreadyClosed = "Esta orden ya fue cerrada y no puede modificarse.";
        public const string MileageBelowLast = "El kilometraje no puede ser menor al del último mantenimiento registrado.";
        public const string DiagnosisRequired = "Debes completar el diagnóstico antes de cerrar la orden.";
    }

    public static class Inventory
    {
        public const string MaterialNotFound = "El material no fue encontrado.";
        public const string LotNotFound = "El lote no fue encontrado.";
        public const string InsufficientStock = "Stock insuficiente para realizar la operación.";
        public const string LotExpired = "El lote seleccionado está vencido.";
    }

    public static class General
    {
        public const string UnexpectedError = "Ocurrió un error inesperado. Por favor intente nuevamente.";
        public const string ValidationError = "Los datos ingresados no son válidos.";
        public const string NotFound = "El recurso solicitado no fue encontrado.";
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Shared/Constants/AlertTypes.cs
namespace MaintManager.Shared.Constants;

/// <summary>Tipos de alerta del sistema. Deben coincidir con alert_config.alert_type en BD.</summary>
public static class AlertTypes
{
    public const string MantenimientoProximoKm = "MANTENIMIENTO_PROXIMO_KM";
    public const string ComponentePorCaducar = "COMPONENTE_POR_CADUCAR";
    public const string LotePorVencer = "LOTE_POR_VENCER";
    public const string StockBajo = "STOCK_BAJO";
}
