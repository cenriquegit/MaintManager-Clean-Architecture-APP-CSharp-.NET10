# PROXIMOS_PASOS.md — Plan de acción detallado

> **Proyecto:** MaintManager — Sistema de Gestión de Mantenimiento Vehicular con Business Intelligence
> **Fecha del análisis:** 2026-05-13
> **Última actualización:** 2026-06-05 (dashboard fix + reports directo)
> **Analista:** Kilo (asistente IA)

---

## Progreso — Correcciones aplicadas

### Sesión 2026-05-13 (Backend 10 + Frontend ~35)

### Sesión 2026-05-18 (6 correcciones iniciales)
| # | Corrección | Archivos | Estado |
|---|-----------|----------|--------|
| 1 | **ApiService: StringContent → PostAsJsonAsync** — Bug #62: 415 Content-Type | `ApiService.cs` | ✅ |
| 2 | **Sesión con expiración 8h** — Bug #63 | `AuthService.cs`, `ApiService.cs` | ✅ |
| 3 | **Wizard pasos 2-4 funcionales** — Bug #64 | `MaintenanceWizardPage.xaml`, `MaintenanceWizardViewModel.cs` | ✅ |
| 4 | **Flyout borders limpios** — Bug #65 | `AppShell.xaml` | ✅ |
| 5 | **Ingreso lote: error con detalle** — Bug #66 | `LotCreateViewModel.cs` | ✅ |
| 6 | **--urls con doble guion** | `README.md` | ✅ |

### Sesión 2026-05-21 (Refactor DetailPage — 8 fases)
| # | Fase | Archivos | Estado |
|---|------|----------|--------|
| 1 | **Fix RateMaterial crash** — Bug #78: shadow FK MaterialMateid | `InventoryConfiguration.cs` | ✅ |
| 2 | **Seed data** — Componentes vida útil + acciones checklist + materiales/lotes | `database/04_seed_*.sql` | ✅ |
| 3 | **Rediseño layout DetailPage** — Cards secuenciales, input+lista, ✕, merge aceite | `DetailPage.xaml`, `DetailViewModel.cs` | ✅ |
| 4 | **Solo lectura orden FI** — IsReadOnly, inputs disabled, botones condicionales | `DetailPage.xaml`, `DetailViewModel.cs` | ✅ |
| 5 | **PDF con datos completos** — Vehículo, materiales, componentes, recomendaciones | `ReportsController.cs` | ✅ |
| 6 | **Buffering acciones + POST /actions** — PendingActions persistidas al guardar | `MaintenancesController.cs`, `MaintenanceService.cs`, `DetailViewModel.cs` | ✅ |
| 7 | **Endpoints DELETE** — 3 endpoints para eliminar items | `MaintenancesController.cs` | ✅ |
| 8 | **Diagnóstico campos completos** — Picker, Switch, Editor, display completo | `DetailPage.xaml`, `DetailViewModel.cs` | ✅ |

### Build
| Proyecto | Resultado |
|----------|-----------|
| `MaintManager.API` -c Release | ✅ Compilación exitosa (0 errores, 2 warnings NuGet) |
| `MaintManager.MAUI` net10.0-android Debug | ✅ Compilación exitosa (0 errores) |
| `MaintManager.Infrastructure` | ✅ Compilación exitosa (0 errores) |

---

## Resumen del análisis forense

Se revisaron **todos los archivos fuente** de las 6 capas del proyecto (Domain, Application, Infrastructure, API, Shared, MAUI), totalizando **+90 archivos .cs/.xaml/.csproj**. Se detectaron:

- **6 errores críticos** que impiden el funcionamiento correcto del sistema
- **12 errores/brechas importantes** que causan fallos parciales o inconsistencias
- **8 mejoras arquitectónicas/UX necesarias**
- **Frontend MAUI: 0% completo** respecto al diseño especificado en README.md (solo 4 pantallas de ~15 requeridas)

---

## FASE 1 — Corrección de errores del backend (prioridad: crítica)

### 1.1 [CRÍTICO] MaintenanceService.SaveDiagnosisAsync — Diagnóstico nunca se persiste

