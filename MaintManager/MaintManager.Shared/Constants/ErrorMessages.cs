
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

