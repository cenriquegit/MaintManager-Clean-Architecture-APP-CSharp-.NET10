namespace MaintManager.Application.DTOs.Maintenance;

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
