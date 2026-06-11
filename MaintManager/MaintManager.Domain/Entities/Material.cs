
/// <summary>
/// Material del inventario. stock_total = suma de todos los lotes activos.
/// Se actualiza con cada ingreso de lote y cada consumo.
/// </summary>
namespace MaintManager.Domain.Entities;

public sealed class Material
{
    public int Mateid { get; private set; }
    public short Macaid { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = string.Empty;
    public decimal StockTotal { get; private set; }
    public decimal StockMinimum { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public bool Status { get; private set; } = true;

    // Navegación
    public MaterialCategory? Category { get; private set; }
    public ICollection<MaterialLot> Lots { get; private set; } = [];
    public ICollection<MaterialRating> Ratings { get; private set; } = [];

    private Material() { }

    public static Material Create(short macaid, string name, string unitOfMeasure,
        decimal stockMinimum, int createdBy, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del material es obligatorio.", nameof(name));
        if (string.IsNullOrWhiteSpace(unitOfMeasure))
            throw new ArgumentException("La unidad de medida es obligatoria.", nameof(unitOfMeasure));
        if (stockMinimum < 0)
            throw new ArgumentException("El stock mínimo no puede ser negativo.", nameof(stockMinimum));

        return new Material
        {
            Macaid = macaid,
            Name = name,
            UnitOfMeasure = unitOfMeasure,
            StockMinimum = stockMinimum,
            Description = description,
            CreatedBy = createdBy,
            StockTotal = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>Incrementa el stock al ingresar un lote.</summary>
    public void AddStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a agregar debe ser positiva.", nameof(quantity));

        StockTotal += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Decrementa el stock al consumir o descartar material.</summary>
    public void DeductStock(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a descontar debe ser positiva.", nameof(quantity));
        if (quantity > StockTotal)
            throw new InvalidOperationException(
                $"Stock insuficiente. Disponible: {StockTotal} {UnitOfMeasure}. Requerido: {quantity}.");

        StockTotal -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Indica si el stock está por debajo del mínimo configurado.</summary>
    public bool IsBelowMinimum() => StockTotal < StockMinimum;

    public void Update(short macaid, string name, string unitOfMeasure, decimal stockMinimum)
    {
        Macaid = macaid;
        Name = name;
        UnitOfMeasure = unitOfMeasure;
        StockMinimum = stockMinimum;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disable()
    {
        Status = false;
        UpdatedAt = DateTime.UtcNow;
    }
}


