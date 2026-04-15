// MaintManager.Domain/Entities/Existing/Worker.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class Worker
{
    public int Workid { get; init; }
    public int Persid { get; init; }
    public short Jobid { get; init; }
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? Email { get; init; }
    public bool Status { get; init; }
    public bool Locked { get; init; }
    public Person? Person { get; init; }
}
