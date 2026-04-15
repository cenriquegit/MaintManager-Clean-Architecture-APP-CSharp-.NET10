
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de generación y gestión de alertas.</summary>
public interface IAlertService
{
    /// <summary>Ejecuta todas las verificaciones y genera alertas pendientes.</summary>
    Task CheckAndGenerateAlertsAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(int alloid, int readByWorkid, CancellationToken ct = default);
    Task ResolveAsync(int alloid, int resolvedByWorkid, CancellationToken ct = default);
}
