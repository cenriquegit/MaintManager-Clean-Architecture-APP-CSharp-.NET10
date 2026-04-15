namespace MaintManager.Domain.Entities;

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


// MaintManager.Domain/Entities/MaintenanceActionDetail.cs

/// <summary>
/// Detalle de una acción realizada en un mantenimiento.
/// Representa una fila del checklist de la orden de servicio.
/// </summary>
