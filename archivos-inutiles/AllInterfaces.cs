// MaintManager.Domain/Interfaces/Repositories/IGenericRepository.cs
namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio genérico con operaciones CRUD básicas.</summary>
public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Repositories/IMaintenanceRepository.cs
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio de órdenes de mantenimiento con consultas especializadas.</summary>
public interface IMaintenanceRepository : IGenericRepository<Maintenance>
{
    Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<Maintenance?> GetWithDetailsAsync(int mainid, CancellationToken ct = default);
    Task<Maintenance?> GetLastByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task<IReadOnlyList<Maintenance>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Repositories/IVehicleRepository.cs
using MaintManager.Domain.Entities.Existing;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio de vehículos (tablas existentes — solo lectura).</summary>
public interface IVehicleRepository
{
    Task<IReadOnlyList<Vehicle>> GetActiveVehiclesAsync(CancellationToken ct = default);
    Task<Vehicle?> GetByIdAsync(int prcoid, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el km actual del vehículo: último kilometer_end de rentexecute
    /// o mileage del registro si no hay rentas.
    /// </summary>
    Task<int> GetCurrentKmAsync(int prcoid, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Repositories/IInventoryRepository.cs
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio del inventario de materiales y lotes.</summary>
public interface IInventoryRepository
{
    // Materiales
    Task<Material?> GetMaterialByIdAsync(int mateid, CancellationToken ct = default);
    Task<IReadOnlyList<Material>> GetMaterialsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Material>> GetLowStockMaterialsAsync(CancellationToken ct = default);
    Task AddMaterialAsync(Material material, CancellationToken ct = default);
    void UpdateMaterial(Material material);

    // Lotes
    Task<MaterialLot?> GetLotByIdAsync(int maloid, CancellationToken ct = default);
    Task<IReadOnlyList<MaterialLot>> GetActiveLotsByMaterialAsync(int mateid, CancellationToken ct = default);

    /// <summary>
    /// Devuelve los lotes activos de un material ordenados FIFO por vencimiento.
    /// Los lotes sin fecha de vencimiento van al final.
    /// </summary>
    Task<IReadOnlyList<MaterialLot>> GetFifoLotsAsync(int mateid, CancellationToken ct = default);

    Task<IReadOnlyList<MaterialLot>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default);
    Task AddLotAsync(MaterialLot lot, CancellationToken ct = default);
    void UpdateLot(MaterialLot lot);

    // Consumos y descartes
    Task AddConsumptionAsync(MaterialConsumption consumption, CancellationToken ct = default);
    Task AddDiscardAsync(MaterialDiscard discard, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Repositories/IAlertRepository.cs
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Repositories;

/// <summary>Repositorio del sistema de alertas.</summary>
public interface IAlertRepository
{
    Task<IReadOnlyList<AlertLog>> GetUnresolvedAlertsAsync(CancellationToken ct = default);
    Task<AlertLog?> GetByIdAsync(int alloid, CancellationToken ct = default);
    Task AddAsync(AlertLog alert, CancellationToken ct = default);
    void Update(AlertLog alert);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Services/IMaintenanceService.cs
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de gestión de órdenes de mantenimiento.</summary>
public interface IMaintenanceService
{
    Task<Maintenance> CreateAsync(int prcoid, short matyid, int mileage,
        int assignedTo, int registeredBy, short? setyid, string? note, CancellationToken ct = default);

    Task<Maintenance> GetWithDetailsAsync(int mainid, CancellationToken ct = default);
    Task<IReadOnlyList<Maintenance>> GetByVehicleAsync(int prcoid, CancellationToken ct = default);
    Task CompleteActionAsync(int madeid, char actionCode, string? productUsed,
        string? quantity, string? origin, string? observation, int? maloid, CancellationToken ct = default);
    Task SaveDiagnosisAsync(int mainid, string generalStatus, bool vehicleOperative,
        string? observations, string? futureRecommendations, CancellationToken ct = default);
    Task CloseMaintenanceAsync(int mainid, bool? isEmergencyComplete, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Services/ISchedulingService.cs
using MaintManager.Domain.Entities;

namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de calendarización y recalendarización.</summary>
public interface ISchedulingService
{
    Task<VehicleSchedule?> GetScheduleAsync(int prcoid, CancellationToken ct = default);
    Task<VehicleSchedule> CreateScheduleAsync(int prcoid, int currentKm, int createdBy, CancellationToken ct = default);
    Task RescheduleAsync(int prcoid, int serviceKm, CancellationToken ct = default);

    /// <summary>
    /// Verifica si un vehículo está próximo a su próximo mantenimiento
    /// según el umbral configurado en config_system.
    /// </summary>
    Task<bool> IsMaintenanceDueSoonAsync(int prcoid, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Services/IInventoryService.cs
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de gestión de inventario.</summary>
public interface IInventoryService
{
    Task CreateMaterialAsync(short macaid, string name, string unit, decimal stockMin, int createdBy, CancellationToken ct = default);
    Task RegisterLotAsync(int mateid, decimal quantity, decimal unitCost, int createdBy,
        DateOnly? expirationDate, int? provid, string? supplierLot, CancellationToken ct = default);

    /// <summary>
    /// Consume material de los lotes usando FIFO por fecha de vencimiento.
    /// Si el consumo abarca más de un lote, genera múltiples registros de MaterialConsumption.
    /// </summary>
    Task<IReadOnlyList<int>> ConsumeStockFifoAsync(int mateid, decimal quantity,
        int mainid, CancellationToken ct = default);

    Task DiscardLotAsync(int maloid, decimal quantity, string reason,
        int discardedBy, string? note, CancellationToken ct = default);
    Task RateMaterialAsync(int mateid, int mainid, short rating, int ratedBy, string? observation, CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Services/IBiReportService.cs
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de Business Intelligence y reportes.</summary>
public interface IBiReportService
{
    Task<object> GetDashboardSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetCostPerKmAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetEmergencyRateAsync(CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetMonthlyCostAsync(int months, CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetExpiringLotsAsync(int daysThreshold, CancellationToken ct = default);
    Task<IReadOnlyList<object>> GetCalendarComplianceAsync(CancellationToken ct = default);
    Task<byte[]> ExportMaintenanceToPdfAsync(int mainid, CancellationToken ct = default);
    Task<byte[]> ExportCostReportToExcelAsync(CancellationToken ct = default);
}

// ─────────────────────────────────────────────────────────────────────────────

// MaintManager.Domain/Interfaces/Services/IAlertService.cs
namespace MaintManager.Domain.Interfaces.Services;

/// <summary>Contrato del servicio de generación y gestión de alertas.</summary>
public interface IAlertService
{
    /// <summary>Ejecuta todas las verificaciones y genera alertas pendientes.</summary>
    Task CheckAndGenerateAlertsAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(int alloid, int readByWorkid, CancellationToken ct = default);
    Task ResolveAsync(int alloid, int resolvedByWorkid, CancellationToken ct = default);
}
