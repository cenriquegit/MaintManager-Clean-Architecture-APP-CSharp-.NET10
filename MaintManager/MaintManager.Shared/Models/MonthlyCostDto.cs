namespace MaintManager.Shared.Models;

public sealed class MonthlyCostDto
{
    public DateTime Month { get; init; }
    public int Prcoid { get; init; }
    public string LicensePlate { get; init; } = string.Empty;
    public int ServicesCount { get; init; }
    public decimal MonthlyCost { get; init; }
}
