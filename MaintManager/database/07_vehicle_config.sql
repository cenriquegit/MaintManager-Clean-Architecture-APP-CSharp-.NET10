-- ============================================================
-- Script 07: Configuración por Vehículo
-- Tablas para asociar acciones, materiales y componentes
-- permitidos específicamente a cada vehículo de la flota.
-- ============================================================
-- Fecha: 2026-06-06

BEGIN;

-- ── Acciones permitidas por vehículo ──────────────────────────
CREATE TABLE IF NOT EXISTS maintenance.vehicle_allowed_action (
    vaacid SERIAL PRIMARY KEY,
    prcoid INTEGER NOT NULL REFERENCES product.company(prcoid) ON DELETE CASCADE,
    acatid INTEGER NOT NULL REFERENCES maintenance.action_catalog(acatid) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_vehicle_allowed_action UNIQUE (prcoid, acatid)
);

CREATE INDEX IF NOT EXISTS idx_vehicle_allowed_action_prcoid
    ON maintenance.vehicle_allowed_action(prcoid);

-- ── Materiales permitidos por vehículo ────────────────────────
CREATE TABLE IF NOT EXISTS maintenance.vehicle_allowed_material (
    vamid SERIAL PRIMARY KEY,
    prcoid INTEGER NOT NULL REFERENCES product.company(prcoid) ON DELETE CASCADE,
    mateid INTEGER NOT NULL REFERENCES maintenance.material(mateid) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_vehicle_allowed_material UNIQUE (prcoid, mateid)
);

CREATE INDEX IF NOT EXISTS idx_vehicle_allowed_material_prcoid
    ON maintenance.vehicle_allowed_material(prcoid);

-- ── Componentes permitidos por vehículo ───────────────────────
CREATE TABLE IF NOT EXISTS maintenance.vehicle_allowed_component (
    vacoid SERIAL PRIMARY KEY,
    prcoid INTEGER NOT NULL REFERENCES product.company(prcoid) ON DELETE CASCADE,
    acatid INTEGER NOT NULL REFERENCES maintenance.action_catalog(acatid) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_vehicle_allowed_component UNIQUE (prcoid, acatid)
);

CREATE INDEX IF NOT EXISTS idx_vehicle_allowed_component_prcoid
    ON maintenance.vehicle_allowed_component(prcoid);

COMMIT;