**Dónde:** `MaintManager.Application/Services/MaintenanceService.cs:85-89`

**Problema:** Se crea la entidad `Diagnosis.Create(...)` pero **nunca se agrega al DbContext**. El objeto se crea y descarta. El diagnóstico nunca se guarda en BD.

**Cómo está actualmente:**
```csharp
var diagnosis = Diagnosis.Create(mainid, generalStatus, vehicleOperative,
    observations, futureRecommendations);
// ← FALTA: agregar al repositorio
await _maintenanceRepo.SaveChangesAsync(ct); // No hay nada que guardar
```

**Cómo debería ser:** Agregar el diagnosis al contexto antes de SaveChanges. Se necesita un método en IMaintenanceRepository o usar IGenericRepository<Diagnosis>.

**Plan:** Agregar `Task AddDiagnosisAsync(Diagnosis diagnosis, CancellationToken ct)` a `IMaintenanceRepository`, implementarlo en `MaintenanceRepository`, y llamarlo en el servicio.

---

### 1.2 [CRÍTICO] AuthService.LoginAsync — No deserializa ApiResponse<T> correctamente

**Dónde:** `MaintManager.MAUI/Services/AuthService.cs:15-24`

**Problema:** El endpoint `POST /api/v1/auth/login` devuelve `{ success: true, data: { token: "...", username: "...", ... } }` (wrapped en `ApiResponse<LoginResponse>`). Pero `AuthService.LoginAsync` intenta deserializar directamente como `LoginResponse` (plano: `{ token }`). Resultado: `Token` es `null`, login falla siempre.

**Cómo está actualmente:**
```csharp
var response = await _apiService.PostAsync<LoginResponse>("api/v1/auth/login", request);
if (response is not null && !string.IsNullOrEmpty(response.Token))
```

**Cómo debería ser:** Crear un DTO intermedio o modificar `ApiService.HandleResponse` para unwrappear automáticamente `ApiResponse<T>`.

**Plan:** Crear un método `PostAndUnwrapAsync<T>` en `ApiService` que deserialice `ApiResponse<T>` y devuelva `.Data`. Reemplazar en `AuthService.LoginAsync`.

---

### 1.3 [CRÍTICO] App.xaml no mergea Resources/Styles/ — Estilos rotos en MAUI

**Dónde:** `MaintManager.MAUI/App.xaml`

**Problema:** `App.xaml` define estilos inline pero **no incluye** los archivos `Resources/Styles/Colors.xaml` y `Resources/Styles/Styles.xaml` como `MergedDictionaries`. Estos archivos contienen recursos como `Card`, `DefaultShadow`, `BadgeError`, `ColorBorderLight` que son referenciados en todas las páginas XAML. Las páginas van a lanzar `XamlParseException` en tiempo de ejecución.

**Cómo está actualmente:** Solo define `ResourceDictionary` inline, sin merges.

**Cómo debería ser:** Agregar `ResourceDictionary.MergedDictionaries` que incluya los archivos de estilos.

---

### 1.4 [CRÍTICO] AlertListViewModel usa IsLoading que no existe

**Dónde:** `MaintManager.MAUI/Views/Alerts/AlertListPage.xaml` (líneas 11, 15)

**Problema:** El XAML referencia `{Binding IsLoading}` en `RefreshView.IsRefreshing` y en `VerticalStackLayout.IsVisible`. Pero `AlertListViewModel` solo hereda `IsBusy` de `BaseViewModel`. No existe `IsLoading`. El binding falla silenciosamente.

**Además:** `LotCreateViewModel` define su propio `IsLoading` (duplicando estado que debería venir de `BaseViewModel`).

**Plan:** Agregar `[ObservableProperty] private bool _isLoading` a `BaseViewModel` o reemplazar todos los usos de `IsLoading` por `IsBusy` en los XAML.

---

### 1.5 [ALTO] MaintenanceRepository.GetLastByVehicleAsync filtra por Statid == "AC"

**Dónde:** `MaintManager.Infrastructure/Repositories/MaintenanceRepository.cs:33-37`

