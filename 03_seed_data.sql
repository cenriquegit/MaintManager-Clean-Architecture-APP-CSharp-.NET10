-- =====================================================================
-- SEED DATA COMPLETO — Neo Plus Business S.A.C.
-- MaintManager — Fase 1 (actualizado para BD-FINAL)
-- Ejecutar DESPUÉS de: script principal BD-FINAL + 02_ajustes_fase1.sql
-- =====================================================================
-- CAMBIOS RESPECTO A VERSIÓN ANTERIOR:
-- [S1] Agregar zone y agency ANTES de client (agenid NOT NULL en client)
-- [S2] public.company necesaria antes de company.worker
-- [S3] list.companyconditionlist, companystatuslist, taxpayertypelist
--      necesarias antes de public.company
-- [S4] Seed de assigned_to en mantenimientos (columna de ajuste [A1])
-- [S5] Catálogo de acciones actualizado según lista proporcionada
-- [S6] Herror Ortiz asume como Gerente General y administrador único;
--      se elimina a Christian Ortiz del sistema.
-- =====================================================================

BEGIN;

-- ─────────────────────────────────────────────────────────────────────
-- 1. DATOS DE REFERENCIA (list.*)
-- ─────────────────────────────────────────────────────────────────────

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

INSERT INTO list.civilstatus (name) VALUES
('Soltero'), ('Casado'), ('Divorciado')
ON CONFLICT DO NOTHING;

INSERT INTO list.identitydocumenttype (name) VALUES
('DNI'), ('RUC'), ('Carnet de Extranjería')
ON CONFLICT DO NOTHING;

INSERT INTO list.jobcategory (name, description) VALUES
('Administrativo', 'Personal de administración'),
('Técnico',        'Personal técnico operativo'),
('Gerencial',      'Personal de dirección')
ON CONFLICT DO NOTHING;

-- [S3] Necesarias para public.company
INSERT INTO list.companyconditionlist (name) VALUES
('Habida'), ('No habida')
ON CONFLICT DO NOTHING;

INSERT INTO list.companystatuslist (name) VALUES
('Activo'), ('Inactivo'), ('Suspendido')
ON CONFLICT DO NOTHING;

INSERT INTO list.taxpayertypelist (name) VALUES
('Persona Natural'), ('Persona Jurídica')
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────────────
-- 2. GEOGRAFÍA BÁSICA
-- ─────────────────────────────────────────────────────────────────────

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

-- ─────────────────────────────────────────────────────────────────────
-- 3. ÁREAS Y CARGOS
-- ─────────────────────────────────────────────────────────────────────

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

-- ─────────────────────────────────────────────────────────────────────
-- 4. PERSONAS Y TRABAJADORES (3 usuarios del sistema – Christian eliminado)
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE v_iddoid smallint;
BEGIN
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'DNI';

    INSERT INTO public.person (enstid, iddoid, document, fln, mln, name, sex, birthdate)
    VALUES
        ('AC', v_iddoid, '45821036', 'Ortiz',  'Mamani',   'Christian',  'M', '1985-03-15'),
        ('AC', v_iddoid, '46392018', 'Ortiz',  'Mamani',   'Herror',     'M', '1988-07-22'),
        ('AC', v_iddoid, '72841095', 'Quispe', 'Flores',   'Juan Carlos','M', '1992-11-05'),
        ('AC', v_iddoid, '71934062', 'Mamani', 'Condori',  'Pedro',      'M', '1990-04-18')
    ON CONFLICT DO NOTHING;
END;
$$;

-- Herror Ortiz como Gerente General (admin)
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

-- ─────────────────────────────────────────────────────────────────────
-- 5. [S1] ZONE Y AGENCY — necesarias ANTES de client
-- BD-FINAL: public.client.agenid NOT NULL con FK a public.agency
-- ─────────────────────────────────────────────────────────────────────

