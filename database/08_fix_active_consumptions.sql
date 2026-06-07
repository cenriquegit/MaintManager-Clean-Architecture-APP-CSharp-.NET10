-- ============================================================
-- FIX: Agregar consumos a órdenes ACTIVAS sin consumo
-- ============================================================
BEGIN;

-- prcoid=88 (B0J-433, VW Passat) - mainid=272
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 272, 61, 5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 272, 67, 1, (SELECT maloid FROM maintenance.material_lot WHERE mateid=67 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

-- prcoid=89 (D8S-151, Subaru Legacy) - mainid=277
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 277, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

-- prcoid=91 (B9N-233, Volvo S60) - mainid=289
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 289, 61, 4.5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

-- prcoid=93 (F1W-570, Volvo 460) - mainid=297
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 297, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

-- prcoid=95 (APS-421, Chevrolet Cavalier) - mainid=310
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 310, 61, 3.5, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

-- prcoid=96 (V1T-291, SsangYong) - mainid=314
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT 314, 61, 4, (SELECT maloid FROM maintenance.material_lot WHERE mateid=61 AND current_quantity>0 ORDER BY current_quantity DESC LIMIT 1), 'Stock propio', NOW();

COMMIT;

-- Verificación final del dashboard
-- SELECT * FROM maintenance.vw_bi_dashboard_summary;
-- SELECT prcoid, vehicle_name, total_services, ROUND(cost_per_km::numeric,4) as costo_km FROM maintenance.vw_cost_per_km ORDER BY cost_per_km DESC;
