namespace MaintManager.Domain.Entities;

public sealed class ActionCatalog
{
    public int Acatid { get; init; }
    public short Altoid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Category { get; init; }
    public string? RecommendedProduct { get; init; }
    public string? RecommendedQuantity { get; init; }
    public string? UnitOfMeasure { get; init; }
    public int? UsefulLifeKm { get; init; }

    /// <summary>
    /// true = el componente caduca por tiempo (ej: aceite de motor, líquido de frenos).
    /// false = solo se cambia por km (ej: filtro de aire, pastillas de freno).
    /// </summary>
    public bool ExpiresByTime { get; init; }

    /// <summary>Días de vida útil. Solo cuando ExpiresByTime = true.</summary>
    public int? UsefulLifeDays { get; init; }

    public string? Description { get; init; }
    public bool Status { get; init; } = true;

    // Navegación
    public ActionListType? ActionListType { get; init; }
}


// MaintManager.Domain/Entities/VehicleSchedule.cs

/// <summary>
/// Cronograma de mantenimiento por vehículo.
/// next_km se recalcula automáticamente al completar un servicio.
/// </summary>