INSERT INTO public.zone (name, enabled, workid, status)
SELECT 'Arequipa Metropolitana', true, w.workid, true
FROM public.worker w WHERE w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

INSERT INTO public.agency (zoneid, code, name, startdate, enabled, workid, status)
SELECT z.zoneid, 'ARE', 'Agencia Principal Arequipa', CURRENT_DATE, true, w.workid, true
FROM public.zone z, public.worker w
WHERE z.name = 'Arequipa Metropolitana' AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────────────
-- 6. [S2] PUBLIC.COMPANY (empresa Neo Plus — necesaria para company.worker)
-- ─────────────────────────────────────────────────────────────────────

INSERT INTO public.company (enstid, name, ruc, tptlid, coslid, coclid, anaid)
SELECT 'AC', 'Neo Plus Business S.A.C.', '20601234560',
       tp.tptlid, cs.coslid, cc.coclid, w.workid
FROM list.taxpayertypelist tp, list.companystatuslist cs,
     list.companyconditionlist cc, public.worker w
WHERE tp.name = 'Persona Jurídica'
  AND cs.name = 'Activo'
  AND cc.name = 'Habida'
  AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- ─────────────────────────────────────────────────────────────────────
-- 7. CENTRO DE COSTO Y PRODUCTOS PARA VEHÍCULOS
-- ─────────────────────────────────────────────────────────────────────

INSERT INTO public.costcenter (name, status, workid)
SELECT 'Flota Vehicular NeoPlus', true, w.workid
FROM public.worker w WHERE w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- Insertar los 12 vehículos
DO $$
DECLARE
    v_workid  integer;
    v_coinid  smallint;
    v_prodid  integer;
    v_prcoid  integer;
    vehicles  text[][] := ARRAY[
        ARRAY['Toyota Hilux 2020 Blanco',          'ARC-841', 'JTFEU9FJ1L5123401', 'PK', 'DI', '2020', 'Blanco',  '48500', 'Pickup Doble Cabina'],
        ARRAY['Toyota Hilux 2019 Plata',            'AQJ-327', 'JTFEU9FJ1K5098712', 'PK', 'DI', '2019', 'Plata',   '72300', 'Pickup Doble Cabina'],
        ARRAY['Toyota RAV4 2021 Negro',             'ARS-194', 'JTMRFREV1MD084215', 'SU', 'GA', '2021', 'Negro',   '31200', 'SUV Mediano'],
        ARRAY['Toyota Yaris 2022 Rojo',             'ART-562', 'JTDBT4K3XN3014821', 'SE', 'GA', '2022', 'Rojo',    '18750', 'Sedán Compacto'],
        ARRAY['Hyundai Tucson 2020 Azul',           'ARU-781', 'KM8J3CA46LU187032', 'SU', 'GA', '2020', 'Azul',    '54100', 'SUV Mediano'],
        ARRAY['Hyundai Accent 2019 Gris',           'ARV-093', 'KMHCT41AAKP281045', 'SE', 'GA', '2019', 'Gris',    '89200', 'Sedán Económico'],
        ARRAY['Kia Sportage 2021 Blanco',           'ARW-415', 'KNDJT2A25B7802134', 'SU', 'GA', '2021', 'Blanco',  '42800', 'SUV Compacto'],
        ARRAY['Kia Rio 2022 Plateado',              'ARX-628', 'KNADM4A38N6149037', 'SE', 'GA', '2022', 'Plateado','22100', 'Sedán Económico'],
        ARRAY['Nissan Frontier 2020 Negro',         'ARY-257', '1N6AD0EV1LN728413', 'PK', 'DI', '2020', 'Negro',   '61800', 'Pickup Trabajo'],
        ARRAY['Mercedes-Benz Sprinter 2019 Blanco', 'ARZ-840', 'WD3PF1CD8KP542037', 'VN', 'DI', '2019', 'Blanco', '118400','Van de Pasajeros'],
        ARRAY['Toyota Fortuner 2021 Gris',          'BAA-193', 'MR0GX8FB0M5042817', 'SU', 'DI', '2021', 'Gris',   '38700', 'SUV 7 Asientos'],
        ARRAY['Hyundai H-1 2020 Blanco',            'BAB-374', 'KMHWH41BXLU024918', 'VN', 'DI', '2020', 'Blanco', '76500', 'Van de Carga']
    ];
    i integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    SELECT coinid INTO v_coinid FROM list.coin WHERE isocode = 'PEN';

    FOR i IN 1..array_length(vehicles, 1) LOOP
        INSERT INTO public.product (name, prtyid, coinid, workid, cost, status, webvisible)
        VALUES (vehicles[i][1], 'VH', v_coinid, v_workid, 0, true, false)
        RETURNING prodid INTO v_prodid;

        INSERT INTO product.company (prodid, description, qty, status, workid, prstid)
        VALUES (v_prodid, vehicles[i][1], 1, true, v_workid, 'A')
        RETURNING prcoid INTO v_prcoid;

        INSERT INTO product.vehicle (
            prcoid, prodid, license_plate_number, vin_number, vetyid, futyid,
            year_of_manufacture, color, mileage, category, status,
            description, qty, workid, prstid
        )
        SELECT v_prcoid, v_prodid,
               vehicles[i][2], vehicles[i][3], vehicles[i][4], vehicles[i][5],
               vehicles[i][6]::smallint, vehicles[i][7], vehicles[i][8]::integer,
               vehicles[i][9], true, vehicles[i][1], 1, v_workid, 'A'
        ON CONFLICT (prcoid) DO NOTHING;
    END LOOP;
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 8. DATOS DE RENTA (para km real de vehículos)
-- ─────────────────────────────────────────────────────────────────────

