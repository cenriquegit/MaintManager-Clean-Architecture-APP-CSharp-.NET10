
/// <summary>
/// Registro de consumo de material en un mantenimiento.
/// Si origin=StockPropio y abarca 2 lotes → 2 registros separados.
/// </summary>
namespace MaintManager.Domain.Entities;

public sealed class MaterialConsumption
{
    public int Macoid { get; private set; }
    public int Mainid { get; private set; }
    public int Mateid { get; private set; }
    public int? Maloid { get; private set; }
    public decimal Quantity { get; private set; }
    public string Origin { get; private set; } = "Stock propio";
    public DateTime ConsumedAt { get; private set; }

    // Navegación
    public MaterialLot? Lot { get; private set; }

    private MaterialConsumption() { }

    public static MaterialConsumption Create(int mainid, int mateid, decimal quantity,
        string origin, int? maloid = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad consumida debe ser positiva.", nameof(quantity));

        return new MaterialConsumption
        {
            Mainid = mainid,
            Mateid = mateid,
            Quantity = quantity,
            Origin = origin,
            Maloid = maloid,
            ConsumedAt = DateTime.UtcNow
        };
    }
}


