namespace MaintManager.Shared.Models;

public sealed class MaintenanceListItemDto
{
    public int Mainid { get; set; }
    public string? LicensePlate { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string MaintenanceType { get; set; } = string.Empty;
    public string? ServiceType { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public int Mileage { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}