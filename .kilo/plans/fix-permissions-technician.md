# Plan: Corrección de permisos y autorización para técnicos

## Diagnóstico forense

### Bug 1: Técnico ve órdenes de otros usuarios

**Root cause:** `MaintenanceRepository.GetPagedListItemsAsync()` consulta TODAS las órdenes sin filtrar por `AssignedTo`. El `MaintenancesController.GetAll()` no distingue entre admin y técnico.

**Fix (3 archivos):**

| Archivo | Cambio |
|---|---|
| `Domain/Interfaces/Repositories/IMaintenanceRepository.cs:12` | Agregar `int? assignedTo = null` al parámetro |
| `Infrastructure/Repositories/MaintenanceRepository.cs:48-74` | Agregar filtro `where (assignedTo == null || m.AssignedTo == assignedTo)` |
| `API/Controllers/MaintenancesController.cs:54-56` | Pasar `workid` del JWT cuando el usuario NO es admin |

---

### Bug 2: Botón "Nuevo material" visible para técnicos

**Root cause:** `InventoryListPage.xaml:233-236` — el botón no tiene `IsVisible`.

**Fix (1 archivo):**

| Archivo | Cambio |
|---|---|
| `MAUI/Views/Inventory/InventoryListPage.xaml:233` | Agregar `IsVisible="{Binding IsAdmin}"` al `Border` del botón |

---

### Bug 3: Botones "Editar/Eliminar/Nuevo lote" visibles para técnicos

**Root cause:** `MaterialDetailPage.xaml:25-29,88` — los botones no tienen `IsVisible`.

**Fix (1 archivo):**

| Archivo | Cambio |
|---|---|
| `MAUI/Views/Inventory/MaterialDetailPage.xaml:25-29` | Agregar `IsVisible="{Binding IsAdmin}"` al `HorizontalStackLayout` que contiene Editar/Eliminar |
| `MAUI/Views/Inventory/MaterialDetailPage.xaml:88` | Agregar `IsVisible="{Binding IsAdmin}"` al `Button` "Nuevo lote" |

---

### Bug 4: Reporte "Costo por Km" crashea en vez de mostrar mensaje

**Root cause:** `ReportsViewModel` no tiene `AuthService` ni `IsAdmin`. Llama al API directamente, el API retorna 403 → `GetByteArrayAsync` lanza `HttpRequestException` → `ExecuteAsync` muestra pantalla de error genérico en vez de un mensaje amigable.

**Fix (2 archivos):**

| Archivo | Cambio |
|---|---|
| `MAUI/ViewModels/Reports/ReportsViewModel.cs` | Inyectar `AuthService`, agregar `IsAdmin` property, verificar antes de llamar al API |
| `MAUI/ViewModels/Reports/ReportsViewModel.cs:63-83` | En `GenerateReport("cost-per-km")`: si `!IsAdmin`, mostrar `DisplayAlert("Acceso restringido", "Solo el administrador o jefe de mantenimiento puede generar este reporte.", "Aceptar")` y retornar |

---

## Plan de implementación por fases

### Fase 1: Filtrar órdenes por técnico asignado (backend)

3 archivos: interfaz del repositorio, implementación, controlador.

### Fase 2: Ocultar "Nuevo material" a técnicos (UI)

1 archivo: `InventoryListPage.xaml`

### Fase 3: Ocultar "Editar/Eliminar/Nuevo lote" a técnicos (UI)

1 archivo: `MaterialDetailPage.xaml`

### Fase 4: Mensaje amigable en reporte Costo por Km (UI)

2 archivos: `ReportsViewModel.cs`
