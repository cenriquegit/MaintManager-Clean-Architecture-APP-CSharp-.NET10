# Análisis de problemas con los seed SQL

## Problemas detectados en `04_seed_expanded.sql`

### 1. Tablas que referencia pero no existen (si falta `02_ajustes_fase1.sql`)

| Tabla | Creada en | ¿Existe sin 02_ajustes? |
|---|---|---|
| `technician_assignment` | `02_ajustes_fase1.sql` | ❌ NO |
| `material_discard` | `bd-final.sql` | ✅ SÍ |
| `schedule_action` | `bd-final.sql` | ✅ SÍ |
| `installed_component` | `bd-final.sql` | ✅ SÍ |

Si el usuario ejecutó `bd-final.sql → 03_seed_data.sql` **sin** `02_ajustes_fase1.sql`, el INSERT en `technician_assignment` falla → toda la transacción se revierte (BEGIN/COMMIT) → quedan los datos del seed anterior pero no los nuevos.

### 2. Duplicación de proveedores

`03_seed_data.sql` ya crea "Repuestos del Sur S.R.L." (RUC 20601234561). Mi seed crea "Lubricantes del Sur S.A.C." (RUC 20601234562) que es redundante y crea problemas de trazabilidad en lotes.

### 3. KM de mantenimientos inconsistentes

Mi seed usa `v_vehicles_km` calculado desde `product.vehicle.mileage` pero las rentas (`03_seed_data.sql`) establecen los km reales desde `service.rentexecute.kilometer_end`. Si el `mileage` en `product.vehicle` difiere del `kilometer_end` de rentas, los km calculados están mal.

Ejemplo vehículo 5 (ARU-781, Hyundai Tucson): 
- `03_seed_data.sql` renta: km_start=51800, km_end=54100
- Mi seed calcula v_km del `product.vehicle.mileage` que podría ser 54100 (actualizado por renta) o el valor original del INSERT (54100). OK en este caso, pero frágil.

Para vehículos con múltiples rentas, el mileage en vehicle NO se actualiza automáticamente. Mi seed lo lee y no refleja el km real de la última renta.

### 4. Orden de mantenimientos ilógico

Mi seed inserta mantenimientos en 2024-2025-2026 con km decrecientes:
```
Mantenimiento 2024: mileage = v_km - 15000 - (i*500)
Mantenimiento 2025: mileage = v_km - 5000 - (i*300)
Mantenimiento 2026: mileage = v_km
```

El problema: la BD tiene vehículos adquiridos entre 2019-2022, pero mi seed no tiene registros de los años intermedios. El km_since_last del primer mantenimiento (2024) se calcula contra el último mantenimiento REAL (que no existe) y queda en "null" — ok. Pero los km de 2024 a 2026 no son consistentes con el paso del tiempo.

### 5. El seed se ejecuta dentro de BEGIN/COMMIT

Si FALLA CUALQUIER PARTE → TODO se revierte. Las líneas de verificación al final (fuera del DO block) se ejecutan después del COMMIT. Si la transacción se revierte, no hay datos nuevos pero las verificaciones muestran solo los datos viejos.

### 6. Fecha de mantenimiento

En la 4ta iteración:
```sql
CASE WHEN v_rnd < 0.3 THEN CURRENT_DATE - INTERVAL '15 days'
```
Esto genera fechas FUTURAS o MUY RECIENTES (según cuando se ejecute). No refleja el ciclo real de mantenimiento.

### 7. `config_system` sin seed en `bd-final.sql`

La tabla existe pero **no tiene datos seed** dentro de `bd-final.sql`. Mi seed no la puebla. Esto significa que los parámetros `intervalo_km` y `alerta_km_umbral` del SchedulingService devuelven NULL. El SchedulingService usa `IGenericRepository<ConfigSystem>` y si no hay registros, `GetIntValue()` lanza `InvalidCastException`.

## Resumen de correcciones necesarias

1. **Simplificar seed**: Eliminar `04_seed_expanded.sql` y crear un seed final único que respete los km reales de rentas
2. **Agregar seed de `config_system`** con `intervalo_km=5000` y `alerta_km_umbral=800`
3. **Agregar seed de `alert_log`** con alertas de ejemplo (ya lo hace mi seed)
4. **Garantizar consistencia**: Calcular km de mantenimientos desde `service.rentexecute` y no desde `product.vehicle.mileage`
5. **Diversidad de estados**: Mantener los `UPDATE statid = 'FI'` y `'CA'` para que los filtros funcionen
6. **Ejecutar `02_ajustes_fase1.sql`** ANTES del seed expandido

## Orden de ejecución recomendado

```sql
1. bd-final.sql            → Esquema completo
2. 02_ajustes_fase1.sql    → Tablas adicionales (technician_assignment, etc.)
3. 03_seed_data.sql        → Seed básico (vehículos, personas, materiales)
4. 04_seed_expanded.sql    → Seed demo (mantenimientos masivos, alertas, etc.)
```
