namespace MaintManager.Shared.Models;

public sealed class ExpiringLotDto
{
    public string MaterialName { get; init; } = string.Empty;
    public decimal CurrentQuantity { get; init; }
    public int DaysUntilExpiry { get; init; }
    public decimal AtRiskCost { get; init; }
}
