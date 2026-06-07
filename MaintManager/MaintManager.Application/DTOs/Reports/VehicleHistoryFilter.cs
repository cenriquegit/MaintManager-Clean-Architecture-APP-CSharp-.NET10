namespace MaintManager.Application.DTOs.Reports;

public sealed record VehicleHistoryFilter(
    int Prcoid,
    DateOnly? DateFrom,
    DateOnly? DateTo
);