-- Persona cliente genérica
DO $$
DECLARE v_iddoid smallint;
BEGIN
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'DNI';
    INSERT INTO public.person (enstid, iddoid, document, fln, name, sex, birthdate)
    VALUES ('AC', v_iddoid, '00000001', 'Cliente', 'Cliente Genérico', 'M', '1990-01-01')
    ON CONFLICT DO NOTHING;
END;
$$;

-- [S1] agenid requerido en client (FK nueva en BD-FINAL)
INSERT INTO public.client (enstid, persid, agenid, workid, origin, startdate)
SELECT 'AC', p.persid, a.agenid, w.workid, 'I', CURRENT_DATE
FROM public.person p, public.agency a, public.worker w
WHERE p.document = '00000001'
  AND a.name = 'Agencia Principal Arequipa'
  AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- company.worker para rentexecute (received_cowoid, delivery_cowoid)
INSERT INTO company.worker (compid, persid, status)
SELECT c.compid, w.persid, true
FROM public.company c, public.worker w
WHERE c.name = 'Neo Plus Business S.A.C.'
  AND w.username = 'herror.ortiz'
ON CONFLICT DO NOTHING;

-- Rentas con km para cada vehículo
DO $$
DECLARE
    v_workid  integer;
    v_clieid  integer;
    v_coceid  integer;
    v_cowoid  integer;
    rent_data text[][] := ARRAY[
        ARRAY['ARC-841', '45300', '48500', '2026-03-15'],
        ARRAY['AQJ-327', '69100', '72300', '2026-03-10'],
        ARRAY['ARS-194', '28900', '31200', '2026-03-20'],
        ARRAY['ART-562', '16500', '18750', '2026-03-25'],
        ARRAY['ARU-781', '51800', '54100', '2026-03-12'],
        ARRAY['ARV-093', '85900', '89200', '2026-03-08'],
        ARRAY['ARW-415', '40200', '42800', '2026-03-18'],
        ARRAY['ARX-628', '19700', '22100', '2026-03-22'],
        ARRAY['ARY-257', '58600', '61800', '2026-03-05'],
        ARRAY['ARZ-840','115200','118400', '2026-03-01'],
        ARRAY['BAA-193', '35400', '38700', '2026-03-28'],
        ARRAY['BAB-374', '73100', '76500', '2026-03-14']
    ];
    i        integer;
    v_prcoid integer;
    v_prodid integer;
    v_sereid integer;
    v_persid integer;
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
        FROM product.vehicle v WHERE v.license_plate_number = rent_data[i][1] LIMIT 1;

        INSERT INTO service.rentrequest (
            coceid, persid, driver, prodid, price, pricecoin,
            guarantee, frecuency, guaranteecoin,
            deliverydate, returndate, exactreturn, statid
        )
        VALUES (
            v_coceid, v_persid, v_persid, v_prodid, 150.00, 1,
            0, 'D', 1,
            rent_data[i][4]::timestamp - interval '3 days',
            rent_data[i][4]::timestamp, true, 'FI'
        )
        RETURNING sereid INTO v_sereid;

        INSERT INTO service.rentexecute (
            coceid, workid, clieid, delivered_date, delivered_workid,
            received_cowoid, kilometer_start, kilometer_end,
            return_date, made_sell_document, checklist, sereid, statid
        )
        VALUES (
            v_coceid, v_workid, v_clieid,
            rent_data[i][4]::timestamp - interval '3 days',
            v_workid, v_cowoid,
            rent_data[i][2]::integer, rent_data[i][3]::integer,
            rent_data[i][4]::timestamp,
            false, 1, v_sereid, 'FI'
        );
    END LOOP;
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 9. MATERIALES E INVENTARIO
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE
    v_workid     integer;
    v_lub smallint; v_fil smallint; v_flu smallint; v_rep smallint;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    SELECT macaid INTO v_lub FROM maintenance.material_category WHERE name = 'Lubricantes';
    SELECT macaid INTO v_fil FROM maintenance.material_category WHERE name = 'Filtros';
    SELECT macaid INTO v_flu FROM maintenance.material_category WHERE name = 'Fluidos';
    SELECT macaid INTO v_rep FROM maintenance.material_category WHERE name = 'Repuestos';

    INSERT INTO maintenance.material (macaid, name, unit_of_measure, stock_total, stock_minimum, created_by) VALUES
        (v_lub,'Aceite Motor 5W-30 Sintético',     'Litros', 48.0, 12.0, v_workid),
        (v_lub,'Aceite Motor 10W-40 Semi-sintético','Litros', 32.0,  8.0, v_workid),
        (v_lub,'Aceite Transmisión ATF',            'Litros', 20.0,  6.0, v_workid),
        (v_fil,'Filtro de Aceite Toyota',           'Unidad', 18,    6,   v_workid),
        (v_fil,'Filtro de Aceite Hyundai/Kia',      'Unidad', 12,    4,   v_workid),
        (v_fil,'Filtro de Aire Toyota',             'Unidad', 10,    4,   v_workid),
        (v_fil,'Filtro de Combustible Diésel',      'Unidad', 12,    4,   v_workid),
        (v_flu,'Líquido de Frenos DOT 4',           'Litros', 12.0,  4.0, v_workid),
        (v_flu,'Refrigerante Long Life 50/50',      'Litros', 24.0,  6.0, v_workid),
        (v_rep,'Pastillas de Freno Delanteras',     'Par',    6,     2,   v_workid),
        (v_rep,'Bujías NGK (set x4)',               'Set',    8,     4,   v_workid),
        (v_rep,'Correa de Distribución',            'Unidad', 4,     2,   v_workid)
    ON CONFLICT DO NOTHING;
