-- =====================================================================
-- SEED FINAL DEFINITIVO — MaintManager Demo Completo
-- 16 VEHÍCULOS REALES DESDE ODS
-- =====================================================================
-- ORDEN DE EJECUCIÓN:
--   1. psql -U postgres -d neoplus_maintenance -f bd-final.sql
--   2. psql -U postgres -d neoplus_maintenance -f 02_ajustes_fase1.sql
--   3. psql -U postgres -d neoplus_maintenance -f 05_seed_final.sql
-- =====================================================================
-- NO requiere 03_seed_data.sql — este seed lo reemplaza completamente.
-- Basado en archivo ODS real + proyecto documentado (184 páginas).
-- =====================================================================

BEGIN;

-- ═══════════════════════════════════════════════════════════════════════
-- 1. DATOS DE REFERENCIA (list.*)
-- ═══════════════════════════════════════════════════════════════════════
INSERT INTO list.producttype (prtyid, name) VALUES
('VH', 'Vehículo'), ('SE', 'Servicio'), ('MA', 'Material')
ON CONFLICT DO NOTHING;

INSERT INTO list.coin (name, symbol, isocode) VALUES
('Sol Peruano', 'S/', 'PEN'), ('Dólar Americano', '$', 'USD')
ON CONFLICT DO NOTHING;

INSERT INTO list.vehicletype (vetyid, name, description) VALUES
('SE', 'Sedán',  'Vehículo de pasajeros sedán'),
('SU', 'SUV',    'Vehículo utilitario deportivo'),
('PK', 'Pickup', 'Camioneta cabina simple o doble'),
('VN', 'Van',    'Transporte de personas o carga liviana')
ON CONFLICT DO NOTHING;

INSERT INTO list.fueltype (futyid, name) VALUES
('GA', 'Gasolina'), ('DI', 'Diésel'), ('HI', 'Híbrido')
ON CONFLICT DO NOTHING;

INSERT INTO list.entitystatus (enstid, name) VALUES
('AC', 'Activo'), ('IN', 'Inactivo'), ('PA', 'Pendiente')
ON CONFLICT DO NOTHING;

INSERT INTO list.status (statid, name) VALUES
('AC', 'Activo'), ('IN', 'Inactivo'), ('CA', 'Cancelado'), ('FI', 'Finalizado')
ON CONFLICT DO NOTHING;

INSERT INTO list.civilstatus (name) VALUES ('Soltero'), ('Casado'), ('Divorciado')
ON CONFLICT DO NOTHING;

INSERT INTO list.identitydocumenttype (name) VALUES
('DNI'), ('RUC'), ('Carnet de Extranjería')
ON CONFLICT DO NOTHING;

INSERT INTO list.jobcategory (name, description) VALUES
('Administrativo', 'Personal de administración'),
('Técnico',        'Personal técnico operativo'),
('Gerencial',      'Personal de dirección')
ON CONFLICT DO NOTHING;

INSERT INTO list.companyconditionlist (name) VALUES ('Habida'), ('No habida')
ON CONFLICT DO NOTHING;

INSERT INTO list.companystatuslist (name) VALUES ('Activo'), ('Inactivo'), ('Suspendido')
ON CONFLICT DO NOTHING;

INSERT INTO list.taxpayertypelist (name) VALUES ('Persona Natural'), ('Persona Jurídica')
ON CONFLICT DO NOTHING;

-- ═══════════════════════════════════════════════════════════════════════
-- 2. GEOGRAFÍA BÁSICA
-- ═══════════════════════════════════════════════════════════════════════
INSERT INTO public.country (name, demonym, phoneprefix) VALUES
('Perú', 'Peruano', '+51') ON CONFLICT DO NOTHING;

INSERT INTO public.department (counid, name, isocode)
SELECT counid, 'Arequipa', 'AQP' FROM public.country WHERE name = 'Perú'
ON CONFLICT DO NOTHING;

INSERT INTO public.province (depaid, name)
SELECT depaid, 'Arequipa' FROM public.department WHERE name = 'Arequipa'
ON CONFLICT DO NOTHING;

INSERT INTO public.district (provid, name, ubigeo)
SELECT provid, 'José Luis Bustamante y Rivero', '040108'
FROM public.province WHERE name = 'Arequipa'
ON CONFLICT DO NOTHING;

-- ═══════════════════════════════════════════════════════════════════════
-- 3. ÁREAS Y CARGOS
-- ═══════════════════════════════════════════════════════════════════════
INSERT INTO public.jobarea (name) VALUES
('Gerencia General'), ('Área de Mantenimiento'), ('Área de Alquiler / Rentas')
ON CONFLICT DO NOTHING;

INSERT INTO public.job (joarid, name, jocaid)
SELECT ja.joarid, 'Gerente General', jc.jocaid
FROM public.jobarea ja, list.jobcategory jc
WHERE ja.name = 'Gerencia General' AND jc.name = 'Gerencial'
ON CONFLICT DO NOTHING;

INSERT INTO public.job (joarid, name, jocaid)
SELECT ja.joarid, 'Jefe de Mantenimiento', jc.jocaid
FROM public.jobarea ja, list.jobcategory jc
WHERE ja.name = 'Área de Mantenimiento' AND jc.name = 'Gerencial'
ON CONFLICT DO NOTHING;

INSERT INTO public.job (joarid, name, jocaid)
SELECT ja.joarid, 'Mecánico Técnico', jc.jocaid
FROM public.jobarea ja, list.jobcategory jc
WHERE ja.name = 'Área de Mantenimiento' AND jc.name = 'Técnico'
ON CONFLICT DO NOTHING;

