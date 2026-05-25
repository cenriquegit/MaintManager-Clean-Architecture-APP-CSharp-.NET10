# Solicitud de datos complementarios para el Seed

## Instrucciones

Si durante las rondas de corrección noto que faltan datos en el seed que afectan
a funcionalidades de la app, crearé un archivo como este explicando QUÉ falta,
POR QUÉ falta y QUÉ impacto tiene. Al final de las correcciones, si decides
agregar más datos, usa esta guía como referencia.

Cada "lote" de datos solicitados será secuencial: un dato nuevo puede requerir
ajustar otros datos existentes (ej: agregar un mantenimiento nuevo requiere
actualizar km_since_last del siguiente).

---

## Lote pendiente actual: datos para KPIs del Dashboard

### 1. Órdenes activas (`statid = 'AC'`) con fechas recientes

**¿Qué falta?**  
El seed actual tiene 29 mantenimientos, todos con `statid = 'FI'` (Finalizados).
Ninguno está en estado `'AC'` (Activo).

**¿Por qué?**  
La vista `vw_bi_dashboard_summary` calcula `services_this_month` así:
```sql
SELECT COUNT(*) FROM maintenance.maintenance
WHERE statid = 'AC'
  AND maintenance_date >= DATE_TRUNC('month', CURRENT_DATE)
```
Si no hay registros AC, el KPI "Servicios del Mes" siempre es 0.

**¿Qué impacto tiene?**
- KPI "Servicios del Mes" = 0
- KPI "% Emergencia" = 0 (también filtra por `statid = 'AC'`)
- Sección "Resumen de Mantenimientos": Programados = 0, En Progreso = 0
- El dashboard BI muestra servicios del mes como 0

**Datos necesarios (3-5 registros):**
```
Placa     | Tipo        | Servicio | Fecha       | Km     | Técnico asignado
----------|-------------|----------|-------------|--------|-----------------
VDG-361   | Calendarizado | A       | 2026-05-10  | 9847   | juan.quispe
VDW-869   | Calendarizado | A       | 2026-05-15  | 20555  | pedro.mamani
VBQ-302   | Emergencia   | null     | 2026-05-18  | 39814  | juan.quispe
VBQ-285   | Calendarizado | B       | 2026-05-20  | 50250  | pedro.mamani
```

**Dependencias:** Ninguna. Solo requiere que el vehículo exista (16 ya creados).

---

### 2. Alertas en `alert_log`

**¿Qué falta?**  
El seed no insertó registros en `maintenance.alert_log`. Las alertas existen
como concepto (tabla + endpoint `POST /api/v1/alerts/check`) pero no hay datos
iniciales.

**¿Por qué?**  
La sección 12 del `05_seed_final.sql` tiene un DO block para insertar alertas,
pero produjo 0 filas (probablemente porque las condiciones no matchearon con
los datos existentes en ese momento de la ejecución).

**¿Qué impacto tiene?**
- KPI "Alertas" = 0
- Página de Alertas vacía
- No se puede probar "Marcar como leída" ni "Resolver"

**Datos necesarios (6-8 registros):**
```
Tipo alerta                | Vehículo/Material      | Mensaje                                              | Fecha
---------------------------|------------------------|------------------------------------------------------|----------
MANTENIMIENTO_PROXIMO_KM   | V0U-053 (VW Gol)      | Vehículo V0U-053 próximo a mantenimiento (155,198 km)| 2026-05-15
MANTENIMIENTO_PROXIMO_KM   | VBQ-285 (Chevrolet Joy)| Vehículo VBQ-285 próximo a mantenimiento (50,250 km) | 2026-05-15
STOCK_BAJO                 | Correa de Distribución| Stock bajo de Correa de Distribución: 4 unid (mín 2) | 2026-05-10
LOTE_POR_VENCER            | Aceite 5W-30 lote     | Lote ACE-5W30-PRX vence en 20 días                  | 2026-05-18
MANTENIMIENTO_PROXIMO_KM   | B0J-433 (VW Passat)   | Vehículo B0J-433 próximo a mantenimiento (175,786 km)| 2026-05-15
STOCK_BAJO                 | Pastillas Freno       | Stock bajo de Pastillas de Freno Delanteras: 6 par   | 2026-05-12
```

**Dependencias:** Requiere que `alert_config` tenga los 4 tipos (ya en bd-final.sql).

---

### 3. Data en vistas auxiliares (`vw_low_stock`, `vw_expiring_lots`, `vw_cost_per_km`)

**¿Qué falta?**  
Las vistas `vw_low_stock`, `vw_expiring_lots` y `vw_cost_per_km` están vacías
o devuelven 0 filas porque los datos base no cumplen las condiciones.

**¿Por qué?**  
- `vw_low_stock`: compara `stock_total <= stock_minimum`. Los materiales tienen
  stock_total ~12-20 y stock_minimum ~4-6. Si el consumo no redujo el stock
  por debajo del mínimo, la vista queda vacía.
- `vw_expiring_lots`: busca lotes con `expiration_date` próxima. El seed tiene
  lotes con fechas 2026-12-01 y 2027-05-01, demasiado lejanas.
- `vw_cost_per_km`: requiere `material_consumption` con costos para calcular
  costo/km. Si no hay consumos con `unit_cost`, el cálculo da 0.

**Datos necesarios:**

a) Stock bajo: Reducir `stock_total` de 2-3 materiales por debajo de su mínimo.
   O crear un nuevo lote con `current_quantity = 2` y `stock_minimum = 4`.

b) Lotes por vencer: Crear 2 lotes con `expiration_date` en los próximos 30 días.
   ```
   Material         | Fecha vencimiento | Cantidad
   -----------------|-------------------|---------
   Líquido Frenos   | 2026-06-10        | 5 litros
   Aceite 5W-30     | 2026-06-15        | 8 litros
   ```

c) Costo/km: Agregar `unit_cost` a los consumos existentes o nuevos.
   ```
   Material   | Cantidad | Costo unit. | Mantenimiento asociado
   -----------|----------|-------------|-----------------------
   Aceite 5W30| 4.5      | S/28.50     | ORD-2024-VDG361
   Filtro Ace.| 1        | S/35.00     | ORD-2024-VDG361
   ```

**Dependencias:** Los items (a) y (b) son cambios directos a tablas existentes.
El item (c) requiere que existan maintenance y material_consumption.

---

## Formato de entrega

Si decides agregar estos datos, proporciona la información en formato SQL
o en tablas como las de arriba. Yo transformaré las tablas en sentencias
INSERT/UPDATE y las agregaré como un nuevo archivo `06_seed_complementos.sql`
que se ejecuta DESPUÉS de `05_seed_final.sql`.

Orden de ejecución final:
```sql
1. bd-final.sql
2. 02_ajustes_fase1.sql
3. 05_seed_final.sql
4. 06_seed_complementos.sql   ← datos adicionales (este archivo)
```
