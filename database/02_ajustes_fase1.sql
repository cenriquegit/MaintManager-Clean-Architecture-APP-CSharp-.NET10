-- =====================================================================
-- AJUSTES COMPLEMENTARIOS — MaintManager Fase 1
-- Aplicar DESPUÉS del script principal (BD-FINAL COMPLETAMENTE CORREGIDA)
-- =====================================================================
-- NOTA: La nueva BD ya incluye todas las correcciones F1-F11.
-- Este script agrega SOLO lo necesario para el sistema de mantenimiento
-- que no está en la BD base.
-- =====================================================================

-- ─────────────────────────────────────────────────────────────────────
-- [A1] Campo assigned_to en maintenance.maintenance
-- POSICIÓN: Después de "workid integer NOT NULL" en la tabla maintenance
-- POR QUÉ: workid = quien REGISTRA la orden (puede ser el jefe).
--          assigned_to = mecánico que EJECUTA el trabajo físico.
--          Con 2 mecánicos + 2 jefes esta diferencia es crítica para BI.
-- ─────────────────────────────────────────────────────────────────────
ALTER TABLE maintenance.maintenance
    ADD COLUMN IF NOT EXISTS assigned_to integer;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'maintenance_assigned_to_fkey') THEN
        ALTER TABLE maintenance.maintenance
            ADD CONSTRAINT maintenance_assigned_to_fkey
                FOREIGN KEY (assigned_to) REFERENCES public.worker (workid);
    END IF;
END;
$$;

COMMENT ON COLUMN maintenance.maintenance.assigned_to IS
    'Mecánico que ejecuta físicamente el trabajo. '
    'Diferente de workid (quien registra la orden en el sistema).';

CREATE INDEX IF NOT EXISTS idx_maintenance_assigned_to
    ON maintenance.maintenance (assigned_to);

CREATE INDEX IF NOT EXISTS idx_maintenance_statid_matyid
    ON maintenance.maintenance (statid, matyid);

CREATE INDEX IF NOT EXISTS idx_maintenance_assigned_date
    ON maintenance.maintenance (assigned_to, maintenance_date DESC);

-- ─────────────────────────────────────────────────────────────────────
-- [A2] Campo updated_at en maintenance.material
-- POSICIÓN: Después de "created_at timestamp..." en la tabla material
-- POR QUÉ: Auditoría completa. Los dashboards BI necesitan saber
--          cuándo cambió el stock de un material.
-- ─────────────────────────────────────────────────────────────────────
ALTER TABLE maintenance.material
    ADD COLUMN IF NOT EXISTS updated_at timestamp without time zone
        NOT NULL DEFAULT CURRENT_TIMESTAMP;

COMMENT ON COLUMN maintenance.material.updated_at IS
    'Fecha de última modificación. Se actualiza con cada cambio de stock.';

-- ─────────────────────────────────────────────────────────────────────
-- [A3] Campo next_service_type_code en maintenance.vehicle_schedule
-- POSICIÓN: Después de "alert_km_threshold" en vehicle_schedule
-- POR QUÉ: Indica si el próximo servicio es A o B.
--          El patrón A→B→A→B se alterna automáticamente al recalendarizar.
-- ─────────────────────────────────────────────────────────────────────
ALTER TABLE maintenance.vehicle_schedule
    ADD COLUMN IF NOT EXISTS next_service_type_code character(1) DEFAULT 'A';

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'vehicle_schedule_svc_type_check') THEN
        ALTER TABLE maintenance.vehicle_schedule
            ADD CONSTRAINT vehicle_schedule_svc_type_check
                CHECK (next_service_type_code IS NULL OR
                       next_service_type_code = ANY (ARRAY['A'::character, 'B'::character]));
    END IF;
END;
$$;

COMMENT ON COLUMN maintenance.vehicle_schedule.next_service_type_code IS
    'Tipo del próximo servicio: A (básico) o B (completo). '
    'Se alterna automáticamente: A→B→A→B al recalendarizar.';

-- ─────────────────────────────────────────────────────────────────────
-- [A4] Tabla maintenance.technician_assignment (escalabilidad futura)
-- POSICIÓN: Al final, después de maintenance.alert_log
-- POR QUÉ: Hoy hay 2 mecánicos (1 por orden). Mañana puede haber 5.
--          Esta tabla permite asignar múltiples técnicos a una orden
--          sin cambiar el modelo actual. Hoy tiene 1 registro por orden.
-- ─────────────────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS maintenance.technician_assignment_teasid_seq;

CREATE TABLE IF NOT EXISTS maintenance.technician_assignment
(
    teasid      integer NOT NULL DEFAULT nextval('maintenance.technician_assignment_teasid_seq'),
    mainid      integer NOT NULL,
    workid      integer NOT NULL,
    role_in_job character varying(50) DEFAULT 'Principal',
    assigned_at timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    assigned_by integer NOT NULL,
    CONSTRAINT technician_assignment_pkey PRIMARY KEY (teasid),
    CONSTRAINT ta_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT ta_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT ta_assigned_by_fkey FOREIGN KEY (assigned_by)
        REFERENCES public.worker (workid),
    CONSTRAINT ta_mainid_workid_unique UNIQUE (mainid, workid)
);

COMMENT ON TABLE maintenance.technician_assignment IS
    'Asignación de técnicos a órdenes. Hoy: 1 registro por orden. '
    'Escalable: múltiples técnicos en el futuro. '
    'role_in_job: Principal, Asistente, Especialista.';

CREATE INDEX IF NOT EXISTS idx_ta_mainid ON maintenance.technician_assignment (mainid);
CREATE INDEX IF NOT EXISTS idx_ta_workid ON maintenance.technician_assignment (workid);

-- ─────────────────────────────────────────────────────────────────────
-- VERIFICACIÓN FINAL
-- ─────────────────────────────────────────────────────────────────────
DO $$
DECLARE v boolean;
BEGIN
    SELECT EXISTS (SELECT 1 FROM information_schema.columns
        WHERE table_schema='maintenance' AND table_name='maintenance'
          AND column_name='assigned_to') INTO v;
    IF v THEN RAISE NOTICE '[OK] maintenance.assigned_to';
    ELSE RAISE WARNING '[FALLA] maintenance.assigned_to'; END IF;

    SELECT EXISTS (SELECT 1 FROM information_schema.columns
        WHERE table_schema='maintenance' AND table_name='material'
          AND column_name='updated_at') INTO v;
    IF v THEN RAISE NOTICE '[OK] material.updated_at';
    ELSE RAISE WARNING '[FALLA] material.updated_at'; END IF;

    SELECT EXISTS (SELECT 1 FROM information_schema.columns
        WHERE table_schema='maintenance' AND table_name='vehicle_schedule'
          AND column_name='next_service_type_code') INTO v;
    IF v THEN RAISE NOTICE '[OK] vehicle_schedule.next_service_type_code';
    ELSE RAISE WARNING '[FALLA] vehicle_schedule.next_service_type_code'; END IF;

    SELECT EXISTS (SELECT 1 FROM information_schema.tables
        WHERE table_schema='maintenance'
          AND table_name='technician_assignment') INTO v;
    IF v THEN RAISE NOTICE '[OK] technician_assignment';
    ELSE RAISE WARNING '[FALLA] technician_assignment'; END IF;

    RAISE NOTICE 'Verificación completada.';
END;
$$;