namespace MaintManager.Application.DTOs.Maintenance;

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
