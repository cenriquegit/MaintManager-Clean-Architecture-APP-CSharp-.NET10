namespace MaintManager.Domain.Entities;

public sealed class ActionListType
{
    public short Altoid { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Status { get; init; } = true;

    public ICollection<ActionCatalog> Actions { get; init; } = [];
}


// MaintManager.Domain/Entities/ActionCatalog.cs

/// <summary>
/// Catálogo maestro de acciones de mantenimiento.
/// Define qué se hace, con qué producto y si el componente caduca por tiempo.
/// </summary>