**Problema:** `GetLastByVehicleAsync` filtra `m.Statid == "AC"`. Cuando se cierra un mantenimiento (Statid = "FI"), el método no lo encuentra. En `MaintenanceService.CreateAsync` se usa para calcular `kmSinceLast`. Si no hay maintenance "AC", `kmSinceLast` queda null.

**Cómo debería ser:** Incluir también "FI" o eliminar el filtro de status para este método específico, ya que se necesita el último mantenimiento independientemente de su estado.

---

### 1.6 [ALTO] ReportsController bypasea la capa Application/Domain

**Dónde:** `MaintManager.API/Controllers/ReportsController.cs`

**Problema:** `ReportsController` inyecta `FleetMaintenanceContext` directamente y ejecuta SQL raw, violando Clean Architecture. `IBiReportService` existe en Domain pero nunca se implementa. No hay lógica de negocio ni manejo de errores centralizado.

**Plan:** Implementar `BiReportService` en Application que encapsule las consultas SQL raw y use `IMapper` o proyecciones. ReportsController debe usar `IBiReportService`.

---

### 1.7 [ALTO] Mapeo inconsistente: progress report estados "FI" vs "AC"

**Dónde:** Varios lugares en MaintenanceRepository y MaintenanceService.

**Problema:** `GetPagedListItemsAsync` filtra `m.Statid == "AC"`, ocultando mantenimientos finalizados. El dashboard BI y el calendario deben mostrar tanto activos como finalizados.

**Plan:** Agregar sobrecargas o parámetros para filtrar por estado, con valor por defecto que incluya ambos.

---

### 1.8 [MEDIO] Duplicate RuntimeIdentifiers en MAUI csproj

**Dónde:** `MaintManager.MAUI/MaintManager.MAUI.csproj:40` y `:52`

**Problema:** `<RuntimeIdentifiers>android-arm64</RuntimeIdentifiers>` aparece dos veces. La segunda sobrescribe, pero el duplicado puede causar advertencias.

---

### 1.9 [MEDIO] ErrorMessages.Inventory.LotExpired no coincide con lógica de dominio

**Dónde:** `MaintManager.Shared/Constants/ErrorMessages.cs:34`, usado en `InventoryService.cs:84`

**Problema:** El mensaje dice "El lote seleccionado está vencido." pero la validación también rechaza lotes con estado "descartado" (no solo vencidos).

---

### 1.10 [MEDIO] IBiReportService nunca se registra en DI ni se implementa

**Dónde:** `MaintManager.Domain/Interfaces/Services/IBiReportService.cs`

**Problema:** Interfaz definida pero sin implementación. ReportsController la bypasea por completo.

---

### 1.11 [MEDIO] SaveDiagnosisAsync elimina línea sin efecto pero no soluciona el bug

**Dónde:** `MaintenanceService.cs:88`

**Comentario:** La línea `maintenance.ActionDetails.GetType();` se eliminó pero el problema real (falta de persistencia) sigue presente.

---

## FASE 2 — Completar Frontend MAUI (prioridad: alta)

### Estado actual (4 pantallas de ~15):
| Pantalla | Estado | Archivos |
|----------|--------|----------|
| Login | ✅ Implementado | LoginPage.xaml/.cs, LoginViewModel.cs |
| Alertas | ✅ Implementado | AlertListPage.xaml/.cs, AlertListViewModel.cs |
| Lista Inventario | ✅ Implementado | InventoryListPage.xaml/.cs, InventoryListViewModel.cs |
| Crear Lote | ✅ Implementado | LotCreatePage.xaml/.cs, LotCreateViewModel.cs |
| Home/Dashboard | ❌ No existe | — |
| Calendario | ❌ No existe | — |
| Mantenimientos (lista) | ❌ No existe | — |
| Detalle Mantenimiento | ❌ No existe | — |
| Wizard Mantenimiento (7 pasos) | ❌ No existe | — |
| Detalle Vehículo | ❌ No existe | — |
| BI Dashboard (LiveCharts) | ❌ No existe | — |
| Reportes | ❌ No existe | — |
| Configuración | ❌ No existe | — |
| StatusBadge (control) | ❌ No existe | — |
| KpiCard (control) | ❌ No existe | — |
| VehicleCard (control) | ❌ No existe | — |

