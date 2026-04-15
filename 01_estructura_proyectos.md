# Estructura de Proyectos — MaintManager
# Neo Plus Business S.A.C.
# Clean Architecture Híbrida (.NET 10)

```
MaintManager/
│
├── MaintManager.slnx
│
├── MaintManager.Shared/                          ← Constantes y helpers comunes
│   ├── MaintManager.Shared.csproj
│   ├── Constants/
│   │   ├── ApiRoutes.cs                          ← Rutas de la API (/api/v1/...)
│   │   ├── ErrorMessages.cs                      ← Mensajes de error estandarizados
│   │   ├── RoleNames.cs                          ← "Admin", "Tecnico"
│   │   └── AlertTypes.cs                         ← Tipos de alerta del sistema
│   └── Helpers/
│       └── DateHelper.cs                         ← Utilidades de fecha/hora
│
├── MaintManager.Domain/                          ← Núcleo del negocio (sin dependencias)
│   ├── MaintManager.Domain.csproj
│   ├── Entities/
│   │   ├── Existing/                             ← Scaffolded (tablas existentes — SOLO LECTURA)
│   │   │   ├── Vehicle.cs                        ← product.vehicle
│   │   │   ├── Worker.cs                         ← public.worker
│   │   │   ├── RentExecute.cs                    ← service.rentexecute
│   │   │   ├── Person.cs                         ← public.person
│   │   │   └── Provider.cs                       ← public.provider
│   │   ├── Maintenance.cs                        ← maintenance.maintenance (TABLA CENTRAL)
│   │   ├── MaintenanceActionDetail.cs            ← maintenance.maintenance_action_detail
│   │   ├── Diagnosis.cs                          ← maintenance.diagnosis
│   │   ├── VehicleSchedule.cs                    ← maintenance.vehicle_schedule
│   │   ├── ScheduleAction.cs                     ← maintenance.schedule_action
│   │   ├── ActionCatalog.cs                      ← maintenance.action_catalog
│   │   ├── ActionListType.cs                     ← maintenance.action_list_type
│   │   ├── MaintenanceType.cs                    ← maintenance.maintenance_type
│   │   ├── ServiceType.cs                        ← maintenance.service_type
│   │   ├── Material.cs                           ← maintenance.material
│   │   ├── MaterialCategory.cs                   ← maintenance.material_category
│   │   ├── MaterialLot.cs                        ← maintenance.material_lot
│   │   ├── MaterialConsumption.cs                ← maintenance.material_consumption
│   │   ├── MaterialDiscard.cs                    ← maintenance.material_discard
│   │   ├── MaterialRating.cs                     ← maintenance.material_rating
│   │   ├── InstalledComponent.cs                 ← maintenance.installed_component
│   │   ├── AlertConfig.cs                        ← maintenance.alert_config
│   │   ├── AlertLog.cs                           ← maintenance.alert_log
│   │   └── ConfigSystem.cs                       ← maintenance.config_system
│   ├── Enums/
│   │   ├── MaintenanceTypeEnum.cs                ← Calendarizado = 1, Emergencia = 2
│   │   ├── ServiceTypeEnum.cs                    ← A = 1, B = 2
│   │   ├── ActionCode.cs                         ← A, C, I, R
│   │   ├── LotStatus.cs                          ← Activo, Agotado, Vencido, Descartado
│   │   ├── MaterialOrigin.cs                     ← StockPropio, Externo
│   │   └── DiscardReason.cs                      ← Vencimiento, Daño, Otro
│   └── Interfaces/
│       ├── Repositories/
│       │   ├── IMaintenanceRepository.cs
│       │   ├── IVehicleRepository.cs
│       │   ├── IInventoryRepository.cs
│       │   ├── IAlertRepository.cs
│       │   └── IGenericRepository.cs
│       └── Services/
│           ├── IMaintenanceService.cs
│           ├── ISchedulingService.cs
│           ├── IInventoryService.cs
│           ├── IInstalledComponentService.cs
│           ├── IBiReportService.cs
│           └── IAlertService.cs
│
├── MaintManager.Application/                    ← Casos de uso, DTOs, validaciones
│   ├── MaintManager.Application.csproj
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── LoginRequest.cs
│   │   │   └── LoginResponse.cs
│   │   ├── Vehicle/
│   │   │   ├── VehicleResponse.cs
│   │   │   └── VehicleListResponse.cs
│   │   ├── Maintenance/
│   │   │   ├── MaintenanceCreateRequest.cs
│   │   │   ├── MaintenanceUpdateRequest.cs
│   │   │   ├── MaintenanceResponse.cs
│   │   │   ├── MaintenanceListResponse.cs
│   │   │   ├── ActionDetailRequest.cs
│   │   │   └── DiagnosisRequest.cs
│   │   ├── Inventory/
│   │   │   ├── MaterialCreateRequest.cs
│   │   │   ├── MaterialResponse.cs
│   │   │   ├── LotCreateRequest.cs
│   │   │   ├── LotResponse.cs
│   │   │   └── ConsumptionRequest.cs
│   │   ├── Schedule/
│   │   │   ├── VehicleScheduleResponse.cs
│   │   │   └── ScheduleActionRequest.cs
│   │   ├── Alert/
│   │   │   ├── AlertResponse.cs
│   │   │   └── AlertResolveRequest.cs
│   │   ├── Reports/
│   │   │   ├── CostPerKmResponse.cs
│   │   │   ├── EmergencyRateResponse.cs
│   │   │   ├── MonthlyCostreResponse.cs
│   │   │   ├── DashboardSummaryResponse.cs
│   │   │   └── ExpiringLotResponse.cs
│   │   └── Common/
│   │       ├── PagedRequest.cs
│   │       └── ApiResponse.cs                   ← Wrapper de respuesta estandarizada
│   ├── Services/
│   │   ├── MaintenanceService.cs
│   │   ├── SchedulingService.cs
│   │   ├── InventoryService.cs
│   │   ├── InstalledComponentService.cs
│   │   ├── BiReportService.cs
│   │   └── AlertService.cs
│   ├── Validators/
│   │   ├── MaintenanceCreateValidator.cs
│   │   ├── LoginRequestValidator.cs
│   │   ├── MaterialCreateValidator.cs
│   │   └── LotCreateValidator.cs
│   └── Mappings/
│       ├── MaintenanceMappings.cs               ← Métodos ToDto() / ToEntity()
│       ├── VehicleMappings.cs
│       └── InventoryMappings.cs
│
├── MaintManager.Infrastructure/                 ← EF Core, repositorios, seed
│   ├── MaintManager.Infrastructure.csproj
│   ├── Data/
│   │   ├── FleetMaintenanceContext.cs           ← DbContext único (híbrido)
│   │   └── Configurations/                      ← Fluent API por entidad
│   │       ├── MaintenanceConfiguration.cs
│   │       ├── VehicleScheduleConfiguration.cs
│   │       ├── MaterialConfiguration.cs
│   │       ├── MaterialLotConfiguration.cs
│   │       ├── InstalledComponentConfiguration.cs
│   │       └── AlertLogConfiguration.cs
│   ├── Repositories/
│   │   ├── GenericRepository.cs
│   │   ├── MaintenanceRepository.cs
│   │   ├── VehicleRepository.cs
│   │   ├── InventoryRepository.cs
│   │   └── AlertRepository.cs
│   ├── Migrations/                              ← Solo para esquema maintenance (Code First)
│   └── Seed/
│       ├── DatabaseSeeder.cs                    ← Orquestador del seed
│       ├── VehicleSeed.cs                       ← 12 vehículos realistas
│       ├── WorkerSeed.cs                        ← 4 usuarios (2 Admin + 2 Tecnico)
│       ├── MaterialSeed.cs                      ← Materiales y lotes
│       ├── MaintenanceSeed.cs                   ← Historial de mantenimientos
│       └── RentSeed.cs                          ← Datos de renta para km real
│
├── MaintManager.API/                            ← ASP.NET Web API
│   ├── MaintManager.API.csproj
│   ├── Program.cs
│   ├── appsettings.json                         ← Sin datos sensibles
│   ├── appsettings.Development.json             ← Connection string y JWT (en .gitignore)
│   ├── Controllers/
│   │   ├── AuthController.cs                    ← POST /api/v1/auth/login
│   │   ├── VehiclesController.cs                ← GET /api/v1/vehicles
│   │   ├── MaintenancesController.cs            ← CRUD /api/v1/maintenances
│   │   ├── InventoryController.cs               ← CRUD /api/v1/inventory
│   │   ├── SchedulesController.cs               ← GET/PUT /api/v1/schedules
│   │   ├── AlertsController.cs                  ← GET/PUT /api/v1/alerts
│   │   ├── ComponentsController.cs              ← GET /api/v1/components
│   │   └── ReportsController.cs                 ← GET /api/v1/reports/*
│   ├── Middleware/
│   │   ├── GlobalExceptionMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs       ← Registro de DI
│       ├── SwaggerExtensions.cs
│       └── JwtExtensions.cs
│
└── MaintManager.MAUI/                           ← App móvil/escritorio
    ├── MaintManager.MAUI.csproj
    ├── AppShell.xaml / .cs
    ├── MauiProgram.cs
    ├── Resources/
    │   ├── Colors.xaml                          ← Paleta NeoPlus
    │   ├── Styles.xaml                          ← Estilos globales
    │   └── Images/                              ← Logo NeoPlus
    ├── Services/
    │   ├── IApiService.cs
    │   ├── ApiService.cs                        ← HttpClient con retry
    │   ├── IAuthService.cs
    │   └── AuthService.cs                       ← Almacena JWT token
    ├── ViewModels/
    │   ├── BaseViewModel.cs                     ← IsLoading, HasError, ErrorMessage
    │   ├── Auth/
    │   │   └── LoginViewModel.cs
    │   ├── Dashboard/
    │   │   └── DashboardViewModel.cs
    │   ├── Vehicles/
    │   │   └── VehicleListViewModel.cs
    │   ├── Maintenances/
    │   │   ├── MaintenanceListViewModel.cs
    │   │   ├── MaintenanceDetailViewModel.cs
    │   │   └── MaintenanceCreateViewModel.cs
    │   ├── Inventory/
    │   │   ├── InventoryListViewModel.cs
    │   │   └── LotCreateViewModel.cs
    │   └── Alerts/
    │       └── AlertListViewModel.cs
    └── Views/
        ├── Auth/
        │   └── LoginPage.xaml / .cs
        ├── Dashboard/
        │   └── DashboardPage.xaml / .cs
        ├── Vehicles/
        │   └── VehicleListPage.xaml / .cs
        ├── Maintenances/
        │   ├── MaintenanceListPage.xaml / .cs
        │   ├── MaintenanceDetailPage.xaml / .cs
        │   └── MaintenanceCreatePage.xaml / .cs
        ├── Inventory/
        │   ├── InventoryListPage.xaml / .cs
        │   └── LotCreatePage.xaml / .cs
        └── Alerts/
            └── AlertListPage.xaml / .cs
```
