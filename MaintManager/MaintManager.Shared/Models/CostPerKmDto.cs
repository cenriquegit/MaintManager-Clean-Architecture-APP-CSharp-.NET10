namespace MaintManager.Shared.Models;

public sealed class CostPerKmDto
{
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public string VehicleName { get; init; } = string.Empty;
    public int TotalServices { get; init; }
    public decimal TotalMaterialCost { get; init; }
    public int CurrentKm { get; init; }
    public decimal CostPerKm { get; init; }
}
