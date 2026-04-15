// MaintManager.Domain/Entities/Existing/Person.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>Persona natural. Mapea: public.person.</summary>
public sealed class Person
{
    public int Persid { get; init; }
    public string Fln { get; init; } = string.Empty;
    public string? Mln { get; init; }
    public string Name { get; init; } = string.Empty;
}