END;
$$;

-- Proveedor de materiales
DO $$
DECLARE v_iddoid smallint; v_workid integer;
BEGIN
    SELECT iddoid INTO v_iddoid FROM list.identitydocumenttype WHERE name = 'RUC';
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    INSERT INTO public.person (enstid, iddoid, document, fln, name, sex, birthdate)
    VALUES ('AC', v_iddoid, '20601234561', 'Proveedor', 'Repuestos del Sur S.R.L.', 'M', '2000-01-01')
    ON CONFLICT DO NOTHING;
    INSERT INTO public.provider (persid, workid, status)
    SELECT p.persid, v_workid, true
    FROM public.person p WHERE p.document = '20601234561'
    ON CONFLICT DO NOTHING;
END;
$$;

-- Lotes con fechas de vencimiento
DO $$
DECLARE
    v_workid integer; v_provid integer; v_mateid integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    SELECT provid INTO v_provid FROM public.provider
        JOIN public.person p ON public.provider.persid = p.persid
        WHERE p.document = '20601234561' LIMIT 1;

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 5W-30 Sintético';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 24.0, 24.0, 28.50, '2027-04-01', v_provid, 'activo', v_workid),
           (v_mateid, 24.0, 24.0, 29.00, '2027-07-15', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 10W-40 Semi-sintético';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 32.0, 32.0, 22.00, '2027-05-01', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Líquido de Frenos DOT 4';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 6.0, 6.0, 35.00, '2027-09-01', v_provid, 'activo', v_workid),
           (v_mateid, 6.0, 6.0, 36.00, '2028-02-01', v_provid, 'activo', v_workid);

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Refrigerante Long Life 50/50';
    INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, expiration_date, provid, lot_status, created_by)
    VALUES (v_mateid, 24.0, 24.0, 18.50, '2027-12-01', v_provid, 'activo', v_workid);

    -- Filtros y repuestos (sin fecha de vencimiento)
    FOR v_mateid IN
        SELECT mateid FROM maintenance.material
        WHERE name IN ('Filtro de Aceite Toyota','Filtro de Aceite Hyundai/Kia',
                       'Filtro de Aire Toyota','Filtro de Combustible Diésel',
                       'Pastillas de Freno Delanteras','Bujías NGK (set x4)','Correa de Distribución')
    LOOP
        INSERT INTO maintenance.material_lot (mateid, initial_quantity, current_quantity, unit_cost, provid, lot_status, created_by)
        VALUES (v_mateid, 10, 10, 50.00, v_provid, 'activo', v_workid)
        ON CONFLICT DO NOTHING;
    END LOOP;
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 10. CATÁLOGO DE ACCIONES (actualizado según lista proporcionada)
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE v_l1 smallint; v_l2 smallint;
BEGIN
    SELECT altoid INTO v_l1 FROM maintenance.action_list_type WHERE name = 'Elementos de Reemplazo y Aplicación';
    SELECT altoid INTO v_l2 FROM maintenance.action_list_type WHERE name = 'Operaciones';

    -- Elementos de Reemplazo y Aplicación
    INSERT INTO maintenance.action_catalog (altoid, name, category, recommended_quantity, unit_of_measure, useful_life_km, expires_by_time, useful_life_days) VALUES
        (v_l1,'Aceite de Motor',                        'Fluido',   '4-5','Litros', 5000,  true,  365),
        (v_l1,'Aceite de Transmisión',                  'Fluido',   '2-3','Litros', NULL, true,  730),
        (v_l1,'Aceite de Dirección',                    'Fluido',   '1',  'Litros', NULL, true,  730),
        (v_l1,'Líquido Refrigerante de Motor',          'Fluido',   '1',  'Litros', NULL, true,  730),
        (v_l1,'Limpieza de Inyectores',                 'Aditivo',  '1',  'Unidad', 20000,false, NULL),
        (v_l1,'Líquido de Frenos/Embrague',             'Fluido',   '0.5','Litros', NULL, true,  730),
        (v_l1,'Solvente para limpieza de Frenos',       'Químico',  '1',  'Unidad', NULL, false, NULL),
        (v_l1,'Champú limpiador de parabrisas',         'Químico',  '1',  'Unidad', NULL, false, NULL),
        (v_l1,'Filtro de Aceite de Motor',              'Filtro',   '1',  'Unidad', 5000, false, NULL),
        (v_l1,'Filtro de Aire de Motor',                'Filtro',   '1',  'Unidad', 15000,false, NULL),
        (v_l1,'Filtro de Aire de Cabina (Si existiera)','Filtro',   '1',  'Unidad', 15000,false, NULL),
        (v_l1,'Filtro de Combustible',                  'Filtro',   '1',  'Unidad', 20000,false, NULL),
        (v_l1,'Bujías (Motor a Gasolina)',              'Repuesto', '4',  'Unidad', 20000,false, NULL),
        (v_l1,'Correa de Accesorios',                   'Repuesto', '1',  'Unidad', 60000,false, NULL),
        (v_l1,'Correa de Distribución de Motor (Si existiera)','Repuesto','1','Unidad',60000,false, NULL),
        (v_l1,'Tensor de Correa de Distribución (Si existiera)','Repuesto','1','Unidad',60000,false, NULL),
        (v_l1,'Termostato',                             'Repuesto', '1',  'Unidad', NULL, false, NULL),
        (v_l1,'Arandela de Tapón de Carter',            'Repuesto', '1',  'Unidad', 5000, false, NULL)
    ON CONFLICT DO NOTHING;

    -- Operaciones
    INSERT INTO maintenance.action_catalog (altoid, name, category, recommended_quantity, unit_of_measure, useful_life_km, expires_by_time, useful_life_days) VALUES
        (v_l2,'Lubricar bisagras de cada puerta',               'Lubricación',   NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar funcionamiento de seguros en cada puerta','Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Nivelar y alinear Luces Principales',            'Ajuste',        NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar luces del tablero de instrumentos',      'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar luces interiores y exteriores',          'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar juego libre de pedal de Freno',          'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar juego libre del pedal de Embrague (Si corresponde)','Inspección',NULL,NULL,NULL,false,NULL),
        (v_l2,'Limpieza y regulación de Frenos',                'Ajuste',        NULL,NULL, NULL, false, NULL),
        (v_l2,'Regular freno de estacionamiento',               'Ajuste',        NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar líneas de freno, mangueras y conexiones','Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar líneas, mangueras y conexiones de la Dirección','Inspección',NULL,NULL,NULL,false,NULL),
        (v_l2,'Revisar funcionamiento de la Dirección y articulaciones','Inspección',NULL,NULL,NULL,false,NULL),
        (v_l2,'Juego lateral de cojinetes de rueda delantera',  'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar rótulas y bocinas de suspensión delantera','Inspección',   NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar estado de guardapolvo de palier delanteros','Inspección',  NULL,NULL, NULL, false, NULL),
        (v_l2,'Juego axial de cojinetes de rueda trasera',      'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Revisar bocinas y articulaciones de suspensión trasera','Inspección',NULL,NULL,NULL,false,NULL),
        (v_l2,'Ultrasonido inyectores a gasolina o aplique producto (diesel)','Limpieza',NULL,NULL,20000,false,NULL),
        (v_l2,'Revisar presión y estado de neumáticos',         'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Rotación de neumáticos',                         'Servicio',      NULL,NULL, 10000,false,NULL),
        (v_l2,'Ajuste de ruedas al torque especificado por fábrica','Ajuste',     NULL,NULL, NULL, false, NULL),
        (v_l2,'Escaneo de unidad',                              'Diagnóstico',   NULL,NULL, NULL, false, NULL),
        (v_l2,'Bornes de batería / Nivel electrolito',          'Inspección',    NULL,NULL, NULL, false, NULL),
        (v_l2,'Limpieza de válvula y ductos de sistema EGR (si corresponde)','Limpieza',NULL,NULL,40000,false,NULL)
    ON CONFLICT DO NOTHING;
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 11. CRONOGRAMAS POR VEHÍCULO
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE v_workid integer; v_prcoid integer; v_km integer;
BEGIN
    SELECT workid INTO v_workid FROM public.worker WHERE username = 'herror.ortiz';
    FOR v_prcoid, v_km IN
        SELECT v.prcoid, v.mileage FROM product.vehicle v WHERE v.status = true
    LOOP
        INSERT INTO maintenance.vehicle_schedule
            (prcoid, interval_km, next_km, alert_km_threshold, created_by)
        VALUES (v_prcoid, 5000, (v_km / 5000 + 1) * 5000, 800, v_workid)
        ON CONFLICT (prcoid) DO NOTHING;
    END LOOP;
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 12. HISTORIAL DE MANTENIMIENTOS (con [S4] assigned_to)
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE
    v_jefe_id  integer; v_mec1_id  integer; v_mec2_id  integer;
    v_h1 integer; v_h2 integer; v_r1 integer; v_yn integer; v_sp integer;
    v_cal smallint; v_eme smallint; v_sa smallint; v_sb smallint;
    v_mainid integer;
BEGIN
    SELECT workid INTO v_jefe_id FROM public.worker WHERE username = 'herror.ortiz';
    SELECT workid INTO v_mec1_id FROM public.worker WHERE username = 'juan.quispe';
    SELECT workid INTO v_mec2_id FROM public.worker WHERE username = 'pedro.mamani';
    SELECT prcoid INTO v_h1 FROM product.vehicle WHERE license_plate_number = 'ARC-841';
    SELECT prcoid INTO v_h2 FROM product.vehicle WHERE license_plate_number = 'AQJ-327';
    SELECT prcoid INTO v_r1 FROM product.vehicle WHERE license_plate_number = 'ARS-194';
    SELECT prcoid INTO v_yn FROM product.vehicle WHERE license_plate_number = 'ART-562';
    SELECT prcoid INTO v_sp FROM product.vehicle WHERE license_plate_number = 'ARZ-840';
    SELECT matyid INTO v_cal FROM maintenance.maintenance_type WHERE name = 'Calendarizado';
    SELECT matyid INTO v_eme FROM maintenance.maintenance_type WHERE name = 'Emergencia';
    SELECT setyid INTO v_sa  FROM maintenance.service_type WHERE code = 'A';
    SELECT setyid INTO v_sb  FROM maintenance.service_type WHERE code = 'B';

    -- Mant. 1: Hilux ARC-841 — Calendarizado A
    INSERT INTO maintenance.maintenance
        (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
         oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
         origin_service, assigned_to, workid)
    VALUES (v_h1, v_cal, v_sa, 'ORD-2026-001', '2026-01-10', 43500, 5100,
            'Mobil', '5W-30', true, 'Taller propio', v_mec1_id, v_jefe_id)
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative)
    VALUES (v_mainid, 'Bueno', true);

    -- Mant. 2: Hilux AQJ-327 — Emergencia completa
    INSERT INTO maintenance.maintenance
        (prcoid, matyid, order_number, maintenance_date, mileage,
         is_emergency_complete, origin_service, assigned_to, workid, note)
    VALUES (v_h2, v_eme, 'ORD-2026-002', '2026-02-05', 69800,
            true, 'Taller propio', v_mec2_id, v_jefe_id,
            'Falla en sistema de frenos - pastillas desgastadas')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'Reparado', true, 'Revisar pastillas traseras en próximo servicio.');

    -- Mant. 3: RAV4 — Calendarizado B
    INSERT INTO maintenance.maintenance
        (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
         oil_brand, oil_viscosity_sae, show_oil_in_next_maintenance,
         origin_service, assigned_to, workid)
    VALUES (v_r1, v_cal, v_sb, 'ORD-2026-003', '2026-03-01', 26200, 5050,
            'Castrol', '5W-30', true, 'Taller propio', v_mec1_id, v_jefe_id)
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative)
    VALUES (v_mainid, 'Excelente', true);

    -- Mant. 4: Sprinter — Calendarizado B
    INSERT INTO maintenance.maintenance
        (prcoid, matyid, setyid, order_number, maintenance_date, mileage, km_since_last,
         oil_brand, oil_viscosity_sae, origin_service, assigned_to, workid)
    VALUES (v_sp, v_cal, v_sb, 'ORD-2026-004', '2026-02-20', 113400, 5200,
            'Shell', '10W-40', 'Taller propio', v_mec2_id, v_jefe_id)
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'Regular', true, 'Programar revisión de inyectores en próximo servicio.');

    -- Mant. 5: Yaris — Emergencia parcial
    INSERT INTO maintenance.maintenance
        (prcoid, matyid, order_number, maintenance_date, mileage,
         is_emergency_complete, origin_service, assigned_to, workid, note)
    VALUES (v_yn, v_eme, 'ORD-2026-005', '2026-03-25', 18100,
            false, 'Taller propio', v_mec1_id, v_jefe_id,
            'Recalentamiento del motor - solo se atendió lo urgente')
    RETURNING mainid INTO v_mainid;
    INSERT INTO maintenance.diagnosis (mainid, general_status, vehicle_operative, future_recommendations)
    VALUES (v_mainid, 'En observación', true,
            'Diagnóstico completo del sistema de enfriamiento en próxima semana.');
