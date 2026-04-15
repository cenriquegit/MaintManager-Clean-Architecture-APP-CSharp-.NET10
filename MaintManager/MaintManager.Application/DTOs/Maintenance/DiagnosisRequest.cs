namespace MaintManager.Application.DTOs.Maintenance;

public sealed record DiagnosisRequest(
    string GeneralStatus,
    bool VehicleOperative,
    string? Observations,
    string? FutureRecommendations
);

/// <summary>Diagnóstico en la respuesta.</summary>
