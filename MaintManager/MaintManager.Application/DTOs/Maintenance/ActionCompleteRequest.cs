namespace MaintManager.Application.DTOs.Maintenance;

public sealed record ActionCompleteRequest(
    char ActionCode,
    string? ProductUsed,
    string? QuantityUsed,
    string? OriginProduct,
    string? Observation,
    int? Maloid
);

/// <summary>Detalle de acción en la respuesta.</summary>
