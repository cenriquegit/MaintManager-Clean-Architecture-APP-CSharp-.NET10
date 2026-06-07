-- ============================================================
-- Script 08: Gestión de Vehículos + SUNARP
-- Tabla managed_vehicle + sincronización legacy + 
-- modificaciones a tablas de configuración.
-- ============================================================

BEGIN;

-- ── 1. Tabla de vehículos gestionados ────────────────────────
CREATE TABLE IF NOT EXISTS maintenance.managed_vehicle (
    mv_id SERIAL PRIMARY KEY,
    prcoid INTEGER REFERENCES product.company(prcoid),
    license_plate VARCHAR(20) NOT NULL,
    vehicle_name VARCHAR(200) NOT NULL,
    brand VARCHAR(100),
    model VARCHAR(100),
    year SMALLINT,
    color VARCHAR(50),
    vin VARCHAR(50),
    engine_number VARCHAR(50),
    source VARCHAR(20) NOT NULL DEFAULT 'managed' CHECK (source IN ('legacy', 'managed')),
    status BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(license_plate),
    UNIQUE(prcoid)
);

CREATE INDEX IF NOT EXISTS idx_managed_vehicle_plate
    ON maintenance.managed_vehicle(license_plate);
CREATE INDEX IF NOT EXISTS idx_managed_vehicle_source
    ON maintenance.managed_vehicle(source);
CREATE INDEX IF NOT EXISTS idx_managed_vehicle_status
    ON maintenance.managed_vehicle(status);
CREATE INDEX IF NOT EXISTS idx_managed_vehicle_created
    ON maintenance.managed_vehicle(created_at);


-- ── 2. Sincronizar vehículos legacy (product.company) ────────
INSERT INTO maintenance.managed_vehicle (prcoid, license_plate, vehicle_name, source)
SELECT
    pc.prcoid,
    COALESCE(pv.license_plate_number, 'LEG-' || pc.prcoid),
    COALESCE(pc.description, 'Vehículo ' || pc.prcoid),
    'legacy'
FROM product.company pc
LEFT JOIN product.vehicle pv ON pv.prcoid = pc.prcoid
WHERE pc.prcoid NOT IN (
    SELECT mv.prcoid FROM maintenance.managed_vehicle mv WHERE mv.prcoid IS NOT NULL
)
ORDER BY pc.prcoid;

-- Actualizar campos adicionales desde product.vehicle
UPDATE maintenance.managed_vehicle mv
SET
    brand = pv.vetyid,
    model = pv.futyid,
    year = pv.year_of_manufacture,
    color = pv.color,
    vin = pv.vin_number,
    engine_number = pv.engine_number
FROM product.vehicle pv
WHERE mv.prcoid = pv.prcoid
  AND mv.source = 'legacy';


-- ── 3. Modificar tablas de configuración ─────────────────────
-- vehicle_allowed_action: agregar mv_id y hacer prcoid nullable
ALTER TABLE maintenance.vehicle_allowed_action
    ADD COLUMN IF NOT EXISTS mv_id INTEGER REFERENCES maintenance.managed_vehicle(mv_id),
    ALTER COLUMN prcoid DROP NOT NULL;

ALTER TABLE maintenance.vehicle_allowed_action
    DROP CONSTRAINT IF EXISTS uq_vehicle_allowed_action;

CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_action_legacy
    ON maintenance.vehicle_allowed_action(prcoid, acatid) WHERE prcoid IS NOT NULL AND mv_id IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_action_managed
    ON maintenance.vehicle_allowed_action(mv_id, acatid) WHERE mv_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_action_both
    ON maintenance.vehicle_allowed_action(prcoid, mv_id, acatid) WHERE prcoid IS NOT NULL AND mv_id IS NOT NULL;


-- vehicle_allowed_material: agregar mv_id y hacer prcoid nullable
ALTER TABLE maintenance.vehicle_allowed_material
    ADD COLUMN IF NOT EXISTS mv_id INTEGER REFERENCES maintenance.managed_vehicle(mv_id),
    ALTER COLUMN prcoid DROP NOT NULL;

ALTER TABLE maintenance.vehicle_allowed_material
    DROP CONSTRAINT IF EXISTS uq_vehicle_allowed_material;

CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_material_legacy
    ON maintenance.vehicle_allowed_material(prcoid, mateid) WHERE prcoid IS NOT NULL AND mv_id IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_material_managed
    ON maintenance.vehicle_allowed_material(mv_id, mateid) WHERE mv_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_material_both
    ON maintenance.vehicle_allowed_material(prcoid, mv_id, mateid) WHERE prcoid IS NOT NULL AND mv_id IS NOT NULL;


-- vehicle_allowed_component: agregar mv_id y hacer prcoid nullable
ALTER TABLE maintenance.vehicle_allowed_component
    ADD COLUMN IF NOT EXISTS mv_id INTEGER REFERENCES maintenance.managed_vehicle(mv_id),
    ALTER COLUMN prcoid DROP NOT NULL;

ALTER TABLE maintenance.vehicle_allowed_component
    DROP CONSTRAINT IF EXISTS uq_vehicle_allowed_component;

CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_component_legacy
    ON maintenance.vehicle_allowed_component(prcoid, acatid) WHERE prcoid IS NOT NULL AND mv_id IS NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_component_managed
    ON maintenance.vehicle_allowed_component(mv_id, acatid) WHERE mv_id IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS uq_vehicle_allowed_component_both
    ON maintenance.vehicle_allowed_component(prcoid, mv_id, acatid) WHERE prcoid IS NOT NULL AND mv_id IS NOT NULL;


COMMIT;