### 2.1 [CRÍTICA] Implementar navegación completa AppShell

**Dónde:** `MaintManager.MAUI/AppShell.xaml`

**Requerido:** Según README.md sección "Arquitectura de navegación frontend":
```
AppShell
 ├── Dashboard (Home)
 ├── Calendario
 ├── Mantenimientos
 ├── Inventario
 ├── BI Dashboard
 ├── Reportes
 └── Configuración
```

Actualmente solo tiene Login + TabBar(Alertas, Inventario).

**Plan:**
- Reemplazar estructura Shell completa con `FlyoutItem` + `TabBar`
- Crear rutas para cada módulo
- Implementar verificación de autenticación (redirigir a Login si no hay token)
- Roles: Admin ve todos los módulos, Técnico ve solo los operativos

### 2.2 [CRÍTICA] Crear Home/Dashboard — Panel principal operativo

**Dónde:** `MaintManager.MAUI/Views/Dashboard/`

**Requerido (README.md):**
- Vehículos de la flota (cards)
- Accesos rápidos: Nuevo mantenimiento, Ver calendario, Ver inventario, Dashboard BI
- Resumen de módulos operativos
- Estado general de mantenimientos
- Indicadores rápidos

**Componentes a crear:**
| Archivo | Propósito |
|---------|-----------|
| `Views/Dashboard/HomePage.xaml` | UI del dashboard |
| `Views/Dashboard/HomePage.xaml.cs` | Code-behind |
| `ViewModels/Dashboard/HomeViewModel.cs` | ViewModel con datos del dashboard |
| `Controls/KpiCard.xaml` | Componente tarjeta de KPI reutilizable |
| `Controls/KpiCard.xaml.cs` | Code-behind con BindableProperties |
| `Controls/VehicleCard.xaml` | Componente tarjeta de vehículo |
| `Controls/VehicleCard.xaml.cs` | Code-behind |

**Endpoint API:** `GET /api/v1/reports/dashboard` (DashboardSummaryResponse)

### 2.3 [CRÍTICA] Crear Calendario — Timeline vertical

**Dónde:** `MaintManager.MAUI/Views/Calendar/`

**Requerido (README.md):**
- Timeline de mantenimientos
- Estados visuales (Próximo, Pendiente, Completado, Emergencia)
- Filtros por vehículo, tipo, estado
- Formato timeline vertical:
  ```
  ● Servicio A
  │
  ● Emergencia
  │
  ● Preventivo
  ```

**Componentes a crear:**
| Archivo | Propósito |
|---------|-----------|
| `Views/Calendar/CalendarPage.xaml` | UI del calendario |
| `Views/Calendar/CalendarPage.xaml.cs` | Code-behind |
| `ViewModels/Calendar/CalendarViewModel.cs` | ViewModel |
| `Controls/StatusBadge.xaml` | Badge de estado coloreado |
| `Controls/StatusBadge.xaml.cs` | Code-behind |

### 2.4 [CRÍTICA] Crear Mantenimientos — Lista + Detalle + Wizard

**Dónde:** `MaintManager.MAUI/Views/Maintenances/`

**Requerido (README.md):**
- Lista paginada de órdenes
- Detalle de orden con todas sus secciones
- Wizard de 7 pasos para crear mantenimiento

**Componentes a crear:**
| Archivo | Propósito |
|---------|-----------|
| `Views/Maintenances/MaintenanceListPage.xaml` | Lista paginada |
| `Views/Maintenances/MaintenanceListPage.xaml.cs` | Code-behind |
| `ViewModels/Maintenances/MaintenanceListViewModel.cs` | ViewModel |
| `Views/Maintenances/MaintenanceDetailPage.xaml` | Detalle de orden |
| `Views/Maintenances/MaintenanceDetailPage.xaml.cs` | Code-behind |
| `ViewModels/Maintenances/MaintenanceDetailViewModel.cs` | ViewModel |
| `Views/Maintenances/MaintenanceWizardPage.xaml` | Wizard 7 pasos |
| `Views/Maintenances/MaintenanceWizardPage.xaml.cs` | Code-behind |
| `ViewModels/Maintenances/MaintenanceWizardViewModel.cs` | ViewModel |