-- ═══════════════════════════════════════════════════════════════════════
-- 4. PERSONAS Y TRABAJADORES
-- ═══════════════════════════════════════════════════════════════════════
DO $$
DECLARE v_iddoid smallint;
BEGIN
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'DNI';

    INSERT INTO public.person (enstid, iddoid, document, fln, mln, name, sex, birthdate)
    VALUES
        ('AC', v_iddoid, '46392018', 'Ortiz',  'Mamani',   'Herror',     'M', '1988-07-22'),
        ('AC', v_iddoid, '72841095', 'Quispe', 'Flores',   'Juan Carlos','M', '1992-11-05'),
        ('AC', v_iddoid, '71934062', 'Mamani', 'Condori',  'Pedro',      'M', '1990-04-18')
    ON CONFLICT DO NOTHING;
END;
$$;

-- Herror Ortiz = Gerente General (admin)
INSERT INTO public.worker (wenstid, persid, jobid, startdate, username, password, email)
SELECT 'AC', p.persid, j.jobid, '2021-06-01',
       'herror.ortiz', md5('Admin2026!'), 'herror.ortiz@neoplus.pe'
FROM public.person p, public.job j
WHERE p.document = '46392018' AND j.name = 'Gerente General'
ON CONFLICT DO NOTHING;

-- Mecánicos
INSERT INTO public.worker (wenstid, persid, jobid, startdate, username, password, email)
SELECT 'AC', p.persid, j.jobid, '2022-03-01',
       'juan.quispe', md5('Tecnico2026!'), 'juan.quispe@neoplus.pe'
FROM public.person p, public.job j
WHERE p.document = '72841095' AND j.name = 'Mecánico Técnico'
ON CONFLICT DO NOTHING;

INSERT INTO public.worker (wenstid, persid, jobid, startdate, username, password, email)
SELECT 'AC', p.persid, j.jobid, '2023-01-15',
       'pedro.mamani', md5('Tecnico2026!'), 'pedro.mamani@neoplus.pe'
FROM public.person p, public.job j
WHERE p.document = '71934062' AND j.name = 'Mecánico Técnico'
ON CONFLICT DO NOTHING;

-- ═══════════════════════════════════════════════════════════════════════
-- 5. ZONE + AGENCY + COMPANY + COST CENTER
-- ═══════════════════════════════════════════════════════════════════════
INSERT INTO public.zone (name, enabled, workid, status)
SELECT 'Arequipa Metropolitana', true, w.workid, true
FROM public.worker w WHERE w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

INSERT INTO public.agency (zoneid, code, name, startdate, enabled, workid, status)
SELECT z.zoneid, 'ARE', 'Agencia Principal Arequipa', CURRENT_DATE, true, w.workid, true
FROM public.zone z, public.worker w
WHERE z.name = 'Arequipa Metropolitana' AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

INSERT INTO public.company (enstid, name, ruc, tptlid, coslid, coclid, anaid)
SELECT 'AC', 'Neo Plus Business S.A.C.', '20601234560',
       tp.tptlid, cs.coslid, cc.coclid, w.workid
FROM list.taxpayertypelist tp, list.companystatuslist cs,
     list.companyconditionlist cc, public.worker w
WHERE tp.name = 'Persona Jurídica' AND cs.name = 'Activo'
  AND cc.name = 'Habida' AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

INSERT INTO public.costcenter (name, status, workid)
SELECT 'Flota Vehicular NeoPlus', true, w.workid
FROM public.worker w WHERE w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- ═══════════════════════════════════════════════════════════════════════
-- 6. LOS 16 VEHÍCULOS ACTIVOS (desde ODS real)
-- ═══════════════════════════════════════════════════════════════════════
DO $$
DECLARE
    v_workid integer; v_coinid smallint;
    v_prodid integer; v_prcoid integer;
    v_vehicles text[][] := ARRAY[
        -- #num | name | plate | vin_est | vetyid | futyid | year | color | km | category
        ARRAY['1',  'SWM G05',                       'VDG-361',  'SWMVDG361000000001', 'SU', 'GA', '2023', 'Rojo',         '9847',  'SUV Compacto'],
        ARRAY['2',  'Great Wall Poer',               'VDW-869',  'GWPVDA869000000002', 'PK', 'DI', '2023', 'Blanco',      '20555', 'Pickup Doble Cabina'],
        ARRAY['3',  'DongFeng SX6',                  'VBQ-302',  'DFSXVBQ30200000003', 'SU', 'GA', '2022', 'Rojo',        '39814', 'SUV 7 Asientos'],
        ARRAY['4',  'Chevrolet Joy',                 'VBQ-285',  'CHVJVBQ28500000004', 'SE', 'GA', '2022', 'Gris',        '50250', 'Sedán Compacto'],
        ARRAY['5',  'FAW Sirius R7',                 'VAJ-339',  'FAWSRVAJ3390000005', 'SU', 'GA', '2024', 'Azul',        '15000', 'SUV Mediano'],
        ARRAY['6',  'VW Gol',                        'V0U-053',  'VWGOLV0U0530000006', 'SE', 'GA', '2019', 'Rojo Flash',  '155198','Sedán Económico'],
        ARRAY['7',  'Mitsubishi L200',               'VAK-826',  'MITLVAC82600000007', 'PK', 'DI', '2020', 'Blanco',      '79113', 'Pickup Trabajo'],
        ARRAY['8',  'VW Passat',                     'B0J-433',  'VWPASB0J4330000008', 'SE', 'GA', '2015', 'Plata',       '175786','Sedán Ejecutivo'],
        ARRAY['9',  'Subaru Legacy',                 'D8S-151',  'SUBLED8S1510000009', 'SE', 'GA', '2023', 'Plata',       '15000', 'Sedán Deportivo'],
        ARRAY['10', 'Mercedes Benz C230',            'A1Y-218',  'MBZCA1Y21800000010', 'SE', 'GA', '2014', 'Negro',       '18000', 'Sedán Premium'],
        ARRAY['11', 'Volvo S60',                     'B9N-233',  'VOLVB9N23300000011', 'SE', 'GA', '2014', 'Rojo',        '12000', 'Sedán Premium'],
        ARRAY['12', 'Volvo S60',                     'A1N-346',  'VOLVA1N34600000012', 'SE', 'GA', '2013', 'Negro',       '10000', 'Sedán Premium'],
        ARRAY['13', 'Volvo 460',                     'F1W-570',  'VOL4F1W57000000013', 'SE', 'GA', '2013', 'Gris',        '30000', 'Sedán Clásico'],
        ARRAY['14', 'Toyota Land Cruiser Prado',     'C9R-513',  'TLC3C9R51300000014', 'SU', 'GA', '2011', 'Plateado',    '25000', 'SUV Premium'],
        ARRAY['15', 'Chevrolet Cavalier',            'APS-421',  'CHVCAPS42100000015', 'SE', 'GA', '1998', 'Verde',       '15000', 'Sedán Clásico'],
        ARRAY['16', 'SsangYong Actyon 2.0 XDI',      'V1T-291',  'SSYAV1T29100000016', 'SU', 'DI', '2012', 'Azul Marino', '20000', 'SUV Mediano']
    ];
    i integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    SELECT coinid INTO v_coinid FROM list.coin WHERE isocode = 'PEN';

    FOR i IN 1..array_length(v_vehicles, 1) LOOP
        INSERT INTO public.product (name, prtyid, coinid, workid, cost, status, webvisible)
        VALUES (v_vehicles[i][2], 'VH', v_coinid, v_workid, 0, true, false)
        RETURNING prodid INTO v_prodid;

        INSERT INTO product.company (prodid, description, qty, status, workid, prstid)
        VALUES (v_prodid, v_vehicles[i][2], 1, true, v_workid, 'A')
        RETURNING prcoid INTO v_prcoid;

        INSERT INTO product.vehicle (
            prcoid, prodid, license_plate_number, vin_number, vetyid, futyid,
            year_of_manufacture, color, mileage, category, status,
            description, qty, workid, prstid
        ) VALUES (
            v_prcoid, v_prodid,
            v_vehicles[i][3], v_vehicles[i][4],
            v_vehicles[i][5], v_vehicles[i][6],
            v_vehicles[i][7]::smallint, v_vehicles[i][8],
            v_vehicles[i][9]::integer, v_vehicles[i][10], true,
            v_vehicles[i][2], 1, v_workid, 'A'
        );
    END LOOP;
