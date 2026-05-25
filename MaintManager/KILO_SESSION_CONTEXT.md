# Contexto de Sesión — MaintManager

> Última actualización: 2026-05-21
> Cargar este archivo al iniciar una nueva sesión de Kilo para restaurar el contexto completo.

---

## 1. Arquitectura del Proyecto

```
MaintManager.sln
├── MaintManager.MAUI          → App móvil (MAUI + MVVM + CommunityToolkit.Mvvm)
│   ├── Views/                 → XAML pages (Auth, Dashboard, Alerts, Calendar, Inventory, Maintenances, Reports, BiDashboard, Settings, Profile)
│   ├── ViewModels/            → ViewModels con [ObservableProperty] y [RelayCommand]
│   ├── Services/              → ApiService, AuthService
│   ├── Models/                → MaterialItem, MaterialOption, VehicleOption, etc.
│   ├── Converters/            → AllConverters.cs (IntToBool, StringEquals, InvertedBool, IsNotNull, etc.)
│   ├── Resources/
│   │   ├── Styles/Colors.xaml → Paleta Primary = #1565C0 (azul corporativo)
│   │   ├── Styles/Styles.xaml → Estilos globales MAUI
│   ├── App.xaml               → Recursos globales + colores personalizados
│   ├── AppShell.xaml          → Shell + FlyoutContentTemplate personalizado
│   └── MauiProgram.cs         → DI + LiveChartsCore + SkiaSharp
│
├── MaintManager.API           → Backend .NET 10
│   ├── Controllers/           → Auth, Vehicles, Maintenances, Inventory, Alerts, Reports, Workers
│   ├── Program.cs             → Middleware, JWT, CORS, Swagger, QuestPDF license
│   └── Middleware/             → GlobalExceptionMiddleware
│
├── MaintManager.Application  → DTOs, Validadores FluentValidation
├── MaintManager.Domain        → Entidades (Worker, Person, Vehicle, Maintenance, etc.)
├── MaintManager.Infrastructure → EF Core FleetMaintenanceContext, BiReportService (SQL raw)
└── MaintManager.Shared        → Constantes de rutas API, modelos compartidos
```

## 2. Stack Tecnológico

| Componente | Versión | Notas |
|-----------|---------|-------|
| .NET | 10.0 | target net10.0-android + net10.0-windows |
| MAUI | 10.0.0 | Forzado en csproj |
| CommunityToolkit.Mvvm | 8.4.0 | Source generators |
| LiveChartsCore.SkiaSharpView.Maui | 2.1.0-dev-570 | Gráficos BI Dashboard + UseSkiaSharp() |
| QuestPDF | Última | Exportación PDF |
| EF Core | 10.x | Con PostgreSQL (Npgsql) |
| PostgreSQL | 16+ | BD en localhost:5432, DB: neoplus_maintenance |

## 3. Bugs Corregidos (78 total)

Ver `BUGS_HISTORY.md` para detalle completo. Resumen de los más críticos:

