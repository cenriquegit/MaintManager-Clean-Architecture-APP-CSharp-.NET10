# Contexto de Sesión — MaintManager

> Última actualización: 2026-06-21
> Cargar este archivo al iniciar una nueva sesión de Kilo para restaurar el contexto completo.

---

## 0. Estado Rápido — Inicio de Sesión

```
API:   cd MaintManager.API && dotnet run --urls "http://0.0.0.0:5056"
MAUI:  cd MaintManager.MAUI && dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormats=apk
       adb install -r bin\Release\net10.0-android\publish\*-Signed.apk
Login: herror.ortiz / Admin2026! (Admin) | juan.quispe / Tecnico2026! (Técnico)
```

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

## 3. Bugs Corregidos (102 total)

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
| 92 | KM actual incorrecto | VehicleManagementController + AgendaController + VehicleRepository |
| 93 | Tipo servicio A/B siempre igual | SchedulingService + DetermineInitialServiceTypeAsync |
| 94 | SaveDiagnosis borra checklists | MaintenanceDetailViewModel (sin await Load) |
| 95 | Items mal clasificados (Acción/Componente) | CreateActionViewModel (Category solo "Acción") |
| 96 | Dashboard summary KPI en cero | BiReportService (statid='FI' SQL directo) |
| 97 | Gráficos BiDashboard vacíos | BiReportService (ORDER BY alias match) |
| 98 | Gráfico expiring lots vacío | BiDashboardViewModel (DaysUntilExpiry.HasValue) |
| 99 | InventoryListPage rota (Grid.Row) | InventoryListPage.xaml (Grid.Row fix) |
| 100 | InventoryListViewModel corrupto | InventoryListViewModel.cs (reescrito) |
| 101 | CreateMaterial siempre "Nuevo Material" | CreateMaterialViewModel (ApplyQueryAttributes type) |
| 102 | Reasignar Técnico visible no-admin | MaintenanceDetailViewModel (AuthService.IsAdmin) |

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
- Dashboard BI balanceado: emergencias variadas (20% global), lotes categorizados (crítico/próximo/normal), 7 servicios este mes
- Dashboard BI con x1000 fix (barras visibles en Costo/km)
- Dashboard BI restaurado a versión estable (LabelsRotation -20, sin LabelsPaint, sin DataLabels conflictivos)
- Reportes generados directamente (sin filter page), incluye Historial por Vehículo con prompt
- LOT auto-generado con formato "LOT-YYYY-MM-DD"
- Discard lot con opciones predefinidas (Vencimiento/Daño/Otro)
- Estado de Switch de vencimiento visible (ThumbColor gris)
- Low-stock endpoint reparado (`/materials/low-stock`)
- CloseOrder con `IsEmergencyComplete = false` para evitar 400

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
| `BiDashboardViewModel.cs` | Restaurado a versión estable c95af1c (LabelsRotation -20, sin LabelsPaint, x1000) |
| `ReportsViewModel.cs` | Reportes directos sin filter page, Historial con prompt vehicular |
| `ReportsPage.xaml` | CommandParameter Route→Type fix |
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

*Documento generado al cierre de sesión el 2026-05-15. Actualizado el 2026-05-18, 2026-05-21, y 2026-06-07 (VehicleConfig + VehicleManagement).*

## 11. Sesión 2026-06-07 — VehicleConfig + VehicleManagement

### Módulos nuevos
- **VehicleConfig**: CRUD de config por vehículo (acciones, materiales, componentes permitidos)
- **VehicleManagement**: Cards de vehículos, crear/editar con SUNARP opcional
- **DB**: `managed_vehicle`, `vehicle_allowed_action/material/component` (con mv_id + prcoid)
- **Scripts**: `07_vehicle_config.sql`, `08_managed_vehicle.sql`

