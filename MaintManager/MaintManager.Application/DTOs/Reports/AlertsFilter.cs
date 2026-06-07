namespace MaintManager.Application.DTOs.Reports;

public sealed record AlertsFilter(
    DateOnly? DateFrom,
    DateOnly? DateTo,
    bool? Resolved,
    string? AlertType
);
