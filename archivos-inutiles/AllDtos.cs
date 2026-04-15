// =====================================================================
// MaintManager.Application/DTOs — Todos los DTOs del sistema
// =====================================================================

// ── Auth ──────────────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Auth;

/// <summary>Datos de inicio de sesión.</summary>
public sealed record LoginRequest(string Username, string Password);

/// <summary>Respuesta con token JWT y datos del usuario autenticado.</summary>
public sealed record LoginResponse(
    string Token,
    string Username,
    string FullName,
    string Role,
    DateTime ExpiresAt
);

// ── Common ────────────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Common;

/// <summary>
/// Wrapper de respuesta estandarizada para todos los endpoints.
/// Success=true + Data en casos exitosos.
/// Success=false + Message en casos de error.
/// </summary>
public sealed class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string message, IReadOnlyList<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}

/// <summary>Respuesta paginada para endpoints de listado.</summary>
public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>Parámetros de paginación.</summary>
public sealed record PagedRequest(int Page = 1, int PageSize = 20);

// ── Vehicle ───────────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Vehicle;

/// <summary>Datos del vehículo para listados y detalle.</summary>
public sealed record VehicleResponse(
    int Prcoid,
    string? LicensePlate,
    string? VinNumber,
    string VehicleName,
    string? VehicleType,
    string? FuelType,
    short? Year,
    string? Color,
    string? Category,
    int CurrentKm,
    int? NextMaintenanceKm,
    int? KmUntilNextService,
    bool IsMaintenanceDueSoon,
    bool IsActive
);

/// <summary>Datos simplificados para la lista de vehículos.</summary>
public sealed record VehicleListItem(
    int Prcoid,
    string? LicensePlate,
    string VehicleName,
    int CurrentKm,
    int? NextMaintenanceKm,
    bool IsMaintenanceDueSoon
);

// ── Maintenance ───────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Maintenance;

/// <summary>Crear nueva orden de mantenimiento.</summary>
public sealed record MaintenanceCreateRequest(
    int Prcoid,
    short Matyid,
    int Mileage,
    int AssignedTo,
    short? Setyid,
    string? Note,
    string OriginService = "Taller propio"
);

/// <summary>Detalle completo de una orden de mantenimiento.</summary>
public sealed record MaintenanceResponse(
    int Mainid,
    int Prcoid,
    string? LicensePlate,
    string VehicleName,
    string MaintenanceType,
    string? ServiceType,
    string? OrderNumber,
    DateTime MaintenanceDate,
    int Mileage,
    int? KmSinceLast,
    string? OilBrand,
    string? OilViscositySae,
    string? ClimateSeason,
    bool ShowOilInNextMaintenance,
    string OriginService,
    bool? IsEmergencyComplete,
    string AssignedToName,
    string RegisteredByName,
    string? Note,
    string Status,
    IReadOnlyList<ActionDetailResponse> ActionDetails,
    DiagnosisResponse? Diagnosis
);

/// <summary>Elemento de lista de mantenimientos.</summary>
public sealed record MaintenanceListItem(
    int Mainid,
    string? LicensePlate,
    string VehicleName,
    string MaintenanceType,
    string? ServiceType,
    DateTime MaintenanceDate,
    int Mileage,
    string AssignedToName,
    string Status
);

/// <summary>Completar una acción del checklist.</summary>
public sealed record ActionCompleteRequest(
    char ActionCode,
    string? ProductUsed,
    string? QuantityUsed,
    string? OriginProduct,
    string? Observation,
    int? Maloid
);

/// <summary>Detalle de acción en la respuesta.</summary>
public sealed record ActionDetailResponse(
    int Madeid,
    int Acatid,
    string ActionName,
    string ActionCategory,
    bool Completed,
    char? ActionPerformed,
    string? ProductUsed,
    string? QuantityUsed,
    string? OriginProduct,
    string? Observation
);

/// <summary>Guardar diagnóstico final del mecánico.</summary>
public sealed record DiagnosisRequest(
    string GeneralStatus,
    bool VehicleOperative,
    string? Observations,
    string? FutureRecommendations
);