END;
$$;

-- Cliente genérico para rentas
DO $$
DECLARE v_iddoid smallint;
BEGIN
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'DNI';
    INSERT INTO public.person (enstid, iddoid, document, fln, name, sex, birthdate)
    VALUES ('AC', v_iddoid, '00000001', 'Cliente', 'Cliente Genérico', 'M', '1990-01-01')
    ON CONFLICT DO NOTHING;
END;
$$;

INSERT INTO public.client (enstid, persid, agenid, workid, origin, startdate)
SELECT 'AC', p.persid, a.agenid, w.workid, 'I', CURRENT_DATE
FROM public.person p, public.agency a, public.worker w
WHERE p.document = '00000001' AND a.name = 'Agencia Principal Arequipa'
  AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

INSERT INTO company.worker (compid, persid, status)
SELECT c.compid, w.persid, true
FROM public.company c, public.worker w
WHERE c.name = 'Neo Plus Business S.A.C.' AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- Rentas con km para cada vehículo (última renta registrada)
DO $$
DECLARE
    v_workid integer; v_clieid integer; v_coceid integer;
    v_cowoid integer; v_persid integer; v_sereid integer;
    v_prcoid integer; v_prodid integer;
    rent_data text[][] := ARRAY[
        ARRAY['VDG-361',  '7800',  '9847',  '2026-04-30'],
        ARRAY['VDW-869',  '18900', '20555', '2026-04-15'],
        ARRAY['VBQ-302',  '36000', '39814', '2026-04-21'],
        ARRAY['VBQ-285',  '47000', '50250', '2026-04-10'],
        ARRAY['VAJ-339',  '13000', '15000', '2026-04-01'],
        ARRAY['V0U-053',  '152000','155198','2026-04-13'],
        ARRAY['VAK-826',  '76000', '79113', '2026-04-13'],
        ARRAY['B0J-433',  '172000','175786','2026-04-17'],
        ARRAY['D8S-151',  '13000', '15000', '2026-04-01'],
        ARRAY['A1Y-218',  '16000', '18000', '2026-04-01'],
        ARRAY['B9N-233',  '10000', '12000', '2026-04-01'],
        ARRAY['A1N-346',  '8000',  '10000', '2026-04-01'],
        ARRAY['F1W-570',  '28000', '30000', '2026-04-01'],
        ARRAY['C9R-513',  '23000', '25000', '2026-04-01'],
        ARRAY['APS-421',  '13000', '15000', '2026-04-01'],
        ARRAY['V1T-291',  '18000', '20000', '2026-04-01']
    ];
    i integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    SELECT clieid INTO v_clieid FROM public.client
        JOIN public.person p ON public.client.persid = p.persid
        WHERE p.document = '00000001' LIMIT 1;
    SELECT coceid INTO v_coceid FROM public.costcenter WHERE name = 'Flota Vehicular NeoPlus';
    SELECT cowoid INTO v_cowoid FROM company.worker
        JOIN public.worker w ON company.worker.persid = w.persid
        WHERE w.username = 'herror.ortiz' LIMIT 1;
    SELECT persid INTO v_persid FROM public.person WHERE document = '00000001';

    FOR i IN 1..array_length(rent_data, 1) LOOP
        SELECT v.prcoid, v.prodid INTO v_prcoid, v_prodid
        FROM product.vehicle v WHERE v.license_plate_number = rent_data[i][1];

        INSERT INTO service.rentrequest (
            coceid, persid, driver, prodid, price, pricecoin,
            guarantee, frecuency, guaranteecoin,
            deliverydate, returndate, exactreturn, statid
        ) VALUES (
            v_coceid, v_persid, v_persid, v_prodid, 150.00, 1, 0, 'D', 1,
            rent_data[i][4]::timestamp - interval '7 days',
            rent_data[i][4]::timestamp, true, 'FI'
        ) RETURNING sereid INTO v_sereid;

        INSERT INTO service.rentexecute (
            coceid, workid, clieid, delivered_date, delivered_workid,
            received_cowoid, kilometer_start, kilometer_end,
            return_date, made_sell_document, checklist, sereid, statid
        ) VALUES (
            v_coceid, v_workid, v_clieid,
            rent_data[i][4]::timestamp - interval '7 days',
            v_workid, v_cowoid,
            rent_data[i][2]::integer, rent_data[i][3]::integer,
            rent_data[i][4]::timestamp, false, 1, v_sereid, 'FI'
        );
    END LOOP;
