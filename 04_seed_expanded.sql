-- =====================================================================
-- SEED EXPANDIDO — MaintManager Demo Completo
-- Ejecutar DESPUÉS de: 03_seed_data.sql
-- Agrega +200 registros para que todas las funcionalidades tengan datos
-- =====================================================================
-- TABLAS QUE POBLA:
--   maintenance.maintenance          ~50 registros
--   maintenance.diagnosis            ~30 registros
--   maintenance.material_consumption ~60 registros
--   maintenance.material_lot         ~15 lotes adicionales
--   maintenance.installed_component  ~24 registros
--   maintenance.maintenance_action_detail  ~40 registros
--   maintenance.alert_log            ~15 registros
--   maintenance.material_rating      ~12 registros
--   maintenance.technician_assignment ~10 registros
--   maintenance.schedule_action      ~36 registros
-- TOTAL: ~300+ registros nuevos
-- =====================================================================

BEGIN;

-- ─────────────────────────────────────────────────────────────────────
-- OBTENER IDs DE REFERENCIA
-- ─────────────────────────────────────────────────────────────────────
DO $$
DECLARE
    v_jefe   integer; v_mec1  integer; v_mec2  integer;
    v_cal    smallint; v_eme   smallint;
    v_sa     smallint; v_sb    smallint;
    v_ac     character(2) := 'AC';
    v_prcoid integer; v_plate varchar; v_prodid integer; v_km integer;
    v_mainid integer;

    v_mat_5w30   integer;
    v_mat_10w40  integer;
    v_mat_filtro_aceite_toyota integer;
    v_mat_filtro_aceite_hyundai integer;
    v_mat_filtro_aire_toyota integer;
    v_mat_filtro_combustible integer;
    v_mat_liquido_frenos integer;
    v_mat_refrigerante integer;
    v_mat_pastillas_freno integer;
    v_mat_bujias integer;
    v_mat_correa integer;
    v_mat_aceite_transmision integer;

    v_maloid_5w30_lote1 integer;
    v_maloid_5w30_lote2 integer;
    v_maloid_filtro_aceite_toyota integer;
    v_maloid_filtro_aceite_hyundai integer;
    v_maloid_filtro_aire_toyota integer;
    v_maloid_pastillas_freno integer;
    v_maloid_bujias integer;
    v_maloid_liquido_frenos integer;
    v_maloid_refrigerante integer;
    v_maloid_10w40 integer;
    v_maloid_filtro_combustible integer;

    v_acat_aceite_motor integer;
    v_acat_filtro_aceite integer;
    v_acat_filtro_aire integer;
    v_acat_filtro_combustible integer;
    v_acat_bujias integer;
    v_acat_pastillas_freno integer;
    v_acat_liquido_frenos integer;
    v_acat_refrigerante integer;
    v_acat_correa_distribucion integer;
    v_acat_lubricar_puertas integer;
    v_acat_revisar_luces integer;
    v_acat_revisar_frenos integer;
    v_acat_rotacion_neumaticos integer;
    v_acat_escaneo integer;
    v_acat_bateria integer;

    v_alco_mantenimiento integer;
    v_alco_componente integer;
    v_alco_lote integer;
    v_alco_stock integer;
    v_provid_repuestos integer;
    v_provid_aceites integer;
    v_persid_proveedor_aceites integer;

    v_veshid integer;
    v_idx integer := 0;
    v_count integer;
    v_vehicles_prcoid integer[]; v_vehicles_plate varchar[]; v_vehicles_prodid integer[]; v_vehicles_km integer[];
    v_vehicle_types integer[];
    v_main_ids integer[];
    v_calendar_main_ids integer[];
    v_rnd real;
