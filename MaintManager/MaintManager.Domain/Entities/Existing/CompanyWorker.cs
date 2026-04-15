// MaintManager.Domain/Entities/Existing/CompanyWorker.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>
public sealed class CompanyWorker
{
    public int Cowoid { get; init; }
    public int Compid { get; init; }
    public int Persid { get; init; }
    public bool Status { get; init; }
    public Company? Company { get; init; }
    public Person? Person { get; init; }
}