END;
$$;

-- ═══════════════════════════════════════════════════════════════════════
-- 7. MATERIALES + CATEGORÍAS + LOTES + PROVEEDOR
-- ═══════════════════════════════════════════════════════════════════════
DO $$
DECLARE
    v_workid integer; v_lub smallint; v_fil smallint; v_flu smallint; v_rep smallint;
    v_provid integer; v_iddoid smallint;
    v_mateid integer; v_maloid integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';

    -- Categorías
    INSERT INTO maintenance.material_category (name, description) VALUES
        ('Lubricantes', 'Aceites y lubricantes para motor y transmisión'),
        ('Filtros', 'Filtros de aceite, aire, combustible y cabina'),
        ('Fluidos', 'Líquidos de frenos, refrigerantes y aditivos'),
        ('Repuestos', 'Pastillas, bujías, correas y otros repuestos'),
        ('Neumáticos', 'Llantas y neumáticos')
    ON CONFLICT DO NOTHING;

    SELECT macaid INTO v_lub FROM maintenance.material_category WHERE name = 'Lubricantes';
    SELECT macaid INTO v_fil FROM maintenance.material_category WHERE name = 'Filtros';
    SELECT macaid INTO v_flu FROM maintenance.material_category WHERE name = 'Fluidos';
    SELECT macaid INTO v_rep FROM maintenance.material_category WHERE name = 'Repuestos';

    -- Materiales
    INSERT INTO maintenance.material (macaid, name, unit_of_measure, stock_total, stock_minimum, created_by) VALUES
        (v_lub,'Aceite Motor 5W-30 Sintético',     'Litros', 30.0, 10.0, v_workid),
        (v_lub,'Aceite Motor 10W-40 Semi-sintético','Litros', 20.0, 6.0, v_workid),
        (v_lub,'Aceite Transmisión ATF',            'Litros', 15.0, 5.0, v_workid),
        (v_fil,'Filtro de Aceite Universal',        'Unidad', 18,   6,   v_workid),
        (v_fil,'Filtro de Aire Universal',          'Unidad', 10,   4,   v_workid),
        (v_fil,'Filtro de Combustible Diésel',      'Unidad', 8,    4,   v_workid),
        (v_fil,'Filtro de Aceite Premium',          'Unidad', 12,   4,   v_workid),
        (v_flu,'Líquido de Frenos DOT 4',           'Litros', 10.0, 4.0, v_workid),
        (v_flu,'Refrigerante Long Life 50/50',      'Litros', 20.0, 6.0, v_workid),
        (v_rep,'Pastillas de Freno Delanteras',     'Par',    6,    2,   v_workid),
        (v_rep,'Bujías NGK (set x4)',               'Set',    8,    4,   v_workid),
        (v_rep,'Correa de Distribución',            'Unidad', 4,    2,   v_workid)
    ON CONFLICT DO NOTHING;

    -- Proveedor
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'RUC';
    INSERT INTO public.person (enstid, iddoid, document, fln, name, sex, birthdate)
    VALUES ('AC', v_iddoid, '20601234561', 'Proveedor', 'Repuestos del Sur S.R.L.', 'M', '2000-01-01')
    ON CONFLICT DO NOTHING;
    INSERT INTO public.provider (persid, workid, status)
    SELECT p.persid, v_workid, true
    FROM public.person p WHERE p.document = '20601234561'
    ON CONFLICT DO NOTHING;
    SELECT provid INTO v_provid FROM public.provider
        JOIN public.person p ON public.provider.persid = p.persid
        WHERE p.document = '20601234561' LIMIT 1;

    -- Lotes
    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 5W-30 Sintético';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 20.0, 20.0, 28.50, '2026-12-01', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 10W-40 Semi-sintético';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 20.0, 20.0, 22.00, '2027-05-01', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Líquido de Frenos DOT 4';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 10.0, 10.0, 35.00, '2027-09-01', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Refrigerante Long Life 50/50';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 20.0, 20.0, 18.50, '2027-12-01', v_provid, 'activo', v_workid);

    FOR v_mateid IN SELECT mateid FROM maintenance.material WHERE name IN
        ('Filtro de Aceite Universal','Filtro de Aire Universal',
         'Filtro de Combustible Diésel','Filtro de Aceite Premium',
         'Pastillas de Freno Delanteras','Bujías NGK (set x4)','Correa de Distribución')
    LOOP
        INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, provid, lot_status, created_by)
        VALUES (v_mateid, 10, 10, 50.00, v_provid, 'activo', v_workid);
    END LOOP;

    -- Lote próximo a vencer (para alertas)
    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 5W-30 Sintético';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 10.0, 10.0, 30.00, CURRENT_DATE + INTERVAL '20 days', v_provid, 'activo', v_workid);

    -- Lote agotado
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 5.0, 0.0, 27.00, '2026-03-01', v_provid, 'agotado', v_workid);
END;
$$;

