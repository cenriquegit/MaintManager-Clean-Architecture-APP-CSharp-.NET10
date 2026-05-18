# Contexto de Sesión — MaintManager

> Última actualización: 2026-05-15
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
| LiveChartsCore.SkiaSharpView.Maui | 2.0.2 | Gráficos BI Dashboard |
| QuestPDF | Última | Exportación PDF |
| EF Core | 10.x | Con PostgreSQL (Npgsql) |
| PostgreSQL | 16+ | BD en localhost:5432, DB: neoplus_maintenance |

## 3. Bugs Corregidos (45 total)

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

## 4. Estado Actual del Proyecto

### ✅ Funcional
- Login + JWT + persistencia de sesión (SecureStorage)
- Panel principal con KPIs + acciones rápidas + flota
- Alertas (lista, marcar leídas, resolver, check automático)
- Calendario (vista mensual, filtros por vehículo/tipo/estado)
- Mantenimientos (lista paginada con búsqueda + filtros)
- Inventario (lista con búsqueda + stock bajo)
- BI Dashboard (5 gráficos con LiveChartsCore)
- Reportes (exportar Excel costo/km con Share dialog)
- Mi Perfil (info usuario + crear usuario si admin)
- Configuración (URL API editable + PIN 1234)
- Detalle de orden con exportación PDF vía Share
- Wizard de nueva orden (multi-step)

### ⚠️ En Progreso / Pendiente
- Steps 2-4 del wizard de nueva orden: pueden tener problemas de visualización en dispositivo físico
- Reportes "Órdenes de Mantenimiento" y "Alertas": muestran "no disponible" (no hay endpoint)
- Validación de datos en el wizard (campos requeridos antes de avanzar)
- Transacción en POST /api/v1/workers (Person + Worker en dos SaveChanges)
- Tema oscuro (RadioButtons placeholder en Settings)

### 🔴 Problemas Conocidos que Requieren Debug en Dispositivo
1. El botón "Siguiente" en el wizard ahora usa `IsNextButtonVisible`/`IsSaveButtonVisible` sin DataTrigger — verificar en físico
2. El CollectionView dentro del wizard (pasos 3-4) con Entry dentro de DataTemplate puede tener problemas de foco en Android
3. Si `Materials` y `Operations` están vacíos, los pasos 3-4 muestran CollectionView sin items — considerar mensaje de "sin datos"

## 5. Flujo de Navegación (Shell)

```
//Login (oculto del flyout)
//Dashboard → Panel principal
//Alerts → Alertas
//Calendar → Calendario
//Maintenances → Lista de mantenimientos
  /Detail?id={id} → Detalle de orden (sub-página)
  /Create → Nueva orden (sub-página)
//Inventory → Inventario
  /CreateLot → Ingresar lote (sub-página)
//BiDashboard → Dashboard BI (gráficos)
//Reports → Reportes
//Profile → Mi Perfil
//Settings → Configuración (PIN 1234)
```

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
- `GET /api/v1/maintenances?page=&pageSize=` → Lista paginada
- `GET /api/v1/maintenances/{id}` → Detalle
- `POST /api/v1/maintenances` → Crear (Admin/Técnico)
- `PUT /api/v1/maintenances/{id}/actions/{actionId}/complete` → Completar acción
- `POST /api/v1/maintenances/{id}/diagnosis` → Guardar diagnóstico
- `PUT /api/v1/maintenances/{id}/close` → Cerrar orden
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
| `AppShell.xaml` + `.cs` | Flyout personalizado con iconos + navegación por Tapped |
| `BaseViewModel.cs` | Timeout 30s en ExecuteAsync + IsEmpty=false en catch |
| `BiDashboardViewModel.cs` | 6 APIs secuenciales con try/catch individual |
| `BiDashboardPage.xaml` | 5 gráficos LiveChartsCore, sin x:DataType |
| `HomePage.xaml` | Acciones rápidas con Border + GestureRecognizer |
| `InventoryListPage.xaml` | Sticky footer, SearchBar con colores, CollectionView margin |
| `MaintenanceListPage.xaml` | Sticky footer, estados fuera del RefreshView |
| `MaintenanceWizardPage.xaml` | Wizard simplificado, pasos sin DataTrigger |
| `MaintenanceWizardViewModel.cs` | LoadVehicles con ApiResponse wrapper |
| `LotCreateViewModel.cs` | LoadMaterials con API real |
| `App.xaml` | FormPicker con TitleColor, estilos Label/Entry/Editor globales |
| `Resources/Styles/Colors.xaml` | Primary/Secondary/Tertiary azules (#1565C0) |
| `WorkersController.cs` | POST /api/v1/workers para crear usuarios |

## 10. Cómo Continuar — Prompt para Nueva Sesión

```markdown
He estado trabajando en el proyecto MaintManager (gestión de mantenimiento de flota vehicular).
La arquitectura es MAUI (.NET 10) + API .NET 10 + PostgreSQL con MVVM usando CommunityToolkit.Mvvm.

Lee el archivo KILO_SESSION_CONTEXT.md, BUGS_HISTORY.md y README.md para tener el contexto completo.

Proyecto en: C:\Users\carlo\Desktop\proyect\MaintManager

Estado actual:
- 45 bugs corregidos (ver BUGS_HISTORY.md)
- Login, Dashboard, Alertas, Calendario, Mantenimientos, Inventario funcionales
- BI Dashboard con 5 gráficos LiveChartsCore
- Nueva orden: wizard multi-paso (pasos 2-4 requieren verificación en físico)
- PDF y Excel export funcionando con Share dialog
- Menú hamburguesa personalizado con iconos
- Sesión persistente vía SecureStorage
- Configuración protegida con PIN 1234

Para continuar trabajando, necesito:
1. Leer el código actual
2. Tener el contexto de la última sesión
3. Continuar depurando los problemas pendientes
4. Haber leido cada archivo de cada carpeta o subcarpeta para que entiendas todo por completo.
5. haber leido y comprendido la bd completo,se tiene que leer los 3 archivos de forma secuencial y detalalda, entiendo todo por completo 
```

---

*Documento generado al cierre de sesión el 2026-05-15.*
