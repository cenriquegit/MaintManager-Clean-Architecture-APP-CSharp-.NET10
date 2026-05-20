namespace MaintManager.Shared.Models;

public sealed class ComplianceDto
{
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleName { get; init; } = string.Empty;
    public int KmDeviation { get; init; }
    public string ComplianceStatus { get; init; } = string.Empty;
}
