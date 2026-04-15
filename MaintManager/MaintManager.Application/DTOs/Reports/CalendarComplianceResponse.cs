namespace MaintManager.Application.DTOs.Reports;

public sealed record CalendarComplianceResponse(
    int Prcoid,
    string LicensePlate,
    string VehicleName,
    int Mainid,
    DateTime MaintenanceDate,
    int ServiceKm,
    int ScheduledKm,
    int KmDeviation,
    string ComplianceStatus
);

/// <summary>Alerta activa del sistema.</summary>
