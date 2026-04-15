// MaintManager.Domain/Entities/MaintenanceType.cs
namespace MaintManager.Domain.Entities;

/// <summary>Tipo de mantenimiento. Solo 2 registros: Calendarizado y Emergencia.</summary>
public sealed class MaintenanceType
{
    public short Matyid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/ServiceType.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Tipo de servicio calendarizado.
/// A = Servicio Básico. B = Servicio Completo.
/// </summary>
public sealed class ServiceType
{
    public short Setyid { get; init; }
    public char Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/ActionListType.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Lista de acciones del manual del vehículo.
/// Lista 1: Elementos de Reemplazo y Aplicación (A/C/I).
/// Lista 2: Operaciones de mano de obra (I/R).
/// </summary>
public sealed class ActionListType
{
    public short Altoid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;

    public ICollection<ActionCatalog> Actions { get; init; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/ActionCatalog.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Catálogo maestro de acciones de mantenimiento.
/// Define qué se hace, con qué producto y si el componente caduca por tiempo.
/// </summary>
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/VehicleSchedule.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Cronograma de mantenimiento por vehículo.
/// next_km se recalcula automáticamente al completar un servicio.
/// </summary>
public sealed class VehicleSchedule
{
    public int Veshid { get; private set; }
    public int Prcoid { get; private set; }
    public int IntervalKm { get; private set; } = 5000;
    public int NextKm { get; private set; }
    public int? AlertKmThreshold { get; private set; } = 800;
    public DateTime CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Status { get; private set; } = true;

    private VehicleSchedule() { }

    public static VehicleSchedule Create(int prcoid, int currentKm, int createdBy,
        int intervalKm = 5000, int alertThreshold = 800)
    {
        if (intervalKm <= 0)
            throw new ArgumentException("El intervalo de km debe ser mayor a cero.", nameof(intervalKm));

        return new VehicleSchedule
        {
            Prcoid = prcoid,
            IntervalKm = intervalKm,
            NextKm = currentKm + intervalKm,
            AlertKmThreshold = alertThreshold,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Recalendariza el próximo servicio desde el km del mantenimiento completado.
    /// Solo aplica en calendarizados o emergencias completas.
    /// </summary>
    public void Reschedule(int serviceKm)
    {
        NextKm = serviceKm + IntervalKm;
        UpdatedAt = DateTime.UtcNow;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/ScheduleAction.cs
namespace MaintManager.Domain.Entities;

/// <summary>Acción programada por km según el manual del vehículo.</summary>
public sealed class ScheduleAction
{
    public int Shacid { get; init; }
    public int Veshid { get; init; }
    public int Acatid { get; init; }
    public int ScheduledKm { get; init; }
    public char ActionCode { get; init; }
    public bool Status { get; init; } = true;

    // Navegación
    public VehicleSchedule? VehicleSchedule { get; init; }
    public ActionCatalog? ActionCatalog { get; init; }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/MaintenanceActionDetail.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Detalle de una acción realizada en un mantenimiento.
/// Representa una fila del checklist de la orden de servicio.
/// </summary>
public sealed class MaintenanceActionDetail
{
    public int Madeid { get; private set; }
    public int Mainid { get; private set; }
    public int Acatid { get; private set; }
    public bool Completed { get; private set; }
    public char? ActionPerformed { get; private set; }
    public string? ProductUsed { get; private set; }
    public string? QuantityUsed { get; private set; }
    public string? OriginProduct { get; private set; }
    public string? Observation { get; private set; }
    public int? Maloid { get; private set; }

    // Navegación
    public ActionCatalog? ActionCatalog { get; private set; }
    public MaterialLot? MaterialLot { get; private set; }

    private MaintenanceActionDetail() { }

    public static MaintenanceActionDetail Create(int mainid, int acatid) =>
        new() { Mainid = mainid, Acatid = acatid, Completed = false };

    public void Complete(char actionCode, string? productUsed,
        string? quantityUsed, string? originProduct, string? observation, int? maloid = null)
    {
        Completed = true;
        ActionPerformed = actionCode;
        ProductUsed = productUsed;
        QuantityUsed = quantityUsed;
        OriginProduct = originProduct;
        Observation = observation;
        Maloid = maloid;
    }
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/Diagnosis.cs
namespace MaintManager.Domain.Entities;

/// <summary>Diagnóstico final del mecánico al cerrar un mantenimiento.</summary>
public sealed class Diagnosis
{
    public int Diagid { get; private set; }
    public int Mainid { get; private set; }
    public string GeneralStatus { get; private set; } = string.Empty;
    public string? Observations { get; private set; }
    public bool VehicleOperative { get; private set; } = true;
    public string? FutureRecommendations { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Diagnosis() { }

    public static Diagnosis Create(int mainid, string generalStatus,
        bool vehicleOperative, string? observations = null, string? futureRecommendations = null)
    {
        if (string.IsNullOrWhiteSpace(generalStatus))
            throw new ArgumentException("El estado general del diagnóstico es obligatorio.", nameof(generalStatus));

        return new Diagnosis
        {
            Mainid = mainid,
            GeneralStatus = generalStatus,
            VehicleOperative = vehicleOperative,
            Observations = observations,
            FutureRecommendations = futureRecommendations,
            CreatedAt = DateTime.UtcNow
        };
    }
}