| # | Bug | Fix Clave |
|---|-----|-----------|
| 6 | Crash al navegar al Dashboard | KpiItems inicializado con placeholders |
| 13 | SQL column casing en BiReportService | Alias `AS "PascalCase"` en queries raw |
| 24 | Crash por `x:Static` con `assembly=netstandard` | Reemplazado por binding a ViewModel |
| 25 | Crash por `x:DataType` con tipos anidados `+` | Eliminado `x:DataType` de DataTemplates + pages |
| 39 | BI Dashboard crash con LiveChartsCore | Eliminado `x:DataType` de BiDashboardPage |
| 40 | Ingresar lote: Mateid falsos | LoadMaterials ahora llama API real |
| 45+ | Texto blanco en fondo blanco | Estilos globales Label/Entry/Editor con ColorTextPrimary fijo |
| 62 | 415 Content-Type en POST/PUT | StringContent → PostAsJsonAsync |
| 63 | Sesión no expira tras 8h | Verificación de ExpiresAt en TryRestoreSessionAsync |
| 64 | Wizard pasos 2-4 sin campos | RadioButtons → Picker, CollectionView → BindableLayout |
| 65 | Flyout borders duplicados | BoxView como borde inferior único |
| 66 | Ingreso lote error genérico | catch(HttpRequestException) con mensaje real del servidor |
| 67 | BI Dashboard crash AOT (series vacías) | Series/Axis `[]` → `null` |
| 68 | Startup crash AOT (data.GetType) | `JsonSerializer.Serialize` → `JsonContent.Create` |
| 69 | Namespace conflict ApiResponse<T> | Eliminado de Shared, eliminados duplicados de Application |
| 70 | DTOs duplicados Application/MAUI | Movidos `MaintenanceCreateRequest`, `LotCreateRequest`, `LoginResponse` a Shared/Models |
| 71 | BI Dashboard crash CPURenderMode | UseSkiaSharp() + LiveChartsCore 2.1.0-dev-570 |
| 72 | Shell routing relativo en .NET 10 | Navegaciones `///` absolutas |
| 73 | DetailPage no carga datos | GetAsync<ApiResponse<T>> wrapper |
| 74 | Actions vs actionDetails mismatch | [JsonPropertyName("actionDetails")] |
| 75 | OilInfo anidado vs API plana | Propiedades planas en lugar de OilInfo |
| 76 | CloseOrder sin body (400) | PutAsync con new { } |
| 77 | SwipeUpClean en dispositivo low-memory | Documentado, no requiere fix |
| 78 | RateMaterial crash MaterialMateid | HasOne<Material>().WithMany() explícito |
| F3 | Layout DetailPage rediseñado | Cards secuenciales + input/lista + ✕ |
| F4 | Solo lectura orden FI | IsReadOnly bindeado a inputs/visibilidad |
| F5 | PDF con datos completos | Vehicle, materials, components incluidos |
| F6 | Buffering local acciones + POST /actions | Persiste acciones pendientes al guardar |
| F7 | Endpoints DELETE items | 3 endpoints para acciones, materiales, componentes |
| F8 | Diagnóstico campos completos | Picker GeneralStatus, Switch operative, Editor recommendations |
| SD | Seed data componentes + materiales | action_catalog + material + lots nuevos |

## 4. Estado Actual del Proyecto

### ✅ Funcional
- Login + JWT + persistencia de sesión con expiración (SecureStorage + 8h)
- Panel principal con KPIs + acciones rápidas + flota
- Alertas (lista, marcar leídas, resolver, check automático)
- Calendario (vista mensual, filtros por vehículo/tipo/estado)
- Mantenimientos (lista paginada con búsqueda + filtros)
- Inventario (lista con búsqueda + stock bajo + ingreso lote con materiales reales)
- BI Dashboard (5 gráficos con LiveChartsCore v2.1.0-dev-570 + UseSkiaSharp)
- Reportes (exportar Excel costo/km con Share dialog)
- Mi Perfil (info usuario + crear usuario si admin)
- Configuración (URL API editable + PIN 1234)
- Detalle de orden con exportación PDF vía Share (PDF con datos completos: vehículo, servicios, acciones, materiales, componentes, diagnóstico)
- Cerrar orden (PUT con body, solo en estado AC)
- Wizard de nueva orden (4 pasos, guardado con PostAndUnwrapAsync<int>)
- POST/PUT con Content-Type correcto (JsonContent.Create, AOT-compatible)
- Menú flyout con bordes inferiores limpios, sin duplicación
- DTOs compartidos en `Shared/Models` para request/response concretos (AOT compatible)
- Sesión con expiración calculada localmente (no depende del API)
- Navegación Shell con rutas absolutas `///` (compatible .NET 10)
- DetailPage con layout cards secuenciales (InfoGeneral → Acciones → Consumo → Componentes → Reasignar → Diagnóstico)
- Botón ✕ para eliminar items de cada lista (acciones, materiales, componentes)
- Acciones agregadas localmente y persistidas batch al guardar diagnóstico
- Picker de acciones filtrado por categoría (Acción/Componente)
- Cambio de aceite inline en header con fallback "No hay información"
- Solo lectura para órdenes finalizadas (FI): inputs deshabilitados, botones ocultos
- Exportar PDF visible solo en orden finalizada
- Diagnóstico completo con Picker (GeneralStatus), Switch (VehicleOperative), Editor (Observations, FutureRecommendations)
- Endpoints POST /actions y DELETE /actions, /materials, /components
- Seed data: componentes con vida útil (Batería 1095d, Neumáticos 50000km, etc.), 10 acciones checklist, materiales nuevos con lotes
- Rating de materiales guardado localmente, enviado batch al guardar diagnóstico
- Acciones rápidas role-based (Admin → Create/Lot directo, Mecánico → lista)
- Alertas: historial de resueltas con Switch "Mostrar resueltas"
- Dashboard BI poblado con datos reales (124 órdenes, 16 vehículos con costos)

