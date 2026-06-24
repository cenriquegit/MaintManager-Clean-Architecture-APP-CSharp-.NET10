# Plan: Corrección de bugs múltiples — Alertas, Calificaciones, Inventario

## Diagnóstico forense

### Bug 1: Alertas KPI cambia al cargar la página

**Causa:** `CheckAlertsCommand` → POST `/api/v1/alerts/check` → `CheckAndGenerateAlertsAsync()` genera NUEVAS alertas basadas en estado actual de la BD. El KPI usa `COUNT(alert_log WHERE resolved = false)`.

**Conclusión:** Funciona correctamente. El botón regenera alertas reales. No es bug.

---

### Bug 2: Botón "Resuelta" no funciona

**Archivo:** `Views/Alerts/AlertListPage.xaml:73`
```xml
Command="{Binding ... MarkResolvedCommand}"
```

**ViewModel:** `AlertListViewModel.cs:79` — `[RelayCommand] Resolve()` genera `ResolveCommand`, NO `MarkResolvedCommand`.

**Fix:** Cambiar `MarkResolvedCommand` → `ResolveCommand`.

---

### Bug 3: Fecha "Resuelta" y estado nunca aparecen

`AlertLog` tiene `ResolvedAt`, `ReadAt`. La API NO devuelve `ResolvedAt` ni `ReadAt`.
El modelo cliente solo muestra `CreatedAtFormatted` sin fecha de resolución.

**Fix (3 archivos):**
1. `Application/DTOs/Reports/AlertResponse.cs` — agregar `DateTime? ReadAt, DateTime? ResolvedAt`
2. `Application/Mappings/InventoryMappings.cs:62-74` — mapear `ReadAt: al.ReadAt, ResolvedAt: al.ResolvedAt`
3. `MAUI/Models/AlertItem.cs` — agregar propiedades + actualizar `LevelLabel` y `CreatedAtFormatted`:
   ```
   Creada: 15/06 10:30 · Leída: 16/06 08:15 · Resuelta: 16/06 14:00
   ```

---

### Bug 4: "Sin calificaciones" cuando sí hay calificaciones

**Backend** devuelve solo la ÚLTIMA calificación (`FirstOrDefault()`). El usuario quiere el PROMEDIO.

**Fix:**
1. `Application/DTOs/Inventory/MaterialResponse.cs` — cambiar `MaterialRatingInfo` a:
   - `double? AverageRating` (promedio de todas las calificaciones)
   - `int TotalRatings` (cantidad de calificaciones)
2. `Application/Mappings/InventoryMappings.cs:35-38`:
   ```csharp
   var ratings = m.Ratings.Select(r => (double)r.Rating).ToList();
   var avg = ratings.Count > 0 ? Math.Round(ratings.Average(), 1) : (double?)null;
   new MaterialRatingInfo(AverageRating: avg, TotalRatings: ratings.Count)
   ```
3. `MAUI/ViewModels/Inventory/MaterialDetailViewModel.cs:60-72` — mostrar:
   ```
   ⭐ 3.2/5 (8 calificaciones)
   ```
   O si no hay calificaciones: `"Sin calificaciones"`

---

### Bug 5: Badge "Lotes por vencer" en cada material del inventario (NUEVO)

**Lo que existe hoy:**
- Solo badge `Stock bajo` en cada card de material (`IsBelowMinimum`)
- KPI del dashboard muestra `COUNT(*) FROM vw_expiring_lots` (lotes activos que vencen en ≤30 días)

**Lo que el usuario quiere:**
- Un badge **adicional** en cada card de material, similar al de `Stock bajo`, que diga por ejemplo `⏰ 3 lotes por vencer`
- Solo visible si ese material tiene ≥1 lote activo próximo a vencer (≤60 días, mismo criterio que el endpoint `expiring-lots`)
- La SUMA de todos los conteos por material debe coincidir con el número del KPI del dashboard

**Datos necesarios:**
- `MaterialListItem` NO incluye lotes (el query actual solo hace `.Include(m => m.Category)`)
- `ToListItem()` NO tiene acceso a `m.Lots`

**Fix (5 archivos):**

| # | Archivo | Cambio |
|---|---|---|
| 1 | `API/Controllers/InventoryController.cs:83-88` | Agregar `.Include(m => m.Lots)` en `GetMaterialsFilteredAsync` |
| 2 | `API/Controllers/InventoryController.cs:66` | Agregar `.Include(m => m.Lots)` en el query con `vehicleId` |
| 3 | `Application/DTOs/Inventory/MaterialListItem.cs` | Agregar `int ExpiringLotsCount` |
| 4 | `Application/Mappings/InventoryMappings.cs:9-19` | Calcular: `m.Lots.Count(l => l.LotStatus == "activo" && l.ExpirationDate.HasValue && l.ExpirationDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)))` |
| 5 | `MAUI/Models/MaterialItem.cs` | Agregar `int ExpiringLotsCount`, `bool HasExpiringLots` (computed) |
| 6 | `MAUI/Views/Inventory/InventoryListPage.xaml` | Agregar badge debajo del badge `Stock bajo`: |
   ```xml
   <Border IsVisible="{Binding HasExpiringLots}" Style="{StaticResource BadgeWarning}">
       <Label Text="{Binding ExpiringLotsCount, StringFormat='⏰ {0} por vencer'}"
              FontSize="11" FontAttributes="Bold" TextColor="{StaticResource ColorWarning}"/>
   </Border>
   ```

**Verificación de consistencia con KPI:**
- El KPI usa `vw_expiring_lots` (≤30 días)
- Si queremos que coincida, usar el mismo criterio de 30 días en vez de 60
- Cambiar `AddDays(60)` → `AddDays(30)` en el mapping para que el total de badges coincida con el KPI

---

## Fases de implementación

### Fase 1: Botón "Resuelta" (1 archivo)
| `Views/Alerts/AlertListPage.xaml:73` | `MarkResolvedCommand` → `ResolveCommand` |

### Fase 2: Fecha y estado de resolución (3 archivos)
| `Application/DTOs/Reports/AlertResponse.cs` | Agregar `DateTime? ReadAt, DateTime? ResolvedAt` |
| `Application/Mappings/InventoryMappings.cs:72` | Agregar `ReadAt: al.ReadAt, ResolvedAt: al.ResolvedAt` |
| `MAUI/Models/AlertItem.cs` | Agregar props + actualizar display |

### Fase 3: Promedio de calificaciones (3 archivos)
| `Application/DTOs/Inventory/MaterialResponse.cs` | Cambiar `MaterialRatingInfo` a avg + count |
| `Application/Mappings/InventoryMappings.cs:35-38` | Calcular promedio |
| `MAUI/ViewModels/Inventory/MaterialDetailViewModel.cs` | Mostrar formato correcto |

### Fase 4: Badge lotes por vencer (5 archivos)
| `API/Controllers/InventoryController.cs` | `.Include(m => m.Lots)` en queries |
| `Application/DTOs/Inventory/MaterialListItem.cs` | `int ExpiringLotsCount` |
| `Application/Mappings/InventoryMappings.cs:9-19` | Calcular conteo de lotes por vencer |
| `MAUI/Models/MaterialItem.cs` | `int ExpiringLotsCount`, `bool HasExpiringLots` |
| `MAUI/Views/Inventory/InventoryListPage.xaml` | Badge `⏰ X por vencer` |
