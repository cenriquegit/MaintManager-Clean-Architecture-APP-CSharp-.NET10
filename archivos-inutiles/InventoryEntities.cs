// MaintManager.Domain/Entities/MaterialCategory.cs
namespace MaintManager.Domain.Entities;

/// <summary>Categoría de material: Lubricantes, Filtros, Fluidos, Repuestos, Otros.</summary>
public sealed class MaterialCategory
{
    public short Macaid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;

    public ICollection<Material> Materials { get; init; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/Material.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Material del inventario. stock_total = suma de todos los lotes activos.
/// Se actualiza con cada ingreso de lote y cada consumo.
/// </summary>
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
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/MaterialLot.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Lote de material. Unidad de ingreso al inventario.
/// FIFO por vencimiento: se consume primero el lote más próximo a vencer.
/// </summary>
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/MaterialConsumption.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Registro de consumo de material en un mantenimiento.
/// Si origin=StockPropio y abarca 2 lotes → 2 registros separados.
/// </summary>
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/MaterialDiscard.cs
namespace MaintManager.Domain.Entities;

/// <summary>Registro de merma o descarte de material. Alimenta el BI de costos perdidos.</summary>
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/MaterialRating.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Calificación (1-5 estrellas) de un material durante un mantenimiento.
/// Si rating ≤ 3, la observación es obligatoria.
/// </summary>
public sealed class MaterialRating
{
    public int Matraid { get; private set; }
    public int Mateid { get; private set; }
    public int Mainid { get; private set; }
    public short Rating { get; private set; }
    public string? Observation { get; private set; }
    public int RatedBy { get; private set; }
    public DateTime RatedAt { get; private set; }

    private MaterialRating() { }

    public static MaterialRating Create(int mateid, int mainid, short rating, int ratedBy, string? observation = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("La calificación debe estar entre 1 y 5.", nameof(rating));
        if (rating <= 3 && string.IsNullOrWhiteSpace(observation))
            throw new InvalidOperationException("La observación es obligatoria cuando la calificación es 3 o menor.");

        return new MaterialRating
        {
            Mateid = mateid,
            Mainid = mainid,
            Rating = rating,
            Observation = observation,
            RatedBy = ratedBy,
            RatedAt = DateTime.UtcNow
        };
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/InstalledComponent.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Componente actualmente instalado en un vehículo.
/// Permite rastrear caducidad por tiempo (ej: aceite motor = 12 meses).
/// </summary>
public sealed class InstalledComponent
{
    public int Incoid { get; private set; }
    public int Prcoid { get; private set; }
    public int Acatid { get; private set; }
    public int Mainid { get; private set; }
    public int? Maloid { get; private set; }
    public DateTime InstallationDate { get; private set; }
    public int InstallationKm { get; private set; }

    /// <summary>Calculado: InstallationDate + UsefulLifeDays del catálogo. Null si no caduca.</summary>
    public DateOnly? ExpirationDate { get; private set; }

    public bool Active { get; private set; } = true;

    /// <summary>ID del componente que lo reemplazó (historial de sustituciones).</summary>
    public int? ReplacedByIncoid { get; private set; }

    // Navegación
    public ActionCatalog? ActionCatalog { get; private set; }
    public MaterialLot? Lot { get; private set; }

    private InstalledComponent() { }

    public static InstalledComponent Create(int prcoid, int acatid, int mainid,
        int installationKm, int? maloid = null, int? usefulLifeDays = null)
    {
        if (installationKm < 0)
            throw new ArgumentException("El km de instalación no puede ser negativo.", nameof(installationKm));

        DateOnly? expirationDate = null;
        if (usefulLifeDays.HasValue && usefulLifeDays > 0)
            expirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(usefulLifeDays.Value));

        return new InstalledComponent
        {
            Prcoid = prcoid,
            Acatid = acatid,
            Mainid = mainid,
            Maloid = maloid,
            InstallationDate = DateTime.UtcNow,
            InstallationKm = installationKm,
            ExpirationDate = expirationDate,
            Active = true
        };
    }

    /// <summary>Desactiva el componente al ser reemplazado por uno nuevo.</summary>
    public void Replace(int newComponentId)
    {
        Active = false;
        ReplacedByIncoid = newComponentId;
    }
}
