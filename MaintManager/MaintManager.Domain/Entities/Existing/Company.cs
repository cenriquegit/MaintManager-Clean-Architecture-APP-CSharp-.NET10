// MaintManager.Domain/Entities/Existing/Company.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class Company
{
    public int Compid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Ruc { get; init; } = string.Empty;
    public string? TradeName { get; init; }
    public bool Status { get; init; }
}
