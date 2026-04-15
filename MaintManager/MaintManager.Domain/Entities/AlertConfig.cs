namespace MaintManager.Domain.Entities;

/// <summary>Configuración de tipos de alerta del sistema.</summary>
public sealed class AlertConfig
{
    public int Alcoid { get; init; }
    public string AlertType { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool Enabled { get; init; } = true;
    public string? ThresholdValue { get; init; }
    public string? ThresholdUnit { get; init; }
}
