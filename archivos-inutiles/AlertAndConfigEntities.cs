// MaintManager.Domain/Entities/AlertConfig.cs
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/AlertLog.cs
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

    // Navegación
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

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Entities/ConfigSystem.cs
namespace MaintManager.Domain.Entities;

/// <summary>
/// Parámetros configurables del sistema por clave-valor.
/// Ejemplos: alerta_km_umbral, intervalo_km, alerta_vencimiento_dias.
/// </summary>
public sealed class ConfigSystem
{
    public int Cosyid { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string DataType { get; init; } = "string";
    public DateTime UpdatedAt { get; init; }
    public int? UpdatedBy { get; init; }
    public bool Status { get; init; } = true;

    /// <summary>Obtiene el valor como entero. Lanza si el tipo no es integer.</summary>
    public int GetIntValue()
    {
        if (!int.TryParse(Value, out var result))
            throw new InvalidCastException($"El parámetro '{Key}' no puede convertirse a entero. Valor actual: '{Value}'.");
        return result;
    }

    /// <summary>Obtiene el valor como booleano.</summary>
    public bool GetBoolValue()
    {
        if (!bool.TryParse(Value, out var result))
            throw new InvalidCastException($"El parámetro '{Key}' no puede convertirse a booleano. Valor actual: '{Value}'.");
        return result;
    }
}
