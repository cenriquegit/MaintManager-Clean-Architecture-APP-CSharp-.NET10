-- ============================================================
-- Seed Data: Poblar Dashboard BI con datos reales
-- Fecha: 2026-05-21
-- Proyecto: MaintManager
-- ============================================================
-- Este script agrega:
--   1. Consumos de material para órdenes ACTIVAS (mainid=36,37,38)
--   2. Una orden de emergencia con consumo
--   3. Lotes por vencer (próximos 30 días)
--   4. Instalaciones de componentes con vida útil
-- ============================================================

BEGIN;

-- ============================================================
-- 1. LOTES POR VENCER (próximos 30 días)
-- ============================================================
-- Estos lotes expiran pronto para que aparezcan en el dashboard

UPDATE maintenance.material_lot
SET expiration_date = CURRENT_DATE + 15
WHERE maloid = 82 AND expiration_date IS DISTINCT FROM CURRENT_DATE + 15;

UPDATE maintenance.material_lot
SET expiration_date = CURRENT_DATE + 25
WHERE maloid = 83 AND expiration_date IS DISTINCT FROM CURRENT_DATE + 25;

-- ============================================================
-- 2. CONSUMOS DE MATERIAL para órdenes ACTIVAS
-- ============================================================
-- mainid=36, 37, 38 son las únicas órdenes activas (statid='AC')
-- Se consumen materiales con costo real para que el dashboard muestre datos

-- mainid=36: Consumo de aceite (mateid=61, maloid=77, costo 25.00/L)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (36, 61, 4.5, 77, 'Stock propio', NOW() - INTERVAL '5 days')
ON CONFLICT DO NOTHING;

-- mainid=37: Consumo de filtro aceite (mateid=67, maloid=82, costo 45.00/uni)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (37, 67, 1, 82, 'Stock propio', NOW() - INTERVAL '3 days')
ON CONFLICT DO NOTHING;

-- mainid=38: Consumo de aceite transmisión (mateid=63, maloid=80, costo 38.50/L)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (38, 63, 2, 80, 'Stock propio', NOW() - INTERVAL '1 day')
ON CONFLICT DO NOTHING;

-- mainid=36: Consumo adicional de filtro
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (36, 67, 1, 82, 'Stock propio', NOW() - INTERVAL '5 days')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 3. NUEVA ORDEN DE EMERGENCIA (activa)
-- ============================================================
-- Para que la tasa de emergencia no sea 0%

INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
SELECT v.prcoid, 2, NULL, v.mileage + 500, 'Falla en sistema eléctrico - Emergencia en ruta', 'Taller propio', 17, 16, 'AC', NOW() - INTERVAL '2 days', NOW(), NOW()
FROM product.vehicle v WHERE v.prcoid = 90 AND NOT EXISTS (SELECT 1 FROM maintenance.maintenance m WHERE m.prcoid = 90 AND m.statid = 'AC')
LIMIT 1;

-- Consumo de material para la orden de emergencia
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 73, 1, ml.maloid, 'Stock propio', NOW() - INTERVAL '2 days'
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 73 AND lot_status = 'activo' LIMIT 1) ml
WHERE m.prcoid = 90 AND m.statid = 'AC' AND m.matyid = 2
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid)
LIMIT 1;

-- Instalar batería como componente en la emergencia
INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, installation_km, installation_date, active)
SELECT m.prcoid, 201, m.mainid, m.mileage, NOW() - INTERVAL '2 days', true
FROM maintenance.maintenance m
WHERE m.prcoid = 90 AND m.statid = 'AC' AND m.matyid = 2
AND NOT EXISTS (SELECT 1 FROM maintenance.installed_component ic WHERE ic.mainid = m.mainid)
LIMIT 1;

-- ============================================================
-- 4. AGREGAR OTRA ORDEN ACTIVA para más variedad en dashboard
-- ============================================================
-- Para vehículo 81 (VDG-361), una orden Calendarizado activa
INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
SELECT v.prcoid, 1, 2, v.mileage, 'Mantenimiento programado', 'Taller propio', 17, 16, 'AC', NOW(), NOW(), NOW()
FROM product.vehicle v WHERE v.prcoid = 81
AND NOT EXISTS (SELECT 1 FROM maintenance.maintenance m WHERE m.prcoid = 81 AND m.statid = 'AC')
LIMIT 1;

-- Consumo de materiales para esta nueva orden
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 73, 1, ml.maloid, 'Stock propio', NOW()
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 73 AND lot_status = 'activo' LIMIT 1) ml
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid)
LIMIT 1;

-- Instalar batería como componente
INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, installation_km, installation_date, active)
SELECT m.prcoid, 201, m.mainid, m.mileage, NOW(), true
FROM maintenance.maintenance m
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.installed_component ic WHERE ic.mainid = m.mainid)
LIMIT 1;

-- Consumo adicional de aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 61, 4.5, ml.maloid, 'Stock propio', NOW()
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 61 AND lot_status = 'activo' ORDER BY expiration_date NULLS LAST LIMIT 1) ml
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid AND mc.mateid = 61)
LIMIT 1;

-- ============================================================
-- 5. CALENDAR COMPLIANCE: actualizar vehicle_schedule
-- ============================================================
-- Asegurar que los vehículos activos tengan schedule configurado

INSERT INTO maintenance.vehicle_schedule (prcoid, interval_km, next_km, alert_km_threshold, next_service_type_code, created_by, status)
SELECT v.prcoid, 5000, v.mileage + 5000, 800, 'A', 16, true
FROM product.vehicle v
WHERE NOT EXISTS (SELECT 1 FROM maintenance.vehicle_schedule vs WHERE vs.prcoid = v.prcoid)
LIMIT 5
ON CONFLICT DO NOTHING;

COMMIT;

-- ============================================================
-- VERIFICACIONES
-- ============================================================
-- SELECT * FROM maintenance.vw_bi_dashboard_summary;
-- SELECT * FROM maintenance.vw_cost_per_km;
-- SELECT * FROM maintenance.vw_emergency_rate;
-- SELECT * FROM maintenance.vw_monthly_cost ORDER BY month;
-- SELECT * FROM maintenance.vw_calendar_compliance;