### Bugs de esta sesión (79-91)
| # | Bug | Causa | Fix |
|---|-----|-------|-----|
| 79 | Crash al entrar Config | Faltaba FlyoutItem + xmlns | Agregado |
| 80 | Componentes no aparecen | `== "Componente"` vs "Componente Eléctrico" | `.Contains()` |
| 81 | Nav perdida Crear Material | Stack Inventory vs ConfigVehicle | Ruta `ConfigVehicle/CreateMaterial` |
| 82 | Datos crudos Picker | Record sin ToString() | Wrapper VehicleOption |
| 83 | Icono duplicado ⚙ | Dos páginas mismo ícono | 🚗 |
| 84 | Lista no refresca | IsBusy nesting ExecuteAsync | API manual + finally |
| 85 | Crash ColorAccent | StaticResource no existe | → ColorAccentDark |
| 86 | SUNARP no funcional | Anti-bot/reCAPTCHA | Documentado |
| 87 | Listas no cargan | SelectedVehicle dentro ExecuteAsync | Fuera |
| 88 | Sin botón volver | FlyoutItem separado | BackToVehicles |
| 89 | Editar campos vacíos | IQueryAttributable en VM no en Page | Page lo implementa |
| 90 | Filtros no funcionan | OnSelectedSourceChanged ausente | Agregado |
| 91 | UI cards feas | Badge padding, poca info | Mejorado |

### Bug pendiente (#92)
Filtro en MaintenanceDetail — los selects de acciones/materiales siguen mostrando TODO aunque el vehículo tenga config. 7 intentos fallidos (server-side LINQ, client-side filter, debug logs). El handler `LoadComponentActionsAsync` parece no ejecutar correctamente el flujo de datos.

### Lecciones críticas
1. **DataTemplates**: NO `AncestorType`, usar TapGestureRecognizer + code-behind
2. **ExecuteAsync**: NUNCA anidar, el `if(IsBusy)return` bloquea todo
3. **Shell rutas**: Máximo 2 niveles (FlyoutItem/SubRoute)
4. **IQueryAttributable**: Va en ContentPage, no en ViewModel
5. **StaticResources**: Verificar existencia antes de usar en XAML
6. **SUNARP**: Scraper HTTP no viable, requiere navegador real
7. **Config tables**: Backfill mv_id crea dual IDs, mejor unificar

### Navegación actualizada
```
//Vehicles → VehicleManagementPage (Admin only)
  ///CreateVehicle → Crear/Editar
//ConfigVehicle → VehicleConfigPage (FlyoutItem oculto)
  ///CreateAction → Nueva acción
  ///CreateComponent → Nuevo componente
  ///CreateMaterial → Nuevo material
```

### Archivos nuevos (42)
Domain: ManagedVehicle, VehicleAllowedAction/Material/Component (mod), ISunarpService, IManagedVehicleRepository, IVehicleConfigRepository (mod)
Infra: ManagedVehicleConfig/Repo, SunarpService, 3 config EF (mod), VehicleConfigRepo (mod), FleetMaintenanceContext (mod)
App: VehicleConfigService (mod), VehicleConfigResponse
API: VehicleConfigController, VehicleManagementController, Program.cs (mod), MaintenancesController (mod), InventoryController (mod)
MAUI: VehicleConfigPage/VM, VehicleManagementPage/VM, CreateVehiclePage/VM, CreateActionPage/VM, CreateComponentPage/VM, AppShell (mod), MauiProgram (mod)
DB: 07_vehicle_config.sql, 08_managed_vehicle.sql

## 12. Sesión 2026-06-21 — Correcciones Integrales + Checklist Redesign + Unificación Material/Componente + NeoCar Rebrand

### Plan aplicado: `.kilo/plans/correcciones-integrales.md` (14 errores en 6 fases)

### FASE 1 — Regresiones críticas
| Error | Descripción | Fix |
|-------|------------|-----|
| 1-2 | Botón Cancelar duplicado y mal posicionado en MaintenanceDetailPage | Eliminado "Cancelar Orden" de DataTemplate, movido a `Shell.TitleView`. Botón "Cerrar Orden" duplicado eliminado. Grid bottom fix (2 cols). |

