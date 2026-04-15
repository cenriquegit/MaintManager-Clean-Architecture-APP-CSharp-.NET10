// MaintManager.Domain/Entities/Existing/Product.cs
namespace MaintManager.Domain.Entities.Existing;

/// <summary>Producto base. Mapea: public.product.</summary>
public sealed class Product
{
    public int Prodid { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Status { get; init; }
}