### ⚠️ En Progreso / Pendiente
- Reportes "Órdenes de Mantenimiento" y "Alertas": muestran "no disponible" (no hay endpoint)
- Validación de datos en el wizard (campos requeridos antes de avanzar)
- Transacción en POST /api/v1/workers (Person + Worker en dos SaveChanges)
- Tema oscuro (RadioButtons placeholder en Settings)

### 🔴 Problemas Conocidos que Requieren Debug en Dispositivo
1. Si `Materials` retorna vacío desde la API, el paso 3 se ve sin items — depende de que existan materiales en BD
2. RateMaterial puede devolver 400 por shadow FK (fijo en código, requiere rebuild APK)
3. Sesión 8h: verificar en dispositivo que no pida login antes de tiempo

## 5. Flujo de Navegación (Shell)

```
//Login (oculto del flyout)
//Dashboard → Panel principal
//Alerts → Alertas
//Calendar → Calendario
//Maintenances → Lista de mantenimientos
  ///Detail?id={id} → Detalle de orden (sub-página, ruta absoluta)
  ///Create → Nueva orden (sub-página, ruta absoluta)
//Inventory → Inventario
  ///CreateLot → Ingresar lote (sub-página)
//BiDashboard → Dashboard BI (gráficos)
//Reports → Reportes
//Profile → Mi Perfil
//Settings → Configuración (PIN 1234)
```

> **Importante:** En .NET 10, las sub-rutas de FlyoutItems SOLO funcionan con prefijo `///` (ruta absoluta). Las rutas relativas como `Maintenances/Detail` lanzan excepción.

## 6. Colores Corporativos

| Clave | Valor | Uso |
|-------|-------|-----|
| `ColorPrimary` | `#1565C0` | Botones, acentos, header flyout |
| `Primary` | `#1565C0` | MAUI built-in accent color |
| `ColorPrimaryDark` | `#0D47A1` | Variante oscura |
| `ColorBackground` | `#F5F5F5` | Fondo de páginas |
| `ColorSurface` | `#FFFFFF` | Fondo de cards |
| `ColorTextPrimary` | `#212121` | Texto principal |
| `ColorError` | `#C62828` | Errores, alertas |
| `ColorSuccess` | `#2E7D32` | Éxito, completado |

## 7. API Endpoints

### Públicos
- `POST /api/v1/auth/login` → JWT

### Autenticados (requieren JWT)
- `GET /api/v1/vehicles` → Lista de vehículos
- `GET /api/v1/vehicles/{id}` → Detalle vehículo
- `GET /api/v1/vehicles/{id}/current-km` → KM actual
- `GET /api/v1/vehicles/{id}/schedule` → Programación