/// <summary>Diagnóstico en la respuesta.</summary>
public sealed record DiagnosisResponse(
    string GeneralStatus,
    bool VehicleOperative,
    string? Observations,
    string? FutureRecommendations,
    DateTime CreatedAt
);

/// <summary>Cerrar una orden de mantenimiento.</summary>
public sealed record MaintenanceCloseRequest(bool? IsEmergencyComplete);

// ── Inventory ─────────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Inventory;

/// <summary>Crear un nuevo material en inventario.</summary>
public sealed record MaterialCreateRequest(
    short Macaid,
    string Name,
    string UnitOfMeasure,
    decimal StockMinimum,
    string? Description
);

/// <summary>Material en respuesta.</summary>
public sealed record MaterialResponse(
    int Mateid,
    string Category,
    string Name,
    string UnitOfMeasure,
    decimal StockTotal,
    decimal StockMinimum,
    bool IsBelowMinimum,
    string? Description,
    IReadOnlyList<LotResponse> ActiveLots
);

/// <summary>Elemento simplificado para listados.</summary>
public sealed record MaterialListItem(
    int Mateid,
    string Category,
    string Name,
    string UnitOfMeasure,
    decimal StockTotal,
    decimal StockMinimum,
    bool IsBelowMinimum
);

/// <summary>Ingresar un nuevo lote de material.</summary>
public sealed record LotCreateRequest(
    int Mateid,
    decimal Quantity,
    decimal UnitCost,
    DateOnly? ExpirationDate,
    int? Provid,
    string? SupplierLotNumber,
    string? Note
);

/// <summary>Lote en respuesta.</summary>
public sealed record LotResponse(
    int Maloid,
    decimal InitialQuantity,
    decimal CurrentQuantity,
    decimal UnitCost,
    DateTime EntryDate,
    DateOnly? ExpirationDate,
    int? DaysUntilExpiry,
    string LotStatus
);

/// <summary>Descartar cantidad de un lote.</summary>
public sealed record LotDiscardRequest(
    decimal Quantity,
    string Reason,
    string? Note
);

/// <summary>Calificar un material.</summary>
public sealed record MaterialRatingRequest(
    int Mateid,
    int Mainid,
    short Rating,
    string? Observation
);

// ── Reports / BI ──────────────────────────────────────────────────────

namespace MaintManager.Application.DTOs.Reports;

/// <summary>KPIs del panel principal del dashboard.</summary>
public sealed record DashboardSummaryResponse(
    int TotalVehicles,
    int ServicesThisMonth,
    decimal GlobalEmergencyRatePercent,
    int LowStockMaterials,
    int UnresolvedAlerts,
    int ExpiringLots,
    decimal FleetAvgCostPerKm
);

/// <summary>Costo por km por vehículo.</summary>
public sealed record CostPerKmResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int TotalServices,
    decimal TotalMaterialCost,
    int CurrentKm,
    decimal CostPerKm
);

/// <summary>Tasa de emergencias por vehículo.</summary>
public sealed record EmergencyRateResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int ScheduledCount,
    int EmergencyCount,
    int TotalCount,
    decimal EmergencyRatePercent
);

/// <summary>Costo mensual por vehículo.</summary>
public sealed record MonthlyCostResponse(
    DateTime Month,
    int Prcoid,
    string LicensePlate,
    int ServicesCount,
    decimal MonthlyCost
);

/// <summary>Lote próximo a vencer.</summary>
public sealed record ExpiringLotResponse(
    int Maloid,
    int Mateid,
    string MaterialName,
    string Category,
    decimal CurrentQuantity,
    string UnitOfMeasure,
    DateOnly ExpirationDate,
    int DaysUntilExpiry,
    decimal UnitCost,
    decimal AtRiskCost,
    string LotStatus
);

/// <summary>Cumplimiento del calendario de mantenimiento.</summary>
public sealed record CalendarComplianceResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int Mainid,
    DateTime MaintenanceDate,
    int ServiceKm,
    int ScheduledKm,
    int KmDeviation,
    string ComplianceStatus
);

/// <summary>Alerta activa del sistema.</summary>
public sealed record AlertResponse(
    int Alloid,
    string AlertType,
    string Message,
    DateTime AlertDate,
    string? LicensePlate,
    string? MaterialName,
    bool IsRead,
    bool IsResolved
);
