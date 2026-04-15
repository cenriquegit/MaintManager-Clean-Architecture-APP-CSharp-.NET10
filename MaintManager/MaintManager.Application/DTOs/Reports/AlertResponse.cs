namespace MaintManager.Application.DTOs.Reports;

public sealed record AlertResponse(
    int Alloid,
    string AlertType,
    string Message,
    DateTime AlertDate,
    string? LicensePlate,
    string? MaterialName,
    bool IsRead,
    bool IsResolved
);
