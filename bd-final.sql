-- =====================================================================
-- SISTEMA DE GESTIÓN DE MANTENIMIENTO VEHICULAR CON BI
-- Neo Plus Business S.A.C.
-- Base de datos: PostgreSQL
-- Enfoque: Híbrido (Database First existente + Code First nuevas)
-- Versión: BD-FINAL — COMPLETAMENTE CORREGIDA
-- =====================================================================
--
-- CORRECCIONES APLICADAS (respecto a versión anterior):
--   [F1]  Secuencia list.companyconditionlist_coclid_seq duplicada → eliminada
--   [F2]  Secuencias sin esquema corregidas: address_addrid_seq, zone_zoneid_seq,
--         agency_agenid_seq, bankaccounts_baacid_seq → todas con prefijo public.
--   [F3]  Secuencia public.windows_windid_seq agregada (faltaba)
--   [F4]  Orden de creación corregido: todas las tablas list.* van ANTES de public.*
--         para que las FK no fallen
--   [F5]  Tablas faltantes del esquema list agregadas en orden correcto:
--         documenttype, moneymovetype, banks, reasonforcancellation,
--         sunatelectronicanswer, companyconditionlist, companystatuslist,
--         taxpayertypelist
--   [F6]  Tablas faltantes del esquema public agregadas:
--         windows, sale, saleitems, bankaccounts, payments, salepayments,
--         voucher, voucheritems, voucherpayments
--   [F7]  product.vehicle: agregado INHERITS (product.company) que faltaba
--   [F8]  company.worker: FK a public.company re-incluida
--   [F9]  public.residence: nextval usa public.address_addrid_seq con esquema
--   [F10] public.zone/agency/windows/bankaccounts: nextval con esquema explícito
--   [F11] Mantenidos todos los arreglos originales C1-C14 de la versión anterior
-- =====================================================================


-- =====================================================================
-- ESQUEMAS
-- =====================================================================
CREATE SCHEMA IF NOT EXISTS list;
CREATE SCHEMA IF NOT EXISTS product;
CREATE SCHEMA IF NOT EXISTS company;
CREATE SCHEMA IF NOT EXISTS service;
CREATE SCHEMA IF NOT EXISTS maintenance;


-- =====================================================================
-- SECUENCIAS — ESQUEMA LIST
-- =====================================================================
CREATE SEQUENCE IF NOT EXISTS list.coin_coinid_seq;
CREATE SEQUENCE IF NOT EXISTS list.civilstatus_cistid_seq;
CREATE SEQUENCE IF NOT EXISTS list.identitydocumenttype_iddoid_seq;
CREATE SEQUENCE IF NOT EXISTS list.companyconditionlist_coclid_seq;  -- [F1] solo una vez
CREATE SEQUENCE IF NOT EXISTS list.companystatuslist_coslid_seq;
CREATE SEQUENCE IF NOT EXISTS list.taxpayertypelist_tptlid_seq;
CREATE SEQUENCE IF NOT EXISTS list.jobcategory_jocaid_seq;
CREATE SEQUENCE IF NOT EXISTS list.documentlist_doliid_seq;
CREATE SEQUENCE IF NOT EXISTS list.documenttype_dotyid_seq;
CREATE SEQUENCE IF NOT EXISTS list.moneymovetype_mmtyid_seq;
CREATE SEQUENCE IF NOT EXISTS list.banks_bankid_seq;
CREATE SEQUENCE IF NOT EXISTS list.reasonforcancellation_recaid_seq;
CREATE SEQUENCE IF NOT EXISTS list.accountingplanelementtype_aetyid_seq;

-- =====================================================================
-- SECUENCIAS — ESQUEMA PUBLIC
-- =====================================================================
CREATE SEQUENCE IF NOT EXISTS public.country_counid_seq;
CREATE SEQUENCE IF NOT EXISTS public.department_depaid_seq;
CREATE SEQUENCE IF NOT EXISTS public.province_provid_seq;
CREATE SEQUENCE IF NOT EXISTS public.district_distid_seq;
CREATE SEQUENCE IF NOT EXISTS public.jobarea_joarid_seq;
CREATE SEQUENCE IF NOT EXISTS public.job_jobid_seq;
CREATE SEQUENCE IF NOT EXISTS public.product_prodid_seq;
CREATE SEQUENCE IF NOT EXISTS public.person_persid_seq;
CREATE SEQUENCE IF NOT EXISTS public.company_compid_seq;
CREATE SEQUENCE IF NOT EXISTS public.worker_workid_seq;
CREATE SEQUENCE IF NOT EXISTS public.client_clieid_seq;
CREATE SEQUENCE IF NOT EXISTS public.provider_provid_seq;
CREATE SEQUENCE IF NOT EXISTS public.costcenter_coceid_seq;
CREATE SEQUENCE IF NOT EXISTS public.sale_saleid_seq;
CREATE SEQUENCE IF NOT EXISTS public.saleitems_saitid_seq;
CREATE SEQUENCE IF NOT EXISTS public.payments_paymid_seq;
CREATE SEQUENCE IF NOT EXISTS public.salepayments_sapaid_seq;
CREATE SEQUENCE IF NOT EXISTS public.windows_windid_seq;          -- [F3] agregada
CREATE SEQUENCE IF NOT EXISTS public.zone_zoneid_seq;             -- [F2] con esquema
CREATE SEQUENCE IF NOT EXISTS public.agency_agenid_seq;           -- [F2] con esquema
CREATE SEQUENCE IF NOT EXISTS public.bankaccounts_baacid_seq;     -- [F2] con esquema
CREATE SEQUENCE IF NOT EXISTS public.address_addrid_seq;          -- [F2] con esquema
CREATE SEQUENCE IF NOT EXISTS public.baucher_baucid_seq;
CREATE SEQUENCE IF NOT EXISTS public.baucheritems_baitid_seq;

-- =====================================================================
-- SECUENCIAS — ESQUEMAS PRODUCT, SERVICE, COMPANY
-- =====================================================================
CREATE SEQUENCE IF NOT EXISTS product.company_prcoid_seq;
CREATE SEQUENCE IF NOT EXISTS product.productbrand_prbrid_seq;
CREATE SEQUENCE IF NOT EXISTS product.productmodel_prmoid_seq;
CREATE SEQUENCE IF NOT EXISTS product.productcategory_prcaid_seq;
CREATE SEQUENCE IF NOT EXISTS product.productmerchandise_prmeid_seq;
CREATE SEQUENCE IF NOT EXISTS service.rentrequest_sereid_seq;
CREATE SEQUENCE IF NOT EXISTS service.rentexecute_seexid_seq;
CREATE SEQUENCE IF NOT EXISTS company.worker_cowoid_seq;

-- =====================================================================
-- SECUENCIAS — ESQUEMA MAINTENANCE
-- =====================================================================
CREATE SEQUENCE IF NOT EXISTS maintenance.config_system_cosyid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.maintenance_type_matyid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.service_type_setyid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.action_list_type_altoid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.action_catalog_acatid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.vehicle_schedule_veshid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.schedule_action_shacid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.maintenance_mainid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.maintenance_action_detail_madeid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.diagnosis_diagid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_category_macaid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_mateid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_lot_maloid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_consumption_macoid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_discard_madiid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.installed_component_incoid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.alert_config_alcoid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.alert_log_alloid_seq;
CREATE SEQUENCE IF NOT EXISTS maintenance.material_rating_matraid_seq;


-- =====================================================================
-- PARTE 1: TABLAS — ESQUEMA LIST
-- [F4] Todas las tablas list.* van ANTES de public.* para respetar FKs
-- =====================================================================