BEGIN
    -- Trabajadores
    SELECT workid INTO v_jefe FROM public.worker WHERE username = 'herror.ortiz';
    SELECT workid INTO v_mec1 FROM public.worker WHERE username = 'juan.quispe';
    SELECT workid INTO v_mec2 FROM public.worker WHERE username = 'pedro.mamani';

    -- Tipos de mantenimiento
    SELECT matyid INTO v_cal FROM maintenance.maintenance_type WHERE name = 'Calendarizado';
    SELECT matyid INTO v_eme FROM maintenance.maintenance_type WHERE name = 'Emergencia';

    -- Tipos de servicio
    SELECT setyid INTO v_sa FROM maintenance.service_type WHERE code = 'A';
    SELECT setyid INTO v_sb FROM maintenance.service_type WHERE code = 'B';

    -- Materiales
    SELECT mateid INTO v_mat_5w30 FROM maintenance.material WHERE name = 'Aceite Motor 5W-30 Sintético';
    SELECT mateid INTO v_mat_10w40 FROM maintenance.material WHERE name = 'Aceite Motor 10W-40 Semi-sintético';
    SELECT mateid INTO v_mat_filtro_aceite_toyota FROM maintenance.material WHERE name = 'Filtro de Aceite Toyota';
    SELECT mateid INTO v_mat_filtro_aceite_hyundai FROM maintenance.material WHERE name = 'Filtro de Aceite Hyundai/Kia';
    SELECT mateid INTO v_mat_filtro_aire_toyota FROM maintenance.material WHERE name = 'Filtro de Aire Toyota';
    SELECT mateid INTO v_mat_filtro_combustible FROM maintenance.material WHERE name = 'Filtro de Combustible Diésel';
    SELECT mateid INTO v_mat_liquido_frenos FROM maintenance.material WHERE name = 'Líquido de Frenos DOT 4';
    SELECT mateid INTO v_mat_refrigerante FROM maintenance.material WHERE name = 'Refrigerante Long Life 50/50';
    SELECT mateid INTO v_mat_pastillas_freno FROM maintenance.material WHERE name = 'Pastillas de Freno Delanteras';
    SELECT mateid INTO v_mat_bujias FROM maintenance.material WHERE name = 'Bujías NGK (set x4)';
    SELECT mateid INTO v_mat_correa FROM maintenance.material WHERE name = 'Correa de Distribución';
    SELECT mateid INTO v_mat_aceite_transmision FROM maintenance.material WHERE name = 'Aceite Transmisión ATF';

    -- Lotes existentes
    SELECT maloid INTO v_maloid_5w30_lote1 FROM maintenance.material_lot WHERE mateid = v_mat_5w30 AND lot_status = 'activo' ORDER BY maloid LIMIT 1;
    SELECT maloid INTO v_maloid_5w30_lote2 FROM maintenance.material_lot WHERE mateid = v_mat_5w30 AND lot_status = 'activo' ORDER BY maloid LIMIT 1 OFFSET 1;
    SELECT maloid INTO v_maloid_filtro_aceite_toyota FROM maintenance.material_lot WHERE mateid = v_mat_filtro_aceite_toyota AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_filtro_aceite_hyundai FROM maintenance.material_lot WHERE mateid = v_mat_filtro_aceite_hyundai AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_filtro_aire_toyota FROM maintenance.material_lot WHERE mateid = v_mat_filtro_aire_toyota AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_pastillas_freno FROM maintenance.material_lot WHERE mateid = v_mat_pastillas_freno AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_bujias FROM maintenance.material_lot WHERE mateid = v_mat_bujias AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_liquido_frenos FROM maintenance.material_lot WHERE mateid = v_mat_liquido_frenos AND lot_status = 'activo' LIMIT 1;
    SELECT maloid INTO v_maloid_refrigerante FROM maintenance.material_lot WHERE mateid = v_mat_refrigerante AND lot_status = 'activo' LIMIT 1;

    -- Catálogo de acciones
    SELECT acatid INTO v_acat_aceite_motor FROM maintenance.action_catalog WHERE name LIKE 'Aceite de Motor%' LIMIT 1;
    SELECT acatid INTO v_acat_filtro_aceite FROM maintenance.action_catalog WHERE name LIKE 'Filtro de Aceite de Motor%' LIMIT 1;
    SELECT acatid INTO v_acat_filtro_aire FROM maintenance.action_catalog WHERE name LIKE 'Filtro de Aire de Motor%' LIMIT 1;
    SELECT acatid INTO v_acat_filtro_combustible FROM maintenance.action_catalog WHERE name LIKE 'Filtro de Combustible%' LIMIT 1;
    SELECT acatid INTO v_acat_bujias FROM maintenance.action_catalog WHERE name LIKE 'Bujías%' LIMIT 1;
    SELECT acatid INTO v_acat_pastillas_freno FROM maintenance.action_catalog WHERE name LIKE 'Pastillas%' LIMIT 1;
    SELECT acatid INTO v_acat_correa_distribucion FROM maintenance.action_catalog WHERE name LIKE 'Correa de Distribución de Motor%' LIMIT 1;
    SELECT acatid INTO v_acat_liquido_frenos FROM maintenance.action_catalog WHERE name LIKE 'Líquido de Frenos%' LIMIT 1;
    SELECT acatid INTO v_acat_refrigerante FROM maintenance.action_catalog WHERE name LIKE 'Líquido Refrigerante%' LIMIT 1;
    SELECT acatid INTO v_acat_lubricar_puertas FROM maintenance.action_catalog WHERE name LIKE 'Lubricar bisagras%' LIMIT 1;
    SELECT acatid INTO v_acat_revisar_luces FROM maintenance.action_catalog WHERE name LIKE 'Revisar luces%' LIMIT 1;
    SELECT acatid INTO v_acat_revisar_frenos FROM maintenance.action_catalog WHERE name LIKE 'Revisar líneas de freno%' LIMIT 1;
    SELECT acatid INTO v_acat_rotacion_neumaticos FROM maintenance.action_catalog WHERE name LIKE 'Rotación de neumáticos%' LIMIT 1;
    SELECT acatid INTO v_acat_escaneo FROM maintenance.action_catalog WHERE name LIKE 'Escaneo de unidad%' LIMIT 1;
    SELECT acatid INTO v_acat_bateria FROM maintenance.action_catalog WHERE name LIKE 'Bornes de batería%' LIMIT 1;

    -- Alert config
    SELECT alcoid INTO v_alco_mantenimiento FROM maintenance.alert_config WHERE alert_type = 'MANTENIMIENTO_PROXIMO_KM';
    SELECT alcoid INTO v_alco_componente FROM maintenance.alert_config WHERE alert_type = 'COMPONENTE_POR_CADUCAR';
    SELECT alcoid INTO v_alco_lote FROM maintenance.alert_config WHERE alert_type = 'LOTE_POR_VENCER';
    SELECT alcoid INTO v_alco_stock FROM maintenance.alert_config WHERE alert_type = 'STOCK_BAJO';

    -- Proveedor
    SELECT provid INTO v_provid_repuestos FROM public.provider
        JOIN public.person p ON provider.persid = p.persid WHERE p.document = '20601234561' LIMIT 1;

    -- Cargar vehículos en arrays
    SELECT array_agg(v.prcoid ORDER BY v.prcoid),
           array_agg(v.license_plate_number ORDER BY v.prcoid),
           array_agg(v.prodid ORDER BY v.prcoid),
           array_agg(COALESCE(
               (SELECT re.kilometer_end FROM service.rentexecute re
                JOIN service.rentrequest rr ON re.sereid = rr.sereid
                WHERE rr.prodid = v.prodid AND re.kilometer_end IS NOT NULL
                ORDER BY re.return_date DESC LIMIT 1), v.mileage) ORDER BY v.prcoid)
    INTO v_vehicles_prcoid, v_vehicles_plate, v_vehicles_prodid, v_vehicles_km
    FROM product.vehicle v WHERE v.status = true;

    -- ─────────────────────────────────────────────────────────────────
    -- 2. LOTES ADICIONALES (+12 lotes)
    -- ─────────────────────────────────────────────────────────────────
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_5w30, 20.0, 20.0, 30.00, CURRENT_DATE + INTERVAL '15 days',
            v_provid_repuestos, 'LOT-PRX-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_liquido_frenos, 10.0, 10.0, 38.00, CURRENT_DATE + INTERVAL '20 days',
            v_provid_repuestos, 'LOT-PRX-002', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_refrigerante, 12.0, 3.0, 20.00, CURRENT_DATE - INTERVAL '10 days',
            v_provid_repuestos, 'LOT-VNC-001', 'vencido', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_bujias, 8, 6, 65.00, '2027-08-01', v_provid_repuestos, 'LOT-BUJ-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_correa, 6, 6, 120.00, '2028-01-01', v_provid_repuestos, 'LOT-COR-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_filtro_combustible, 8, 8, 45.00, v_provid_repuestos, 'LOT-FCM-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_5w30, 10.0, 0.0, 27.00, '2026-12-01', v_provid_repuestos, 'LOT-5W30-AGR', 'agotado', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_refrigerante, 6.0, 0.0, 19.00, '2026-01-15', v_provid_repuestos, 'LOT-REF-DSC', 'descartado', v_jefe);

    -- Crear proveedor de aceites
    INSERT INTO public.person (enstid, iddoid, document, fln, name, sex, birthdate)
    SELECT 'AC', iddoid, '20601234562', 'Proveedor', 'Lubricantes del Sur S.A.C.', 'M', '2000-01-01'
    FROM list.identitydocumenttype WHERE name = 'RUC'
    ON CONFLICT DO NOTHING;

    SELECT persid INTO v_persid_proveedor_aceites FROM public.person WHERE document = '20601234562';
    INSERT INTO public.provider (persid, workid, status)
    VALUES (v_persid_proveedor_aceites, v_jefe, true)
    ON CONFLICT DO NOTHING;
    SELECT provid INTO v_provid_aceites FROM public.provider WHERE persid = v_persid_proveedor_aceites;

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_10w40, 30.0, 28.0, 24.00, '2027-11-01', v_provid_aceites, 'ACE-10W40-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_aceite_transmision, 15.0, 15.0, 45.00, '2028-03-01', v_provid_aceites, 'ACE-ATF-001', 'activo', v_jefe);

    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, supplier_lot_number, lot_status, created_by)
    VALUES (v_mat_5w30, 30.0, 30.0, 31.50, '2027-10-01', v_provid_aceites, 'ACE-5W30-003', 'activo', v_jefe);

    -- Obtener lote nuevo de 10w40
    SELECT maloid INTO v_maloid_10w40 FROM maintenance.material_lot WHERE supplier_lot_number = 'ACE-10W40-001' LIMIT 1;
    SELECT maloid INTO v_maloid_filtro_combustible FROM maintenance.material_lot WHERE supplier_lot_number = 'LOT-FCM-001' LIMIT 1;

    -- ─────────────────────────────────────────────────────────────────
    -- 3. MANTENIMIENTOS MASIVOS (~48 registros)
    --    4 mantenimientos por cada uno de los 12 vehículos
    -- ─────────────────────────────────────────────────────────────────
    FOR v_idx IN 1..cardinality(v_vehicles_prcoid)
    LOOP
        v_prcoid := v_vehicles_prcoid[v_idx];
        v_plate := v_vehicles_plate[v_idx];
        v_prodid := v_vehicles_prodid[v_idx];
        v_km := v_vehicles_km[v_idx];
        v_rnd := random();

        -- Mantenimiento 1: 2024 - Calendarizado A (todos)
        INSERT INTO maintenance.maintenance
            (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
             oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
             origin_service, assigned_to, workid, statid, note)
        VALUES (v_prcoid, v_cal, v_sa,
                'ORD-2024-' || LPAD(v_idx::text, 3, '0'),
                ('2024-03-' || LPAD((v_idx % 25 + 1)::text, 2, '0'))::timestamp,
                v_km - 15000 - (v_idx * 500), 5000,
                CASE WHEN v_plate IN ('ARC-841','AQJ-327','ARS-194','BAA-193') THEN 'Mobil' ELSE 'Castrol' END,
                CASE WHEN v_plate IN ('ARC-841','AQJ-327','ARS-194','ARY-257','BAA-193') THEN '5W-30' ELSE '10W-40' END,
                true, 'Taller propio',
                CASE WHEN v_idx % 2 = 0 THEN v_mec1 ELSE v_mec2 END,
                v_jefe, v_ac,
                'Mantenimiento rutinario ' || v_plate)
        RETURNING mainid INTO v_mainid;
        v_main_ids := array_append(v_main_ids, v_mainid);

        INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
        VALUES (v_mainid,
                CASE WHEN v_rnd < 0.7 THEN 'Bueno' WHEN v_rnd < 0.85 THEN 'Excelente' ELSE 'Regular' END,
                true, 'Continuar con el plan de mantenimiento programado.');

        -- Consumo de aceite
        IF v_idx <= 6 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote1, 4.5, 'Stock propio');
        ELSE
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_10w40, v_maloid_10w40, 4.5, 'Stock propio');
        END IF;

        v_rnd := random();

        -- Mantenimiento 2: 2025 - Calendarizado A o B
        INSERT INTO maintenance.maintenance
            (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
             oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
             origin_service, assigned_to, workid, statid, note)
        VALUES (v_prcoid,
                CASE WHEN v_rnd < 0.15 THEN v_eme ELSE v_cal END,
                CASE WHEN v_rnd < 0.5 THEN v_sb ELSE v_sa END,
                'ORD-2025-' || LPAD((v_idx + 50)::text, 3, '0'),
                ('2025-02-' || LPAD((v_idx % 25 + 1)::text, 2, '0'))::timestamp,
                v_km - 5000 - (v_idx * 300), 5000, 'Shell',
                CASE WHEN v_plate IN ('ARC-841','AQJ-327','ARS-194','ARY-257','BAA-193') THEN '5W-30' ELSE '10W-40' END,
                true, 'Taller propio',
                CASE WHEN v_idx % 2 = 0 THEN v_mec1 ELSE v_mec2 END,
                v_jefe, v_ac,
                CASE WHEN v_rnd < 0.15 THEN 'Atención de emergencia - revisión completa' ELSE 'Cambio de aceite y filtros' END)
        RETURNING mainid INTO v_mainid;
        v_main_ids := array_append(v_main_ids, v_mainid);

        INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
        VALUES (v_mainid,
                CASE WHEN v_rnd < 0.6 THEN 'Bueno' WHEN v_rnd < 0.85 THEN 'Excelente' ELSE 'Regular' END,
                true,
                CASE WHEN v_rnd < 0.3 THEN 'Revisar frenos traseros en próximo servicio.' ELSE 'Todo en orden.' END);

        -- Consumo de aceite
        IF v_idx <= 6 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote2, 4.5, 'Stock propio');
        ELSE
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_10w40, v_maloid_10w40, 4.5, 'Stock propio');
        END IF;

        v_rnd := random();

        -- Mantenimiento 3: 2025 tardío - Calendarizado o Emergencia
        INSERT INTO maintenance.maintenance
            (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
             oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance, is_emergency_complete,
             origin_service, assigned_to, workid, statid, note)
        VALUES (v_prcoid,
                CASE WHEN v_rnd < 0.20 THEN v_eme ELSE v_cal END,
                CASE WHEN v_rnd < 0.5 THEN v_sb ELSE v_sa END,
                'ORD-2025-' || LPAD((v_idx + 100)::text, 3, '0'),
                ('2025-09-' || LPAD((v_idx % 28 + 1)::text, 2, '0'))::timestamp,
                v_km - 2000 - (v_idx * 100), 5000,
                'Shell Helix',
                CASE WHEN v_plate IN ('ARC-841','AQJ-327','ARS-194','ARY-257','BAA-193') THEN '5W-30' ELSE '10W-40' END,
                true,
                CASE WHEN v_rnd < 0.20 THEN true ELSE NULL END,
                'Taller propio',
                CASE WHEN v_idx % 2 = 0 THEN v_mec1 ELSE v_mec2 END,
                v_jefe, v_ac,
                CASE WHEN v_rnd < 0.10 THEN 'Revisión de frenos y suspensión'
                     WHEN v_rnd < 0.25 THEN 'Cambio de aceite y filtro de aire'
                     ELSE 'Servicio de mantenimiento programado' END)
        RETURNING mainid INTO v_mainid;
        v_main_ids := array_append(v_main_ids, v_mainid);

        INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
        VALUES (v_mainid,
                CASE WHEN v_rnd < 0.5 THEN 'Bueno' WHEN v_rnd < 0.75 THEN 'Excelente' WHEN v_rnd < 0.9 THEN 'Regular' ELSE 'Reparado' END,
                CASE WHEN v_rnd < 0.90 THEN true ELSE false END,
                CASE WHEN v_rnd < 0.2 THEN 'Programar cambio de correa de distribución en los próximos 5000 km.'
                     WHEN v_rnd < 0.4 THEN 'Revisar estado de batería y bornes.'
                     ELSE 'Sin observaciones.' END);

        -- Consumo de aceite
        IF v_idx <= 6 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote1, 4.5, 'Stock propio');
        ELSE
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_10w40, v_maloid_10w40, 4.5, 'Stock propio');
        END IF;

        -- Consumo de filtro de aceite variable
        IF v_rnd < 0.4 THEN
            IF v_plate IN ('ARC-841','AQJ-327') THEN
                INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
                VALUES (v_mainid, v_mat_filtro_aceite_toyota, v_maloid_filtro_aceite_toyota, 1, 'Stock propio');
            ELSE
                INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
                VALUES (v_mainid, v_mat_filtro_aceite_hyundai, v_maloid_filtro_aceite_hyundai, 1, 'Stock propio');
            END IF;
        END IF;

        v_rnd := random();

        -- Mantenimiento 4: 2026 RECIENTE
        INSERT INTO maintenance.maintenance
            (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
             oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance, is_emergency_complete,
             origin_service, assigned_to, workid, statid, note)
        VALUES (v_prcoid,
                CASE WHEN v_rnd < 0.20 THEN v_eme ELSE v_cal END,
                CASE WHEN v_rnd < 0.4 THEN v_sb WHEN v_rnd < 0.7 THEN v_sa ELSE NULL END,
                'ORD-2026-' || LPAD((v_idx + 150)::text, 3, '0'),
                CASE
                    WHEN v_rnd < 0.3 THEN CURRENT_DATE - INTERVAL '15 days'
                    WHEN v_rnd < 0.6 THEN CURRENT_DATE - INTERVAL '45 days'
                    ELSE CURRENT_DATE - INTERVAL '90 days'
                END,
                v_km, 5000, 'Mobil 1',
                CASE WHEN v_plate IN ('ARC-841','AQJ-327','ARS-194','ARY-257','BAA-193') THEN '5W-30' ELSE '10W-40' END,
                true,
                CASE WHEN v_rnd < 0.20 THEN true ELSE NULL END,
                'Taller propio',
                CASE WHEN v_idx % 2 = 0 THEN v_mec1 ELSE v_mec2 END,
                v_jefe, v_ac,
                CASE
                    WHEN v_rnd < 0.10 THEN 'Falla en sistema eléctrico - revisión de alternador'
                    WHEN v_rnd < 0.25 THEN 'Frenos delanteros desgastados - reemplazo urgente'
                    ELSE 'Servicio completo programado'
                END)
        RETURNING mainid INTO v_mainid;
        v_main_ids := array_append(v_main_ids, v_mainid);
        v_calendar_main_ids := array_append(v_calendar_main_ids, v_mainid);

        INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
        VALUES (v_mainid,
                CASE WHEN v_rnd < 0.5 THEN 'Bueno' WHEN v_rnd < 0.75 THEN 'Excelente'
                     WHEN v_rnd < 0.85 THEN 'Reparado' ELSE 'En observación' END,
                CASE WHEN v_rnd < 0.85 THEN true ELSE false END,
                CASE
                    WHEN v_rnd < 0.15 THEN 'Programar cambio de correa de distribución en 5000 km.'
                    WHEN v_rnd < 0.35 THEN 'Revisar estado de batería en próxima visita.'
                    ELSE 'Sin observaciones adicionales.'
                END);

        -- Consumo de aceite
        IF v_idx <= 6 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote1, 4.5, 'Stock propio');
        ELSE
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_10w40, v_maloid_10w40, 4.5, 'Stock propio');
        END IF;

        -- Consumos adicionales aleatorios
        IF v_rnd < 0.2 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_pastillas_freno, v_maloid_pastillas_freno, 1, 'Stock propio');
        ELSIF v_rnd < 0.4 THEN
            INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
            VALUES (v_mainid, v_mat_refrigerante, v_maloid_refrigerante, 1.0, 'Stock propio');
        END IF;
    END LOOP;

    -- Mantenimientos extra para ARC-841 (Toyota Hilux flagship)
    v_prcoid := v_vehicles_prcoid[1];
    v_km := v_vehicles_km[1];

    INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
        oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance, origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_cal, v_sb, 'ORD-2025-HLX-001', '2025-08-15', v_km - 8000, 5000,
            'Mobil', '5W-30', true, 'Taller propio', v_mec1, v_jefe, v_ac,
            'Servicio completo B - cambio de aceite, filtros y revisión general')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative) VALUES (v_mainid, 'Excelente', true);
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote1, 4.5, 'Stock propio');
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_mainid, v_mat_filtro_aceite_toyota, v_maloid_filtro_aceite_toyota, 1, 'Stock propio');

    INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
        oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance, origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_cal, v_sa, 'ORD-2025-HLX-002', '2025-12-01', v_km - 3000, 5000,
            'Mobil 1', '5W-30', true, 'Taller propio', v_mec2, v_jefe, v_ac,
            'Cambio de aceite y bujías')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative) VALUES (v_mainid, 'Bueno', true);
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_mainid, v_mat_5w30, v_maloid_5w30_lote2, 4.5, 'Stock propio');

    -- ARZ-840 (Sprinter Van): emergencias extra
    v_prcoid := v_vehicles_prcoid[10];
    v_km := v_vehicles_km[10];

    INSERT INTO maintenance.maintenance (prcoid, matyid, order_number, maintenance_date, mileage, km_since_last,
        oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
        origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_eme, 'ORD-2025-SPR-001', '2025-11-20', 117000, 3600,
            'Shell', '10W-40', false, 'Taller externo', v_mec2, v_jefe, v_ac,
            'EMERGENCIA - Falla en sistema de refrigeración, cambio de termostato y mangueras')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'Reparado', true, 'Revisar nivel de refrigerante semanalmente.');
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_mainid, v_mat_refrigerante, v_maloid_refrigerante, 2.0, 'Stock propio');

    INSERT INTO maintenance.maintenance (prcoid, matyid, order_number, maintenance_date, mileage, km_since_last,
        is_emergency_complete, origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_eme, 'ORD-2026-SPR-001', '2026-04-10', v_km, 1400,
            false, 'Taller externo', v_mec1, v_jefe, v_ac,
            'EMERGENCIA PARCIAL - Revisión de frenos por ruido excesivo')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'En observación', true, 'Urge reemplazo de pastillas de freno traseras. Programar en 500 km.');

    -- BAA-193 (Fortuner): emergencias extra
    v_prcoid := v_vehicles_prcoid[11];
    v_km := v_vehicles_km[11];
    INSERT INTO maintenance.maintenance (prcoid, matyid, order_number, maintenance_date, mileage, origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_eme, 'ORD-2025-FRT-001', '2025-06-15', 32500, 'Taller propio', v_mec2, v_jefe, v_ac,
            'EMERGENCIA - Reemplazo de neumático por pinchazo en carretera')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative) VALUES (v_mainid, 'Reparado', true);

    INSERT INTO maintenance.maintenance (prcoid, matyid, order_number, maintenance_date, mileage, origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, v_eme, 'ORD-2026-FRT-001', '2026-05-01', v_km - 500, 'Taller propio', v_mec1, v_jefe, v_ac,
            'EMERGENCIA - Luz de check engine encendida. Diagnóstico con escáner.')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'En observación', true, 'Sensor de oxígeno presenta lecturas anormales. Programar reemplazo.');

    -- ─────────────────────────────────────────────────────────────────
    -- 4. MAINTENANCE_ACTION_DETAIL (~40 registros)
    -- ─────────────────────────────────────────────────────────────────
    FOREACH v_mainid IN ARRAY v_main_ids
    LOOP
        IF v_mainid IS NOT NULL THEN
            -- Siempre agregar cambio de aceite (acción básica)
            IF v_acat_aceite_motor IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed, origin_product)
                VALUES (v_mainid, v_acat_aceite_motor, true, 'C', 'Stock propio');
            END IF;

            -- Acciones variables según mainid
            IF v_mainid % 2 = 0 AND v_acat_revisar_luces IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
                VALUES (v_mainid, v_acat_revisar_luces, true, 'I');
            END IF;
            IF v_mainid % 3 = 0 AND v_acat_escaneo IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
                VALUES (v_mainid, v_acat_escaneo, true, 'R');
            END IF;
            IF v_mainid % 4 = 0 AND v_acat_filtro_aceite IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed, origin_product)
                VALUES (v_mainid, v_acat_filtro_aceite, true, 'C', 'Stock propio');
            END IF;
            IF v_mainid % 5 = 0 AND v_acat_rotacion_neumaticos IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
                VALUES (v_mainid, v_acat_rotacion_neumaticos, true, 'R');
            END IF;
            IF v_mainid % 7 = 0 AND v_acat_bateria IS NOT NULL THEN
                INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
                VALUES (v_mainid, v_acat_bateria, true, 'I');
            END IF;
        END IF;
    END LOOP;

    -- ─────────────────────────────────────────────────────────────────
    -- 5. INSTALLED_COMPONENT (~24 registros)
    -- ─────────────────────────────────────────────────────────────────
    FOREACH v_mainid IN ARRAY v_calendar_main_ids
    LOOP
        DECLARE
            v_prcoid_ic integer;
            v_mileage_ic integer;
            v_maindate_ic timestamp;
        BEGIN
            SELECT prcoid, maintenance_date, mileage INTO v_prcoid_ic, v_maindate_ic, v_mileage_ic
            FROM maintenance.maintenance WHERE mainid = v_mainid;

            IF v_acat_aceite_motor IS NOT NULL THEN
                INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, maloid, installation_date, installation_km, active)
                VALUES (v_prcoid_ic, v_acat_aceite_motor, v_mainid,
                        v_maloid_5w30_lote1, v_maindate_ic, v_mileage_ic, true);
            END IF;

            IF v_mainid % 3 = 0 AND v_acat_filtro_aire IS NOT NULL THEN
                INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, maloid, installation_date, installation_km, active)
                VALUES (v_prcoid_ic, v_acat_filtro_aire, v_mainid,
                        v_maloid_filtro_aire_toyota, v_maindate_ic, v_mileage_ic, true);
            END IF;

            IF v_mainid % 5 = 0 AND v_acat_pastillas_freno IS NOT NULL THEN
                INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, maloid, installation_date, installation_km, expiration_date, active)
                VALUES (v_prcoid_ic, v_acat_pastillas_freno, v_mainid,
                        v_maloid_pastillas_freno, v_maindate_ic, v_mileage_ic,
                        v_maindate_ic::date + INTERVAL '2 years', true);
            END IF;
        END;
    END LOOP;

    -- ─────────────────────────────────────────────────────────────────
    -- 6. SCHEDULE_ACTION (~36 registros, 3 por vehículo)
    -- ─────────────────────────────────────────────────────────────────
    FOR v_idx IN 1..cardinality(v_vehicles_prcoid)
    LOOP
        SELECT veshid INTO v_veshid
        FROM maintenance.vehicle_schedule WHERE prcoid = v_vehicles_prcoid[v_idx];
        CONTINUE WHEN v_veshid IS NULL;

        IF v_acat_aceite_motor IS NOT NULL THEN
            INSERT INTO maintenance.schedule_action (veshid, acatid, scheduled_km, action_code)
            VALUES (v_veshid, v_acat_aceite_motor, v_vehicles_km[v_idx] + 5000, 'C');
        END IF;
        IF v_acat_filtro_aceite IS NOT NULL THEN
            INSERT INTO maintenance.schedule_action (veshid, acatid, scheduled_km, action_code)
            VALUES (v_veshid, v_acat_filtro_aceite, v_vehicles_km[v_idx] + 5000, 'C');
        END IF;
        IF v_acat_revisar_frenos IS NOT NULL THEN
            INSERT INTO maintenance.schedule_action (veshid, acatid, scheduled_km, action_code)
            VALUES (v_veshid, v_acat_revisar_frenos, v_vehicles_km[v_idx] + 10000, 'I');
        END IF;
    END LOOP;

    -- ─────────────────────────────────────────────────────────────────
    -- 7. TECHNICIAN_ASSIGNMENT (~10 registros)
    -- ─────────────────────────────────────────────────────────────────
    v_idx := 0;
    FOREACH v_mainid IN ARRAY v_main_ids
    LOOP
        v_idx := v_idx + 1;
        EXIT WHEN v_idx > 10;
        INSERT INTO maintenance.technician_assignment (mainid, workid, role_in_job, assigned_by)
        VALUES (v_mainid,
                CASE WHEN v_idx % 2 = 0 THEN v_mec1 ELSE v_mec2 END,
                'Principal', v_jefe);
    END LOOP;

    -- ─────────────────────────────────────────────────────────────────
    -- 8. ALERT_LOG (~15 registros)
    -- ─────────────────────────────────────────────────────────────────
    -- Alertas de mantenimiento próximo (no leídas, no resueltas)
    FOR v_idx IN 1..LEAST(5, cardinality(v_vehicles_prcoid))
    LOOP
        INSERT INTO maintenance.alert_log (alcoid, prcoid, message, alert_date, read, resolved)
        VALUES (v_alco_mantenimiento, v_vehicles_prcoid[v_idx],
                'Vehículo ' || v_vehicles_plate[v_idx] || ' próximo a mantenimiento (' || (v_vehicles_km[v_idx] + 3000) || ' km)',
                CURRENT_DATE - INTERVAL '5 days', false, false);
    END LOOP;

    -- Alertas de stock bajo
    INSERT INTO maintenance.alert_log (alcoid, mateid, message, alert_date, read, resolved)
    VALUES (v_alco_stock, v_mat_filtro_combustible,
            'Stock bajo de Filtro de Combustible Diésel: 8 unidades (mínimo 4)',
            CURRENT_DATE - INTERVAL '2 days', false, false);

    INSERT INTO maintenance.alert_log (alcoid, mateid, message, alert_date, read, resolved)
    VALUES (v_alco_stock, v_mat_correa,
            'Stock bajo de Correa de Distribución: 6 unidades (mínimo 2)',
            CURRENT_DATE - INTERVAL '3 days', false, false);

    -- Alertas de lote por vencer
    INSERT INTO maintenance.alert_log (alcoid, maloid, mateid, message, alert_date, read, resolved)
    SELECT v_alco_lote, ml.maloid, ml.mateid,
           'Lote ' || COALESCE(ml.supplier_lot_number, 'S/N') || ' de ' || COALESCE((SELECT m.name FROM maintenance.material m WHERE m.mateid = ml.mateid), 'material') || ' vence en 15 días',
           CURRENT_DATE - INTERVAL '1 day', false, false
    FROM maintenance.material_lot ml
    WHERE ml.expiration_date = CURRENT_DATE + INTERVAL '15 days'
    LIMIT 2;

    -- Alertas ya resueltas (histórico)
    FOR v_idx IN 1..3
    LOOP
        INSERT INTO maintenance.alert_log (alcoid, prcoid, message, alert_date, read, read_at, read_by, resolved, resolved_at, resolved_by)
        VALUES (v_alco_mantenimiento, v_vehicles_prcoid[v_idx],
                'Vehículo ' || v_vehicles_plate[v_idx] || ' requería mantenimiento urgente',
                CURRENT_DATE - INTERVAL '30 days',
                true, CURRENT_DATE - INTERVAL '25 days', v_jefe,
                true, CURRENT_DATE - INTERVAL '20 days', v_jefe);
    END LOOP;

    -- ─────────────────────────────────────────────────────────────────
    -- 9. MATERIAL_RATING (~12 registros)
    -- ─────────────────────────────────────────────────────────────────
    INSERT INTO maintenance.material_rating (mateid, mainid, rating, observation, rated_by)
    SELECT v_mat_5w30, mainid, 5, 'Excelente rendimiento, motor más silencioso', v_mec1
    FROM maintenance.maintenance WHERE matyid = v_cal AND statid = v_ac ORDER BY mainid LIMIT 1;

    INSERT INTO maintenance.material_rating (mateid, mainid, rating, observation, rated_by)
    SELECT v_mat_pastillas_freno, mainid, 4, 'Buena calidad, frena bien', v_mec2
    FROM maintenance.maintenance WHERE matyid = v_eme AND statid = v_ac ORDER BY mainid LIMIT 1;

    INSERT INTO maintenance.material_rating (mateid, mainid, rating, observation, rated_by)
    SELECT v_mat_bujias, mainid, 2, 'Las bujías vinieron defectuosas, una no encendía', v_mec1
    FROM maintenance.maintenance WHERE matyid = v_cal AND statid = v_ac ORDER BY mainid LIMIT 1 OFFSET 2;

    INSERT INTO maintenance.material_rating (mateid, mainid, rating, observation, rated_by)
    SELECT v_mat_refrigerante, mainid, 3, 'Cumple su función pero el precio es elevado', v_mec2
    FROM maintenance.maintenance WHERE matyid = v_eme AND statid = v_ac ORDER BY mainid LIMIT 1 OFFSET 3;

    -- ─────────────────────────────────────────────────────────────────
    -- 10. ACTUALIZAR STOCK según consumos
    -- ─────────────────────────────────────────────────────────────────
    UPDATE maintenance.material m
    SET stock_total = GREATEST(0, stock_total - (
        SELECT COALESCE(SUM(mc.quantity), 0)
        FROM maintenance.material_consumption mc
        WHERE mc.mateid = m.mateid AND mc.origin = 'Stock propio'
    ))
    WHERE EXISTS (
        SELECT 1 FROM maintenance.material_consumption mc
        WHERE mc.mateid = m.mateid AND mc.origin = 'Stock propio'
    );

    UPDATE maintenance.material_lot ml
    SET current_quantity = GREATEST(0, current_quantity - (
        SELECT COALESCE(SUM(mc.quantity), 0)
        FROM maintenance.material_consumption mc
        WHERE mc.maloid = ml.maloid
    ))
    WHERE EXISTS (
        SELECT 1 FROM maintenance.material_consumption mc
        WHERE mc.maloid = ml.maloid
    );

    UPDATE maintenance.material_lot
    SET lot_status = 'agotado'
    WHERE current_quantity <= 0 AND lot_status = 'activo';

    RAISE NOTICE 'Seed expandido completado exitosamente.';
