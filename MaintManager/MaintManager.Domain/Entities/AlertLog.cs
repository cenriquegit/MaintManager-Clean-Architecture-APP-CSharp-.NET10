namespace MaintManager.Domain.Entities;

/// <summary>
/// Historial de alertas generadas por el sistema.
/// Alimenta el dashboard BI (alertas sin resolver).
/// </summary>
public sealed class AlertLog
{
    public int Alloid { get; private set; }
    public int Alcoid { get; private set; }
    public int? Prcoid { get; private set; }
    public int? Mateid { get; private set; }
    public int? Maloid { get; private set; }
    public int? Incoid { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DateTime AlertDate { get; private set; }
    public bool Read { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public int? ReadBy { get; private set; }
    public bool Resolved { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public int? ResolvedBy { get; private set; }

    public AlertConfig? AlertConfig { get; private set; }

    private AlertLog() { }

    public static AlertLog Create(int alcoid, string message, int? prcoid = null,
        int? mateid = null, int? maloid = null, int? incoid = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("El mensaje de la alerta es obligatorio.", nameof(message));

        return new AlertLog
        {
            Alcoid = alcoid,
            Message = message,
            Prcoid = prcoid,
            Mateid = mateid,
            Maloid = maloid,
            Incoid = incoid,
            AlertDate = DateTime.UtcNow,
            Read = false,
            Resolved = false
        };
    }

    public void MarkAsRead(int readByWorkid)
    {
        Read = true;
        ReadAt = DateTime.UtcNow;
        ReadBy = readByWorkid;
    }

    public void Resolve(int resolvedByWorkid)
    {
        Resolved = true;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = resolvedByWorkid;
        if (!Read) MarkAsRead(resolvedByWorkid);
    }
}
