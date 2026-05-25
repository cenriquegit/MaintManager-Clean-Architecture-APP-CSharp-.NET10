-- =====================================================================
-- FIX VISTAS BI — eliminar filtro statid="AC" para incluir histórico
-- =====================================================================
-- Las vistas BI originales solo muestran datos de órdenes activas
-- (statid='AC'). Para un dashboard con datos históricos seed,
-- necesitamos incluir también las órdenes finalizadas (statid='FI').
-- =====================================================================

-- 1. Costo por km por vehículo
CREATE OR REPLACE VIEW maintenance.vw_cost_per_km AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    COUNT(m.mainid)                          AS total_services,
    COALESCE(SUM(mc_cost.cost_total), 0)     AS total_material_cost,
    vk.current_km,
    CASE
        WHEN vk.current_km > 0
        THEN ROUND(COALESCE(SUM(mc_cost.cost_total), 0) / vk.current_km, 4)
        ELSE 0
    END                                      AS cost_per_km
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
LEFT JOIN LATERAL (
    SELECT SUM(mc.quantity * COALESCE(ml.unit_cost, 0)) AS cost_total
    FROM maintenance.material_consumption mc
    LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
    WHERE mc.mainid = m.mainid
      AND mc.origin = 'Stock propio'
) mc_cost ON true
WHERE m.statid IN ('AC', 'FI')
GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name, vk.current_km;

COMMENT ON VIEW maintenance.vw_cost_per_km IS
    'Costo por km por vehículo. Incluye AC + FI.';

-- 2. Tasa de emergencias por vehículo
CREATE OR REPLACE VIEW maintenance.vw_emergency_rate AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    COUNT(*) FILTER (WHERE mt.name = 'Calendarizado') AS scheduled_count,
    COUNT(*) FILTER (WHERE mt.name = 'Emergencia')    AS emergency_count,
    COUNT(*)                                           AS total_count,
    CASE WHEN COUNT(*) > 0
        THEN ROUND(
            COUNT(*) FILTER (WHERE mt.name = 'Emergencia')::numeric
            / COUNT(*)::numeric * 100, 2)
        ELSE 0
    END                                                AS emergency_rate_percent
FROM maintenance.maintenance m
JOIN maintenance.maintenance_type      mt ON m.matyid = mt.matyid
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
WHERE m.statid IN ('AC', 'FI')
GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name;

COMMENT ON VIEW maintenance.vw_emergency_rate IS
    'Tasa de emergencias por vehículo. Incluye AC + FI.';

-- 3. Costo mensual de mantenimiento
CREATE OR REPLACE VIEW maintenance.vw_monthly_cost AS
SELECT
    DATE_TRUNC('month', m.maintenance_date) AS month,
    m.prcoid,
    vk.license_plate_number,
    COUNT(m.mainid)                         AS services_count,
    COALESCE(SUM(mc.quantity * COALESCE(ml.unit_cost, 0)), 0) AS monthly_cost
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km    vk  ON m.prcoid  = vk.prcoid
LEFT JOIN maintenance.material_consumption mc  ON mc.mainid = m.mainid AND mc.origin = 'Stock propio'
LEFT JOIN maintenance.material_lot         ml  ON mc.maloid = ml.maloid
WHERE m.statid IN ('AC', 'FI')
GROUP BY DATE_TRUNC('month', m.maintenance_date), m.prcoid, vk.license_plate_number
ORDER BY month DESC, monthly_cost DESC;

COMMENT ON VIEW maintenance.vw_monthly_cost IS
    'Costo mensual por vehículo. Incluye AC + FI.';

-- 4. Cumplimiento del calendario (solo Calendarizado, todos los estados)
CREATE OR REPLACE VIEW maintenance.vw_calendar_compliance AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    m.mainid,
    m.maintenance_date,
    m.mileage                                       AS service_km,
    vs.next_km - vs.interval_km                     AS scheduled_km,
    m.mileage - (vs.next_km - vs.interval_km)       AS km_deviation,
    CASE
        WHEN m.mileage <= (vs.next_km - vs.interval_km) + vs.alert_km_threshold
        THEN 'A tiempo'
        ELSE 'Retrasado'
    END                                             AS compliance_status
FROM maintenance.maintenance            m
JOIN maintenance.maintenance_type        mt ON m.matyid  = mt.matyid
JOIN maintenance.vehicle_schedule        vs ON m.prcoid  = vs.prcoid
JOIN maintenance.vw_vehicle_current_km   vk ON m.prcoid  = vk.prcoid
WHERE m.statid IN ('AC', 'FI')
  AND mt.name = 'Calendarizado'
ORDER BY m.prcoid, m.maintenance_date DESC;

COMMENT ON VIEW maintenance.vw_calendar_compliance IS
    'Cumplimiento del calendario. Incluye AC + FI.';

-- 5. Dashboard BI principal: KPIs
CREATE OR REPLACE VIEW maintenance.vw_bi_dashboard_summary AS
SELECT
    (SELECT COUNT(*) FROM product.vehicle WHERE status = true)
        AS total_vehicles,
    (SELECT COUNT(*) FROM maintenance.maintenance
     WHERE statid IN ('AC', 'FI')
       AND maintenance_date >= DATE_TRUNC('month', CURRENT_DATE))
        AS services_this_month,
    (SELECT ROUND(
        COUNT(*) FILTER (WHERE mt.name = 'Emergencia')::numeric
        / NULLIF(COUNT(*), 0)::numeric * 100, 2)
     FROM maintenance.maintenance m
     JOIN maintenance.maintenance_type mt ON m.matyid = mt.matyid
     WHERE m.statid IN ('AC', 'FI'))
        AS global_emergency_rate_percent,
    (SELECT COUNT(*) FROM maintenance.vw_low_stock)
        AS low_stock_materials,
    (SELECT COUNT(*) FROM maintenance.alert_log WHERE resolved = false)
        AS unresolved_alerts,
    (SELECT COUNT(*) FROM maintenance.vw_expiring_lots)
        AS expiring_lots,
    (SELECT ROUND(AVG(cost_per_km), 4) FROM maintenance.vw_cost_per_km WHERE cost_per_km > 0)
        AS fleet_avg_cost_per_km;

COMMENT ON VIEW maintenance.vw_bi_dashboard_summary IS
    'KPIs principales del dashboard BI. Incluye AC + FI.';
