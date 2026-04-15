// MaintManager.Domain/Entities/Existing/Zone.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class Zone
{
    public short Zoneid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
}
