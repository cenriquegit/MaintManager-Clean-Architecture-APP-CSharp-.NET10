-- ============================================================
-- MEGA SEED: Datos masivos congruentes para MaintManager
-- 2026-05-21
-- ============================================================
-- Genera ~100+ maintenance orders con consumos, componentes,
-- diagnósticos, acciones, ratings, schedules y lotes.
-- ============================================================
BEGIN;

-- ============================================================
-- 0. VEHICLE SCHEDULES (para todos los 16 vehículos)
-- ============================================================
DO $$
DECLARE
    v RECORD;
BEGIN
    FOR v IN SELECT prcoid, mileage FROM product.vehicle ORDER BY prcoid LOOP
        IF NOT EXISTS (SELECT 1 FROM maintenance.vehicle_schedule WHERE prcoid = v.prcoid) THEN
            INSERT INTO maintenance.vehicle_schedule (prcoid, interval_km, next_km, alert_km_threshold, next_service_type_code, created_by, status)
            VALUES (v.prcoid, 5000, v.mileage + 5000, 800, 'A', 16, true);
        END IF;
    END LOOP;
END $$;

-- ============================================================
-- 1. MATERIAL LOTS (lotes adicionales con vencimientos variados)
-- ============================================================
INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, entry_date, expiration_date, supplier_lot_number, lot_status, created_by) VALUES
-- Aceite Motor (mateid=61): nuevo lote caro
(61, 20, 20, 32.50, '2026-04-01', '2027-04-01', 'LOT-ACEITE-003', 'activo', 16),
-- Filtro Aceite (mateid=67): lote por vencer
(67, 15, 15, 48.00, '2025-06-15', '2026-06-10', 'LOT-FILTRO-002', 'activo', 16),
-- Pastillas Freno (mateid=70): lote nuevo
(70, 8, 8, 95.00, '2026-05-01', '2028-05-01', 'LOT-FRENO-002', 'activo', 16),
-- Lote expirado (para discard)
(65, 5, 5, 0, '2024-01-15', '2025-01-15', 'LOT-CADUCADO-001', 'vencido', 16),
-- Lote por vencer crítico (7 días)
(74, 3, 3, 280.00, '2026-04-01', CURRENT_DATE + 5, 'LOT-NEUM-002-CRIT', 'activo', 16)
ON CONFLICT DO NOTHING;

-- ============================================================
-- 2. MAINTENANCE ORDERS masivas (80+ órdenes)
-- ============================================================
DO $$
DECLARE
    vehicles INT[] := ARRAY[81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96];
    v_id INT;
    base_km INT;
    m_mainid INT;
    m_date DATE;
    m_type INT;
    m_status BPCHAR(2);
    m_sety INT;
    m_mileage INT;
    m_workid INT;
    m_note TEXT;
    i INT;
    j INT;
    actions INT[] := ARRAY[100,101,102,103,104,105,106,107,211,212,213,214,215,216,217,218];
    comps INT[] := ARRAY[91,92,93,94,95,96,98,99,100,201,202,203,204];
    materials INT[] := ARRAY[61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76];
    r_rating INT;
    r_obs TEXT;
