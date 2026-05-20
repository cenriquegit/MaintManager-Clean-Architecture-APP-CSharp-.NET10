namespace MaintManager.Shared.Models;

public sealed record MaintenanceCreateRequest(
    int Prcoid,
    short Matyid,
    int Mileage,
    int AssignedTo,
    short? Setyid,
    string? Note,
    string OriginService = "Taller propio"
);