### Mantenimientos (requieren JWT)
- `GET /api/v1/maintenances?page=&pageSize=&status=` → Lista paginada con filtro
- `GET /api/v1/maintenances/{id}` → Detalle completo
- `POST /api/v1/maintenances` → Crear (Admin/Técnico)
- `PUT /api/v1/maintenances/{id}/actions/{actionId}/complete` → Completar acción
- `POST /api/v1/maintenances/{id}/diagnosis` → Guardar diagnóstico
- `PUT /api/v1/maintenances/{id}/assign` → Reasignar técnico
- `POST /api/v1/maintenances/{id}/consume` → Consumir material
- `POST /api/v1/maintenances/{id}/components` → Instalar componente
- `PUT /api/v1/maintenances/{id}/close` → Cerrar orden (requiere body `{ isEmergencyComplete: bool? }`)
- `GET /api/v1/maintenances/stats` → Estadísticas rápidas
- `GET /api/v1/maintenances/actions/catalog` → Catálogo de acciones
- `GET /api/v1/vehicles/{id}/maintenances` → Mantenimientos por vehículo

### Inventario (requieren JWT)
- `GET /api/v1/inventory/materials` → Lista materiales
- `GET /api/v1/inventory/materials/{id}` → Detalle material
- `POST /api/v1/inventory/materials` → Crear material (Admin)
- `POST /api/v1/inventory/materials/{id}/lots` → Ingresar lote (Admin)
- `POST /api/v1/inventory/lots/{lotId}/discard` → Descartar lote (Admin)
- `GET /api/v1/inventory/low-stock` → Stock bajo
- `GET /api/v1/inventory/expiring-lots?days=` → Lotes por vencer
- `POST /api/v1/inventory/materials/{mateid}/ratings` → Calificar material (Admin)

### Alertas (requieren JWT)
- `GET /api/v1/alerts` → No resueltas
- `PUT /api/v1/alerts/{id}/read` → Marcar leída
- `PUT /api/v1/alerts/{id}/resolve` → Resolver (Admin)
- `POST /api/v1/alerts/check` → Verificar alertas (Admin)

### Reportes (requieren JWT)
- `GET /api/v1/reports/dashboard` → Resumen KPIs
- `GET /api/v1/reports/cost-per-km` → Costo por km (Admin)
- `GET /api/v1/reports/emergency-rate` → Tasa emergencia (Admin)
- `GET /api/v1/reports/monthly-cost?months=` → Costos mensuales (Admin)
- `GET /api/v1/reports/calendar-compliance` → Cumplimiento calendario (Admin)
- `GET /api/v1/reports/maintenances/{id}/pdf` → Exportar PDF
- `GET /api/v1/reports/cost-excel` → Exportar Excel (Admin)

### Trabajadores (requieren JWT)
- `GET /api/v1/workers/technicians` → Técnicos disponibles
- `POST /api/v1/workers` → Crear trabajador (Admin)

## 8. BD PostgreSQL — Esquema `maintenance` y `public`

Vistas clave para BI:
- `maintenance.vw_bi_dashboard_summary` → KPIs del dashboard
- `maintenance.vw_cost_per_km` → Costo por km por vehículo
- `maintenance.vw_emergency_rate` → Tasa de emergencia
- `maintenance.vw_monthly_cost` → Costos mensuales
- `maintenance.vw_calendar_compliance` → Cumplimiento de cronograma

Tablas principales:
- `public.worker` + `public.person` → Usuarios
- `public.job` → Roles
- `product.vehicle` + `public.product` → Vehículos
- `maintenance.maintenance` + `maintenance.maintenance_type` + `maintenance.service_type` → Órdenes
- `maintenance.material` + `maintenance.lot` → Inventario
- `maintenance.alert_log` + `maintenance.alert_config` → Alertas

## 9. Archivos Clave Modificados Recientemente