BEGIN
    FOR i IN 1..array_length(vehicles, 1) LOOP
        v_id := vehicles[i];
        
        -- Obtener mileage base
        SELECT COALESCE(mileage, 5000) INTO base_km FROM product.vehicle WHERE prcoid = v_id;
        
        -- 3-6 órdenes históricas por vehículo (2024-2026)
        FOR j IN 1..(3 + (i % 4)) LOOP
            m_date := DATE '2024-01-01' + (random() * 850)::INT;
            m_type := CASE WHEN random() < 0.2 THEN 2 ELSE 1 END; -- 20% emergencia
            m_status := 'FI';
            m_sety := CASE WHEN m_type = 1 THEN (1 + (random() * 1)::INT) ELSE NULL END;
            m_mileage := base_km - 2000 + (j * 3000) + (random() * 1000)::INT;
            IF m_mileage < 1000 THEN m_mileage := 1000 + (j * 2000); END IF;
            m_workid := CASE WHEN random() < 0.4 THEN 17 ELSE 18 END;
            m_note := CASE (random() * 6)::INT
                WHEN 0 THEN 'Cambio de aceite y filtros'
                WHEN 1 THEN 'Revisión de frenos y suspensión'
                WHEN 2 THEN 'Mantenimiento programado'
                WHEN 3 THEN 'Revisión general'
                WHEN 4 THEN 'Alineación y balanceo'
                ELSE 'Servicio completo'
            END;
            
            INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
            VALUES (v_id, m_type, m_sety, m_mileage, m_note, 'Taller propio', m_workid, 16, m_status, m_date, m_date, m_date + 1)
            RETURNING mainid INTO m_mainid;
            
            -- Diagnosis para 80% de órdenes
            IF random() < 0.8 THEN
                INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, observations, future_recommendations, created_at)
                VALUES (m_mainid, 
                    CASE (random() * 3)::INT WHEN 0 THEN 'Excelente' WHEN 1 THEN 'Bueno' WHEN 2 THEN 'Regular' ELSE 'Reparado' END,
                    random() < 0.9,
                    CASE (random() * 3)::INT WHEN 0 THEN 'Sin observaciones' WHEN 1 THEN 'Se recomienda revisar en próximo servicio' ELSE 'Vehículo en buenas condiciones' END,
                    CASE WHEN random() < 0.4 THEN 'Próximo cambio de aceite recomendado' ELSE NULL END,
                    m_date + 1);
            END IF;
            
            -- Consumir 1-3 materiales por orden
            FOR k IN 1..(1 + (random() * 2)::INT) LOOP
                DECLARE
                    c_mateid INT;
                    c_maloid INT;
                    c_qty NUMERIC;
                    c_cost NUMERIC;
                BEGIN
                    c_mateid := materials[1 + (random() * 15)::INT];
                    c_qty := 0.5 + (random() * 4)::NUMERIC;
                    
                    SELECT maloid, unit_cost INTO c_maloid, c_cost
                    FROM maintenance.material_lot 
                    WHERE mateid = c_mateid AND lot_status = 'activo' AND current_quantity > 0
                    ORDER BY expiration_date NULLS LAST LIMIT 1;
                    
                    IF c_maloid IS NOT NULL AND NOT EXISTS (
                        SELECT 1 FROM maintenance.material_consumption WHERE mainid = m_mainid AND maloid = c_maloid
                    ) THEN
                        INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
                        VALUES (m_mainid, c_mateid, c_qty, c_maloid, 'Stock propio', m_date + 1);
                        
                        UPDATE maintenance.material_lot SET current_quantity = GREATEST(current_quantity - c_qty, 0) WHERE maloid = c_maloid;
                    END IF;
                END;
            END LOOP;
            
            -- Instalar 0-2 componentes
            IF random() < 0.4 THEN
                DECLARE
                    comp_acatid INT;
                BEGIN
                    comp_acatid := comps[1 + (random() * 11)::INT];
                    INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, installation_km, installation_date, active)
                    VALUES (v_id, comp_acatid, m_mainid, m_mileage, m_date + 1, true);
                END;
            END IF;
            
            -- 1-3 acciones por orden
            FOR k IN 1..(1 + (random() * 2)::INT) LOOP
                DECLARE
                    a_acatid INT;
                BEGIN
                    a_acatid := actions[1 + (random() * 15)::INT];
                    IF NOT EXISTS (SELECT 1 FROM maintenance.maintenance_action_detail WHERE mainid = m_mainid AND acatid = a_acatid) THEN
                        INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
                        VALUES (m_mainid, a_acatid, random() < 0.8, CASE WHEN random() < 0.5 THEN NULL ELSE 'R' END);
                    END IF;
                END;
            END LOOP;
        END LOOP;
        
        -- 1 orden ACTIVA por vehículo (si no tiene ya)
        IF NOT EXISTS (SELECT 1 FROM maintenance.maintenance WHERE prcoid = v_id AND statid = 'AC') THEN
            m_type := CASE WHEN random() < 0.15 THEN 2 ELSE 1 END;
            m_mileage := base_km + (random() * 2000)::INT;
            m_sety := CASE WHEN m_type = 1 THEN (1 + (random() * 1)::INT) ELSE NULL END;
            m_workid := CASE WHEN random() < 0.4 THEN 17 ELSE 18 END;
            
            INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
            VALUES (v_id, m_type, m_sety, m_mileage, 'Mantenimiento en proceso', 'Taller propio', m_workid, 16, 'AC', 
                NOW() - ((random() * 10)::INT * INTERVAL '1 day'), 
                NOW() - ((random() * 10)::INT * INTERVAL '1 day'), NOW())
            RETURNING mainid INTO m_mainid;
            
            -- Consumir 1 material en la orden activa
            DECLARE
                c_mateid INT := materials[1 + (random() * 15)::INT];
                c_maloid INT;
            BEGIN
                SELECT maloid INTO c_maloid FROM maintenance.material_lot 
                WHERE mateid = c_mateid AND lot_status = 'activo' AND current_quantity > 0
                ORDER BY expiration_date NULLS LAST LIMIT 1;
                IF c_maloid IS NOT NULL THEN
                    INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
                    VALUES (m_mainid, c_mateid, 1 + (random() * 3)::INT, c_maloid, 'Stock propio', NOW());
                END IF;
            END;
        END IF;
    END LOOP;
END $$;

-- ============================================================
-- 3. ALERT LOG (alertas adicionales)
-- ============================================================
INSERT INTO maintenance.alert_log (alcoid, prcoid, mateid, message, alert_date, read, resolved)
SELECT 1, v.prcoid, NULL, 
    'El vehículo ' || v.license_plate_number || ' tiene mantenimiento próximo. Km actual: ' || COALESCE(v.mileage::TEXT, '?') || '.',
    NOW() - ((random() * 30)::INT * INTERVAL '1 day'), 
    random() < 0.3, 
    random() < 0.1
