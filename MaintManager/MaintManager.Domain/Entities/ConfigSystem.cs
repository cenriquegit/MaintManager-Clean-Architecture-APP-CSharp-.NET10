namespace MaintManager.Domain.Entities;

/// <summary>
/// Parámetros configurables del sistema por clave-valor.
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

    public int GetIntValue()
    {
        if (!int.TryParse(Value, out var result))
            throw new InvalidCastException($"El parámetro '{Key}' no puede convertirse a entero. Valor actual: '{Value}'.");
        return result;
    }

    public bool GetBoolValue()
    {
        if (!bool.TryParse(Value, out var result))
            throw new InvalidCastException($"El parámetro '{Key}' no puede convertirse a booleano. Valor actual: '{Value}'.");
        return result;
    }
}
