// MaintManager.Domain/Entities/Existing/Agency.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class Agency
{
    public short Agenid { get; init; }
    public short Zoneid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
    public Zone? Zone { get; init; }
}
