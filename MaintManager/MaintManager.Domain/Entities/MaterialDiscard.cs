
/// <summary>Registro de merma o descarte de material. Alimenta el BI de costos perdidos.</summary>
namespace MaintManager.Domain.Entities;

public sealed class MaterialDiscard
{
    public int Madiid { get; private set; }
    public int Maloid { get; private set; }
    public decimal DiscardedQuantity { get; private set; }
    public DateTime DiscardDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string? Note { get; private set; }
    public int DiscardedBy { get; private set; }

    // Navegación
    public MaterialLot? Lot { get; private set; }

    private MaterialDiscard() { }

    public static MaterialDiscard Create(int maloid, decimal quantity, string reason,
        int discardedBy, string? note = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a descartar debe ser positiva.", nameof(quantity));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("El motivo del descarte es obligatorio.", nameof(reason));

        return new MaterialDiscard
        {
            Maloid = maloid,
            DiscardedQuantity = quantity,
            Reason = reason,
            Note = note,
            DiscardedBy = discardedBy,
            DiscardDate = DateTime.UtcNow
        };
    }
}


