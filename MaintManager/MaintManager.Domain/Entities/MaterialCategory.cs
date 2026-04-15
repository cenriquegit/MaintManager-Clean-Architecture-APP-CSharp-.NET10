
/// <summary>Categoría de material: Lubricantes, Filtros, Fluidos, Repuestos, Otros.</summary>
namespace MaintManager.Domain.Entities;

public sealed class MaterialCategory
{
    public short Macaid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;

    public ICollection<Material> Materials { get; init; } = [];
}