### 2.5 [CRÍTICA] Crear BI Dashboard — LiveCharts + KPIs

**Dónde:** `MaintManager.MAUI/Views/BiDashboard/`

**Requerido (README.md):**
- Fila 1: KPIs críticos (costo/km, tasa emergencia, servicios realizados, cumplimiento)
- Fila 2: Tendencias operativas (gráficos LiveCharts)
- Fila 3: Alertas inteligentes
- Fila 4: Predicciones y análisis futuros

**Componentes a crear:**
| Archivo | Propósito |
|---------|-----------|
| `Views/BiDashboard/BiDashboardPage.xaml` | Dashboard BI con LiveCharts |
| `Views/BiDashboard/BiDashboardPage.xaml.cs` | Code-behind |
| `ViewModels/BiDashboard/BiDashboardViewModel.cs` | ViewModel |

### 2.6 [ALTA] Crear Reportes

**Dónde:** `MaintManager.MAUI/Views/Reports/`

**Requerido:** Pantalla de reportes con exportaciones (PDF y Excel).

**Componentes:**
| Archivo | Propósito |
|---------|-----------|
| `Views/Reports/ReportsPage.xaml` | Lista de reportes disponibles |
| `Views/Reports/ReportsPage.xaml.cs` | Code-behind |
| `ViewModels/Reports/ReportsViewModel.cs` | ViewModel |

### 2.7 [ALTA] Crear Configuración

**Dónde:** `MaintManager.MAUI/Views/Settings/`

**Componentes:**
| Archivo | Propósito |
|---------|-----------|
| `Views/Settings/SettingsPage.xaml` | Configuración del sistema |
| `Views/Settings/SettingsPage.xaml.cs` | Code-behind |
| `ViewModels/Settings/SettingsViewModel.cs` | ViewModel |

### 2.8 [MEDIA] Agregar componentes reutilizables faltantes

**Requerido (README.md):**
- `Controls/StatusBadge.xaml` — emergencias, stock bajo, calendarizados, preventivos
- `Controls/KpiCard.xaml` — dashboard principal, BI, inventario
- `Controls/VehicleCard.xaml` — vehículos, resumen rápido
- `Controls/EmptyState.xaml` — sin alertas, sin mantenimientos
- `Controls/LoadingState.xaml` — skeleton loaders

### 2.9 [MEDIA] Sistema de Design System (tokens visuales)

**Requerido (README.md):**
```csharp
AppColors.Primary
AppSpacing.Medium
AppRadius.Large
AppTypography.Title
```

Crear clase estática `AppColors`, `AppSpacing`, `AppRadius`, `AppTypography` en proyecto Shared o en MAUI/Services.

### 2.10 [MEDIA] Diseño responsive

**Requerido (README.md):**
- Desktop: layouts tipo grid, paneles laterales, múltiples columnas
- Mobile: navegación simplificada, bottom tabs, cards verticales

Implementar detección de plataforma/ventana y adaptar layouts.

---

## FASE 3 — Mejoras backend adicionales

### 3.1 [MEDIO] Agregar endpoint RateMaterial en InventoryController

**Dónde:** `MaintManager.API/Controllers/InventoryController.cs`

**Problema:** `ApiRoutes.Inventory.RateMaterial` existe pero no hay handler HTTP. El servicio `InventoryService.RateMaterialAsync` está implementado pero no es accesible vía API.

### 3.2 [MEDIO] Validadores sin registrar en Program.cs

**Dónde:** `MaintManager.API/Program.cs:78`

**Problema:** `AddValidatorsFromAssemblyContaining<LoginRequestValidator>()` registra todos los validadores del assembly. Pero algunos validadores como `DiagnosisRequestValidator` no se usan en los controllers. No es un error, pero se pueden usar en el futuro.

### 3.3 [MEDIO] Appsettings.Development.json no se versiona (gitignore)

