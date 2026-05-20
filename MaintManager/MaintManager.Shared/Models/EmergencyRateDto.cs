namespace MaintManager.Shared.Models;

public sealed class EmergencyRateDto
{
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleName { get; init; } = string.Empty;
    public int ScheduledCount { get; init; }
    public int EmergencyCount { get; init; }
    public int TotalCount { get; init; }
    public decimal EmergencyRatePercent { get; init; }
}
