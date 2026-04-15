namespace MaintManager.Application.DTOs.Maintenance;

public sealed record DiagnosisResponse(
    string GeneralStatus,
    bool VehicleOperative,
    string? Observations,
    string? FutureRecommendations,
    DateTime CreatedAt
);

/// <summary>Cerrar una orden de mantenimiento.</summary>
