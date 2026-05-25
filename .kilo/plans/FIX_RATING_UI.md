# PLAN: Fixes post-estabilización — Rating, Routing, QuickActions, Historial Alertas

## Objetivo General
Corregir bugs de navegación Shell restantes, implementar acciones rápidas funcionales en Dashboard, agregar historial de alertas resueltas, y pulir detalles de UI/UX.

## Reglas ABSOLUTAS (NO TOCAR)
1. Dashboard BI/LiveCharts/MauiProgram.cs — no se modifican
2. Login/Auth/Wizard — no se tocan
3. Compilación (.csproj, AndroidManifest, launchSettings) — no se modifican

---

## ISSUE #1 — RateMaterial 400 Bad Request (PENDIENTE DEBUG)

### Problema
`POST /api/v1/inventory/materials/{id}/ratings` → 400 en 3-4ms. El `SaveChangesAsync()` lanza excepción sin ejecutar INSERT SQL.

### Diagnóstico
- `MaterialRating.Create(...)` válido
- `AddRatingAsync` agrega al context
- `SaveChangesAsync()` lanza ANTES de generar SQL
- **Causa probable:** Shadow FK `MaterialMateid` aún activa. El fix de Fase 1 (`HasOne<Material>().WithMany()`) podría no ser suficiente si EF Core sigue detectando la relación por convención de nombre (`Mateid` → `Material`).

