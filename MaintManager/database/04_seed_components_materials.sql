-- ============================================================
-- Seed Data: Componentes con vida útil + materiales con lotes
-- Fecha: 2026-05-21
-- Proyecto: MaintManager
-- ============================================================
-- Este script agrega:
--   1. Componentes con useful_life_days/useful_life_km en action_catalog
--   2. Items de Acción para checklist en action_catalog
--   3. Nuevos materiales con lotes (stock, costo unitario, vencimiento)
--   4. Lotes adicionales para materiales existentes
-- ============================================================

BEGIN;

-- ============================================================
-- 1. ACTUALIZAR items existentes con vida útil
-- ============================================================
-- Estos items ya existen en action_catalog, solo se les agrega
-- useful_life_km / useful_life_days para activar alertas de componente

UPDATE maintenance.action_catalog
SET useful_life_km = 5000, expires_by_time = false
WHERE acatid = 95 AND name = 'Filtro de Aceite de Motor';

UPDATE maintenance.action_catalog
SET useful_life_km = 20000, useful_life_days = 365, expires_by_time = true
WHERE acatid = 96 AND name = 'Filtro de Aire de Motor';

UPDATE maintenance.action_catalog
SET useful_life_km = 30000, expires_by_time = false
WHERE acatid = 98 AND name = 'Bujías (Motor a Gasolina)';

UPDATE maintenance.action_catalog
SET useful_life_km = 90000, expires_by_time = false
WHERE acatid = 99 AND name = 'Correa de Distribución';

UPDATE maintenance.action_catalog
SET useful_life_km = 30000, expires_by_time = false
WHERE acatid = 100 AND name = 'Pastillas de Freno Delanteras';

-- ============================================================
-- 2. INSERTAR nuevos componentes con vida útil
-- ============================================================
-- Categoría: Componente (nuevos items con IDs 201+)

INSERT INTO maintenance.action_catalog (acatid, altoid, name, category, useful_life_days, useful_life_km, expires_by_time, status)
VALUES
    (201, 1, 'Batería 12V 70Ah',         'Componente Eléctrico', 1095, NULL,  true,  true),
    (202, 1, 'Neumáticos 205/55R16',     'Componente Rodaje',    NULL, 50000, false, true),
    (203, 1, 'Kit de Distribución',      'Componente Motor',     NULL, 90000, false, true),
    (204, 1, 'Correa de Alternador',     'Componente Motor',     NULL, 60000, false, true)
ON CONFLICT (acatid) DO NOTHING;

-- ============================================================
-- 3. INSERTAR items de Acción para checklist
-- ============================================================

INSERT INTO maintenance.action_catalog (acatid, altoid, name, category, expires_by_time, status)
VALUES
    (211, 2, 'Revisión general',               'Acción',              false, true),
    (212, 2, 'Cambio de aceite',               'Acción',              false, true),
    (213, 2, 'Alineación y balanceo',          'Acción',              false, true),
    (214, 2, 'Calibración de neumáticos',      'Acción',              false, true),
    (215, 2, 'Revisión de frenos',             'Acción',              false, true),
    (216, 2, 'Revisión de luces',              'Acción',              false, true),
    (217, 2, 'Revisión de suspensión',         'Acción',              false, true),
    (218, 2, 'Escaneo electrónico',            'Acción',              false, true),
    (219, 2, 'Inspección general (Emergencia)','Acción Emergencia',   false, true),
    (220, 2, 'Reparación en ruta',             'Acción Emergencia',   false, true)
ON CONFLICT (acatid) DO NOTHING;

-- ============================================================
-- 4. INSERTAR nuevos materiales
-- ============================================================

INSERT INTO maintenance.material (mateid, macaid, name, unit_of_measure, stock_total, stock_minimum, description, created_by, created_at, updated_at, status)
VALUES
    (73, 1, 'Batería 12V 70Ah',         'Unidad', 4,  2,  'Batería libre de mantenimiento 12V 70Ah',   16, NOW(), NOW(), true),
    (74, 1, 'Neumático 205/55R16',      'Unidad', 10, 4,  'Neumático radial 205/55R16 91V',            16, NOW(), NOW(), true),
    (75, 3, 'Kit de Distribución',      'Unidad', 3,  1,  'Kit distribución completo (correa+tensor)',  16, NOW(), NOW(), true),
    (76, 1, 'Correa de Alternador',     'Unidad', 5,  2,  'Correa poly-V para alternador',             16, NOW(), NOW(), true)
ON CONFLICT (mateid) DO NOTHING;

-- ============================================================
-- 5. INSERTAR lotes para los materiales
-- ============================================================

-- Lotes para materiales EXISTENTES
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by)
VALUES
    -- Aceite Motor 5W-30 Sintético (mateid=61): +1 lote de 15L a 28.00
    (61, 15, 15, 28.00, NOW(), '2027-08-15', 'LOT-ACEITE-002', 'activo', 16),
    -- Filtro de Aceite Premium (mateid=67): +1 lote de 10 uni a 45.00
    (67, 10, 10, 45.00, NOW(), '2026-12-31', 'LOT-FILTRO-001', 'activo', 16);

-- Lotes para materiales NUEVOS
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by)
VALUES
    -- Batería 12V 70Ah (mateid=73)
    (73, 4,  4,  320.00, NOW(), '2028-06-01', 'LOT-BATERIA-001', 'activo', 16),
    -- Neumático 205/55R16 (mateid=74)
    (74, 10, 10, 280.00, NOW(), NULL,          'LOT-NEUM-001',    'activo', 16),
    -- Kit de Distribución (mateid=75)
    (75, 3,  3,  450.00, NOW(), '2027-01-01', 'LOT-DIST-001',    'activo', 16),
    -- Correa de Alternador (mateid=76)
    (76, 5,  5,  35.00,  NOW(), NULL,          'LOT-CORREA-001',  'activo', 16);

-- Actualizar stock_total de materiales existentes que recibieron nuevos lotes
UPDATE maintenance.material SET stock_total = stock_total + 15 WHERE mateid = 61;
UPDATE maintenance.material SET stock_total = stock_total + 10 WHERE mateid = 67;

COMMIT;

-- ============================================================
-- VERIFICACIONES
-- ============================================================
-- Para verificar que los datos se insertaron correctamente:
--   SELECT acatid, name, category, useful_life_days, useful_life_km FROM maintenance.action_catalog WHERE useful_life_days IS NOT NULL OR useful_life_km IS NOT NULL ORDER BY acatid;
--   SELECT acatid, name, category FROM maintenance.action_catalog WHERE category IN ('Acción','Acción Emergencia') ORDER BY acatid;
--   SELECT m.mateid, m.name, m.stock_total, ml.maloid, ml.current_quantity, ml.unit_cost FROM maintenance.material m LEFT JOIN maintenance.material_lot ml ON m.mateid = ml.mateid WHERE m.mateid >= 73 ORDER BY m.mateid, ml.maloid;