CREATE TABLE IF NOT EXISTS list.producttype
(
    prtyid  character(2)            NOT NULL,
    name    character varying(50)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT producttype_pkey PRIMARY KEY (prtyid),
    CONSTRAINT producttype_name_key UNIQUE (name),
    CONSTRAINT producttype_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.coin
(
    coinid  smallint NOT NULL DEFAULT nextval('list.coin_coinid_seq'),
    name    character varying(50)   NOT NULL,
    symbol  character varying(3),
    isocode character(3)            NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT coin_pkey PRIMARY KEY (coinid),
    CONSTRAINT coin_isocode_key UNIQUE (isocode),
    CONSTRAINT coin_name_key UNIQUE (name),
    CONSTRAINT coin_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.vehicletype
(
    vetyid      character(2)            NOT NULL,
    name        character varying(25)   NOT NULL,
    description text,
    CONSTRAINT vehicletype_pkey PRIMARY KEY (vetyid)
);

CREATE TABLE IF NOT EXISTS list.fueltype
(
    futyid      character(2)            NOT NULL,
    name        character varying(45),
    description text,
    CONSTRAINT fueltype_pkey PRIMARY KEY (futyid)
);

CREATE TABLE IF NOT EXISTS list.entitystatus
(
    enstid  character(2)            NOT NULL,
    name    character varying(20)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT entitystatus_pkey PRIMARY KEY (enstid),
    CONSTRAINT entitystatus_name_key UNIQUE (name),
    CONSTRAINT entitystatus_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.status
(
    statid  character(2)            NOT NULL,
    name    character varying(50)   NOT NULL,
    CONSTRAINT status_pkey PRIMARY KEY (statid)
);

CREATE TABLE IF NOT EXISTS list.civilstatus
(
    cistid  smallint NOT NULL DEFAULT nextval('list.civilstatus_cistid_seq'),
    name    character varying(20)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT civilstatus_pkey PRIMARY KEY (cistid),
    CONSTRAINT civilstatus_name_key UNIQUE (name),
    CONSTRAINT civilstatus_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.identitydocumenttype
(
    iddoid  smallint NOT NULL DEFAULT nextval('list.identitydocumenttype_iddoid_seq'),
    name    character varying(30)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT identitydocumenttype_pkey PRIMARY KEY (iddoid),
    CONSTRAINT identitydocumenttype_name_key UNIQUE (name),
    CONSTRAINT identitydocumenttype_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.jobcategory
(
    jocaid      smallint NOT NULL DEFAULT nextval('list.jobcategory_jocaid_seq'),
    name        character varying(100)  NOT NULL,
    description text                    NOT NULL,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT jobcategory_pkey PRIMARY KEY (jocaid),
    CONSTRAINT jobcategory_name_key UNIQUE (name),
    CONSTRAINT jobcategory_name_check CHECK (name <> '')
);

-- [F5] companyconditionlist, companystatuslist, taxpayertypelist
--      van AQUÍ (antes de public.company que las referencia)
CREATE TABLE IF NOT EXISTS list.companyconditionlist
(
    coclid  smallint NOT NULL DEFAULT nextval('list.companyconditionlist_coclid_seq'),
    name    character varying(50)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT companyconditionlist_pkey PRIMARY KEY (coclid),
    CONSTRAINT companyconditionlist_name_key UNIQUE (name),
    CONSTRAINT companyconditionlist_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.companystatuslist
(
    coslid  smallint NOT NULL DEFAULT nextval('list.companystatuslist_coslid_seq'),
    name    character varying(50)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT companystatuslist_pkey PRIMARY KEY (coslid),
    CONSTRAINT companystatuslist_name_key UNIQUE (name),
    CONSTRAINT companystatuslist_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.taxpayertypelist
(
    tptlid  smallint NOT NULL DEFAULT nextval('list.taxpayertypelist_tptlid_seq'),
    name    character varying(100)  NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT taxpayertypelist_pkey PRIMARY KEY (tptlid),
    CONSTRAINT taxpayertypelist_name_key UNIQUE (name),
    CONSTRAINT taxpayertypelist_name_check CHECK (name <> '')
);

-- [F5] documentlist y documenttype antes de ser usadas
CREATE TABLE IF NOT EXISTS list.documentlist
(
    doliid  smallint NOT NULL DEFAULT nextval('list.documentlist_doliid_seq'),
    name    character varying(150)  NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT documentlist_pkey PRIMARY KEY (doliid),
    CONSTRAINT documentlist_name_key UNIQUE (name),
    CONSTRAINT documentlist_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.documenttype
(
    dotyid  smallint NOT NULL DEFAULT nextval('list.documenttype_dotyid_seq'),
    name    character varying(50)   NOT NULL,
    code    character varying(3)    NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    doliid  integer,
    CONSTRAINT documenttype_pkey PRIMARY KEY (dotyid),
    CONSTRAINT documenttype_code_key UNIQUE (code),
    CONSTRAINT documenttype_name_key UNIQUE (name),
    CONSTRAINT documenttype_doliid_fkey FOREIGN KEY (doliid)
        REFERENCES list.documentlist (doliid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT documenttype_name_check CHECK (name <> '')
);

-- [F5] moneymovetype, banks, reasonforcancellation, sunatelectronicanswer
CREATE TABLE IF NOT EXISTS list.moneymovetype
(
    mmtyid      integer NOT NULL DEFAULT nextval('list.moneymovetype_mmtyid_seq'),
    name        character varying(225),
    movetype    character(1),
    description text,
    CONSTRAINT moneymovetype_pkey PRIMARY KEY (mmtyid)
);

CREATE TABLE IF NOT EXISTS list.banks
(
    bankid  smallint NOT NULL DEFAULT nextval('list.banks_bankid_seq'),
    name    character varying(150)  NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT banks_pkey PRIMARY KEY (bankid),
    CONSTRAINT banks_name_key UNIQUE (name),
    CONSTRAINT banks_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS list.reasonforcancellation
(
    recaid      smallint NOT NULL DEFAULT nextval('list.reasonforcancellation_recaid_seq'),
    description character varying(255),
    CONSTRAINT reasonforcancellation_pkey PRIMARY KEY (recaid)
);

CREATE TABLE IF NOT EXISTS list.sunatelectronicanswer
(
    suea        character(2)            NOT NULL,
    name        character varying(50),
    description text,
    CONSTRAINT sunatelectronicanswer_pkey PRIMARY KEY (suea)
);

CREATE TABLE IF NOT EXISTS list.accountingplanelementtype
(
    aetyid  character(2) NOT NULL DEFAULT nextval('list.accountingplanelementtype_aetyid_seq'),
    name    character varying(100),
    CONSTRAINT accountingplanelementtype_pkey PRIMARY KEY (aetyid)
);


-- =====================================================================
-- PARTE 1: TABLAS — ESQUEMA PUBLIC (tablas base)
-- =====================================================================

CREATE TABLE IF NOT EXISTS public.accountingplan
(
    account         character varying(255)  NOT NULL,
    name            character varying(255),
    fatheraccount   character varying(255),
    commentary      character varying(500),
    kind            character varying(3),
    status          boolean DEFAULT true,
    aetyid          character(2),
    rcd             boolean,
    level           smallint                NOT NULL,
    CONSTRAINT accountingplan_pkey PRIMARY KEY (account),
    CONSTRAINT accountingplan_aetyid_fkey FOREIGN KEY (aetyid)
        REFERENCES list.accountingplanelementtype (aetyid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT accountingplan_fatheraccount_fkey FOREIGN KEY (fatheraccount)
        REFERENCES public.accountingplan (account)
        ON UPDATE NO ACTION ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS public.country
(
    counid      smallint NOT NULL DEFAULT nextval('public.country_counid_seq'),
    name        character varying(50)   NOT NULL,
    demonym     character varying(50),
    phoneprefix character varying(7),
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT country_pkey PRIMARY KEY (counid),
    CONSTRAINT country_demonym_key UNIQUE (demonym),
    CONSTRAINT country_name_key UNIQUE (name),
    CONSTRAINT country_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.department
(
    depaid      smallint NOT NULL DEFAULT nextval('public.department_depaid_seq'),
    counid      smallint NOT NULL,
    name        character varying(100)  NOT NULL,
    isocode     character varying(3),
    phoneprefix character varying(3),
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT department_pkey PRIMARY KEY (depaid),
    CONSTRAINT department_counid_name_unique UNIQUE (counid, name),
    CONSTRAINT department_fkey_counid FOREIGN KEY (counid)
        REFERENCES public.country (counid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT department_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.province
(
    provid  integer NOT NULL DEFAULT nextval('public.province_provid_seq'),
    depaid  smallint NOT NULL,
    name    character varying(100)  NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT province_pkey PRIMARY KEY (provid),
    CONSTRAINT province_depaid_name_unique UNIQUE (depaid, name),
    CONSTRAINT province_fkey_depaid FOREIGN KEY (depaid)
        REFERENCES public.department (depaid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT province_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.district
(
    distid      integer NOT NULL DEFAULT nextval('public.district_distid_seq'),
    provid      integer NOT NULL,
    name        character varying(100)  NOT NULL,
    ubigeo      character(6)            NOT NULL,
    latitude    numeric(15,12),
    longitude   numeric(15,12),
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT district_pkey PRIMARY KEY (distid),
    CONSTRAINT district_provid_name_unique UNIQUE (provid, name),
    CONSTRAINT district_fkey_provid FOREIGN KEY (provid)
        REFERENCES public.province (provid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT district_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.jobarea
(
    joarid           smallint NOT NULL DEFAULT nextval('public.jobarea_joarid_seq'),
    name             character varying(100)  NOT NULL,
    registrationdate date                    NOT NULL DEFAULT CURRENT_DATE,
    status           boolean                 NOT NULL DEFAULT true,
    CONSTRAINT jobarea_pkey PRIMARY KEY (joarid),
    CONSTRAINT jobarea_name_key UNIQUE (name),
    CONSTRAINT jobarea_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.job
(
    jobid              smallint NOT NULL DEFAULT nextval('public.job_jobid_seq'),
    joarid             smallint,
    name               character varying(100)  NOT NULL,
    jocaid             smallint,
    registrationdate   date                    NOT NULL DEFAULT CURRENT_DATE,
    status             boolean                 NOT NULL DEFAULT true,
    cansignature       boolean DEFAULT false,
    canarrearsdiscount boolean DEFAULT false,
    CONSTRAINT job_pkey PRIMARY KEY (jobid),
    CONSTRAINT job_name_key UNIQUE (name),
    CONSTRAINT job_fkey_joarid FOREIGN KEY (joarid)
        REFERENCES public.jobarea (joarid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT job_fkey_jocaid FOREIGN KEY (jocaid)
        REFERENCES list.jobcategory (jocaid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT job_name_check CHECK (name <> '')
);

-- PERSON: sin referencias circulares (se agregan con DO $$ posterior)
CREATE TABLE IF NOT EXISTS public.person
(
    persid           integer NOT NULL DEFAULT nextval('public.person_persid_seq'),
    enstid           character(2)            NOT NULL DEFAULT 'PA',
    iddoid           smallint                NOT NULL,
    document         character varying(15)   NOT NULL,
    fln              character varying(50)   NOT NULL,
    mln              character varying(50),
    name             character varying(100)  NOT NULL,
    sex              character(1)            NOT NULL,
    birthdate        date                    NOT NULL,
    cistid           smallint,
    nationality      smallint,
    peruvian         boolean,
    birthplace       integer,
    sons             smallint,
    observation      text,
    imagepath        text,
    anaid            integer,
    recorderid       integer,
    registrationdate timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    registrationtime time without time zone       NOT NULL DEFAULT CURRENT_TIME,
    status           boolean                 NOT NULL DEFAULT true,
    CONSTRAINT person_pkey PRIMARY KEY (persid),
    CONSTRAINT person_document_key UNIQUE (document),
    CONSTRAINT person_imagepath_key UNIQUE (imagepath),
    CONSTRAINT person_fkey_birthplace FOREIGN KEY (birthplace)
        REFERENCES public.district (distid),
    CONSTRAINT person_fkey_cistid FOREIGN KEY (cistid)
        REFERENCES list.civilstatus (cistid),
    CONSTRAINT person_fkey_enstid FOREIGN KEY (enstid)
        REFERENCES list.entitystatus (enstid),
    CONSTRAINT person_fkey_iddoid FOREIGN KEY (iddoid)
        REFERENCES list.identitydocumenttype (iddoid),
    CONSTRAINT person_fkey_nationality FOREIGN KEY (nationality)
        REFERENCES public.country (counid),
    CONSTRAINT person_document_check CHECK (document <> ''),
    CONSTRAINT person_fln_check CHECK (fln <> ''),
    CONSTRAINT person_name_check CHECK (name <> ''),
    CONSTRAINT person_sex_check CHECK (sex = ANY (ARRAY['M','F','O','-','U'])),
    CONSTRAINT person_sons_check CHECK (sons >= 0)
);

-- WORKER: sin referencias circulares
CREATE TABLE IF NOT EXISTS public.worker
(
    workid           integer NOT NULL DEFAULT nextval('public.worker_workid_seq'),
    wenstid          character(2)            NOT NULL DEFAULT 'AC',
    persid           integer                 NOT NULL,
    jobid            smallint                NOT NULL,
    startdate        date                    NOT NULL DEFAULT CURRENT_DATE,
    username         character varying(25),
    password         character varying(32),
    webreception     boolean                 NOT NULL DEFAULT false,
    wrecorderid      integer,
    registrationdate date                    NOT NULL DEFAULT CURRENT_DATE,
    registrationtime time without time zone  NOT NULL DEFAULT CURRENT_TIME,
    status           boolean                 NOT NULL DEFAULT true,
    email            character varying(255),
    locked           boolean DEFAULT false,
    CONSTRAINT worker_pkey PRIMARY KEY (workid),
    CONSTRAINT worker_persid_key UNIQUE (persid),
    CONSTRAINT worker_username_key UNIQUE (username),
    CONSTRAINT worker_fkey_jobid FOREIGN KEY (jobid)
        REFERENCES public.job (jobid),
    CONSTRAINT worker_fkey_persid FOREIGN KEY (persid)
        REFERENCES public.person (persid),
    CONSTRAINT worker_fkey_wenstid FOREIGN KEY (wenstid)
        REFERENCES list.entitystatus (enstid),
    CONSTRAINT worker_username_password_check CHECK (
        (username IS NULL AND password IS NULL) OR
        (username IS NOT NULL AND password IS NOT NULL)
    )
);

-- Referencias circulares person ↔ worker y worker ↔ worker
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'person_fkey_anaid') THEN
        ALTER TABLE public.person
            ADD CONSTRAINT person_fkey_anaid FOREIGN KEY (anaid)
                REFERENCES public.worker (workid);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'person_fkey_recorderid') THEN
        ALTER TABLE public.person
            ADD CONSTRAINT person_fkey_recorderid FOREIGN KEY (recorderid)
                REFERENCES public.worker (workid);
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'worker_fkey_wrecorderid') THEN
        ALTER TABLE public.worker
            ADD CONSTRAINT worker_fkey_wrecorderid FOREIGN KEY (wrecorderid)
                REFERENCES public.worker (workid);
    END IF;
END;
$$;

CREATE TABLE IF NOT EXISTS public.product
(
    prodid           integer NOT NULL DEFAULT nextval('public.product_prodid_seq'),
    name             character varying(255)  NOT NULL,
    prtyid           character(2)            NOT NULL,
    description      text,
    coinid           smallint                NOT NULL,
    workid           integer                 NOT NULL,
    cost             numeric(12,3),
    registrationdate timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status           boolean                 NOT NULL DEFAULT true,
    webvisible       boolean                 NOT NULL DEFAULT false,
    icon             text,
    image            text,
    smalldescription text,
    tax              numeric(4,4) DEFAULT 0.00,
    account          character varying(255),
    locked           boolean DEFAULT false,
    modifyprice      boolean DEFAULT false,
    CONSTRAINT product_pkey PRIMARY KEY (prodid),
    CONSTRAINT product_account_fkey FOREIGN KEY (account)
        REFERENCES public.accountingplan (account)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT product_fkey_coinid FOREIGN KEY (coinid)
        REFERENCES list.coin (coinid),
    CONSTRAINT product_fkey_prtyid FOREIGN KEY (prtyid)
        REFERENCES list.producttype (prtyid),
    CONSTRAINT product_fkey_workid FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT product_cost_check CHECK (cost >= 0),
    CONSTRAINT product_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.zone
(
    zoneid           smallint NOT NULL DEFAULT nextval('public.zone_zoneid_seq'),  -- [F10]
    name             character varying(255)  NOT NULL,
    color            character(9),
    enabled          boolean                 NOT NULL DEFAULT true,
    workid           integer,
    registrationdate date                    NOT NULL DEFAULT CURRENT_DATE,
    status           boolean                 NOT NULL DEFAULT true,
    CONSTRAINT zone_pkey PRIMARY KEY (zoneid),
    CONSTRAINT zone_name_key UNIQUE (name),
    CONSTRAINT zone_fkey_workid FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT zone_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.company
(
    compid           integer NOT NULL DEFAULT nextval('public.company_compid_seq'),
    enstid           character(2)            NOT NULL DEFAULT 'PA',
    name             character varying(200)  NOT NULL,
    ruc              character varying(11)   NOT NULL,
    tradename        character varying(200),
    tptlid           smallint                NOT NULL,
    coslid           smallint                NOT NULL,
    coclid           smallint                NOT NULL,
    operationstart   date,
    numberworkers    smallint,
    observation      text,
    anaid            integer,
    recorderid       integer,
    registrationdate timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    registrationtime time without time zone       NOT NULL DEFAULT CURRENT_TIME,
    status           boolean                 NOT NULL DEFAULT true,
    CONSTRAINT company_pkey PRIMARY KEY (compid),
    CONSTRAINT company_name_key UNIQUE (name),
    CONSTRAINT company_ruc_key UNIQUE (ruc),
    CONSTRAINT company_tradename_key UNIQUE (tradename),
    CONSTRAINT company_fkey_anaid FOREIGN KEY (anaid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_fkey_coclid FOREIGN KEY (coclid)
        REFERENCES list.companyconditionlist (coclid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_fkey_coslid FOREIGN KEY (coslid)
        REFERENCES list.companystatuslist (coslid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_fkey_enstid FOREIGN KEY (enstid)
        REFERENCES list.entitystatus (enstid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_fkey_recorderid FOREIGN KEY (recorderid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_fkey_tptlid FOREIGN KEY (tptlid)
        REFERENCES list.taxpayertypelist (tptlid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT company_name_check CHECK (name <> '')
);

-- [F9] public.address_addrid_seq con esquema explícito
CREATE TABLE IF NOT EXISTS public.residence
(
    resiid           integer NOT NULL DEFAULT nextval('public.address_addrid_seq'),
    persid           integer,
    compid           integer,
    partid           integer,
    distid           integer                 NOT NULL,
    zotyid           smallint,
    zonename         character varying(255),
    rotyid           smallint,
    roadname         character varying(255),
    nro              character varying(10),
    km               character varying(10),
    mz               character varying(10),
    inside           character varying(10),
    flat             character varying(10),
    lot              character varying(10),
    address          character varying(255),
    reference        character varying(255),
    waterbill        character varying(30),
    lightbill        character varying(30),
    latitude         numeric(15,12),
    longitude        numeric(15,12),
    main             boolean                 NOT NULL DEFAULT true,
    registrationdate date                    NOT NULL DEFAULT CURRENT_DATE,
    registrationtime time without time zone  NOT NULL DEFAULT CURRENT_TIME,
    status           boolean                 NOT NULL DEFAULT true,
    wamaid           smallint,
    flmaid           smallint,
    romaid           smallint,
    retyid           smallint,
    cobaid           smallint,
    owreid           character(2),
    adtyid           integer,
    CONSTRAINT address_pkey PRIMARY KEY (resiid),
    CONSTRAINT address_fkey_compid FOREIGN KEY (compid)
        REFERENCES public.company (compid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT address_fkey_distid FOREIGN KEY (distid)
        REFERENCES public.district (distid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT address_fkey_persid FOREIGN KEY (persid)
        REFERENCES public.person (persid)
        ON UPDATE NO ACTION ON DELETE NO ACTION
);

CREATE TABLE IF NOT EXISTS public.agency
(
    agenid           smallint NOT NULL DEFAULT nextval('public.agency_agenid_seq'),  -- [F10]
    zoneid           smallint                NOT NULL,
    code             character(3),
    name             character varying(255)  NOT NULL,
    startdate        date                    NOT NULL DEFAULT CURRENT_DATE,
    phonenumber      character(13),
    resiid           integer,
    color            character(9),
    enabled          boolean                 NOT NULL DEFAULT true,
    workid           integer,
    status           boolean                 NOT NULL DEFAULT true,
    haswhatsapp      boolean DEFAULT false,
    vaultlimit       numeric DEFAULT 0,
    vaultworkid      integer,
    vaultalertworkid integer,
    serialnumber     character varying(5),
    email            character varying(255),
    CONSTRAINT agency_pkey PRIMARY KEY (agenid),
    CONSTRAINT unique_name UNIQUE (name),
    CONSTRAINT agency_fkey_workid FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT agency_fkey_zoneid FOREIGN KEY (zoneid)
        REFERENCES public.zone (zoneid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT agency_resiid_fkey FOREIGN KEY (resiid)
        REFERENCES public.residence (resiid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT agency_vaultalertworkid_fkey FOREIGN KEY (vaultalertworkid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT agency_vaultworkid_fkey FOREIGN KEY (vaultworkid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT agency_name_check CHECK (name <> '')
);

-- [F6] public.windows — faltaba completamente en bd-modificar
CREATE TABLE IF NOT EXISTS public.windows
(
    windid       integer NOT NULL DEFAULT nextval('public.windows_windid_seq'),
    agenid       smallint                NOT NULL,
    name         character varying(255)  NOT NULL,
    workid       integer                 NOT NULL,
    main         boolean                 NOT NULL DEFAULT false,
    enabled      boolean                 NOT NULL DEFAULT true,
    status       boolean                 NOT NULL DEFAULT true,
    locked       boolean DEFAULT false,
    issmall      boolean DEFAULT false,
    active       boolean DEFAULT false,
    isworkerbox  boolean DEFAULT false,
    CONSTRAINT windows_pkey PRIMARY KEY (windid),
    CONSTRAINT windows_fkey_agenid FOREIGN KEY (agenid)
        REFERENCES public.agency (agenid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT windows_fkey_workid FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT windows_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS public.client
(
    clieid          integer NOT NULL DEFAULT nextval('public.client_clieid_seq'),
    enstid          character(2)            NOT NULL DEFAULT 'AC',
    persid          integer,
    compid          integer,
    partid          integer,
    code            character(15),
    availablecredit numeric(12,3)           NOT NULL DEFAULT 0,
    username        character varying(50),
    password        character varying(32),
    anaid           integer,
    agenid          smallint                NOT NULL,
    workid          integer                 NOT NULL,
    origin          character(1)            NOT NULL DEFAULT 'I',
    startdate       date                    NOT NULL DEFAULT CURRENT_DATE,
    status          boolean                 NOT NULL DEFAULT true,
    registernumber  integer,
    blacklist       boolean DEFAULT false,
    partner         boolean DEFAULT true,
    clientnumber    integer,
    risk            smallint DEFAULT 0,
    CONSTRAINT client_pkey PRIMARY KEY (clieid),
    CONSTRAINT client_compid_key UNIQUE (compid),
    CONSTRAINT client_partid_key UNIQUE (partid),
    CONSTRAINT client_persid_key UNIQUE (persid),
    CONSTRAINT client_username_key UNIQUE (username),
    CONSTRAINT client_enstid_fkey FOREIGN KEY (enstid)
        REFERENCES list.entitystatus (enstid),
    CONSTRAINT client_fkey_agenid FOREIGN KEY (agenid)
        REFERENCES public.agency (agenid),
    CONSTRAINT client_fkey_anaid FOREIGN KEY (anaid)
        REFERENCES public.worker (workid),
    CONSTRAINT client_fkey_compid FOREIGN KEY (compid)
        REFERENCES public.company (compid),
    CONSTRAINT client_fkey_workid FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT check_one_non_null CHECK (
        (persid IS NOT NULL AND compid IS NULL  AND partid IS NULL) OR
        (persid IS NULL  AND compid IS NOT NULL AND partid IS NULL) OR
        (persid IS NULL  AND compid IS NULL     AND partid IS NOT NULL)
    ),
    CONSTRAINT client_availablecredit_check CHECK (availablecredit >= 0),
    CONSTRAINT client_origin_check CHECK (origin = ANY (ARRAY['I','W']))
);

CREATE TABLE IF NOT EXISTS public.provider
(
    provid    integer NOT NULL DEFAULT nextval('public.provider_provid_seq'),
    persid    integer,
    compid    integer,
    code      character varying(15),
    username  character varying(25),
    password  character varying(32),
    workid    integer                 NOT NULL,
    startdate date DEFAULT CURRENT_DATE,
    status    boolean DEFAULT true,
    note      character varying,
    enstid    character(2) DEFAULT 'AC',
    CONSTRAINT provider_pkey PRIMARY KEY (provid),
    CONSTRAINT provider_compid_fkey FOREIGN KEY (compid)
        REFERENCES public.company (compid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT provider_enstid_fkey FOREIGN KEY (enstid)
        REFERENCES list.entitystatus (enstid),
    CONSTRAINT provider_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid),
    CONSTRAINT provider_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
);

CREATE TABLE IF NOT EXISTS public.costcenter
(
    coceid        integer NOT NULL DEFAULT nextval('public.costcenter_coceid_seq'),
    name          character varying(150)  NOT NULL,
    father_coceid integer,
    status        boolean                 NOT NULL DEFAULT true,
    isexpensive   boolean DEFAULT false,
    workid        integer                 NOT NULL,
    prcoid        integer,
    manager       integer,
    registerdate  timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    code          character varying(20),
    description   text,
    color         character(8),
    CONSTRAINT costcenter_pkey PRIMARY KEY (coceid),
    CONSTRAINT costcenter_fkey_father FOREIGN KEY (father_coceid)
        REFERENCES public.costcenter (coceid),
    CONSTRAINT costcenter_manager_fkey FOREIGN KEY (manager)
        REFERENCES public.worker (workid),
    CONSTRAINT costcenter_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
);

-- [F6] public.bankaccounts — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.bankaccounts
(
    baacid           smallint NOT NULL DEFAULT nextval('public.bankaccounts_baacid_seq'),
    bankid           smallint                NOT NULL,
    coinid           smallint                NOT NULL,
    account          character varying(30)   NOT NULL,
    interbankaccount character varying(30)   NOT NULL,
    registrationdate date                    NOT NULL DEFAULT CURRENT_DATE,
    status           boolean                 NOT NULL DEFAULT true,
    persid           integer,
    compid           integer,
    CONSTRAINT bankaccounts_pkey PRIMARY KEY (baacid),
    CONSTRAINT bankaccounts_bankid_account_key UNIQUE (bankid, account),
    CONSTRAINT bankaccounts_interbankaccount_key UNIQUE (interbankaccount),
    CONSTRAINT bankaccounts_compid_fkey FOREIGN KEY (compid)
        REFERENCES public.company (compid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT bankaccounts_fkey_bankid FOREIGN KEY (bankid)
        REFERENCES list.banks (bankid),
    CONSTRAINT bankaccounts_fkey_coinid FOREIGN KEY (coinid)
        REFERENCES list.coin (coinid),
    CONSTRAINT bankaccounts_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid)
);

-- [F6] public.sale — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.sale
(
    saleid            integer NOT NULL DEFAULT nextval('public.sale_saleid_seq'),
    clieid            integer                 NOT NULL,
    registerdate      timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    dotyid            smallint                NOT NULL,
    coinid            smallint                NOT NULL,
    serialnumber      character varying(5)    NOT NULL,
    correlativenumber integer                 NOT NULL,
    persid            integer,
    subtotal          numeric(12,3),
    igv               numeric(12,3),
    total             numeric(12,3),
    workid            integer                 NOT NULL,
    sellerid          integer                 NOT NULL,
    resiid            integer,
    agenid            integer                 NOT NULL,
    windid            integer                 NOT NULL,
    scpaid            integer,
    statid            character(2) DEFAULT 'AC',
    payed             boolean DEFAULT false,
    description       character varying(255),
    documentdate      timestamp without time zone,
    sunat             character(2) DEFAULT 'NS',
    sunatanswer       text DEFAULT 'NS',
    smallbox          boolean DEFAULT false,
    note              character varying,
    coceid            integer,
    cowoid            integer,
    chliid            integer,
    payeddate         date,
    CONSTRAINT sale_pkey PRIMARY KEY (saleid),
    CONSTRAINT sale_agenid_fkey FOREIGN KEY (agenid)
        REFERENCES public.agency (agenid),
    CONSTRAINT sale_clieid_fkey FOREIGN KEY (clieid)
        REFERENCES public.client (clieid),
    CONSTRAINT sale_coceid_fkey FOREIGN KEY (coceid)
        REFERENCES public.costcenter (coceid),
    CONSTRAINT sale_coinid_fkey FOREIGN KEY (coinid)
        REFERENCES list.coin (coinid),
    CONSTRAINT sale_dotyid_fkey FOREIGN KEY (dotyid)
        REFERENCES list.documenttype (dotyid),
    CONSTRAINT sale_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid),
    CONSTRAINT sale_resiid_fkey FOREIGN KEY (resiid)
        REFERENCES public.residence (resiid),
    CONSTRAINT sale_statid_fkey FOREIGN KEY (statid)
        REFERENCES list.status (statid),
    CONSTRAINT sale_sunat_fkey FOREIGN KEY (sunat)
        REFERENCES list.sunatelectronicanswer (suea),
    CONSTRAINT sale_windid_fkey FOREIGN KEY (windid)
        REFERENCES public.windows (windid),
    CONSTRAINT sale_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT sale_sellerid_fkey FOREIGN KEY (sellerid)
        REFERENCES public.worker (workid)
);

-- [F6] public.saleitems — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.saleitems
(
    saitid      integer NOT NULL DEFAULT nextval('public.saleitems_saitid_seq'),
    saleid      integer,
    prodid      integer                 NOT NULL,
    description character varying,
    qty         numeric(12,3) DEFAULT 1,
    unitprice   numeric(12,3),
    total       numeric(12,3),
    discount    numeric(12,3),
    "position"  smallint,
    tax         numeric(12,3) DEFAULT 0,
    CONSTRAINT saleitems_pkey PRIMARY KEY (saitid),
    CONSTRAINT saleitems_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT saleitems_saleid_fkey FOREIGN KEY (saleid)
        REFERENCES public.sale (saleid)
);

-- [F6] public.payments — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.payments
(
    paymid          integer NOT NULL DEFAULT nextval('public.payments_paymid_seq'),
    amount          numeric(11,3)           NOT NULL,
    coinid          smallint                NOT NULL,
    paydate         timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    checknumber     character varying(25),
    operationcode   character varying(25),
    targetbaacid    integer,
    chargebaacid    integer,
    workid          integer                 NOT NULL,
    windid          integer,
    agenid          integer                 NOT NULL,
    recaid          smallint,
    status          boolean                 NOT NULL DEFAULT true,
    validated       boolean                 NOT NULL DEFAULT true,
    validatedworkid integer,
    note            character varying,
    persid          integer,
    CONSTRAINT payments_pkey PRIMARY KEY (paymid),
    CONSTRAINT payments_agenid_fkey FOREIGN KEY (agenid)
        REFERENCES public.agency (agenid),
    CONSTRAINT payments_chargebaacid_fkey FOREIGN KEY (chargebaacid)
        REFERENCES public.bankaccounts (baacid),
    CONSTRAINT payments_coinid_fkey FOREIGN KEY (coinid)
        REFERENCES list.coin (coinid),
    CONSTRAINT payments_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid),
    CONSTRAINT payments_recaid_fkey FOREIGN KEY (recaid)
        REFERENCES list.reasonforcancellation (recaid),
    CONSTRAINT payments_targetbaacid_fkey FOREIGN KEY (targetbaacid)
        REFERENCES public.bankaccounts (baacid),
    CONSTRAINT payments_validatedworkid_fkey FOREIGN KEY (validatedworkid)
        REFERENCES public.worker (workid),
    CONSTRAINT payments_windid_fkey FOREIGN KEY (windid)
        REFERENCES public.windows (windid),
    CONSTRAINT payments_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT payments_amount_check CHECK (amount >= 0)
);

-- [F6] public.salepayments — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.salepayments
(
    sapaid  integer NOT NULL DEFAULT nextval('public.salepayments_sapaid_seq'),
    saleid  integer NOT NULL,
    paymid  integer NOT NULL,
    CONSTRAINT salepayments_pkey PRIMARY KEY (sapaid),
    CONSTRAINT salepayments_paymid_fkey FOREIGN KEY (paymid)
        REFERENCES public.payments (paymid),
    CONSTRAINT salepayments_saleid_fkey FOREIGN KEY (saleid)
        REFERENCES public.sale (saleid)
);

-- [F6] public.voucher — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.voucher
(
    voucid            integer NOT NULL DEFAULT nextval('public.baucher_baucid_seq'),
    clieid            integer,
    loscid            integer,
    satrid            integer,
    contid            integer,
    inscid            integer,
    coinid            integer                 NOT NULL,
    windid            integer                 NOT NULL,
    workid            integer                 NOT NULL,
    correlativenumber integer                 NOT NULL,
    paymentdate       timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    registerdate      timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    statid            character(2) DEFAULT 'AC',
    total             numeric                 NOT NULL,
    serialnumber      character(2),
    loanid            integer,
    operationnumber   smallint                NOT NULL DEFAULT 0,
    saleid            integer,
    purcid            integer,
    provid            integer,
    note              text,
    payment_type      character(2) DEFAULT 'EF',
    mmtyid            integer,
    updater_workid    integer,
    latitude          numeric(11,8),
    longitude         numeric(11,8),
    CONSTRAINT baucher_pkey PRIMARY KEY (voucid),
    CONSTRAINT baucher_clieid_fkey FOREIGN KEY (clieid)
        REFERENCES public.client (clieid),
    CONSTRAINT baucher_statid_fkey FOREIGN KEY (statid)
        REFERENCES list.status (statid),
    CONSTRAINT baucher_windid_fkey FOREIGN KEY (windid)
        REFERENCES public.windows (windid),
    CONSTRAINT baucher_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT voucher_mmtyid_fkey FOREIGN KEY (mmtyid)
        REFERENCES list.moneymovetype (mmtyid),
    CONSTRAINT voucher_provid_fkey FOREIGN KEY (provid)
        REFERENCES public.provider (provid),
    CONSTRAINT voucher_saleid_fkey FOREIGN KEY (saleid)
        REFERENCES public.sale (saleid),
    CONSTRAINT voucher_updater_workid_fkey FOREIGN KEY (updater_workid)
        REFERENCES public.worker (workid),
    CONSTRAINT baucher_total_check CHECK (total >= 0)
);

-- [F6] public.voucheritems — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.voucheritems
(
    voitid      integer NOT NULL DEFAULT nextval('public.baucheritems_baitid_seq'),
    voucid      integer NOT NULL,
    prodid      integer NOT NULL,
    description character varying,
    total       numeric NOT NULL,
    CONSTRAINT voucheritems_pkey PRIMARY KEY (voitid),
    CONSTRAINT voucheritems_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT voucheritems_voucid_fkey FOREIGN KEY (voucid)
        REFERENCES public.voucher (voucid)
        ON DELETE CASCADE,
    CONSTRAINT baucheritems_total_check CHECK (total >= 0)
);

-- [F6] public.voucherpayments — faltaba en bd-modificar
CREATE TABLE IF NOT EXISTS public.voucherpayments
(
    voucid  integer NOT NULL,
    paymid  integer NOT NULL,
    CONSTRAINT voucherpayments_pkey PRIMARY KEY (voucid, paymid),
    CONSTRAINT voucherpayments_voucid_fkey FOREIGN KEY (voucid)
        REFERENCES public.voucher (voucid),
    CONSTRAINT voucherpayments_paymid_fkey FOREIGN KEY (paymid)
        REFERENCES public.payments (paymid)
);


-- =====================================================================
-- PARTE 1: TABLAS — ESQUEMA PRODUCT
-- =====================================================================

CREATE TABLE IF NOT EXISTS product.company
(
    prcoid       integer NOT NULL DEFAULT nextval('product.company_prcoid_seq'),
    prodid       integer NOT NULL,
    description  text,
    serial_number character varying(150),
    qty          integer DEFAULT 1,
    status       boolean DEFAULT true,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    workid       integer NOT NULL,
    prstid       character(1) DEFAULT 'A',
    CONSTRAINT company_pkey PRIMARY KEY (prcoid),
    CONSTRAINT company_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT company_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid)
);

-- [F7] product.vehicle: INHERITS (product.company) re-agregado
--       Con herencia PostgreSQL, los CONSTRAINTs NO se heredan;
--       la PK y FKs se declaran explícitamente aquí.
CREATE TABLE IF NOT EXISTS product.vehicle
(
    license_plate_number character varying(50),
    vetyid               character(2),
    year_of_manufacture  smallint,
    engine_number        character varying(50),
    futyid               character(2),
    number_of_passengers smallint,
    row_account          smallint,
    gross_vehicle_weight numeric,
    net_vehicle_weight   numeric,
    number_of_axles      smallint,
    number_of_wheels     smallint,
    color                character varying(35),
    mileage              integer,
    power                character varying(20)   DEFAULT '',
    wheel_formula        character varying(10)   DEFAULT '',
    version              character varying(100)  DEFAULT '',
    cylinder_count       smallint DEFAULT 0,
    cylinder_capacity    numeric(12,4) DEFAULT 0.00,
    length               numeric(12,4) DEFAULT 0.00,
    height               numeric(12,4) DEFAULT 0.00,
    width                numeric(12,4) DEFAULT 0.00,
    payload              numeric(12,4) DEFAULT 0.00,
    verification_code    character varying(25)   DEFAULT '',
    publish_number       character varying(25)   DEFAULT '',
    document_date        timestamp without time zone,
    registry_entry       character varying(25)   DEFAULT '',
    dua_dam              character varying(30)   DEFAULT '',
    title                character varying(25)   DEFAULT '',
    title_date           timestamp without time zone,
    condition            character(2) DEFAULT 'NU',
    seat_count           smallint DEFAULT 0,
    vin_number           character varying(50),
    category             character varying(50),
    CONSTRAINT vehicle_pkey PRIMARY KEY (prcoid),
    CONSTRAINT vehicle_futyid_fkey FOREIGN KEY (futyid)
        REFERENCES list.fueltype (futyid),
    CONSTRAINT vehicle_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT vehicle_vetyid_fkey FOREIGN KEY (vetyid)
        REFERENCES list.vehicletype (vetyid),
    CONSTRAINT vehicle_mileage_check CHECK (mileage >= 0)
)
INHERITS (product.company);   -- [F7]

CREATE TABLE IF NOT EXISTS product.productbrand
(
    prbrid  smallint NOT NULL DEFAULT nextval('product.productbrand_prbrid_seq'),
    name    character varying(50)   NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT productbrand_pkey PRIMARY KEY (prbrid),
    CONSTRAINT productbrand_name_key UNIQUE (name),
    CONSTRAINT productbrand_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS product.productcategory
(
    prcaid  smallint NOT NULL DEFAULT nextval('product.productcategory_prcaid_seq'),
    name    character varying(255),
    state   boolean,
    status  boolean DEFAULT true,
    CONSTRAINT productcategory_pkey PRIMARY KEY (prcaid)
);

CREATE TABLE IF NOT EXISTS product.productmodel
(
    prmoid  integer NOT NULL DEFAULT nextval('product.productmodel_prmoid_seq'),
    prbrid  smallint NOT NULL,
    name    character varying(150)  NOT NULL,
    status  boolean                 NOT NULL DEFAULT true,
    CONSTRAINT productmodel_pkey PRIMARY KEY (prmoid),
    CONSTRAINT productmodel_name_key UNIQUE (name),
    CONSTRAINT productmodel_prbrid_fkey FOREIGN KEY (prbrid)
        REFERENCES product.productbrand (prbrid),
    CONSTRAINT productmodel_name_check CHECK (name <> '')
);

CREATE TABLE IF NOT EXISTS product.productmerchandise
(
    prmeid           integer NOT NULL DEFAULT nextval('product.productmerchandise_prmeid_seq'),
    prodid           integer                 NOT NULL,
    prcaid           smallint                NOT NULL,
    prmoid           integer,
    kardex           boolean                 NOT NULL DEFAULT true,
    utility          numeric(12,4),
    utilitybypercent boolean DEFAULT false,
    comercial        boolean                 NOT NULL DEFAULT false,
    CONSTRAINT productmerchandise_pkey PRIMARY KEY (prmeid),
    CONSTRAINT productmerchandise_prodid_unique UNIQUE (prodid),
    CONSTRAINT productmerchandise_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT productmerchandise_prcaid_fkey FOREIGN KEY (prcaid)
        REFERENCES product.productcategory (prcaid),
    CONSTRAINT productmerchandise_prmoid_fkey FOREIGN KEY (prmoid)
        REFERENCES product.productmodel (prmoid)
);


-- =====================================================================
-- PARTE 1: TABLAS — ESQUEMA COMPANY
-- =====================================================================

-- [F8] FK a public.company re-incluida
CREATE TABLE IF NOT EXISTS company.worker
(
    cowoid       integer NOT NULL DEFAULT nextval('company.worker_cowoid_seq'),
    compid       integer NOT NULL,
    persid       integer NOT NULL,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    note         text,
    status       boolean DEFAULT true,
    CONSTRAINT worker_pkey PRIMARY KEY (cowoid),
    CONSTRAINT worker_compid_fkey FOREIGN KEY (compid)
        REFERENCES public.company (compid)
        ON UPDATE NO ACTION ON DELETE NO ACTION,
    CONSTRAINT worker_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid)
        ON UPDATE NO ACTION ON DELETE NO ACTION
);


-- =====================================================================
-- PARTE 1: TABLAS — ESQUEMA SERVICE
-- =====================================================================

-- ENUM de nivel de combustible en el tanque
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_type t
        JOIN pg_namespace n ON n.oid = t.typnamespace
        WHERE t.typname = 'tank_level'
    ) THEN
        CREATE TYPE tank_level AS ENUM (
            '1/8','1/4','3/8','1/2','5/8','3/4','7/8','8/8'
        );
    END IF;
END
$$;

CREATE TABLE IF NOT EXISTS service.rentrequest
(
    sereid        integer NOT NULL DEFAULT nextval('service.rentrequest_sereid_seq'),
    coceid        integer                 NOT NULL,
    persid        integer                 NOT NULL,
    driver        integer                 NOT NULL,
    prodid        integer                 NOT NULL,
    price         numeric(7,2) DEFAULT 0,
    pricecoin     integer                 NOT NULL,
    guarantee     numeric(7,2) DEFAULT 0,
    frecuency     character varying(1)    NOT NULL,
    guaranteecoin integer                 NOT NULL,
    deliverydate  timestamp without time zone NOT NULL,
    returndate    timestamp without time zone NOT NULL,
    exactreturn   boolean                 NOT NULL,
    registerdate  timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    compid        integer,
    statid        character(2) DEFAULT 'AC',
    CONSTRAINT rentrequest_pkey PRIMARY KEY (sereid),
    CONSTRAINT rentrequest_coceid_fkey FOREIGN KEY (coceid)
        REFERENCES public.costcenter (coceid),
    CONSTRAINT rentrequest_compid_fkey FOREIGN KEY (compid)
        REFERENCES public.company (compid),
    CONSTRAINT rentrequest_driver_fkey FOREIGN KEY (driver)
        REFERENCES public.person (persid),
    CONSTRAINT rentrequest_persid_fkey FOREIGN KEY (persid)
        REFERENCES public.person (persid),
    CONSTRAINT rentrequest_prodid_fkey FOREIGN KEY (prodid)
        REFERENCES public.product (prodid),
    CONSTRAINT rentrequest_statid_fkey FOREIGN KEY (statid)
        REFERENCES list.status (statid),
    CONSTRAINT rentrequest_guarantee_check CHECK (guarantee >= 0),
    CONSTRAINT rentrequest_price_check CHECK (price >= 0)
);

CREATE TABLE IF NOT EXISTS service.rentexecute
(
    seexid           integer NOT NULL DEFAULT nextval('service.rentexecute_seexid_seq'),
    coceid           integer                 NOT NULL,
    workid           integer                 NOT NULL,
    clieid           integer                 NOT NULL,
    delivered_date   timestamp without time zone NOT NULL,
    delivered_workid integer                 NOT NULL,
    received_cowoid  integer                 NOT NULL,
    note_start       text,
    kilometer_start  integer,
    kilometer_end    integer,
    delivery_cowoid  integer,
    received_workid  integer,
    return_date      timestamp without time zone,
    note_end         text,
    made_sell_document boolean DEFAULT false,
    checklist        integer                 NOT NULL,
    sereid           integer,
    statid           character(2) DEFAULT 'AC',
    tank_start       tank_level,
    tank_end         tank_level,
    CONSTRAINT rentexecute_pkey PRIMARY KEY (seexid),
    CONSTRAINT rentexecute_clieid_fkey FOREIGN KEY (clieid)
        REFERENCES public.client (clieid),
    CONSTRAINT rentexecute_coceid_fkey FOREIGN KEY (coceid)
        REFERENCES public.costcenter (coceid),
    CONSTRAINT rentexecute_delivered_workid_fkey FOREIGN KEY (delivered_workid)
        REFERENCES public.worker (workid),
    CONSTRAINT rentexecute_delivery_cowoid_fkey FOREIGN KEY (delivery_cowoid)
        REFERENCES company.worker (cowoid),
    CONSTRAINT rentexecute_received_cowoid_fkey FOREIGN KEY (received_cowoid)
        REFERENCES company.worker (cowoid),
    CONSTRAINT rentexecute_received_workid_fkey FOREIGN KEY (received_workid)
        REFERENCES public.worker (workid),
    CONSTRAINT rentexecute_sereid_fkey FOREIGN KEY (sereid)
        REFERENCES service.rentrequest (sereid),
    CONSTRAINT rentexecute_statid_fkey FOREIGN KEY (statid)
        REFERENCES list.status (statid),
    CONSTRAINT rentexecute_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT rentexecute_kilometer_start_check CHECK (kilometer_start >= 0)
);


-- =====================================================================
-- PARTE 2: ESQUEMA MAINTENANCE — TABLAS NUEVAS
-- =====================================================================

-- 1. CONFIGURACIÓN DEL SISTEMA
CREATE TABLE IF NOT EXISTS maintenance.config_system
(
    cosyid      integer NOT NULL DEFAULT nextval('maintenance.config_system_cosyid_seq'),
    key         character varying(100)  NOT NULL,
    value       character varying(255)  NOT NULL,
    description text,
    data_type   character varying(20)   NOT NULL DEFAULT 'string',
    updated_at  timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_by  integer,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT config_system_pkey PRIMARY KEY (cosyid),
    CONSTRAINT config_system_key_unique UNIQUE (key),
    CONSTRAINT config_system_updated_by_fkey FOREIGN KEY (updated_by)
        REFERENCES public.worker (workid),
    CONSTRAINT config_system_key_check CHECK (key <> ''),
    CONSTRAINT config_system_value_check CHECK (value <> '')
);

COMMENT ON TABLE maintenance.config_system IS
    'Parámetros configurables: alerta_km_umbral, intervalo_km, '
    'alerta_vencimiento_componente_dias, alerta_vencimiento_lote_dias, alerta_stock_minimo.';

INSERT INTO maintenance.config_system (key, value, description, data_type) VALUES
('alerta_km_umbral',                   '800',  'Km antes del próximo servicio para alertar',     'integer'),
('intervalo_km',                       '5000', 'Intervalo km entre mantenimientos calendarizados','integer'),
('alerta_vencimiento_componente_dias', '30',   'Días antes de caducidad de componente instalado','integer'),
('alerta_vencimiento_lote_dias',       '30',   'Días antes de caducidad de lote en inventario',  'integer'),
('alerta_stock_minimo',                'true', 'Activar alertas de stock bajo mínimo',            'boolean');


-- 2. TIPO DE MANTENIMIENTO
CREATE TABLE IF NOT EXISTS maintenance.maintenance_type
(
    matyid      smallint NOT NULL DEFAULT nextval('maintenance.maintenance_type_matyid_seq'),
    name        character varying(50)   NOT NULL,
    description text,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT maintenance_type_pkey PRIMARY KEY (matyid),
    CONSTRAINT maintenance_type_name_unique UNIQUE (name),
    CONSTRAINT maintenance_type_name_check CHECK (name <> '')
);

COMMENT ON TABLE maintenance.maintenance_type IS
    'Solo 2 tipos: Calendarizado (~5000 km) y Emergencia (falla inesperada).';

INSERT INTO maintenance.maintenance_type (name, description) VALUES
('Calendarizado', 'Mantenimiento programado cada ~5000 km.'),
('Emergencia',    'Mantenimiento no programado ante falla inesperada, accidente o rotura súbita.');


-- 3. TIPO DE SERVICIO (A / B)
CREATE TABLE IF NOT EXISTS maintenance.service_type
(
    setyid      smallint NOT NULL DEFAULT nextval('maintenance.service_type_setyid_seq'),
    code        character(1)            NOT NULL,
    name        character varying(50)   NOT NULL,
    description text,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT service_type_pkey PRIMARY KEY (setyid),
    CONSTRAINT service_type_code_unique UNIQUE (code),
    CONSTRAINT service_type_name_unique UNIQUE (name),
    CONSTRAINT service_type_code_check CHECK (code = ANY (ARRAY['A','B']))
);

COMMENT ON TABLE maintenance.service_type IS
    'A = Servicio Básico (aceite + revisión). B = Servicio Completo (filtros, fluidos, extensivo).';

INSERT INTO maintenance.service_type (code, name, description) VALUES
('A', 'Servicio Básico',   'Cambio de aceite + revisión general básica'),
('B', 'Servicio Completo', 'Cambio de filtros, fluidos, revisión extensiva + todo del tipo A');


-- 4. TIPO DE LISTA DE ACCIONES
CREATE TABLE IF NOT EXISTS maintenance.action_list_type
(
    altoid      smallint NOT NULL DEFAULT nextval('maintenance.action_list_type_altoid_seq'),
    name        character varying(80)   NOT NULL,
    description text,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT action_list_type_pkey PRIMARY KEY (altoid),
    CONSTRAINT action_list_type_name_unique UNIQUE (name),
    CONSTRAINT action_list_type_name_check CHECK (name <> '')
);

INSERT INTO maintenance.action_list_type (name, description) VALUES
('Elementos de Reemplazo y Aplicación', 'Repuestos, Filtros y Fluidos. A=Aplicar, C=Cambiar, I=Inspeccionar/Calibrar'),
('Operaciones',                         'Mano de obra, revisión, limpieza, lubricación. I=Inspeccionar, R=Realizar');


-- 5. CATÁLOGO DE ACCIONES
CREATE TABLE IF NOT EXISTS maintenance.action_catalog
(
    acatid               integer NOT NULL DEFAULT nextval('maintenance.action_catalog_acatid_seq'),
    altoid               smallint                NOT NULL,
    name                 character varying(200)  NOT NULL,
    category             character varying(80),
    recommended_product  character varying(200),
    recommended_quantity character varying(50),
    unit_of_measure      character varying(30),
    useful_life_km       integer,
    expires_by_time      boolean                 NOT NULL DEFAULT false,
    useful_life_days     integer,
    description          text,
    status               boolean                 NOT NULL DEFAULT true,
    CONSTRAINT action_catalog_pkey PRIMARY KEY (acatid),
    CONSTRAINT action_catalog_altoid_fkey FOREIGN KEY (altoid)
        REFERENCES maintenance.action_list_type (altoid),
    CONSTRAINT action_catalog_name_check CHECK (name <> ''),
    CONSTRAINT action_catalog_life_check CHECK (
        (expires_by_time = false AND useful_life_days IS NULL) OR
        (expires_by_time = true  AND useful_life_days IS NOT NULL AND useful_life_days > 0)
    )
);

COMMENT ON COLUMN maintenance.action_catalog.expires_by_time IS
    'true = caduca por tiempo (ej: aceite motor 12 meses). false = solo por km.';


-- 6. CRONOGRAMA POR VEHÍCULO
CREATE TABLE IF NOT EXISTS maintenance.vehicle_schedule
(
    veshid             integer NOT NULL DEFAULT nextval('maintenance.vehicle_schedule_veshid_seq'),
    prcoid             integer NOT NULL,
    interval_km        integer NOT NULL DEFAULT 5000,
    next_km            integer NOT NULL,
    alert_km_threshold integer          DEFAULT 800,
    created_at         timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by         integer NOT NULL,
    updated_at         timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    status             boolean NOT NULL DEFAULT true,
    CONSTRAINT vehicle_schedule_pkey PRIMARY KEY (veshid),
    CONSTRAINT vehicle_schedule_prcoid_unique UNIQUE (prcoid),
    CONSTRAINT vehicle_schedule_prcoid_fkey FOREIGN KEY (prcoid)
        REFERENCES product.company (prcoid),
    CONSTRAINT vehicle_schedule_created_by_fkey FOREIGN KEY (created_by)
        REFERENCES public.worker (workid),
    CONSTRAINT vehicle_schedule_interval_check CHECK (interval_km > 0),
    CONSTRAINT vehicle_schedule_next_km_check CHECK (next_km >= 0),
    CONSTRAINT vehicle_schedule_alert_check CHECK (alert_km_threshold >= 0)
);

COMMENT ON TABLE maintenance.vehicle_schedule IS
    'Cronograma por vehículo. next_km se recalcula al completar mantenimiento '
    '(km_actual + interval_km).';


-- 7. ACCIONES PROGRAMADAS POR KM
CREATE TABLE IF NOT EXISTS maintenance.schedule_action
(
    shacid       integer NOT NULL DEFAULT nextval('maintenance.schedule_action_shacid_seq'),
    veshid       integer NOT NULL,
    acatid       integer NOT NULL,
    scheduled_km integer NOT NULL,
    action_code  character(1)    NOT NULL,
    status       boolean         NOT NULL DEFAULT true,
    CONSTRAINT schedule_action_pkey PRIMARY KEY (shacid),
    CONSTRAINT schedule_action_veshid_fkey FOREIGN KEY (veshid)
        REFERENCES maintenance.vehicle_schedule (veshid),
    CONSTRAINT schedule_action_acatid_fkey FOREIGN KEY (acatid)
        REFERENCES maintenance.action_catalog (acatid),
    CONSTRAINT schedule_action_code_check CHECK (action_code = ANY (ARRAY['A','C','I','R'])),
    CONSTRAINT schedule_action_km_check CHECK (scheduled_km > 0)
);


-- 11. CATEGORÍA DE MATERIAL  (antes de material y material_lot)
CREATE TABLE IF NOT EXISTS maintenance.material_category
(
    macaid      smallint NOT NULL DEFAULT nextval('maintenance.material_category_macaid_seq'),
    name        character varying(100)  NOT NULL,
    description text,
    status      boolean                 NOT NULL DEFAULT true,
    CONSTRAINT material_category_pkey PRIMARY KEY (macaid),
    CONSTRAINT material_category_name_unique UNIQUE (name),
    CONSTRAINT material_category_name_check CHECK (name <> '')
);

INSERT INTO maintenance.material_category (name, description) VALUES
('Lubricantes', 'Aceites de motor, transmisión, diferencial'),
('Filtros',     'Filtros de aire, aceite, combustible, cabina'),
('Fluidos',     'Líquido de frenos, refrigerante, dirección'),
('Repuestos',   'Bujías, correas, pastillas, componentes mecánicos'),
('Otros',       'Materiales no clasificados');


-- 12. MATERIAL
CREATE TABLE IF NOT EXISTS maintenance.material
(
    mateid          integer NOT NULL DEFAULT nextval('maintenance.material_mateid_seq'),
    macaid          smallint                NOT NULL,
    name            character varying(200)  NOT NULL,
    unit_of_measure character varying(30)   NOT NULL,
    stock_total     numeric(12,3)           NOT NULL DEFAULT 0,
    stock_minimum   numeric(12,3)           NOT NULL DEFAULT 0,
    description     text,
    created_at      timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by      integer                 NOT NULL,
    status          boolean                 NOT NULL DEFAULT true,
    CONSTRAINT material_pkey PRIMARY KEY (mateid),
    CONSTRAINT material_macaid_fkey FOREIGN KEY (macaid)
        REFERENCES maintenance.material_category (macaid),
    CONSTRAINT material_created_by_fkey FOREIGN KEY (created_by)
        REFERENCES public.worker (workid),
    CONSTRAINT material_name_check CHECK (name <> ''),
    CONSTRAINT material_unit_check CHECK (unit_of_measure <> ''),
    CONSTRAINT material_stock_check CHECK (stock_total >= 0),
    CONSTRAINT material_minimum_check CHECK (stock_minimum >= 0)
);

CREATE INDEX IF NOT EXISTS idx_material_macaid ON maintenance.material (macaid);


-- 13. LOTE DE MATERIAL  (con unit_cost para cálculo real de costo)
CREATE TABLE IF NOT EXISTS maintenance.material_lot
(
    maloid              integer NOT NULL DEFAULT nextval('maintenance.material_lot_maloid_seq'),
    mateid              integer NOT NULL,
    initial_quantity    numeric(12,3)           NOT NULL,
    current_quantity    numeric(12,3)           NOT NULL,
    unit_cost           numeric(12,4)           NOT NULL DEFAULT 0,
    entry_date          timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    expiration_date     date,
    provid              integer,
    supplier_lot_number character varying(100),
    note                text,
    lot_status          character varying(20)   NOT NULL DEFAULT 'activo',
    created_by          integer                 NOT NULL,
    CONSTRAINT material_lot_pkey PRIMARY KEY (maloid),
    CONSTRAINT material_lot_mateid_fkey FOREIGN KEY (mateid)
        REFERENCES maintenance.material (mateid),
    CONSTRAINT material_lot_provid_fkey FOREIGN KEY (provid)
        REFERENCES public.provider (provid),
    CONSTRAINT material_lot_created_by_fkey FOREIGN KEY (created_by)
        REFERENCES public.worker (workid),
    CONSTRAINT material_lot_qty_check CHECK (initial_quantity > 0),
    CONSTRAINT material_lot_current_check CHECK (current_quantity >= 0),
    CONSTRAINT material_lot_unit_cost_check CHECK (unit_cost >= 0),
    CONSTRAINT material_lot_status_check CHECK (
        lot_status = ANY (ARRAY['activo','agotado','vencido','descartado'])
    )
);

COMMENT ON COLUMN maintenance.material_lot.unit_cost IS
    'Precio de compra por unidad. Usado en vistas BI para calcular costo real.';
COMMENT ON COLUMN maintenance.material_lot.expiration_date IS
    'NULL si el material no caduca. Con fecha si caduca por tiempo (aceites, fluidos).';

CREATE INDEX IF NOT EXISTS idx_lot_mateid         ON maintenance.material_lot (mateid);
CREATE INDEX IF NOT EXISTS idx_lot_expiration     ON maintenance.material_lot (expiration_date)
    WHERE lot_status = 'activo';
CREATE INDEX IF NOT EXISTS idx_lot_status_mateid  ON maintenance.material_lot (mateid, lot_status);


-- 8. MANTENIMIENTO (tabla principal)
CREATE TABLE IF NOT EXISTS maintenance.maintenance
(
    mainid                       integer NOT NULL DEFAULT nextval('maintenance.maintenance_mainid_seq'),
    prcoid                       integer         NOT NULL,
    matyid                       smallint        NOT NULL,
    setyid                       smallint,
    order_number                 character varying(30),
    maintenance_date             timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    mileage                      integer         NOT NULL,
    km_since_last                integer,
    additional_work              text,
    oil_brand                    character varying(100),
    oil_viscosity_sae            character varying(20),
    climate_season               character varying(50),
    show_oil_in_next_maintenance boolean         NOT NULL DEFAULT false,
    origin_service               character varying(50) NOT NULL DEFAULT 'Taller propio',
    signature_seal               text,
    is_emergency_complete        boolean,
    workid                       integer         NOT NULL,
    note                         text,
    created_at                   timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at                   timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    statid                       character(2)    NOT NULL DEFAULT 'AC',
    CONSTRAINT maintenance_pkey PRIMARY KEY (mainid),
    CONSTRAINT maintenance_prcoid_fkey FOREIGN KEY (prcoid)
        REFERENCES product.company (prcoid),
    CONSTRAINT maintenance_matyid_fkey FOREIGN KEY (matyid)
        REFERENCES maintenance.maintenance_type (matyid),
    CONSTRAINT maintenance_setyid_fkey FOREIGN KEY (setyid)
        REFERENCES maintenance.service_type (setyid),
    CONSTRAINT maintenance_workid_fkey FOREIGN KEY (workid)
        REFERENCES public.worker (workid),
    CONSTRAINT maintenance_statid_fkey FOREIGN KEY (statid)
        REFERENCES list.status (statid),
    CONSTRAINT maintenance_mileage_check CHECK (mileage >= 0),
    CONSTRAINT maintenance_km_since_check CHECK (km_since_last IS NULL OR km_since_last >= 0),
    CONSTRAINT maintenance_origin_check CHECK (
        origin_service = ANY (ARRAY['Taller propio','Taller externo'])
    )
);

COMMENT ON COLUMN maintenance.maintenance.show_oil_in_next_maintenance IS
    'Si true, oil_brand/viscosity/season de ESTE mantenimiento se muestran en el SIGUIENTE.';
COMMENT ON COLUMN maintenance.maintenance.is_emergency_complete IS
    'Solo emergencias: true=completado (recalendariza), false=solo urgente (no recalendariza).';

CREATE INDEX IF NOT EXISTS idx_maintenance_prcoid      ON maintenance.maintenance (prcoid);
CREATE INDEX IF NOT EXISTS idx_maintenance_date        ON maintenance.maintenance (maintenance_date);
CREATE INDEX IF NOT EXISTS idx_maintenance_matyid      ON maintenance.maintenance (matyid);
CREATE INDEX IF NOT EXISTS idx_maintenance_prcoid_date ON maintenance.maintenance (prcoid, maintenance_date);


-- 9. DETALLE DE ACCIONES REALIZADAS
CREATE TABLE IF NOT EXISTS maintenance.maintenance_action_detail
(
    madeid           integer NOT NULL DEFAULT nextval('maintenance.maintenance_action_detail_madeid_seq'),
    mainid           integer         NOT NULL,
    acatid           integer         NOT NULL,
    completed        boolean         NOT NULL DEFAULT false,
    action_performed character(1),
    product_used     character varying(200),
    quantity_used    character varying(50),
    origin_product   character varying(50),
    observation      text,
    maloid           integer,
    CONSTRAINT maintenance_action_detail_pkey PRIMARY KEY (madeid),
    CONSTRAINT mad_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT mad_acatid_fkey FOREIGN KEY (acatid)
        REFERENCES maintenance.action_catalog (acatid),
    CONSTRAINT mad_maloid_fkey FOREIGN KEY (maloid)
        REFERENCES maintenance.material_lot (maloid),
    CONSTRAINT mad_action_check CHECK (
        action_performed IS NULL OR action_performed = ANY (ARRAY['A','C','I','R'])
    ),
    CONSTRAINT mad_origin_check CHECK (
        origin_product IS NULL OR origin_product = ANY (ARRAY['Stock propio','Externo'])
    )
);

COMMENT ON TABLE maintenance.maintenance_action_detail IS
    'Checklist de acciones por mantenimiento.';


-- 10. DIAGNÓSTICO FINAL
CREATE TABLE IF NOT EXISTS maintenance.diagnosis
(
    diagid                 integer NOT NULL DEFAULT nextval('maintenance.diagnosis_diagid_seq'),
    mainid                 integer NOT NULL,
    general_status         character varying(100)  NOT NULL,
    observations           text,
    vehicle_operative      boolean                 NOT NULL DEFAULT true,
    future_recommendations text,
    created_at             timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT diagnosis_pkey PRIMARY KEY (diagid),
    CONSTRAINT diagnosis_mainid_unique UNIQUE (mainid),
    CONSTRAINT diagnosis_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT diagnosis_status_check CHECK (general_status <> '')
);


-- 14. CONSUMO DE MATERIAL
CREATE TABLE IF NOT EXISTS maintenance.material_consumption
(
    macoid      integer NOT NULL DEFAULT nextval('maintenance.material_consumption_macoid_seq'),
    mainid      integer NOT NULL,
    mateid      integer NOT NULL,
    maloid      integer,
    quantity    numeric(12,3)           NOT NULL,
    origin      character varying(50)   NOT NULL DEFAULT 'Stock propio',
    consumed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT material_consumption_pkey PRIMARY KEY (macoid),
    CONSTRAINT mc_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT mc_mateid_fkey FOREIGN KEY (mateid)
        REFERENCES maintenance.material (mateid),
    CONSTRAINT mc_maloid_fkey FOREIGN KEY (maloid)
        REFERENCES maintenance.material_lot (maloid),
    CONSTRAINT mc_quantity_check CHECK (quantity > 0),
    CONSTRAINT mc_origin_check CHECK (origin = ANY (ARRAY['Stock propio','Externo']))
);

CREATE INDEX IF NOT EXISTS idx_consumption_mainid ON maintenance.material_consumption (mainid);
CREATE INDEX IF NOT EXISTS idx_consumption_mateid ON maintenance.material_consumption (mateid);
CREATE INDEX IF NOT EXISTS idx_consumption_maloid ON maintenance.material_consumption (maloid);


-- 15. DESCARTE DE MATERIAL
CREATE TABLE IF NOT EXISTS maintenance.material_discard
(
    madiid             integer NOT NULL DEFAULT nextval('maintenance.material_discard_madiid_seq'),
    maloid             integer NOT NULL,
    discarded_quantity numeric(12,3)           NOT NULL,
    discard_date       timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reason             character varying(50)   NOT NULL,
    note               text,
    discarded_by       integer                 NOT NULL,
    CONSTRAINT material_discard_pkey PRIMARY KEY (madiid),
    CONSTRAINT md_maloid_fkey FOREIGN KEY (maloid)
        REFERENCES maintenance.material_lot (maloid),
    CONSTRAINT md_discarded_by_fkey FOREIGN KEY (discarded_by)
        REFERENCES public.worker (workid),
    CONSTRAINT md_quantity_check CHECK (discarded_quantity > 0),
    CONSTRAINT md_reason_check CHECK (reason = ANY (ARRAY['Vencimiento','Daño','Otro']))
);


-- 16. COMPONENTE INSTALADO EN VEHÍCULO
CREATE TABLE IF NOT EXISTS maintenance.installed_component
(
    incoid             integer NOT NULL DEFAULT nextval('maintenance.installed_component_incoid_seq'),
    prcoid             integer NOT NULL,
    acatid             integer NOT NULL,
    mainid             integer NOT NULL,
    maloid             integer,
    installation_date  timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    installation_km    integer                 NOT NULL,
    expiration_date    date,
    active             boolean                 NOT NULL DEFAULT true,
    replaced_by_incoid integer,
    CONSTRAINT installed_component_pkey PRIMARY KEY (incoid),
    CONSTRAINT ic_prcoid_fkey FOREIGN KEY (prcoid)
        REFERENCES product.company (prcoid),
    CONSTRAINT ic_acatid_fkey FOREIGN KEY (acatid)
        REFERENCES maintenance.action_catalog (acatid),
    CONSTRAINT ic_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT ic_maloid_fkey FOREIGN KEY (maloid)
        REFERENCES maintenance.material_lot (maloid),
    CONSTRAINT ic_replaced_by_fkey FOREIGN KEY (replaced_by_incoid)
        REFERENCES maintenance.installed_component (incoid),
    CONSTRAINT ic_km_check CHECK (installation_km >= 0)
);

CREATE INDEX IF NOT EXISTS idx_ic_prcoid_active ON maintenance.installed_component (prcoid)
    WHERE active = true;
CREATE INDEX IF NOT EXISTS idx_ic_expiration    ON maintenance.installed_component (expiration_date)
    WHERE active = true AND expiration_date IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_ic_acatid        ON maintenance.installed_component (acatid);


-- 17. CALIFICACIÓN DE MATERIALES (estrellas 1-5)
CREATE TABLE IF NOT EXISTS maintenance.material_rating
(
    matraid     integer NOT NULL DEFAULT nextval('maintenance.material_rating_matraid_seq'),
    mateid      integer NOT NULL,
    mainid      integer NOT NULL,
    rating      smallint        NOT NULL,
    observation text,
    rated_by    integer         NOT NULL,
    rated_at    timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT material_rating_pkey PRIMARY KEY (matraid),
    CONSTRAINT mr_mateid_fkey FOREIGN KEY (mateid)
        REFERENCES maintenance.material (mateid),
    CONSTRAINT mr_mainid_fkey FOREIGN KEY (mainid)
        REFERENCES maintenance.maintenance (mainid) ON DELETE CASCADE,
    CONSTRAINT mr_rated_by_fkey FOREIGN KEY (rated_by)
        REFERENCES public.worker (workid),
    CONSTRAINT mr_rating_range CHECK (rating BETWEEN 1 AND 5),
    CONSTRAINT mr_observation_required CHECK (
        (rating > 3) OR
        (rating <= 3 AND observation IS NOT NULL AND observation <> '')
    )
);

COMMENT ON TABLE maintenance.material_rating IS
    'Calificación 1-5 estrellas de materiales. rating ≤ 3 requiere observación obligatoria.';

CREATE INDEX IF NOT EXISTS idx_rating_mateid ON maintenance.material_rating (mateid);
CREATE INDEX IF NOT EXISTS idx_rating_mainid ON maintenance.material_rating (mainid);


-- 18. CONFIGURACIÓN DE ALERTAS
CREATE TABLE IF NOT EXISTS maintenance.alert_config
(
    alcoid          integer NOT NULL DEFAULT nextval('maintenance.alert_config_alcoid_seq'),
    alert_type      character varying(50)   NOT NULL,
    description     text,
    enabled         boolean                 NOT NULL DEFAULT true,
    threshold_value character varying(50),
    threshold_unit  character varying(30),
    CONSTRAINT alert_config_pkey PRIMARY KEY (alcoid),
    CONSTRAINT alert_config_type_unique UNIQUE (alert_type),
    CONSTRAINT alert_config_type_check CHECK (alert_type <> '')
);

INSERT INTO maintenance.alert_config (alert_type, description, threshold_value, threshold_unit) VALUES
('MANTENIMIENTO_PROXIMO_KM', 'Vehículo próximo al siguiente mantenimiento calendarizado', '800',  'km'),
('COMPONENTE_POR_CADUCAR',   'Componente instalado próximo a fecha de caducidad',         '30',   'días'),
('LOTE_POR_VENCER',          'Lote de material en inventario próximo a vencer',           '30',   'días'),
('STOCK_BAJO',               'Material con stock por debajo del mínimo definido',         '0',    'unidades');


-- 19. LOG DE ALERTAS
CREATE TABLE IF NOT EXISTS maintenance.alert_log
(
    alloid      integer NOT NULL DEFAULT nextval('maintenance.alert_log_alloid_seq'),
    alcoid      integer NOT NULL,
    prcoid      integer,
    mateid      integer,
    maloid      integer,
    incoid      integer,
    message     text    NOT NULL,
    alert_date  timestamp without time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    read        boolean NOT NULL DEFAULT false,
    read_at     timestamp without time zone,
    read_by     integer,
    resolved    boolean NOT NULL DEFAULT false,
    resolved_at timestamp without time zone,
    resolved_by integer,
    CONSTRAINT alert_log_pkey PRIMARY KEY (alloid),
    CONSTRAINT al_alcoid_fkey FOREIGN KEY (alcoid)
        REFERENCES maintenance.alert_config (alcoid),
    CONSTRAINT al_prcoid_fkey FOREIGN KEY (prcoid)
        REFERENCES product.company (prcoid),
    CONSTRAINT al_mateid_fkey FOREIGN KEY (mateid)
        REFERENCES maintenance.material (mateid),
    CONSTRAINT al_maloid_fkey FOREIGN KEY (maloid)
        REFERENCES maintenance.material_lot (maloid),
    CONSTRAINT al_incoid_fkey FOREIGN KEY (incoid)
        REFERENCES maintenance.installed_component (incoid),
    CONSTRAINT al_read_by_fkey FOREIGN KEY (read_by)
        REFERENCES public.worker (workid),
    CONSTRAINT al_resolved_by_fkey FOREIGN KEY (resolved_by)
        REFERENCES public.worker (workid),
    CONSTRAINT al_message_check CHECK (message <> '')
);

CREATE INDEX IF NOT EXISTS idx_alert_unread  ON maintenance.alert_log (read)       WHERE read = false;
CREATE INDEX IF NOT EXISTS idx_alert_prcoid  ON maintenance.alert_log (prcoid);
CREATE INDEX IF NOT EXISTS idx_alert_date    ON maintenance.alert_log (alert_date);


-- =====================================================================
-- PARTE 2: VISTAS PARA BUSINESS INTELLIGENCE (14 vistas)
-- =====================================================================

-- Vista 1: KM actual de cada vehículo
CREATE OR REPLACE VIEW maintenance.vw_vehicle_current_km AS
SELECT
    v.prcoid,
    v.license_plate_number,
    v.vin_number,
    p.name                  AS vehicle_name,
    vt.name                 AS vehicle_type,
    v.year_of_manufacture,
    COALESCE(
        (SELECT re.kilometer_end
         FROM service.rentexecute re
         JOIN service.rentrequest rr ON re.sereid = rr.sereid
         WHERE rr.prodid = v.prodid
           AND re.kilometer_end IS NOT NULL
           AND re.statid <> 'CA'
         ORDER BY re.return_date DESC NULLS LAST
         LIMIT 1),
        v.mileage
    )                       AS current_km,
    v.mileage               AS registered_mileage
FROM product.vehicle v
JOIN public.product        p  ON v.prodid  = p.prodid
LEFT JOIN list.vehicletype vt ON v.vetyid  = vt.vetyid
WHERE v.status = true;

COMMENT ON VIEW maintenance.vw_vehicle_current_km IS
    'KM actual de cada vehículo: último kilometer_end de rentexecute (excl. canceladas) o mileage.';


-- Vista 2: Costo por km por vehículo
CREATE OR REPLACE VIEW maintenance.vw_cost_per_km AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    COUNT(m.mainid)                          AS total_services,
    COALESCE(SUM(mc_cost.cost_total), 0)     AS total_material_cost,
    vk.current_km,
    CASE
        WHEN vk.current_km > 0
        THEN ROUND(COALESCE(SUM(mc_cost.cost_total), 0) / vk.current_km, 4)
        ELSE 0
    END                                      AS cost_per_km
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
LEFT JOIN LATERAL (
    SELECT SUM(mc.quantity * COALESCE(ml.unit_cost, 0)) AS cost_total
    FROM maintenance.material_consumption mc
    LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
    WHERE mc.mainid = m.mainid
      AND mc.origin = 'Stock propio'
) mc_cost ON true
WHERE m.statid = 'AC'
GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name, vk.current_km;

COMMENT ON VIEW maintenance.vw_cost_per_km IS
    'Costo por km por vehículo. Solo materiales de stock propio con unit_cost del lote.';


-- Vista 3: Tasa de emergencias por vehículo
CREATE OR REPLACE VIEW maintenance.vw_emergency_rate AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    COUNT(*) FILTER (WHERE mt.name = 'Calendarizado') AS scheduled_count,
    COUNT(*) FILTER (WHERE mt.name = 'Emergencia')    AS emergency_count,
    COUNT(*)                                           AS total_count,
    CASE WHEN COUNT(*) > 0
        THEN ROUND(
            COUNT(*) FILTER (WHERE mt.name = 'Emergencia')::numeric
            / COUNT(*)::numeric * 100, 2)
        ELSE 0
    END                                                AS emergency_rate_percent
FROM maintenance.maintenance m
JOIN maintenance.maintenance_type      mt ON m.matyid = mt.matyid
JOIN maintenance.vw_vehicle_current_km vk ON m.prcoid = vk.prcoid
WHERE m.statid = 'AC'
GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name;

COMMENT ON VIEW maintenance.vw_emergency_rate IS
    'Tasa de emergencias por vehículo. Para total global: SUM de scheduled_count / emergency_count.';


-- Vista 4: Alertas activas no resueltas
CREATE OR REPLACE VIEW maintenance.vw_active_alerts AS
SELECT
    al.alloid,
    ac.alert_type,
    al.message,
    al.alert_date,
    al.prcoid,
    vk.license_plate_number,
    al.mateid,
    mat.name    AS material_name,
    al.maloid,
    al.incoid,
    al.read,
    al.resolved
FROM maintenance.alert_log          al
JOIN maintenance.alert_config        ac  ON al.alcoid = ac.alcoid
LEFT JOIN maintenance.vw_vehicle_current_km vk  ON al.prcoid = vk.prcoid
LEFT JOIN maintenance.material              mat ON al.mateid = mat.mateid
WHERE al.resolved = false
ORDER BY al.alert_date DESC;


-- Vista 5: Stock bajo mínimo
CREATE OR REPLACE VIEW maintenance.vw_low_stock AS
SELECT
    mat.mateid,
    mat.name,
    mc.name         AS category,
    mat.unit_of_measure,
    mat.stock_total,
    mat.stock_minimum,
    mat.stock_total - mat.stock_minimum AS deficit
FROM maintenance.material          mat
JOIN maintenance.material_category mc  ON mat.macaid = mc.macaid
WHERE mat.stock_total < mat.stock_minimum
  AND mat.status = true;


-- Vista 6: Lotes próximos a vencer (≤30 días)
CREATE OR REPLACE VIEW maintenance.vw_expiring_lots AS
SELECT
    ml.maloid,
    mat.mateid,
    mat.name                                          AS material_name,
    mc.name                                           AS category,
    ml.current_quantity,
    mat.unit_of_measure,
    ml.expiration_date,
    ml.expiration_date - CURRENT_DATE                 AS days_until_expiry,
    ml.unit_cost,
    ROUND(ml.current_quantity * ml.unit_cost, 2)      AS at_risk_cost,
    ml.lot_status
FROM maintenance.material_lot      ml
JOIN maintenance.material          mat ON ml.mateid = mat.mateid
JOIN maintenance.material_category mc  ON mat.macaid = mc.macaid
WHERE ml.lot_status = 'activo'
  AND ml.expiration_date IS NOT NULL
  AND ml.expiration_date <= CURRENT_DATE + INTERVAL '30 days'
ORDER BY ml.expiration_date ASC;

COMMENT ON VIEW maintenance.vw_expiring_lots IS
    'Lotes activos que vencen en ≤30 días. at_risk_cost = pérdida potencial si se descarta.';


-- Vista 7: Componentes instalados próximos a caducar
CREATE OR REPLACE VIEW maintenance.vw_expiring_components AS
SELECT
    ic.incoid,
    ic.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    ac.name             AS component_name,
    ic.installation_date,
    ic.installation_km,
    ic.expiration_date,
    ic.expiration_date - CURRENT_DATE AS days_until_expiry
FROM maintenance.installed_component        ic
JOIN maintenance.action_catalog              ac ON ic.acatid = ac.acatid
JOIN maintenance.vw_vehicle_current_km       vk ON ic.prcoid = vk.prcoid
WHERE ic.active = true
  AND ic.expiration_date IS NOT NULL
  AND ic.expiration_date <= CURRENT_DATE + INTERVAL '30 days'
ORDER BY ic.expiration_date ASC;


-- Vista 8: Historial completo de mantenimientos por vehículo
CREATE OR REPLACE VIEW maintenance.vw_maintenance_history AS
SELECT
    m.mainid,
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    mt.name                     AS maintenance_type,
    st.name                     AS service_type,
    m.maintenance_date,
    m.mileage,
    m.km_since_last,
    m.origin_service,
    m.oil_brand,
    m.oil_viscosity_sae,
    m.is_emergency_complete,
    d.general_status            AS diagnosis_status,
    d.vehicle_operative,
    COALESCE(cost_sub.total_cost, 0) AS total_material_cost,
    m.note
FROM maintenance.maintenance           m
JOIN maintenance.maintenance_type       mt  ON m.matyid = mt.matyid
JOIN maintenance.vw_vehicle_current_km  vk  ON m.prcoid = vk.prcoid
LEFT JOIN maintenance.service_type      st  ON m.setyid = st.setyid
LEFT JOIN maintenance.diagnosis         d   ON m.mainid = d.mainid
LEFT JOIN LATERAL (
    SELECT SUM(mc.quantity * COALESCE(ml.unit_cost, 0)) AS total_cost
    FROM maintenance.material_consumption mc
    LEFT JOIN maintenance.material_lot ml ON mc.maloid = ml.maloid
    WHERE mc.mainid = m.mainid AND mc.origin = 'Stock propio'
) cost_sub ON true
WHERE m.statid = 'AC'
ORDER BY m.prcoid, m.maintenance_date DESC;


-- Vista 9: Costo mensual de mantenimiento (para gráfico de barras)
CREATE OR REPLACE VIEW maintenance.vw_monthly_cost AS
SELECT
    DATE_TRUNC('month', m.maintenance_date) AS month,
    m.prcoid,
    vk.license_plate_number,
    COUNT(m.mainid)                         AS services_count,
    COALESCE(SUM(mc.quantity * COALESCE(ml.unit_cost, 0)), 0) AS monthly_cost
FROM maintenance.maintenance m
JOIN maintenance.vw_vehicle_current_km    vk  ON m.prcoid  = vk.prcoid
LEFT JOIN maintenance.material_consumption mc  ON mc.mainid = m.mainid AND mc.origin = 'Stock propio'
LEFT JOIN maintenance.material_lot         ml  ON mc.maloid = ml.maloid
WHERE m.statid = 'AC'
GROUP BY DATE_TRUNC('month', m.maintenance_date), m.prcoid, vk.license_plate_number
ORDER BY month DESC, monthly_cost DESC;

COMMENT ON VIEW maintenance.vw_monthly_cost IS
    'Costo mensual por vehículo. Agrupar por month para ver el total de la flota.';


-- Vista 10: Vida útil promedio de componentes (km entre cambios)
CREATE OR REPLACE VIEW maintenance.vw_component_useful_life AS
SELECT
    ac.acatid,
    ac.name         AS component_name,
    ac.category,
    COUNT(ic.incoid)        AS total_installations,
    ROUND(AVG(
        CASE
            WHEN ic2.installation_km IS NOT NULL
            THEN ic.installation_km - ic2.installation_km
        END
    ), 0)                   AS avg_km_between_changes,
    ac.useful_life_km       AS recommended_life_km
FROM maintenance.installed_component ic
JOIN maintenance.action_catalog ac ON ic.acatid = ac.acatid
LEFT JOIN maintenance.installed_component ic2
    ON ic2.replaced_by_incoid = ic.incoid
WHERE ic.active = false OR ic.replaced_by_incoid IS NOT NULL
GROUP BY ac.acatid, ac.name, ac.category, ac.useful_life_km
HAVING COUNT(ic.incoid) >= 2;

COMMENT ON VIEW maintenance.vw_component_useful_life IS
    'Vida útil real (km) por tipo de componente vs. vida recomendada.';


-- Vista 11: Cumplimiento del calendario
CREATE OR REPLACE VIEW maintenance.vw_calendar_compliance AS
SELECT
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    m.mainid,
    m.maintenance_date,
    m.mileage                                       AS service_km,
    vs.next_km - vs.interval_km                     AS scheduled_km,
    m.mileage - (vs.next_km - vs.interval_km)       AS km_deviation,
    CASE
        WHEN m.mileage <= (vs.next_km - vs.interval_km) + vs.alert_km_threshold
        THEN 'A tiempo'
        ELSE 'Retrasado'
    END                                             AS compliance_status
FROM maintenance.maintenance            m
JOIN maintenance.maintenance_type        mt ON m.matyid  = mt.matyid
JOIN maintenance.vehicle_schedule        vs ON m.prcoid  = vs.prcoid
JOIN maintenance.vw_vehicle_current_km   vk ON m.prcoid  = vk.prcoid
WHERE m.statid = 'AC'
  AND mt.name = 'Calendarizado'
ORDER BY m.prcoid, m.maintenance_date DESC;


-- Vista 12: Resumen de calificaciones por material
CREATE OR REPLACE VIEW maintenance.vw_material_rating_summary AS
SELECT
    mat.mateid,
    mat.name                        AS material_name,
    mc_cat.name                     AS category,
    COUNT(mr.matraid)               AS total_ratings,
    ROUND(AVG(mr.rating), 2)        AS avg_rating,
    COUNT(*) FILTER (WHERE mr.rating <= 2)  AS poor_ratings,
    COUNT(*) FILTER (WHERE mr.rating = 3)   AS regular_ratings,
    COUNT(*) FILTER (WHERE mr.rating >= 4)  AS good_ratings,
    (SELECT mr2.observation
     FROM maintenance.material_rating mr2
     WHERE mr2.mateid = mat.mateid AND mr2.rating <= 3
     ORDER BY mr2.rated_at DESC
     LIMIT 1)                       AS last_critical_observation
FROM maintenance.material              mat
JOIN maintenance.material_category      mc_cat ON mat.macaid = mc_cat.macaid
LEFT JOIN maintenance.material_rating   mr     ON mr.mateid  = mat.mateid
WHERE mat.status = true
GROUP BY mat.mateid, mat.name, mc_cat.name;

COMMENT ON VIEW maintenance.vw_material_rating_summary IS
    'Promedio de calificación por material. Alimenta análisis de calidad de proveedores.';


-- Vista 13: Costo de materiales descartados (merma por mes)
CREATE OR REPLACE VIEW maintenance.vw_discard_cost AS
SELECT
    DATE_TRUNC('month', md.discard_date) AS month,
    mat.mateid,
    mat.name                             AS material_name,
    mc_cat.name                          AS category,
    SUM(md.discarded_quantity)           AS total_discarded_qty,
    mat.unit_of_measure,
    ROUND(SUM(md.discarded_quantity * ml.unit_cost), 2) AS discard_cost,
    md.reason
FROM maintenance.material_discard  md
JOIN maintenance.material_lot       ml     ON md.maloid  = ml.maloid
JOIN maintenance.material           mat    ON ml.mateid  = mat.mateid
JOIN maintenance.material_category  mc_cat ON mat.macaid = mc_cat.macaid
GROUP BY DATE_TRUNC('month', md.discard_date), mat.mateid, mat.name,
         mc_cat.name, mat.unit_of_measure, md.reason
ORDER BY month DESC, discard_cost DESC;

COMMENT ON VIEW maintenance.vw_discard_cost IS
    'Costo de material descartado por mes. Detecta si se compra más de lo que se usa.';


-- Vista 14: Dashboard resumen general (KPIs del panel BI)
CREATE OR REPLACE VIEW maintenance.vw_bi_dashboard_summary AS
SELECT
    (SELECT COUNT(*) FROM product.vehicle WHERE status = true)
        AS total_vehicles,
    (SELECT COUNT(*) FROM maintenance.maintenance
     WHERE statid = 'AC'
       AND maintenance_date >= DATE_TRUNC('month', CURRENT_DATE))
        AS services_this_month,
    (SELECT ROUND(
        COUNT(*) FILTER (WHERE mt.name = 'Emergencia')::numeric
        / NULLIF(COUNT(*), 0)::numeric * 100, 2)
     FROM maintenance.maintenance m
     JOIN maintenance.maintenance_type mt ON m.matyid = mt.matyid
     WHERE m.statid = 'AC')
        AS global_emergency_rate_percent,
    (SELECT COUNT(*) FROM maintenance.vw_low_stock)
        AS low_stock_materials,
    (SELECT COUNT(*) FROM maintenance.alert_log WHERE resolved = false)
        AS unresolved_alerts,
    (SELECT COUNT(*) FROM maintenance.vw_expiring_lots)
        AS expiring_lots,
    (SELECT ROUND(AVG(cost_per_km), 4) FROM maintenance.vw_cost_per_km WHERE cost_per_km > 0)
        AS fleet_avg_cost_per_km;

COMMENT ON VIEW maintenance.vw_bi_dashboard_summary IS
    'KPIs principales para el panel superior del dashboard BI. Una fila con todos los indicadores.';


-- =====================================================================
-- RESUMEN FINAL
-- =====================================================================
-- CORRECCIONES APLICADAS:
--   [F1]  Secuencia list.companyconditionlist_coclid_seq duplicada → eliminada
--   [F2]  Secuencias sin esquema → address, zone, agency, bankaccounts con public.
--   [F3]  Secuencia public.windows_windid_seq agregada (faltaba)
--   [F4]  Orden: todas las tablas list.* antes de public.*
--   [F5]  Tablas list: companyconditionlist, companystatuslist, taxpayertypelist,
--         documenttype, moneymovetype, banks, reasonforcancellation,
--         sunatelectronicanswer → colocadas en orden correcto
--   [F6]  Tablas public faltantes agregadas:
--         windows, bankaccounts, sale, saleitems, payments, salepayments,
--         voucher, voucheritems, voucherpayments
--   [F7]  product.vehicle → INHERITS (product.company) re-agregado
--   [F8]  company.worker → FK a public.company re-incluida
--   [F9]  public.residence → nextval usa public.address_addrid_seq
--   [F10] public.zone/agency/windows/bankaccounts → nextval con esquema
--   [F11] Correcciones C1-C14 de versión anterior mantenidas
--
-- ESQUEMAS: list | public | product | company | service | maintenance
-- TABLAS TOTAL: ~45 tablas (existentes) + 19 tablas (maintenance)
-- VISTAS BI: 14 (todas en esquema maintenance)
-- ÍNDICES: 17
-- =====================================================================
