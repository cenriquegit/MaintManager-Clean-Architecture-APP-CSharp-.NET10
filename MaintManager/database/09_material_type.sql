-- Agrega campo type a material para clasificar Material vs Componente
-- Valores: 'Material' (default), 'Componente'

ALTER TABLE maintenance.material
ADD COLUMN IF NOT EXISTS type character varying(20) DEFAULT 'Material' NOT NULL;

COMMENT ON COLUMN maintenance.material.type IS 'Tipo: Material (consumible) o Componente (instalable)';