FROM product.vehicle v
WHERE random() < 0.3
AND NOT EXISTS (SELECT 1 FROM maintenance.alert_log al WHERE al.prcoid = v.prcoid AND al.alcoid = 1);

-- Alertas de stock bajo
INSERT INTO maintenance.alert_log (alcoid, mateid, message, alert_date, read, resolved)
SELECT 3, m.mateid, 'Stock bajo de ' || m.name || '. Stock actual: ' || m.stock_total::TEXT || ' ' || m.unit_of_measure || '. Mínimo: ' || m.stock_minimum::TEXT || '.',
    NOW() - ((random() * 15)::INT * INTERVAL '1 day'), random() < 0.5, random() < 0.2
FROM maintenance.material m
WHERE m.stock_total < m.stock_minimum
AND NOT EXISTS (SELECT 1 FROM maintenance.alert_log al WHERE al.mateid = m.mateid AND al.alcoid = 3);

-- ============================================================
-- 4. MATERIAL RATINGS (calificaciones para consumos existentes)
-- ============================================================
INSERT INTO maintenance.material_rating (mainid, mateid, rating, observation, rated_at, rated_by)
SELECT mc.mainid, mc.mateid, 
    r.rating_val,
    CASE WHEN r.rating_val <= 3 THEN 'Requiere mejorar calidad del material' ELSE NULL END,
    NOW() - ((random() * 60)::INT * INTERVAL '1 day'),
    CASE WHEN random() < 0.5 THEN 17 ELSE 18 END
FROM maintenance.material_consumption mc
CROSS JOIN LATERAL (SELECT (1 + (random() * 4)::INT) AS rating_val) r
WHERE random() < 0.6
  AND NOT EXISTS (SELECT 1 FROM maintenance.material_rating mr WHERE mr.mainid = mc.mainid AND mr.mateid = mc.mateid);

-- ============================================================
-- 5. TECHNICIAN ASSIGNMENTS
-- ============================================================
INSERT INTO maintenance.technician_assignment (mainid, workid, assigned_by, assigned_at)
SELECT m.mainid, m.assigned_to, 16, m.created_at
FROM maintenance.maintenance m
WHERE m.assigned_to IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM maintenance.technician_assignment ta WHERE ta.mainid = m.mainid);

-- ============================================================
-- 6. ACTUALIZAR LOTES POR VENCER (para dashboard)
-- ============================================================
UPDATE maintenance.material_lot SET expiration_date = CURRENT_DATE + 8
WHERE maloid IN (SELECT maloid FROM maintenance.material_lot WHERE lot_status = 'activo' AND expiration_date IS NOT NULL ORDER BY expiration_date LIMIT 1);

UPDATE maintenance.material_lot SET expiration_date = CURRENT_DATE + 20
WHERE maloid IN (SELECT maloid FROM maintenance.material_lot WHERE lot_status = 'activo' AND expiration_date IS NOT NULL AND expiration_date > CURRENT_DATE + 15 ORDER BY expiration_date LIMIT 1);

-- ============================================================
-- RESUMEN
-- ============================================================
DO $$
DECLARE
    order_count INT;
    consumption_count INT;
    component_count INT;
    diagnosis_count INT;
    action_count INT;
    lot_count INT;
    rating_count INT;
    alert_count INT;
    active_count INT;
BEGIN
    SELECT COUNT(*) INTO order_count FROM maintenance.maintenance;
    SELECT COUNT(*) INTO consumption_count FROM maintenance.material_consumption;
    SELECT COUNT(*) INTO component_count FROM maintenance.installed_component;
    SELECT COUNT(*) INTO diagnosis_count FROM maintenance.diagnosis;
    SELECT COUNT(*) INTO action_count FROM maintenance.maintenance_action_detail;
    SELECT COUNT(*) INTO lot_count FROM maintenance.material_lot WHERE lot_status = 'activo';
    SELECT COUNT(*) INTO rating_count FROM maintenance.material_rating;
    SELECT COUNT(*) INTO alert_count FROM maintenance.alert_log WHERE NOT resolved;
    SELECT COUNT(*) INTO active_count FROM maintenance.maintenance WHERE statid = 'AC';
    
    RAISE NOTICE '=== DATOS GENERADOS ===';
    RAISE NOTICE 'Órdenes mantenimiento: %', order_count;
    RAISE NOTICE '  → Activas (AC): %', active_count;
    RAISE NOTICE 'Consumos materiales: %', consumption_count;
    RAISE NOTICE 'Componentes instalados: %', component_count;
    RAISE NOTICE 'Diagnósticos: %', diagnosis_count;
    RAISE NOTICE 'Acciones realizadas: %', action_count;
    RAISE NOTICE 'Lotes activos: %', lot_count;
    RAISE NOTICE 'Calificaciones: %', rating_count;
    RAISE NOTICE 'Alertas no resueltas: %', alert_count;
    RAISE NOTICE '========================';
END $$;

COMMIT;