-- ═══════════════════════════════════════════════════════════════════════
-- 8. CATÁLOGO DE ACCIONES
-- ═══════════════════════════════════════════════════════════════════════
DO $$
DECLARE v_l1 smallint; v_l2 smallint;
BEGIN
    INSERT INTO maintenance.action_list_type (name) VALUES
        ('Elementos de Reemplazo y Aplicación'),
        ('Operaciones')
    ON CONFLICT DO NOTHING;

    SELECT altoid INTO v_l1 FROM maintenance.action_list_type WHERE name = 'Elementos de Reemplazo y Aplicación';
    SELECT altoid INTO v_l2 FROM maintenance.action_list_type WHERE name = 'Operaciones';

    INSERT INTO maintenance.action_catalog (altoid, name, category, recommended_quantity, unit_of_measure, useful_life_km, expires_by_time, useful_life_days) VALUES
        (v_l1,'Aceite de Motor','Fluido','4-5','Litros',5000,true,365),
        (v_l1,'Aceite de Transmisión','Fluido','2-3','Litros',NULL,true,730),
        (v_l1,'Líquido Refrigerante','Fluido','1','Litros',NULL,true,730),
        (v_l1,'Líquido de Frenos','Fluido','0.5','Litros',NULL,true,730),
        (v_l1,'Filtro de Aceite de Motor','Filtro','1','Unidad',5000,false,NULL),
        (v_l1,'Filtro de Aire de Motor','Filtro','1','Unidad',15000,false,NULL),
        (v_l1,'Filtro de Combustible','Filtro','1','Unidad',20000,false,NULL),
        (v_l1,'Bujías (Motor a Gasolina)','Repuesto','4','Unidad',20000,false,NULL),
        (v_l1,'Correa de Distribución','Repuesto','1','Unidad',60000,false,NULL),
        (v_l1,'Pastillas de Freno Delanteras','Repuesto','1','Par',30000,false,NULL)
    ON CONFLICT DO NOTHING;

    INSERT INTO maintenance.action_catalog (altoid, name, category, useful_life_km) VALUES
        (v_l2,'Lubricar bisagras de cada puerta','Lubricación',NULL),
        (v_l2,'Revisar luces interiores y exteriores','Inspección',NULL),
        (v_l2,'Revisar líneas de freno, mangueras y conexiones','Inspección',NULL),
        (v_l2,'Revisar presión y estado de neumáticos','Inspección',NULL),
        (v_l2,'Rotación de neumáticos','Servicio',10000),
        (v_l2,'Escaneo de unidad','Diagnóstico',NULL),
        (v_l2,'Bornes de batería / Nivel electrolito','Inspección',NULL),
        (v_l2,'Revisar juego libre de pedal de Freno','Inspección',NULL)
    ON CONFLICT DO NOTHING;
END;
$$;

-- =====================================================================
-- 9. CRONOGRAMAS POR VEHÍCULO + CONFIG_SYSTEM
-- =====================================================================
DO $$
DECLARE v_workid integer; v_prcoid integer; v_km integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';

    -- config_system ya tiene seed en bd-final.sql, solo actualizamos valores
    UPDATE maintenance.config_system SET value = '5000', description = 'Intervalo km entre mantenimientos calendarizados',
        updated_at = CURRENT_TIMESTAMP, updated_by = v_workid
    WHERE key = 'intervalo_km';

    UPDATE maintenance.config_system SET value = '800', description = 'Km antes del próximo servicio para alertar',
        updated_at = CURRENT_TIMESTAMP, updated_by = v_workid
    WHERE key = 'alerta_km_umbral';

    FOR v_prcoid, v_km IN
        SELECT v.prcoid, v.mileage FROM product.vehicle v WHERE v.status = true
    LOOP
        INSERT INTO maintenance.vehicle_schedule (prcoid, interval_km, next_km, alert_km_threshold, created_by)
        VALUES (v_prcoid, 5000, (v_km / 5000 + 1) * 5000, 800, v_workid)
        ON CONFLICT (prcoid) DO NOTHING;
    END LOOP;
END;
$$;

-- =====================================================================
-- 10. MANTENIMIENTOS — 16 VEHÍCULOS, ~42 REGISTROS (2024-2026)
-- =====================================================================
-- Cada mantenimiento se crea con: orden + diagnosis + checklist + consumo aceite
-- La función auxiliar se elimina al final
-- =====================================================================
CREATE OR REPLACE FUNCTION maintenance.ins_main(
    p_plate text, p_matyid int, p_setyid int, p_order text,
    p_date text, p_mileage int, p_km_since int,
    p_assigned_username text, p_note text,
    p_oil_brand text, p_oil_visc text,
    p_diag_status text, p_diag_recommend text
) RETURNS int AS $f$
DECLARE
    v_mid int;
    v_prcoid int;
    v_acat_aceite int;
    v_acat_filtro int;
    v_acat_luces int;
    v_maloid_5w30 int;
    v_mateid_5w30 int;
    v_jefe int;
    v_assigned int;
    v_cal int; v_sa int; v_sb int;
BEGIN
    SELECT prcoid INTO v_prcoid FROM product.vehicle WHERE license_plate_number = p_plate;
    SELECT workid INTO v_jefe FROM public.worker WHERE username = 'herror.ortiz';
    SELECT workid INTO v_assigned FROM public.worker WHERE username = p_assigned_username;
    SELECT acatid INTO v_acat_aceite FROM maintenance.action_catalog WHERE name LIKE 'Aceite de Motor%' LIMIT 1;
    SELECT acatid INTO v_acat_filtro FROM maintenance.action_catalog WHERE name LIKE 'Filtro de Aceite%' LIMIT 1;
    SELECT acatid INTO v_acat_luces FROM maintenance.action_catalog WHERE name LIKE 'Revisar luces%' LIMIT 1;
    SELECT mateid INTO v_mateid_5w30 FROM maintenance.material WHERE name LIKE 'Aceite Motor 5W%' LIMIT 1;
    SELECT maloid INTO v_maloid_5w30 FROM maintenance.material_lot WHERE mateid = v_mateid_5w30 AND lot_status = 'activo' ORDER BY expiration_date NULLS LAST LIMIT 1;
    SELECT matyid INTO v_cal FROM maintenance.maintenance_type WHERE name = 'Calendarizado';
    SELECT setyid INTO v_sa FROM maintenance.service_type WHERE code = 'A';
    SELECT setyid INTO v_sb FROM maintenance.service_type WHERE code = 'B';

    INSERT INTO maintenance.maintenance
        (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
         oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
         origin_service, assigned_to, workid, statid, note)
    VALUES (v_prcoid, p_matyid::smallint,
            CASE WHEN p_setyid > 0 THEN p_setyid::smallint ELSE NULL END,
            p_order, p_date::timestamp,
            p_mileage,
            CASE WHEN p_km_since > 0 THEN p_km_since ELSE NULL END,
            NULLIF(p_oil_brand, ''), NULLIF(p_oil_visc, ''), true,
            'Taller propio', v_assigned, v_jefe, 'FI', p_note)
    RETURNING mainid INTO v_mid;

    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mid, p_diag_status, true, p_diag_recommend);

    INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
    VALUES (v_mid, v_acat_aceite, true, 'C');

    IF p_setyid > 0 THEN
        INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
        VALUES (v_mid, v_acat_filtro, true, 'C');
    END IF;

    IF p_setyid = (SELECT setyid FROM maintenance.service_type WHERE code = 'B' LIMIT 1) THEN
        INSERT INTO maintenance.maintenance_action_detail (mainid, acatid, completed, action_performed)
        VALUES (v_mid, v_acat_luces, true, 'I');
    END IF;

    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_mid, v_mateid_5w30, v_maloid_5w30, 4.5, 'Stock propio');

    RETURN v_mid;
