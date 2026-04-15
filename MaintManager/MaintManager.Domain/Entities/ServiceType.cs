namespace MaintManager.Domain.Entities;

public sealed class ServiceType
{
    public short Setyid { get; init; }
    public char Code { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;
}


// MaintManager.Domain/Entities/ActionListType.cs

/// <summary>
/// Lista de acciones del manual del vehículo.
/// Lista 1: Elementos de Reemplazo y Aplicación (A/C/I).
/// Lista 2: Operaciones de mano de obra (I/R).
/// </summary>
