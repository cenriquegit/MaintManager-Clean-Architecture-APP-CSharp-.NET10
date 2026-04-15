// MaintManager.Domain/Entities/Existing/Residence.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class Residence
{
    public int Resiid { get; init; }
    public int? Persid { get; init; }
    public int? Compid { get; init; }
    public int Distid { get; init; }
    public string? Address { get; init; }
    public bool Status { get; init; }
}