END;
$f$ LANGUAGE plpgsql;

-- VEHÍCULO 1: SWM G05 | VDG-361 | 9847 km | Gasolina
SELECT maintenance.ins_main('VDG-361', 1, 1, 'ORD-2024-VDG361',
    '2024-08-15', 5500, 5500, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30 y filtros, vehículo nuevo',
    'Mobil', '5W-30', 'Excelente', 'Vehículo nuevo, primer servicio. Todo en orden.');

SELECT maintenance.ins_main('VDG-361', 1, 1, 'ORD-2025-VDG361',
    '2025-12-10', 8700, 3200, 'juan.quispe',
    'Servicio A de rutina - cambio de aceite 5W-30',
    'Mobil 1', '5W-30', 'Bueno', 'Continuar con el plan de mantenimiento.');

-- VEHÍCULO 2: Great Wall Poer | VDW-869 | 20555 km | Diésel
SELECT maintenance.ins_main('VDW-869', 1, 1, 'ORD-2024-VDW869',
    '2024-05-20', 12500, 12500, 'pedro.mamani',
    'Servicio A - cambio de aceite 10W-40 (motor diésel)',
    'Shell', '10W-40', 'Bueno', 'Próximo servicio B recomendado.');

SELECT maintenance.ins_main('VDW-869', 1, 2, 'ORD-2024-VDW869-B',
    '2024-11-15', 17000, 4500, 'pedro.mamani',
    'Servicio B completo - aceite, filtros y revisión general',
    'Shell Helix', '10W-40', 'Excelente', 'Vehículo en buen estado.');

SELECT maintenance.ins_main('VDW-869', 1, 1, 'ORD-2025-VDW869',
    '2025-08-10', 19500, 2500, 'juan.quispe',
    'Cambio de aceite y filtro de combustible',
    'Shell', '10W-40', 'Bueno', 'Sin observaciones.');

-- VEHÍCULO 3: DongFeng SX6 | VBQ-302 | 39814 km | Gasolina
SELECT maintenance.ins_main('VBQ-302', 1, 1, 'ORD-2024-VBQ302-A',
    '2024-03-01', 30000, 30000, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30',
    'Castrol', '5W-30', 'Bueno', 'Alternar a B en próximo servicio.');

SELECT maintenance.ins_main('VBQ-302', 1, 2, 'ORD-2024-VBQ302-B',
    '2024-09-10', 35500, 5500, 'pedro.mamani',
    'Servicio B completo - aceite, filtros, bujías',
    'Castrol GTX', '5W-30', 'Excelente', 'Vehículo en óptimas condiciones.');

-- VEHÍCULO 4: Chevrolet Joy | VBQ-285 | 50250 km | Gasolina
SELECT maintenance.ins_main('VBQ-285', 1, 1, 'ORD-2024-VBQ285-A',
    '2024-02-10', 41000, 41000, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30',
    'Mobil', '5W-30', 'Bueno', 'Programar servicio B.');

SELECT maintenance.ins_main('VBQ-285', 1, 2, 'ORD-2024-VBQ285-B',
    '2024-08-20', 46500, 5500, 'pedro.mamani',
    'Servicio B completo - aceite, filtros, revisión de frenos',
    'Mobil', '5W-30', 'Regular', 'Pastillas de freno con desgaste medio.');

SELECT maintenance.ins_main('VBQ-285', 1, 1, 'ORD-2025-VBQ285',
    '2025-06-15', 49000, 2500, 'juan.quispe',
    'Cambio de aceite y pastillas de freno delanteras',
    'Mobil Super', '5W-30', 'Bueno', 'Pastillas reemplazadas correctamente.');

-- VEHÍCULO 5: FAW Sirius R7 | VAJ-339 | 15000 km | Gasolina (estimado)
SELECT maintenance.ins_main('VAJ-339', 1, 1, 'ORD-2024-VAJ339',
    '2024-10-05', 10000, 10000, 'juan.quispe',
    'Servicio A - vehículo nuevo, cambio de aceite 5W-30',
    'Mobil', '5W-30', 'Excelente', 'Vehículo en perfecto estado.');

-- VEHÍCULO 6: VW Gol | V0U-053 | 155198 km | Gasolina (alto km)
SELECT maintenance.ins_main('V0U-053', 1, 2, 'ORD-2024-V0U053',
    '2024-01-15', 145000, 145000, 'pedro.mamani',
    'Servicio B - vehículo de alto kilometraje, revisión general',
    'Castrol', '10W-40', 'Regular', 'Revisar suspensión y dirección.');

SELECT maintenance.ins_main('V0U-053', 1, 1, 'ORD-2024-V0U053-B',
    '2024-07-20', 150500, 5500, 'juan.quispe',
    'Servicio A - cambio de aceite 10W-40',
    'Castrol', '10W-40', 'Bueno', 'Continuar monitoreo.');

