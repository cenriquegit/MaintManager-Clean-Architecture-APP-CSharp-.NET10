
namespace MaintManager.Shared.Constants;

/// <summary>Tipos de alerta del sistema. Deben coincidir con alert_config.alert_type en BD.</summary>
public static class AlertTypes
{
    public const string MantenimientoProximoKm = "MANTENIMIENTO_PROXIMO_KM";
    public const string ComponentePorCaducar = "COMPONENTE_POR_CADUCAR";
    public const string LotePorVencer = "LOTE_POR_VENCER";
    public const string StockBajo = "STOCK_BAJO";
}
