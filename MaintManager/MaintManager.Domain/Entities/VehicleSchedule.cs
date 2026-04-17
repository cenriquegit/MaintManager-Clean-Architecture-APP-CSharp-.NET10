namespace MaintManager.Domain.Entities;

public sealed class VehicleSchedule
{
    public int Veshid { get; private set; }
    public int Prcoid { get; private set; }
    public int IntervalKm { get; private set; } = 5000;
    public int NextKm { get; private set; }
    public int? AlertKmThreshold { get; private set; } = 800;

    /// <summary>
    /// Tipo del próximo servicio: 'A' (básico) o 'B' (completo).
    /// Se alterna automáticamente al recalendarizar (A→B→A...).
    /// </summary>
    public string? NextServiceTypeCode { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public int CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool Status { get; private set; } = true;

    private VehicleSchedule() { }

    public static VehicleSchedule Create(int prcoid, int currentKm, int createdBy,
        int intervalKm = 5000, int alertThreshold = 800, string? initialServiceType = "A")
    {
        if (intervalKm <= 0)
            throw new ArgumentException("El intervalo de km debe ser mayor a cero.", nameof(intervalKm));

        return new VehicleSchedule
        {
            Prcoid = prcoid,
            IntervalKm = intervalKm,
            NextKm = currentKm + intervalKm,
            AlertKmThreshold = alertThreshold,
            NextServiceTypeCode = initialServiceType,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Recalendariza el próximo servicio desde el km del mantenimiento completado.
    /// Alterna el tipo de servicio (A↔B) automáticamente.
    /// </summary>
    public void Reschedule(int serviceKm)
    {
        NextKm = serviceKm + IntervalKm;
        NextServiceTypeCode = NextServiceTypeCode == "A" ? "B" : "A";
        UpdatedAt = DateTime.UtcNow;
    }
}