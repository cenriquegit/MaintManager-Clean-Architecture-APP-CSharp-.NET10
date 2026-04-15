// MaintManager.MAUI/Converters/AllConverters.cs
// Converters referenciados en los XAML de la aplicación.
// Se registran en App.xaml como recursos globales.
using System.Globalization;

namespace MaintManager.MAUI.Converters;

// ─────────────────────────────────────────────────────────────────────────────
// InvertedBoolConverter — invierte un booleano (IsLoading → IsEnabled)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Convierte bool → !bool. Uso: IsEnabled = NOT IsLoading.</summary>
public sealed class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}

// ─────────────────────────────────────────────────────────────────────────────
// IsNotNullConverter — true si el valor NO es null (para IsVisible)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Convierte any → bool. true si el valor no es null ni string vacío.
/// Uso: IsVisible = campo != null.
/// </summary>
public sealed class IsNotNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? !string.IsNullOrWhiteSpace(s) : value is not null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// IsNullConverter — true si el valor ES null (inverso del anterior)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Convierte any → bool. true si el valor es null o string vacío.</summary>
public sealed class IsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string s ? string.IsNullOrWhiteSpace(s) : value is null;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// IntToBoolConverter — true si int == parameter
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Compara un int con el parameter.
/// Uso: IsChecked = SelectedTypeId == 1.
/// ConverterParameter="1" → true si value == 1.
/// </summary>
public sealed class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int intVal && parameter is string paramStr
            && int.TryParse(paramStr, out var paramInt))
            return intVal == paramInt;
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b && parameter is string paramStr
            && int.TryParse(paramStr, out var paramInt))
            return paramInt;
        return 0;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// StringEqualsConverter — true si string == parameter
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Compara un string con el parameter.
/// Uso: IsChecked = OriginService == "Taller propio".
/// </summary>
public sealed class StringEqualsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string str && parameter is string param &&
        str.Equals(param, StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && b ? parameter?.ToString() ?? string.Empty : string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// BoolToColorConverter — devuelve uno de dos colores según bool
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Devuelve TrueColor o FalseColor según el valor bool.
/// Uso: TextColor = IsBelowMinimum ? Red : TextSecondary.
/// </summary>
public sealed class BoolToColorConverter : IValueConverter
{
    public Color TrueColor { get; set; } = Colors.Red;
    public Color FalseColor { get; set; } = Colors.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && b ? TrueColor : FalseColor;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// AlertTypeToColorConverter — color según tipo de alerta
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Mapea el tipo de alerta a un color semántico para la UI.</summary>
public sealed class AlertTypeToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "MANTENIMIENTO_PROXIMO_KM" => Color.FromArgb("#1E88E5"),
            "COMPONENTE_POR_CADUCAR"   => Color.FromArgb("#F57C00"),
            "LOTE_POR_VENCER"          => Color.FromArgb("#F57C00"),
            "STOCK_BAJO"               => Color.FromArgb("#E53935"),
            _ => Color.FromArgb("#546E8A")
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// AlertTypeToLabelConverter — etiqueta legible según tipo de alerta
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Convierte el código de tipo de alerta a texto legible para el usuario final.
/// Nunca muestra códigos técnicos al jefe ni a los mecánicos.
/// </summary>
public sealed class AlertTypeToLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "MANTENIMIENTO_PROXIMO_KM" => "Servicio próximo",
            "COMPONENTE_POR_CADUCAR"   => "Componente por caducar",
            "LOTE_POR_VENCER"          => "Lote por vencer",
            "STOCK_BAJO"               => "Stock bajo mínimo",
            _ => "Alerta"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// StockStatusColorConverter — color del stock según si está bajo mínimo
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Devuelve color rojo si IsBelowMinimum, verde si no.
/// Uso: TextColor del stock total en la lista de inventario.
/// </summary>
public sealed class StockStatusColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool isBelowMinimum && isBelowMinimum
            ? Color.FromArgb("#E53935")
            : Color.FromArgb("#2ECC71");

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

// ─────────────────────────────────────────────────────────────────────────────
// DaysUntilExpiryColorConverter — color según días hasta vencimiento del lote
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Rojo si ≤ 7 días, naranja si ≤ 30, verde si más.
/// </summary>
public sealed class DaysUntilExpiryColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int days)
        {
            if (days <= 7)  return Color.FromArgb("#E53935");
            if (days <= 30) return Color.FromArgb("#F57C00");
            return Color.FromArgb("#2ECC71");
        }
        return Color.FromArgb("#546E8A");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
