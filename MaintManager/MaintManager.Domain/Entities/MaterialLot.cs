
/// <summary>
/// Lote de material. Unidad de ingreso al inventario.
/// FIFO por vencimiento: se consume primero el lote más próximo a vencer.
/// </summary>
namespace MaintManager.Domain.Entities;

public sealed class MaterialLot
{
    public int Maloid { get; private set; }
    public int Mateid { get; private set; }
    public decimal InitialQuantity { get; private set; }
    public decimal CurrentQuantity { get; private set; }

    /// <summary>Precio de compra por unidad. Usado en cálculos de costo real.</summary>
    public decimal UnitCost { get; private set; }

    public DateTime EntryDate { get; private set; }
    public DateOnly? ExpirationDate { get; private set; }
    public int? Provid { get; private set; }
    public string? SupplierLotNumber { get; private set; }
    public string? Note { get; private set; }
    public string LotStatus { get; private set; } = "activo";
    public int CreatedBy { get; private set; }

    // Navegación
    public Material? Material { get; private set; }

    private MaterialLot() { }

    public static MaterialLot Create(int mateid, decimal initialQuantity, decimal unitCost,
        int createdBy, DateOnly? expirationDate = null, int? provid = null,
        string? supplierLotNumber = null, string? note = null)
    {
        if (initialQuantity <= 0)
            throw new ArgumentException("La cantidad inicial debe ser mayor a cero.", nameof(initialQuantity));
        if (unitCost < 0)
            throw new ArgumentException("El costo unitario no puede ser negativo.", nameof(unitCost));

        return new MaterialLot
        {
            Mateid = mateid,
            InitialQuantity = initialQuantity,
            CurrentQuantity = initialQuantity,
            UnitCost = unitCost,
            ExpirationDate = expirationDate,
            Provid = provid,
            SupplierLotNumber = supplierLotNumber,
            Note = note,
            CreatedBy = createdBy,
            EntryDate = DateTime.UtcNow,
            LotStatus = "activo"
        };
    }

    /// <summary>Consume cantidad del lote. Actualiza estado si queda en cero.</summary>
    public void Consume(decimal quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("La cantidad a consumir debe ser positiva.", nameof(quantity));
        if (quantity > CurrentQuantity)
            throw new InvalidOperationException(
                $"Cantidad insuficiente en el lote. Disponible: {CurrentQuantity}. Requerido: {quantity}.");

        CurrentQuantity -= quantity;
        if (CurrentQuantity == 0) LotStatus = "agotado";
    }

    /// <summary>Descarta el lote completo o parte de él.</summary>
    public void Discard(decimal quantity)
    {
        if (quantity <= 0 || quantity > CurrentQuantity)
            throw new ArgumentException("Cantidad de descarte inválida.", nameof(quantity));

        CurrentQuantity -= quantity;
        LotStatus = CurrentQuantity == 0 ? "descartado" : LotStatus;
    }

    public void MarkAsExpired() => LotStatus = "vencido";

    /// <summary>Costo total del stock restante en este lote.</summary>
    public decimal GetRemainingCost() => CurrentQuantity * UnitCost;
}