SELECT maintenance.ins_main('V0U-053', 1, 2, 'ORD-2025-V0U053',
    '2025-03-10', 153000, 2500, 'pedro.mamani',
    'Servicio B - aceite, filtros, bujías',
    'Castrol', '10W-40', 'Regular', 'Programar revisión de motor.');

-- VEHÍCULO 7: Mitsubishi L200 | VAK-826 | 79113 km | Diésel
SELECT maintenance.ins_main('VAK-826', 1, 1, 'ORD-2024-VAK826-A',
    '2024-04-10', 70000, 70000, 'pedro.mamani',
    'Servicio A - cambio de aceite 10W-40 (diésel)',
    'Shell', '10W-40', 'Bueno', 'Revisar filtro de combustible.');

SELECT maintenance.ins_main('VAK-826', 1, 2, 'ORD-2024-VAK826-B',
    '2024-11-01', 75500, 5500, 'juan.quispe',
    'Servicio B completo - aceite, filtros, revisión de transmisión',
    'Shell Rimula', '10W-40', 'Bueno', 'Todo correcto.');

SELECT maintenance.ins_main('VAK-826', 2, NULL, 'ORD-2025-VAK826-EME',
    '2025-07-20', 78500, 3000, 'pedro.mamani',
    'EMERGENCIA - Pérdida de potencia. Cambio de filtro de combustible y limpieza de inyectores.',
    NULL, NULL, 'Reparado', 'Monitorear rendimiento de combustible.');

-- VEHÍCULO 8: VW Passat | B0J-433 | 175786 km | Gasolina (alto km)
SELECT maintenance.ins_main('B0J-433', 1, 1, 'ORD-2024-B0J433',
    '2024-06-15', 170000, 170000, 'juan.quispe',
    'Servicio A - cambio de aceite 10W-40',
    'Castrol', '10W-40', 'Regular', 'Vehículo con alto kilometraje. Revisar correa de distribución.');

SELECT maintenance.ins_main('B0J-433', 1, 2, 'ORD-2025-B0J433',
    '2025-02-20', 173000, 3000, 'pedro.mamani',
    'Servicio B - aceite, filtros, bujías y correa de distribución',
    'Castrol Edge', '10W-40', 'Regular', 'Correa de distribución reemplazada.');

-- VEHÍCULO 9: Subaru Legacy | D8S-151 | 15000 km | Gasolina (estimado)
SELECT maintenance.ins_main('D8S-151', 1, 1, 'ORD-2024-D8S151',
    '2024-09-01', 9500, 9500, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30',
    'Mobil 1', '5W-30', 'Excelente', 'Vehículo nuevo, sin novedades.');

-- VEHÍCULO 10: Mercedes Benz C230 | A1Y-218 | 18000 km | Gasolina
SELECT maintenance.ins_main('A1Y-218', 1, 1, 'ORD-2024-A1Y218',
    '2024-07-01', 13000, 13000, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30 sintético',
    'Mobil 1', '5W-30', 'Excelente', 'Sin observaciones.');

SELECT maintenance.ins_main('A1Y-218', 1, 1, 'ORD-2025-A1Y218',
    '2025-11-10', 16000, 3000, 'pedro.mamani',
    'Cambio de aceite y filtro de aceite',
    'Mobil 1', '5W-30', 'Bueno', 'Vehiculo en buen estado general.');

-- VEHÍCULO 11: Volvo S60 | B9N-233 | 12000 km | Gasolina
SELECT maintenance.ins_main('B9N-233', 1, 1, 'ORD-2024-B9N233',
    '2024-10-01', 8000, 8000, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30',
    'Castrol', '5W-30', 'Excelente', 'Vehículo en buen estado.');

-- VEHÍCULO 12: Volvo S60 | A1N-346 | 10000 km | Gasolina
SELECT maintenance.ins_main('A1N-346', 1, 1, 'ORD-2024-A1N346',
    '2024-11-01', 6500, 6500, 'pedro.mamani',
    'Servicio A - cambio de aceite 5W-30',
    'Castrol', '5W-30', 'Excelente', 'Vehículo en buen estado.');

-- VEHÍCULO 13: Volvo 460 | F1W-570 | 30000 km | Gasolina
SELECT maintenance.ins_main('F1W-570', 1, 1, 'ORD-2024-F1W570',
    '2024-05-15', 24000, 24000, 'pedro.mamani',
    'Servicio A - cambio de aceite 10W-40',
    'Castrol', '10W-40', 'Bueno', 'Revisar estado general.');

SELECT maintenance.ins_main('F1W-570', 1, 1, 'ORD-2025-F1W570',
    '2025-07-20', 28000, 4000, 'juan.quispe',
    'Servicio A - cambio de aceite 10W-40',
    'Castrol', '10W-40', 'Bueno', 'Todo correcto.');

-- VEHÍCULO 14: Toyota Land Cruiser Prado | C9R-513 | 25000 km | Gasolina
SELECT maintenance.ins_main('C9R-513', 1, 1, 'ORD-2024-C9R513',
    '2024-06-01', 20000, 20000, 'juan.quispe',
    'Servicio A - cambio de aceite 5W-30',
    'Mobil', '5W-30', 'Excelente', 'Vehículo en excelente estado.');

-- VEHÍCULO 15: Chevrolet Cavalier | APS-421 | 15000 km | Gasolina (clásico)
SELECT maintenance.ins_main('APS-421', 1, 1, 'ORD-2024-APS421',
    '2024-08-01', 11000, 11000, 'pedro.mamani',
    'Servicio A - cambio de aceite 10W-40 (vehículo clásico)',
    'Castrol', '10W-40', 'Bueno', 'Vehiculo clásico, uso estacional.');