END;
$$;

-- ─────────────────────────────────────────────────────────────────────
-- 13. CONSUMOS DE MATERIALES (vinculados a los mantenimientos)
-- ─────────────────────────────────────────────────────────────────────

DO $$
DECLARE
    v_m1 integer; v_m2 integer; v_m3 integer;
    v_maloid integer; v_mateid integer;
BEGIN
    SELECT mainid INTO v_m1 FROM maintenance.maintenance WHERE order_number = 'ORD-2026-001';
    SELECT mainid INTO v_m2 FROM maintenance.maintenance WHERE order_number = 'ORD-2026-002';
    SELECT mainid INTO v_m3 FROM maintenance.maintenance WHERE order_number = 'ORD-2026-003';

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Aceite Motor 5W-30 Sintético';
    SELECT maloid INTO v_maloid FROM maintenance.material_lot WHERE mateid = v_mateid AND lot_status = 'activo' LIMIT 1;
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_m1, v_mateid, v_maloid, 4.5, 'Stock propio'),
           (v_m3, v_mateid, v_maloid, 4.5, 'Stock propio');

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Filtro de Aceite Toyota';
    SELECT maloid INTO v_maloid FROM maintenance.material_lot WHERE mateid = v_mateid AND lot_status = 'activo' LIMIT 1;
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_m1, v_mateid, v_maloid, 1, 'Stock propio');

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Pastillas de Freno Delanteras';
    SELECT maloid INTO v_maloid FROM maintenance.material_lot WHERE mateid = v_mateid AND lot_status = 'activo' LIMIT 1;
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_m2, v_mateid, v_maloid, 1, 'Stock propio');

    SELECT mateid INTO v_mateid FROM maintenance.material WHERE name = 'Filtro de Aire Toyota';
    SELECT maloid INTO v_maloid FROM maintenance.material_lot WHERE mateid = v_mateid AND lot_status = 'activo' LIMIT 1;
    INSERT INTO maintenance.material_consumption (mainid, mateid, maloid, quantity, origin)
    VALUES (v_m3, v_mateid, v_maloid, 1, 'Stock propio');
END;
$$;

COMMIT;

-- Verificación
SELECT 'Trabajadores:'   AS t, COUNT(*) FROM public.worker;
SELECT 'Vehículos:'      AS t, COUNT(*) FROM product.vehicle WHERE status = true;
SELECT 'Materiales:'     AS t, COUNT(*) FROM maintenance.material;
SELECT 'Lotes:'          AS t, COUNT(*) FROM maintenance.material_lot;
SELECT 'Mantenimientos:' AS t, COUNT(*) FROM maintenance.maintenance;
SELECT 'Cronogramas:'    AS t, COUNT(*) FROM maintenance.vehicle_schedule;