### FASE 2 — Datos incorrectos
| Error | Descripción | Fix |
|-------|------------|-----|
| 3 (Bug #92) | KM actual no considera mantenimientos cerrados | `VehicleManagementController.GetAll`: query adicional a Maintenances (statid='FI'), `Math.Max`. `AgendaController.GetCurrentKm`: mismo fix. `VehicleRepository.GetCurrentKmAsync`: `MAX(mileage, rentalKm, maintKm)`. |
| 4 (Bug #93) | Tipo servicio A/B siempre sugiere el mismo | `SchedulingService.CreateScheduleAsync`: `DetermineInitialServiceTypeAsync` consulta último mantenimiento. |

### FASE 3 — Funcionalidad faltante
| Error | Descripción | Fix |
|-------|------------|-----|
| 5 | VehicleHistory sin acceso desde Dashboard | `HomeViewModel.VehicleCard` + `Prcoid`. `NavigateToVehicleHistoryCommand`. TapGestureRecognizer en HomePage. |
| 6 | MaterialDetail no muestra lotes | `MaterialDetailViewModel.Load`: fetch `GET /inventory/materials/{id}/lots`. `LotItem` con Quantity, UnitCost, EntryDate, ExpirationDate. |
| 7 | SupplierName ausente en LotCreate | `LotCreateRequest` + `Supername`. `LotCreateViewModel` + property. XAML + Entry. |
| 8 | Auto-fill material al crear lote desde MaterialDetail | `LoadMaterials`: auto-select `_presetMateid`. `IsMaterialPreset` + `PresetMaterialName`. XAML: picker vs label fijo. |

### FASE 4 — BI Dashboard fixes
| Error | Descripción | Fix |
|-------|------------|-----|
| 9 (Bug #96) | Dashboard summary KPI en cero | `GetDashboardSummaryAsync`: SQL directo con statid='FI' (no vw_bi_dashboard_summary). |
| 10 (Bug #97) | Gráficos CostPerKm y EmergencyRate vacíos | `ORDER BY` cambiado a `"CostPerKm"` y `"EmergencyRatePercent"` (matching alias). |
| 11 | Compliance sin colores condicionales | `BuildComplianceChart`: color por barra (Puntual=verde, Anticipado=naranja, Tardío=rojo). Leyenda en XAML. |
| 12 | KPI "Lotes por Vencer" no visible en Home | `HomeViewModel`: 5° KPI. `HomePage.xaml`: grid 2×3. |

### FASE 5 — Rendimiento
| Error | Descripción | Fix |
|-------|------------|-----|
| 13 | MaintenanceList sin paginación (siempre 50 items) | `MaintenanceListViewModel`: `LoadMoreCommand`, `HasMorePages`, `CurrentPage`, `IsLoadingMore`. XAML: botón "Cargar más..." en CollectionView.Footer. PageSize=30. |

### FASE 6 — Build verification
- API: 0 errores. MAUI: timeout normal en build.

### Checklist Redesign (sustituye Pickers + botón "+")
- **ViewModel**: `ActionChecklistItem`, `MaterialChecklistItem`, `ComponentChecklistItem` con `IsDone`/`IsNotDone` + `GroupKey` para RadioButtons mutuamente excluyentes. `PersistChecklistItemsAsync`: POST batch al cerrar orden.
- **XAML**: 3 secciones con RadioButton "Sí"/"No". Materiales: campo cantidad + origen (Stock propio/Externo) + rating (⭐1-5) + comentario. Componentes: campo cantidad. ReadOnly mode: oculta RadioButtons, muestra ✅/— o ✅ 4.5 Litros.
- Default: `IsNotDone = true`.

### Emergencia Completa vs Parcial
- `CloseOrder`: si es emergencia, `DisplayActionSheet` pregunta "¿Completa o Parcial?". Completa → recalendariza. Parcial → no.

### Stock propio vs Externo
- `ConsumeRequest` + `Origin`. `ConsumeStockFifoAsync` acepta `origin`. XAML: mini-picker "Stock propio"/"Externo".

### Unificación Material/Componente (Opción B)
- **DB**: `09_material_type.sql` → columna `type` en `maintenance.material` ('Material'/'Componente').
- **Domain**: `Material.Type`, `Material.Create(type:)`.
- **EF**: `InventoryConfiguration.cs` mapea `Type`.
- **DTOs**: `MaterialListItem`, `MaterialResponse`, `MaterialItemDto`, `MaterialCreateRequest` + `Type`.
- **API**: `InventoryController.GetMaterials` acepta `?type=`. `CreateMaterial` pasa `Type`.
- **MAUI**: `InventoryListViewModel` + tabs `SelectedTab`. `CreateMaterialViewModel` + `SelectedType`. `VehicleConfigViewModel.LoadConfig`: componentes desde `?type=Componente`. `MaintenanceDetailViewModel.LoadComponentChecklistAsync`: carga desde materials API.
- **Seed**: `10_seed_expiring_lots.sql` → 14 lotes con fechas variadas para visualizar gráfico.

### Rating de materiales
- `MaterialResponse` + `MaterialRatingInfo`. `InventoryMappings.ToResponse`: última calificación.
- `MaterialChecklistItem` + `Rating`, `RatingComment`, `RatingOptions`. Picker ⭐1-5 + Entry comentario.
- `PersistChecklistItemsAsync`: POST `/ratings` si Rating>0.
- `MaterialDetailPage`: muestra `⭐⭐⭐ 3/5 — Buen rendimiento` o "Sin calificaciones".

### Alertas — títulos informativos
- `AlertItem.ShortTitle`: compone `"Stock Bajo — Aceite Motor 5W-30"` o `"Componente Próximo — VDG-361"`.

### NeoCar Rebranding
- **Nombre**: `ApplicationTitle` → NeoCar, `ApplicationId` → com.neocar.app.
- **Icono**: Auto blanco con gotas de agua, fondo azul degradado, ruedas, faros.
- **Splash**: Mismo auto.
- **APK**: `adb install -r com.neocar.app-Signed.apk`.

### Archivos nuevos/modificados clave esta sesión
| Archivo | Cambio |
|---------|--------|
| `MaintenanceDetailPage.xaml` | Shell.TitleView Cancel button, checklist reemplaza pickers, header rediseño |
| `MaintenanceDetailViewModel.cs` | Checklist items, rating, origin, PersistChecklistItemsAsync, AuthService |
| `BiReportService.cs` | SQL directo statid='FI', ORDER BY fix |
| `BiDashboardViewModel.cs` | Compliance colors, Debug.WriteLine, nullable fix |
| `BiDashboardPage.xaml` | Compliance legend |
| `InventoryListViewModel.cs` | Tabs, SwitchToMaterials/SwitchToComponents |
| `InventoryListPage.xaml` | Tabs UI, Grid.Row fix, empty text dinámico |
| `CreateMaterialViewModel.cs` | SelectedType, Title adaptativo, MaterialDetail.Type |
| `CreateMaterialPage.xaml` | Type picker, labels genéricos |
| `VehicleConfigViewModel.cs` | Componentes desde materials, Add/Remove usa /materials |
| `MaintenanceWizardViewModel.cs` | IQueryAttributable, auto-select vehicle from Agenda |
| `Material.cs` | Type field, Create(type:) |
| `MaterialResponse.cs` | Type, MaterialRatingInfo |
| `MaterialCreateRequest.cs` | Type |
| `ConsumeRequest.cs` | Origin |
| `IInventoryService.cs` / `InventoryService.cs` | CreateMaterialAsync(type:), ConsumeStockFifoAsync(origin:) |
| `InventoryController.cs` | GetMaterials(?type=), CreateMaterial(Type) |
| `AlertItem.cs` | ShortTitle |
| `AlertListPage.xaml` | ShortTitle binding |
| `MaterialItem.cs` (Model) | Type |
| `09_material_type.sql` | ALTER TABLE material ADD type |
| `10_seed_expiring_lots.sql` | Lotes con fechas variadas |
| `.csproj` | NeoCar, com.neocar.app |
| `appicon.svg`, `appiconfg.svg`, `splash.svg`, `icon.svg` | NeoCar icon |