END;
$$;

COMMIT;

-- ─────────────────────────────────────────────────────────────────────
-- 11. DIVERSIFICAR ESTADOS (para filtros funcionales)
-- ─────────────────────────────────────────────────────────────────────
-- Marcar algunos mantenimientos antiguos como Finalizados
UPDATE maintenance.maintenance
SET statid = 'FI'
WHERE mainid IN (
    SELECT mainid FROM maintenance.maintenance
    WHERE maintenance_date < '2025-01-01'
    ORDER BY mainid
    LIMIT 12
);

-- Marcar 2 mantenimientos como Cancelados
UPDATE maintenance.maintenance
SET statid = 'CA'
WHERE mainid IN (
    SELECT mainid FROM maintenance.maintenance
    WHERE maintenance_date < '2025-06-01'
    ORDER BY mainid
    LIMIT 2 OFFSET 20
);

-- ─────────────────────────────────────────────────────────────────────
-- VERIFICACIÓN FINAL
-- ─────────────────────────────────────────────────────────────────────
SELECT 'Vehicle schedules' AS item, COUNT(*) FROM maintenance.vehicle_schedule
UNION ALL
SELECT 'Maintenances', COUNT(*) FROM maintenance.maintenance
UNION ALL
SELECT 'Diagnoses', COUNT(*) FROM maintenance.diagnosis
UNION ALL
SELECT 'Material consumptions', COUNT(*) FROM maintenance.material_consumption
UNION ALL
SELECT 'Material lots', COUNT(*) FROM maintenance.material_lot
UNION ALL
SELECT 'Installed components', COUNT(*) FROM maintenance.installed_component
UNION ALL
SELECT 'Action details', COUNT(*) FROM maintenance.maintenance_action_detail
UNION ALL
SELECT 'Alert logs', COUNT(*) FROM maintenance.alert_log
UNION ALL
SELECT 'Technician assignments', COUNT(*) FROM maintenance.technician_assignment
UNION ALL
SELECT 'Schedule actions', COUNT(*) FROM maintenance.schedule_action
UNION ALL
SELECT 'Material ratings', COUNT(*) FROM maintenance.material_rating
ORDER BY item;
