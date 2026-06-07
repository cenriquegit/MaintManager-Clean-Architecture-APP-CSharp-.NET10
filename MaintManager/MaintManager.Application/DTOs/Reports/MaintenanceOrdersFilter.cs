namespace MaintManager.Application.DTOs.Reports;

public sealed record MaintenanceOrdersFilter(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    int? Prcoid,
    string? Status,
    short? Matyid
);