| Archivo | Último Cambio |
|---------|---------------|
| `Shared/Models/*` | 6 nuevos DTOs compartidos |
| `ApiService.cs` | JsonContent.Create (AOT) + TryRestoreSessionAsync |
| `AuthService.cs` | LoginResponse desde Shared.Models |
| `BiDashboardViewModel.cs` | Series/Axis `[]` → `null` |
| `MaintenanceWizardViewModel.cs` | PostAndUnwrapAsync + ruta `///` |
| `MaintenanceListViewModel.cs` | Rutas `///` |
| `MaintenanceDetailViewModel.cs` | IsReadOnly, ConsumedMaterials, ActionCatalogItems, AddAction/RemoveAction, PersistPendingActionsAsync, GeneralStatus picker, VehicleOperative switch, FutureRecommendations, ActionDetailItem `[JsonPropertyName]`, Rating local |
| `MaintenanceDetailPage.xaml` | Layout cards, merge aceite, ✕ oculto en FI, checkbox oculto en FI, `x:DataType` |
| `MaintenancesController.cs` | POST /actions, DELETE /actions, /materials, /components |
| `ReportsController.cs` | PDF con datos completos de vehículo/materiales/componentes |
| `MaintenanceService.cs` | CreateActionAsync |
| `IMaintenanceService.cs` | CreateActionAsync |
| `AddActionRequest.cs` | Nuevo DTO |
| `ApiRoutes.cs` | CreateAction, Alerts.GetHistory |
| `InventoryConfiguration.cs` | Fix relación MaterialRating→Material |
| `InventoryService.cs` | Debug logging RateMaterial |
| `MaintenanceRepository.cs` | GetByVehicleAsync sin filtro Statid |
| `IAlertRepository.cs` | GetResolvedAlertsAsync |
| `AlertRepository.cs` | GetResolvedAlertsAsync |
| `AlertsController.cs` | GET /alerts/history endpoint |
| `AlertListViewModel.cs` | ShowResolved Switch, merge de alertas |
| `AlertListPage.xaml` | Switch "Mostrar resueltas" en header |
| `HomeViewModel.cs` | AuthService inyectado, quick actions role-based |
| `CalendarViewModel.cs` | Fix ruta + parámetro mainid |
| `InventoryListViewModel.cs` | 3 rutas `///` sin try-catch |
| `database/04_seed_components_materials.sql` | Componentes vida útil + acciones + lotes |
| `database/05_seed_dashboard_data.sql` | Dashboard seed (consumos, emergencia, lotes) |
| `database/06_seed_massive_data.sql` | Mega seed (124 órdenes, 119 consumos, etc.) |
| `AppShell.xaml` | Flyout borders limpios |
| `MauiProgram.cs` | UseSkiaSharp() + UseLiveCharts() |

## 10. Cómo Continuar — Prompt para Nueva Sesión

```markdown
He estado trabajando en el proyecto MaintManager (gestión de mantenimiento de flota vehicular).
La arquitectura es MAUI (.NET 10) + API .NET 10 + PostgreSQL con MVVM usando CommunityToolkit.Mvvm.

Lee el archivo KILO_SESSION_CONTEXT.md, BUGS_HISTORY.md y README.md para tener el contexto completo.

Proyecto en: C:\Users\carlo\Desktop\proyect\MaintManager

Estado actual:
- 77 bugs corregidos (ver BUGS_HISTORY.md)
- Login, Dashboard, Alertas, Calendario, Mantenimientos, Inventario funcionales
- BI Dashboard con 5 gráficos LiveChartsCore v2.1.0-dev-570 + UseSkiaSharp (estable en AOT)
- Wizard multi-paso (4 pasos funcionales, guardado con PostAndUnwrapAsync<int>)
- Detalle de orden con acciones, diagnóstico, componentes, consumo materiales, cierre
- PDF y Excel export funcionando con Share dialog
- Menú hamburguesa personalizado con iconos, bordes inferiores limpios
- Sesión persistente vía SecureStorage con expiración a las 8h (calculada localmente)
- Configuración protegida con PIN 1234
- POST/PUT con JsonContent.Create (AOT-compatible)
- Navegación Shell con rutas absolutas `///` (compatible .NET 10)
- Ingreso lote con materiales reales + catch con mensaje real del servidor
- DTOs compartidos en `Shared/Models` (request/response concretos, AOT compatibles)
- API: fallback de AssignedTo cuando se envía 0

Para continuar trabajando, necesito:
1. Leer el código actual
2. Tener el contexto de la última sesión
3. Continuar depurando los problemas pendientes
```

---

*Documento generado al cierre de sesión el 2026-05-15. Actualizado el 2026-05-18 y 2026-05-21.*
