namespace MaintManager.Shared.Models;

public sealed record InstallComponentRequest(
    int ActionCatalogId,
    int? LotId,
    int? UsefulLifeDays
);
