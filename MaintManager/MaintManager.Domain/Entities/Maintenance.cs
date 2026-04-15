// MaintManager.Domain/Entities/Maintenance.cs
// TABLA CENTRAL del sistema — maintenance.maintenance
using MaintManager.Domain.Entities.Existing;

namespace MaintManager.Domain.Entities;

/// <summary>
/// Orden de mantenimiento. Tabla central del sistema.
/// Registra cada servicio realizado a un vehículo de la flota.
/// </summary>
public sealed class Maintenance
{
    public int Mainid { get; private set; }

    /// <summary>ID del vehículo (FK a product.company.prcoid).</summary>
    public int Prcoid { get; private set; }

    /// <summary>Tipo: 1=Calendarizado, 2=Emergencia.</summary>
    public short Matyid { get; private set; }

    /// <summary>Tipo de servicio: 1=A (básico), 2=B (completo). Null en emergencias.</summary>
    public short? Setyid { get; private set; }

    public string? OrderNumber { get; private set; }
    public DateTime MaintenanceDate { get; private set; }

    /// <summary>Kilometraje del vehículo al momento del servicio.</summary>
    public int Mileage { get; private set; }

    /// <summary>Km recorridos desde el último mantenimiento. Calculado al registrar.</summary>
    public int? KmSinceLast { get; private set; }

    public string? AdditionalWork { get; private set; }
    public string? OilBrand { get; private set; }
    public string? OilViscositySae { get; private set; }
    public string? ClimateSeason { get; private set; }

    /// <summary>
    /// Si true, los datos del aceite de ESTE mantenimiento aparecen como
    /// nota informativa en el formulario del SIGUIENTE mantenimiento.
    /// </summary>
    public bool ShowOilInNextMaintenance { get; private set; }

    public string OriginService { get; private set; } = "Taller propio";
    public string? SignatureSeal { get; private set; }

    /// <summary>
    /// Solo para emergencias: true=completó todo (recalendariza),
    /// false=solo lo urgente (NO recalendariza). Null para calendarizados.
    /// </summary>
    public bool? IsEmergencyComplete { get; private set; }

    /// <summary>Mecánico asignado que ejecuta el trabajo.</summary>
    public int AssignedTo { get; private set; }

    /// <summary>Worker que registra la orden en el sistema.</summary>
    public int Workid { get; private set; }

    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string Statid { get; private set; } = "AC";

    // Navegación
    public MaintenanceType? MaintenanceType { get; private set; }
    public ServiceType? ServiceType { get; private set; }
    public ICollection<MaintenanceActionDetail> ActionDetails { get; private set; } = [];
    public Diagnosis? Diagnosis { get; private set; }
    public ICollection<MaterialConsumption> MaterialConsumptions { get; private set; } = [];
    public ICollection<InstalledComponent> InstalledComponents { get; private set; } = [];
    public ICollection<MaterialRating> MaterialRatings { get; private set; } = [];

    // Constructor privado para EF Core
    private Maintenance() { }

    /// <summary>
    /// Crea una nueva orden de mantenimiento con validaciones de dominio.
    /// </summary>
    public static Maintenance Create(
        int prcoid,
        short matyid,
        int mileage,
        int assignedTo,
        int registeredByWorkid,
        string originService = "Taller propio",
        short? setyid = null,
        int? kmSinceLast = null,
        string? orderNumber = null,
        string? note = null)
    {
        if (mileage < 0)
            throw new ArgumentException("El kilometraje no puede ser negativo.", nameof(mileage));

        if (matyid == 2 && setyid.HasValue)
            throw new InvalidOperationException("Los mantenimientos de emergencia no tienen tipo de servicio A/B.");

        return new Maintenance
        {
            Prcoid = prcoid,
            Matyid = matyid,
            Setyid = setyid,
            Mileage = mileage,
            KmSinceLast = kmSinceLast,
            AssignedTo = assignedTo,
            Workid = registeredByWorkid,
            OriginService = originService,
            OrderNumber = orderNumber,
            Note = note,
            MaintenanceDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Statid = "AC"
        };
    }

    /// <summary>
    /// Cierra la emergencia indicando si fue completa o parcial.
    /// </summary>
    public void CloseEmergency(bool isComplete)
    {
        if (Matyid != 2)
            throw new InvalidOperationException("Solo se puede cerrar emergencia en mantenimientos de tipo Emergencia.");

        IsEmergencyComplete = isComplete;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetOilInfo(string brand, string viscositySae, string? climateSeason, bool showInNext)
    {
        OilBrand = brand;
        OilViscositySae = viscositySae;
        ClimateSeason = climateSeason;
        ShowOilInNextMaintenance = showInNext;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNote(string? note)
    {
        Note = note;
        UpdatedAt = DateTime.UtcNow;
    }
}