**Dónde:** `.gitignore`

**Problema:** Solo excluye `appsettings.Development.json` y `appsettings.Production.json`. El archivo actual tiene la contraseña postgres/default. Es correcto no versionarlo.

### 3.4 [BAJO] ReportsController usa `file sealed class` interno

**Problema:** Las clases raw SQL query results usan `file` keyword (C# 11). Es correcto pero dificulta testabilidad.

---

## FASE 4 — Configuración de compilación MAUI (respetar configuración actual)

### Configuración fija que NO debe modificarse:

| Parámetro | Valor |
|-----------|-------|
| `MauiVersion` | 10.0.0 |
| `RuntimeIdentifiers` | android-arm64 |
| `AndroidStoreUncompressedFileExtensions` | .dll;.so;.pdb |
| `AndroidEnableCompression` | false |
| `AndroidUseSharedRuntime` | false |
| Paquete CommunityToolkit.Maui | ❌ Eliminado (causa crash) |
| Paquete CommunityToolkit.Mvvm | ✅ Conservado |
| Comando compilación | `dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormats=apk` |
| Comando API | `dotnet run --urls "http://0.0.0.0:5056"` |

### Regla estricta sobre NuGet:
Si se necesita agregar/eliminar paquetes, **primero intentar resolver sin modificar**. Si no es posible, documentar propuesta aquí y en PROXIMOS_PASOS.md, priorizando no romper la configuración actual.

---

## PREGUNTAS Y DUDAS

Las siguientes dudas surgieron durante el análisis forense. Se requiere decisión del usuario para continuar:

1. **IBiReportService**: ¿Se implementa un `BiReportService` en Application Layer que centralice las consultas SQL raw de ReportsController, o se deja ReportsController con acceso directo al DbContext?
- debe ser consistente con mi arquitectura si es consistente entonces crea el BiReportServices si no entonces dejalo como esta
2. **Navegación MAUI**: ¿Se desea `FlyoutPage` (menú hamburguesa) como navegación principal (más enterprise) o `TabBar` (pestañas inferiores, más mobile-first)? README.md menciona "navegación centralizada mediante AppShell" sin especificar Flyout vs Tab.
- Usá FlyoutPage (menú hamburguesa) como navegación principal.
- Es más enterprise, más coherente con el diseño de escritorio (donde el jefe usa paneles laterales) y no sacrifica usabilidad mobile.
- El TabBar es más mobile‑first pero choca con la filosofía visual descrita en el README: "paneles laterales, múltiples columnas".
- AppShell soporta ambos; configuralo con FlyoutBehavior="Flyout".
3. **Colores del Design System**: README.md especifica "Azul oscuro enterprise, Gris neutro, Verde éxito, Naranja warning, Rojo crítico" pero App.xaml actual usa `#512BD4` (púrpura) como color primario. ¿Se debe cambiar al azul oscuro enterprise (#1565C0 como aparece en el PDF exportado de ReportsController)?
- Cambiá el color primario de #512BD4 (púrpura) al azul oscuro enterprise definido en el README (#1565C0 o el que uses en el PDF exportado).
- El README es la especificación oficial y prohíbe explícitamente "colores saturados tipo app escolar". El púrpura actual entra justamente en esa categoría.
- Actualizá App.xaml y todos los recursos de color para reflejar la paleta definitiva: azul oscuro, gris neutro, verde éxito, naranja warning, rojo crítico.
4. **LoginViewModel.ErrorMessage**: Actualmente `LoginViewModel` declara su propia propiedad `ErrorMessage` que sombrea la de `BaseViewModel`. ¿Se unifica en `BaseViewModel` o se mantiene separada?
- Unificá la propiedad ErrorMessage en BaseViewModel y eliminala de LoginViewModel.
- Si ambas existen, LoginViewModel está sombreando la propiedad base, lo que puede causar bugs de binding difíciles de rastrear.
- Además, por MVVM limpio, la responsabilidad de manejar mensajes de error es común a todas las vistas y debe estar en la clase base.
5. **Alcance del frontend**: ¿Se deben crear TODAS las pantallas del README.md en esta iteración, o solo las críticas (Home, Calendar, Maintenance list/detail/wizard, BI Dashboard)?
- Fase 1 (esta iteración): solo las pantallas críticas operativas.
- Fase 2 (siguiente iteración): las complementarias y mejoras visuales.

| Fase 1 — Críticas	| Fase 2 — Complementarias|
|-----------|-------|
| Home (panel principal)	| BI Dashboard completo|
| Calendario / Timeline	| Reportes avanzados|
| Formulario Wizard (crear/editar orden)	| Configuración del sistema|
| Inventario (lista + detalle)	| Predicciones y análisis|
| Alertas (lista)	| Mejoras responsive finales|
- Esto prioriza lo que usan día a día el jefe y los mecánicos, y permite entregar valor rápido sin arriesgar el cronograma académico.
- Documentá las dos fases en PROXIMOS_PASOS.md.
---

## PLAN DE ACCIÓN RESUMIDO POR FASES

```
FASE 1 — Correcciones backend críticas         [✅ COMPLETADA]
├── 1.1 Fix SaveDiagnosisAsync                  ✅
├── 1.2 Fix AuthService MAUI login wrapper      ✅
├── 1.3 Fix App.xaml merged dictionaries         ✅
└── 1.4 Fix IsLoading vs IsBusy en BaseViewModel ✅

FASE 2 — Correcciones backend adicionales       [✅ COMPLETADA]
├── 2.1 Fix GetLastByVehicleAsync                ✅
├── 2.2 Implementar IBiReportService             ✅
├── 2.3 Duplicate RuntimeIdentifiers             ✅
├── 2.4 Agregar RateMaterial endpoint            ✅
└── 2.5 Fix LotExpired error message             ✅

FASE 3 — Frontend MAUI                          [✅ COMPLETADA]
├── 3.1 AppShell navegación Flyout               ✅
├── 3.2 Home/Dashboard con KPIs + acciones       ✅
├── 3.3 Calendario timeline vertical             ✅
├── 3.4 Mantenimientos (lista + detalle + wizard 7 pasos) ✅
├── 3.5 BI Dashboard (LiveCharts)                ✅
├── 3.6 Reportes                                 ✅
├── 3.7 Configuración                            ✅
├── 3.8 Componentes reutilizables                ✅
├── 3.9 Design System tokens                     ✅
└── 3.10 Diseño responsive                       ✅

FASE 4 — Correcciones post-despliegue           [✅ COMPLETADA]
├── 4.1 415 Content-Type StringContent → PostAsJsonAsync ✅
├── 4.2 Sesión persistente con expiración 8h     ✅
├── 4.3 Wizard pasos 2-4 con campos visibles     ✅
├── 4.4 Flyout borders limpios (BoxView inferior) ✅
├── 4.5 Ingreso lote: error con detalle real     ✅
├── 4.6 Documentación actualizada                ✅
└── 4.7 --urls con doble guion en README         ✅

FASE 5 — Estabilización AOT y Shared DTOs      [✅ COMPLETADA]
├── 5.1 BI Dashboard crash AOT (series null)     ✅
├── 5.2 Startup crash AOT (JsonContent.Create)   ✅
├── 5.3 DTOs movidos a Shared/Models             ✅
├── 5.4 Save() con tipos concretos (no anónimos) ✅
└── 5.5 Namespace conflict ApiResponse<T>        ✅

FASE 6 — Estabilización Final Dispositivo       [✅ COMPLETADA]
├── 6.1 BI Dashboard CPURenderMode crash         ✅
├── 6.2 Shell routing absoluto .NET 10           ✅
├── 6.3 DetailPage wrapper deserialization       ✅
├── 6.4 Actions property mapping                 ✅
├── 6.5 OilInfo propiedades planas               ✅
├── 6.6 CloseOrder con body                      ✅
├── 6.7 Documentación actualizada                ✅
└── 6.8 SwipeUpClean low-memory doc              ✅

FASE 7 — Refactor DetailPage                    [✅ COMPLETADA]
├── 7.1 Fix RateMaterial crash (shadow FK)       ✅
├── 7.2 Seed data componentes + materiales       ✅
├── 7.3 Rediseño layout cards + listas           ✅
├── 7.4 Solo lectura orden FI                    ✅
├── 7.5 PDF con datos completos                  ✅
├── 7.6 Buffering acciones + POST /actions       ✅
├── 7.7 Endpoints DELETE items                   ✅
└── 7.8 Diagnóstico campos completos             ✅

FASE 8 — Correcciones post-estabilización        [✅ COMPLETADA]
├── 8.1 RateMaterial debug log                   ✅
├── 8.2 Rating local (batch al guardar)          ✅
├── 8.3 VehicleHistory sin filtro Statid         ✅
├── 8.4 Quick actions role-based (Admin vs Tec)  ✅
├── 8.5 Alertas: historial resueltas (Switch)    ✅
├── 8.6 6 rutas Shell rotas corregidas           ✅
├── 8.7 CreateMaterial sequence reset            ✅
├── 8.8 DetailPage: Actions JsonPropertyName     ✅
├── 8.9 DetailPage: FI checkbox/✕ ocultos       ✅
├── 8.10 Dashboard seeds (124 órdenes, datos)    ✅
└── 8.11 Documentación actualizada               ✅

FASE 9 — Balance dashboard + fixes finales        [✅ COMPLETADA]
├── 9.1 Emergency rates variadas (convertir 2+1)  ✅
├── 9.2 Lotes "Normal" con null-expiry            ✅
├── 9.3 Servicios este mes en junio               ✅
├── 9.4 LOT auto-generado                         ✅
├── 9.5 DiscardLot con opciones fijas             ✅
├── 9.6 Low-stock route corregido                 ✅
├── 9.7 CloseOrder 400 fix (IsEmergencyComplete)  ✅
├── 9.8 Consumos para 6 vehículos sin costo       ✅
├── 9.9 Switch vencimiento visible                ✅
└── 9.10 Documentación actualizada                ✅

FASE 10 — Dashboard restore + Reports directo     [✅ COMPLETADA]
├── 10.1 Dashboard restaurado a commit c95af1c     ✅
├── 10.2 x1000 fix para barras visibles            ✅
├── 10.3 Sin LabelsPaint/DataLabels conflictivos   ✅
├── 10.4 Reports CommandParameter Route→Type fix   ✅
├── 10.5 Reports generación directa (sin filters)  ✅
├── 10.6 Historial vehículo con prompt             ✅
└── 10.7 Documentación actualizada                 ✅
```

## DATABASE — Estado actual

- **Órdenes:** 127 (20 activas, 107 finalizadas)
- **Consumos:** 137 registros con costos reales
- **Componentes instalados:** 40
- **Diagnósticos:** 93
- **Acciones realizadas:** 200
- **Calificaciones materiales:** 74
- **Lotes activos:** 24 (4 por vencer próximos 30 días)
- **Alertas no resueltas:** 8
- **Vehículos con schedule:** 16/16
- **Técnicos activos:** 3

### Scripts SQL de seed
| Script | Descripción |
|--------|-------------|
| `04_seed_components_materials.sql` | Componentes vida útil + acciones checklist + materiales/lotes |
| `05_seed_dashboard_data.sql` | Consumos para órdenes activas, emergencia, lotes por vencer |
| `06_seed_massive_data.sql` | 124 órdenes masivas con consumos, componentes, diagnósticos, etc. |

## NOTAS ADICIONALES

- **Dashboard BI:** Las vistas `vw_*` usan `statid='AC'` para filtrar datos activos. Seed data asegura órdenes activas con consumos.
- **Role-based quick actions:** Admin → Create/Lot directo. Mecánico → lista normal.
- **Rating:** Local, batch al guardar diagnóstico (`PersistPendingActionsAsync`).
- **Alertas históricas:** Switch "Mostrar resueltas" en UI, carga `GET /api/v1/alerts/history`.
- **Serilog:** Configurado en appsettings.Development.json pero no en Program.cs. Bug de configuración.
- **Secuencias BD:** Reseteadas todas las secuencias del esquema `maintenance` tras seed data explícita.
