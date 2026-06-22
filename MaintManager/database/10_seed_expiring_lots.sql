-- ============================================================
-- Seed Data: Lotes con fechas de vencimiento variadas
-- Para visualizar correctamente el gráfico de "Lotes Próximos a Vencer"
-- ============================================================
-- Hoy: 2026-06-18
-- Crítico (≤7 días): vence antes del 2026-06-25
-- Advertencia (8-30 días): vence entre 2026-06-26 y 2026-07-18
-- Normal (>30 días o sin fecha): vence después del 2026-07-18 o NULL
-- ============================================================

BEGIN;

-- ── Lotes CRÍTICOS (≤7 días) ────────────────────────────────
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by)
VALUES
    (61, 8,  8,  25.00, '2026-05-01', '2026-06-20', 'LOT-ACEITE-CRIT1', 'activo', 16),
    (65, 6,  6,  18.00, '2026-05-15', '2026-06-22', 'LOT-FILTROAIRE-CRIT', 'activo', 16),
    (67, 5,  5,  42.00, '2026-04-20', '2026-06-24', 'LOT-FILTROACEITE-CRIT', 'activo', 16),
    (68, 3,  3,  80.00, '2026-03-10', '2026-06-21', 'LOT-BUJIA-CRIT', 'activo', 16),
    (73, 2,  2, 310.00, '2026-02-01', '2026-06-23', 'LOT-BATERIA-CRIT', 'activo', 16);

-- ── Lotes ADVERTENCIA (8-30 días) ────────────────────────────
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by)
VALUES
    (61, 10, 10, 26.00, '2026-06-01', '2026-07-05', 'LOT-ACEITE-WARN1', 'activo', 16),
    (66, 8,  8,  55.00, '2026-05-20', '2026-07-10', 'LOT-DIESEL-WARN', 'activo', 16),
    (67, 12, 12, 44.00, '2026-06-10', '2026-07-15', 'LOT-FILTROACEITE-WARN', 'activo', 16),
    (63, 4,  4,  22.00, '2026-04-01', '2026-07-08', 'LOT-ACEITE10W-WARN', 'activo', 16);

-- ── Lotes NORMALES (>30 días) ────────────────────────────────
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by)
VALUES
    (61, 20, 20, 27.00, '2026-06-15', '2027-12-31', 'LOT-ACEITE-NORM1', 'activo', 16),
    (61, 15, 15, 28.00, '2026-06-15', '2027-08-15', 'LOT-ACEITE-NORM2', 'activo', 16),
    (67, 10, 10, 45.00, '2026-06-18', '2026-12-31', 'LOT-FILTROACEITE-NORM', 'activo', 16),
    (73, 4,  4, 320.00, '2026-06-18', '2028-06-01', 'LOT-BATERIA-NORM', 'activo', 16),
    (75, 3,  3, 450.00, '2026-06-18', '2027-01-01', 'LOT-DIST-NORM', 'activo', 16);

-- Actualizar stock_total de los materiales afectados
UPDATE maintenance.material SET stock_total = 
    (SELECT COALESCE(SUM(current_quantity), 0) FROM maintenance.material_lot WHERE mateid = material.mateid AND lot_status = 'activo')
WHERE mateid IN (61, 63, 65, 66, 67, 68, 73, 75);

COMMIT;

-- ============================================================
-- VERIFICACIÓN
-- ============================================================
-- SELECT mat.name, ml.current_quantity, ml.expiration_date,
--        (ml.expiration_date - CURRENT_DATE) as days_until,
--        CASE
--            WHEN (ml.expiration_date - CURRENT_DATE) <= 7 THEN 'CRÍTICO'
--            WHEN (ml.expiration_date - CURRENT_DATE) <= 30 THEN 'ADVERTENCIA'
--            ELSE 'NORMAL'
--        END as categoria
-- FROM maintenance.material_lot ml
-- JOIN maintenance.material mat ON ml.mateid = mat.mateid
-- WHERE ml.lot_status = 'activo' AND ml.current_quantity > 0
-- ORDER BY ml.expiration_date;
