namespace MaintManager.Application.DTOs.Maintenance;

public sealed record ComponentResponse(
    int Incoid,
    string ComponentName,
    string? Category,
    DateTime InstallationDate,
    int InstallationKm,
    DateOnly? ExpirationDate,
    bool Active,
    int? UsefulLifeDays
);
