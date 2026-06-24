# Plan: Verificar datos reales en la base de datos `neoplus_maintenance`

## Conexión

```
Host=localhost;Port=5432;Database=neoplus_maintenance;Username=postgres;Password=postgres
```

## Consultas SQL para verificar

### 1. ¿Qué datos hay en `vw_monthly_cost`?

```sql
SELECT * FROM maintenance.vw_monthly_cost ORDER BY month DESC;
```

### 2. ¿La vista filtra `statid = 'AC'` o `'FI'`?

```sql
SELECT definition FROM pg_views WHERE viewname = 'vw_monthly_cost';
```

### 3. ¿Cuántos mantenimientos hay por mes y por estado?

```sql
SELECT date_trunc('month', maintenance_date) AS month,
       statid,
       count(*) AS total
FROM maintenance.maintenance
WHERE maintenance_date >= date_trunc('month', CURRENT_DATE - INTERVAL '6 months')
GROUP BY 1, 2
ORDER BY 1 DESC, 2;
```

### 4. ¿Qué mantenimientos finalizados (`FI`) hay con consumo de material?

```sql
SELECT date_trunc('month', m.maintenance_date) AS month,
       m.prcoid,
       vk.license_plate_number,
       count(m.mainid) AS services,
       COALESCE(sum(mc.quantity * COALESCE(ml.unit_cost, 0)), 0) AS total_cost
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
LEFT JOIN maintenance.material_consumption mc ON mc.mainid = m.mainid AND mc.origin = 'Stock propio'
LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
WHERE m.statid = 'FI'
  AND m.maintenance_date >= date_trunc('month', CURRENT_DATE - INTERVAL '6 months')
GROUP BY 1, m.prcoid, vk.license_plate_number
ORDER BY 1 DESC, total_cost DESC;
```

### 5. ¿Qué mantenimientos ACTIVOS (`AC`) hay con consumo?

```sql
SELECT date_trunc('month', m.maintenance_date) AS month,
       m.prcoid,
       vk.license_plate_number,
       count(m.mainid) AS services,
       COALESCE(sum(mc.quantity * COALESCE(ml.unit_cost, 0)), 0) AS total_cost
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
LEFT JOIN maintenance.material_consumption mc ON mc.mainid = m.mainid AND mc.origin = 'Stock propio'
LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
WHERE m.statid = 'AC'
  AND m.maintenance_date >= date_trunc('month', CURRENT_DATE - INTERVAL '6 months')
GROUP BY 1, m.prcoid, vk.license_plate_number
ORDER BY 1 DESC, total_cost DESC;
```

## Qué esperamos encontrar

- **Consulta 2:** Confirmará si la vista usa `statid = 'AC'` (bug) o `'FI'` (correcto)
- **Consulta 3:** Mostrará cuántos mantenimientos hay por mes y por estado
- **Consulta 4:** Si hay mantenimientos finalizados con costo, deben aparecer en el gráfico
- **Consulta 5:** Si no hay finalizados, los activos son la única fuente de datos actual

## Próximo paso

Ejecutar estas consultas con `psql` y compartir resultados para decidir si:
- Corregir la vista (cambiar `AC` → `FI`) + llenar meses vacíos con $0
- O solo llenar meses vacíos con $0 (mantener la vista como está)