### Solución
1. ✅ Ya agregado: try-catch con `Debug.WriteLine` en `RateMaterialAsync` para ver el error real
2. Probar en dispositivo y revisar el log `[RateMaterial ERROR]`
3. Según el error, decidir si: a) forzar `Ignore()` en la navegación, o b) el rating local (Issue #3) ya evita el problema

### Archivos modificados
- `InventoryService.cs` (log agregado)

---

## ISSUE #2 — Quick Actions del Dashboard funcionales

### Problema
Las 4 acciones rápidas del panel principal solo navegan a FlyoutItems (`//Maintenances`, `//Calendar`, etc.). "Nuevo Mantenimiento" solo abre la lista, no el wizard.

### Solución
Cambiar las navegaciones para que CADA acción rápida navegue DIRECTAMENTE a la funcionalidad:
| Botón | Ruta actual | Nueva ruta |
|-------|-------------|------------|
| 🚗 Nuevo Mantenimiento | `//Maintenances` | `///Maintenances/Create` (wizard directo) |
| 📅 Calendario | `//Calendar` | `//Calendar` (ya está bien) |
| 📦 Inventario | `//Inventory` | `//Inventory` (ya está bien) |
| 📊 BI Dashboard | `//BiDashboard` | `//BiDashboard` (ya está bien) |

### Archivos
- `HomeViewModel.cs` (1 cambio)

---

## ISSUE #3 — Rating local + batch save (Parcial)

### Problema
El rating se enviaba al API inmediatamente después de consumir, pero si el usuario eliminaba el material de la lista local, el rating ya estaba guardado en BD sin sentido.

### Solución
✅ Ya implementado: `ConsumedMaterialItem.Rating` y `RatingObservation` locales. Se persisten en `PersistPendingActionsAsync()` al hacer "Guardar Diagnóstico".

### Archivos
- `MaintenanceDetailViewModel.cs` ✅
- `MaintenanceDetailPage.xaml` ✅ (estrellas visibles en lista)

---

## ISSUE #4 — Picker + botón responsive

### Problema
Picker pequeño, botón "+ Agregar" grande y con texto innecesario.

### Solución
✅ Ya implementado: Picker `FillAndExpand`, botón "+" circular 44x44 cornerRadius=22.

### Archivos
- `MaintenanceDetailPage.xaml` ✅

---

## ISSUE #5 — Orden FI: ocultar secciones editables

### Problema
En orden finalizada los inputs se veían deshabilitados pero seguían ocupando espacio y confundían.

### Solución
✅ Ya implementado: inputs OCULTOS con `IsVisible="{Binding IsReadOnly, Converter=InvertedBoolConverter}"`.
Solo visible: header, info general, listas puras, diagnóstico display, export PDF.

### Archivos
- `MaintenanceDetailPage.xaml` ✅
- `MaintenanceDetailViewModel.cs` ✅

---

## ISSUE #6 — VehicleHistory filtro `Statid == "AC"`

### Problema
`MaintenanceRepository.cs:17` filtra `m.Statid == "AC"` (solo activas). Las órdenes finalizadas no aparecen en el historial del vehículo, mostrando "no tiene mantenimientos registrados".

### Solución
Eliminar `&& m.Statid == "AC"` del Where.

### Archivos
- `MaintenanceRepository.cs`

---

## ISSUE #7 — Calendar → DetailPage: parámetro `id` vs `mainid`

### Problema
`CalendarViewModel.cs:147`: `$"//Maintenances/Detail?id={item.Id}"` — usa `//` (debería `///`) y pasa `id` en vez de `mainid`.

### Solución
Cambiar a: `$"///Maintenances/Detail?mainid={item.Id}"`

### Archivos
- `CalendarViewModel.cs`

---

## ISSUE #8 — VehicleHistory → DetailPage: `//` vs `///`

### Problema
`VehicleHistoryViewModel.cs:63`: `$"//Maintenances/Detail?mainid={item.Mainid}"` usa `//` (absoluto a FlyoutItem) pero debería ser `///` (sub-ruta).

### Solución
Cambiar a: `$"///Maintenances/Detail?mainid={item.Mainid}"`

### Archivos
- `VehicleHistoryViewModel.cs`

---

## ISSUE #9 — Inventory: rutas relativas rotas (3 ocurrencias)

### Problema
`InventoryListViewModel.cs` tiene 3 navegaciones relativas que fallan en .NET 10:
- Línea 71-72: `"Inventory/CreateLot"` → debe ser `"///Inventory/CreateLot"`
- Línea 81-82: `"Inventory/CreateMaterial"` → debe ser `"///Inventory/CreateMaterial"`
- Línea 97: `"Inventory/LotList"` → debe ser `"///Inventory/LotList"` (este NO tiene try-catch → crash directo)

### Solución
Reemplazar las 3 rutas relativas por absolutas con `///`. Eliminar los try-catch redundantes.

### Archivos
- `InventoryListViewModel.cs`

---

## ISSUE #10 — Historial de alertas resueltas (Feature)

### Problema
Actualmente la página de alertas solo muestra alertas no resueltas. No hay forma de ver el histórico.

### Solución (Opción C del usuario)
Agregar un Switch "Mostrar resueltas" en la UI de Alertas que alterna entre:
- `GET /api/v1/alerts` (no resueltas, endpoint existente)
- `GET /api/v1/alerts?resolved=true` (nuevo parámetro, o nuevo endpoint)

### Arquitectura propuesta
1. API: Modificar `AlertsController.GetUnresolved()` para aceptar `[FromQuery] bool? resolved = null`
2. Si `resolved == true` → retorna todas, si `resolved == false` o null → solo no resueltas
3. MAUI: Agregar Switch en `AlertsPage.xaml` + `ShowResolved` property en ViewModel

### Archivos
- `AlertsController.cs`
- `AlertListViewModel.cs`
- `AlertsPage.xaml`

---

## ISSUE #11 — Sesión 8h: debuggear por qué pide login

### Problema
Usuario reporta que a veces pide login aunque esté dentro de las 8h.

### Diagnóstico
`TryRestoreSessionAsync` en `ApiService.cs` lee token de `SecureStorage` y expiración de `Preferences`. Si `SecureStorage.Remove("auth_token")` se ejecuta en algún flujo de error, el token se pierde.
**Posible causa:** El `ClearAuthToken()` se llama en varios catch de errores. Verificar si algún error inesperado está limpiando el token al hacer peticiones.

### Solución
Agregar log en todos los `ClearAuthToken()` para rastrear cuándo se limpia. Si el problema persiste, cambiar a expiración deslizante (refrescar 8h desde la última actividad).

### Archivos
- `ApiService.cs` (solo logging)
- `AuthService.cs` (verificar flujo de logout)

---

## RESUMEN DE ARCHIVOS A MODIFICAR

| Archivo | Issues |
|---------|--------|
| `InventoryService.cs` | #1 (log) |
| `HomeViewModel.cs` | #2 (quick actions) |
| `MaintenanceRepository.cs` | #6 (VehicleHistory filter) |
| `CalendarViewModel.cs` | #7 (routing + param) |
| `VehicleHistoryViewModel.cs` | #8 (routing) |
| `InventoryListViewModel.cs` | #9 (3 routing fixes) |
| `AlertsController.cs` | #10 (resolved param) |
| `AlertListViewModel.cs` | #10 (switch) |
| `AlertsPage.xaml` | #10 (UI) |
| `AuthService.cs` | #11 (debug log) |

## ORDEN DE EJECUCIÓN

1. #6 VehicleHistory filter (1 línea, segura)
2. #7 Calendar routing (1 línea)
3. #8 VehicleHistory routing (1 línea)
4. #9 Inventory routing (6 líneas)
5. #2 Quick Actions (1 línea)
6. #10 Alert history (nuevo endpoint + switch)
7. #11 Session debug (logging)
8. Build + probar
