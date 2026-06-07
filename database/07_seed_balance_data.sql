-- ============================================================
-- BALANCE DATA: Dashboard BI con datos balanceados
-- Creado: 2026-06-03
-- ============================================================
-- Solo usa materiales CON STOCK real: mateid=61 (aceite), 67 (filtro)
-- ============================================================
BEGIN;

-- ============================================================
-- 1. CONSUMOS REALES PARA VEHÍCULOS CON $0/km
-- ============================================================
-- mainid=269 (prcoid=88, VW Passat): 5L aceite + 1 filtro
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 269, 61, 5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-15'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=269);
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 269, 67, 1, (SELECT maloid FROM maintenance.material_lot WHERE mateid=67 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-15'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=269 AND mateid=67);

-- mainid=270 (prcoid=88, VW Passat): 5L aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 270, 61, 5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-06-01'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=270);
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 270, 67, 1, (SELECT maloid FROM maintenance.material_lot WHERE mateid=67 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-06-01'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=270 AND mateid=67);

-- mainid=25 (prcoid=89, Subaru Legacy): 4L aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 25, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-04-15'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=25);

-- mainid=283 (prcoid=91, Volvo S60 - B9N): 4.5L aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 283, 61, 4.5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-04-20'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=283);

-- mainid=293 (prcoid=93, Volvo 460): 4L aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 293, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-25'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=293);

-- mainid=306 (prcoid=95, Chevrolet Cavalier): 3.5L aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 306, 61, 3.5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-05'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=306);

-- mainid=312 (prcoid=96, SsangYong): 4L aceite + 1 filtro
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 312, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-20'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=312);
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 312, 67, 1, (SELECT maloid FROM maintenance.material_lot WHERE mateid=67 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', '2026-05-20'
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_consumption WHERE mainid=312 AND mateid=67);

-- ============================================================
-- 2. DIAGNÓSTICOS FALTANTES
-- ============================================================
INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, observations, created_at)
SELECT m.mainid,
  CASE (m.mainid % 4) WHEN 0 THEN 'Excelente' WHEN 1 THEN 'Bueno' WHEN 2 THEN 'Regular' ELSE 'Reparado' END,
  true, 'Diagnóstico automático - orden finalizada', m.updated_at
FROM maintenance.maintenance m
WHERE m.statid = 'FI' AND NOT EXISTS (SELECT 1 FROM maintenance.diagnosis d WHERE d.mainid = m.mainid)
LIMIT 30;

-- ============================================================
-- 3. MARCAR ALGUNAS ÓRDENES COMO AC PARA DIFERENTES MESES
-- ============================================================
-- Para que vw_monthly_cost muestre datos en abril, mayo y junio,
-- mantenemos varias órdenes como AC (ya están activas)

-- ============================================================
-- 4. RATINGS PARA CONSUMOS NUEVOS
-- ============================================================
INSERT INTO maintenance.material_rating (mainid, mateid, rating, observation, rated_at, rated_by)
SELECT 269, 61, 4, 'Aceite de buena calidad', NOW(), 17
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_rating WHERE mainid=269 AND mateid=61);

INSERT INTO maintenance.material_rating (mainid, mateid, rating, observation, rated_at, rated_by)
SELECT 270, 61, 3, 'Rendimiento aceptable', NOW(), 17
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_rating WHERE mainid=270 AND mateid=61);

INSERT INTO maintenance.material_rating (mainid, mateid, rating, observation, rated_at, rated_by)
SELECT 312, 61, 5, 'Excelente producto', NOW(), 18
WHERE NOT EXISTS (SELECT 1 FROM maintenance.material_rating WHERE mainid=312 AND mateid=61);

COMMIT;

-- ============================================================
-- VERIFICACIONES
-- ============================================================
-- SELECT * FROM maintenance.vw_bi_dashboard_summary;
-- SELECT prcoid, vehicle_name, total_services, ROUND(cost_per_km::numeric,4) FROM maintenance.vw_cost_per_km ORDER BY cost_per_km DESC;
-- SELECT month, SUM(services_count) as svc, ROUND(SUM(monthly_cost)::numeric,2) as cost FROM maintenance.vw_monthly_cost GROUP BY month ORDER BY month;