-- VEHÍCULO 16: SsangYong Actyon | V1T-291 | 20000 km | Diésel
SELECT maintenance.ins_main('V1T-291', 1, 1, 'ORD-2024-V1T291',
    '2024-09-15', 14000, 14000, 'juan.quispe',
    'Servicio A - cambio de aceite 10W-40 (diésel)',
    'Shell', '10W-40', 'Bueno', 'Programar revisión de sistema diésel.');

-- =====================================================================
-- 11. CANCELAR 1 MANTENIMIENTO (para probar filtro "Canceladas")
-- =====================================================================
UPDATE maintenance.maintenance SET statid = 'CA', note = 'Cancelado por reprogramación.'
WHERE order_number = 'ORD-2024-APS421';

-- =====================================================================
-- 12. ALERT LOG (4 TIPOS)
-- =====================================================================
DO $$
DECLARE
    v_alco_mant integer; v_alco_stock integer;
    v_alco_lote integer; v_alco_comp integer;
    v_prcoid integer;
    v_jefe integer;
BEGIN
    SELECT alcoid INTO v_alco_mant FROM maintenance.alert_config WHERE alert_type = 'MANTENIMIENTO_PROXIMO_KM';
    SELECT alcoid INTO v_alco_stock FROM maintenance.alert_config WHERE alert_type = 'STOCK_BAJO';
    SELECT alcoid INTO v_alco_lote FROM maintenance.alert_config WHERE alert_type = 'LOTE_POR_VENCER';
    SELECT alcoid INTO v_alco_comp FROM maintenance.alert_config WHERE alert_type = 'COMPONENTE_POR_CADUCAR';
    SELECT workid INTO v_jefe FROM public.worker WHERE username = 'herror.ortiz';

    -- Alertas de mantenimiento próximo (VW Gol, Passat = alto km)
    INSERT INTO maintenance.alert_log (alcoid, prcoid, message, alert_date, read, resolved)
    SELECT v_alco_mant, v.prcoid,
        'Vehículo ' || v.license_plate_number || ' ha superado el km de mantenimiento programado (' || v.mileage || ' km). Programar servicio urgente.',
        '2026-05-15'::date, false, false
    FROM product.vehicle v
    WHERE v.license_plate_number IN ('V0U-053', 'B0J-433', 'VBQ-285');

    -- Alertas de stock bajo
    INSERT INTO maintenance.alert_log (alcoid, mateid, message, alert_date, read, resolved)
    SELECT v_alco_stock, m.mateid,
        'Stock bajo de ' || m.name || ': ' || m.stock_total || ' ' || m.unit_of_measure || ' (mínimo ' || m.stock_minimum || ')',
        '2026-05-10'::date, false, false
    FROM maintenance.material m
    WHERE m.stock_total <= m.stock_minimum AND m.status
    LIMIT 2;

    -- Alertas ya resueltas
    INSERT INTO maintenance.alert_log (alcoid, prcoid, message, alert_date, read, read_at, read_by, resolved, resolved_at, resolved_by)
    SELECT v_alco_mant, v.prcoid,
        'Recordatorio: mantenimiento programado para ' || v.license_plate_number,
        '2026-04-01'::date, true, '2026-04-02'::date, v_jefe,
        true, '2026-04-05'::date, v_jefe
    FROM product.vehicle v
    WHERE v.license_plate_number IN ('VDG-361', 'VDW-869');
END;
$$;

-- =====================================================================
-- 13. LIMPIAR + ACTUALIZAR STOCK
-- =====================================================================
DROP FUNCTION IF EXISTS maintenance.ins_main(text, int, int, text, text, int, int, int, text, text, text, text, text);

UPDATE maintenance.material m SET stock_total = GREATEST(0, m.stock_total - (
    SELECT COALESCE(SUM(mc.quantity), 0) FROM maintenance.material_consumption mc
    WHERE mc.mateid = m.mateid AND mc.origin = 'Stock propio'
));

UPDATE maintenance.material_lot ml SET current_quantity = GREATEST(0, ml.current_quantity - (
    SELECT COALESCE(SUM(mc.quantity), 0) FROM maintenance.material_consumption mc WHERE mc.maloid = ml.maloid
));

UPDATE maintenance.material_lot SET lot_status = 'agotado' WHERE current_quantity <= 0 AND lot_status = 'activo';

COMMIT;

-- ═══════════════════════════════════════════════════════════════════════
-- VERIFICACIÓN
-- ═══════════════════════════════════════════════════════════════════════
SELECT 'Workers' AS item, COUNT(*) FROM public.worker
UNION ALL
SELECT 'Vehicles', COUNT(*) FROM product.vehicle WHERE status = true
UNION ALL
SELECT 'Maintenances', COUNT(*) FROM maintenance.maintenance
UNION ALL
SELECT '  FI (Finalizados)', COUNT(*) FROM maintenance.maintenance WHERE statid = 'FI'
UNION ALL
SELECT '  AC (Activos)', COUNT(*) FROM maintenance.maintenance WHERE statid = 'AC'
UNION ALL
SELECT '  CA (Cancelados)', COUNT(*) FROM maintenance.maintenance WHERE statid = 'CA'
UNION ALL
SELECT 'Diagnoses', COUNT(*) FROM maintenance.diagnosis
UNION ALL
SELECT 'Consumptions', COUNT(*) FROM maintenance.material_consumption
UNION ALL
SELECT 'Action details', COUNT(*) FROM maintenance.maintenance_action_detail
UNION ALL
SELECT 'Material lots', COUNT(*) FROM maintenance.material_lot
UNION ALL
SELECT 'Installed components', COUNT(*) FROM maintenance.installed_component
UNION ALL
SELECT 'Alert logs', COUNT(*) FROM maintenance.alert_log
UNION ALL
SELECT 'Technician assignments', COUNT(*) FROM maintenance.technician_assignment
UNION ALL
SELECT 'Schedule actions', COUNT(*) FROM maintenance.schedule_action
UNION ALL
SELECT 'Material ratings', COUNT(*) FROM maintenance.material_rating
UNION ALL
SELECT 'Config system', COUNT(*) FROM maintenance.config_system
ORDER BY item;
