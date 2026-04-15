namespace MaintManager.Domain.Entities;

public sealed class MaintenanceType
{
    public short Matyid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;
}


// MaintManager.Domain/Entities/ServiceType.cs

/// <summary>
/// Tipo de servicio calendarizado.
/// A = Servicio Básico. B = Servicio Completo.
/// </summary>
