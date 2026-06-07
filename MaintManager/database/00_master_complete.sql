--
-- PostgreSQL database dump
--

\restrict W7BbFqRnwjDfbHZBcKGsfodrxtgbEfVZfW5vI28Xc9QlbbjxxOrMguOqklsmhnM

-- Dumped from database version 16.13
-- Dumped by pg_dump version 16.13

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: company; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA company;


--
-- Name: list; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA list;


--
-- Name: maintenance; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA maintenance;


--
-- Name: product; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA product;


--
-- Name: service; Type: SCHEMA; Schema: -; Owner: -
--

CREATE SCHEMA service;


--
-- Name: tank_level; Type: TYPE; Schema: public; Owner: -
--

CREATE TYPE public.tank_level AS ENUM (
    '1/8',
    '1/4',
    '3/8',
    '1/2',
    '5/8',
    '3/4',
    '7/8',
    '8/8'
);


--
-- Name: ins_main(text, integer, integer, text, text, integer, integer, text, text, text, text, text, text); Type: FUNCTION; Schema: maintenance; Owner: -
--

CREATE FUNCTION maintenance.ins_main(p_plate text, p_matyid integer, p_setyid integer, p_order text, p_date text, p_mileage integer, p_km_since integer, p_assigned_username text, p_note text, p_oil_brand text, p_oil_visc text, p_diag_status text, p_diag_recommend text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
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
$$;


--
-- Name: worker_cowoid_seq; Type: SEQUENCE; Schema: company; Owner: -
--

CREATE SEQUENCE company.worker_cowoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: worker; Type: TABLE; Schema: company; Owner: -
--

CREATE TABLE company.worker (
    cowoid integer DEFAULT nextval('company.worker_cowoid_seq'::regclass) NOT NULL,
    compid integer NOT NULL,
    persid integer NOT NULL,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    note text,
    status boolean DEFAULT true
);


--
-- Name: accountingplanelementtype_aetyid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.accountingplanelementtype_aetyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: accountingplanelementtype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.accountingplanelementtype (
    aetyid character(2) DEFAULT nextval('list.accountingplanelementtype_aetyid_seq'::regclass) NOT NULL,
    name character varying(100)
);


--
-- Name: banks_bankid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.banks_bankid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: banks; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.banks (
    bankid smallint DEFAULT nextval('list.banks_bankid_seq'::regclass) NOT NULL,
    name character varying(150) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT banks_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: civilstatus_cistid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.civilstatus_cistid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: civilstatus; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.civilstatus (
    cistid smallint DEFAULT nextval('list.civilstatus_cistid_seq'::regclass) NOT NULL,
    name character varying(20) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT civilstatus_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: coin_coinid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.coin_coinid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: coin; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.coin (
    coinid smallint DEFAULT nextval('list.coin_coinid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    symbol character varying(3),
    isocode character(3) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT coin_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: companyconditionlist_coclid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.companyconditionlist_coclid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: companyconditionlist; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.companyconditionlist (
    coclid smallint DEFAULT nextval('list.companyconditionlist_coclid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT companyconditionlist_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: companystatuslist_coslid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.companystatuslist_coslid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: companystatuslist; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.companystatuslist (
    coslid smallint DEFAULT nextval('list.companystatuslist_coslid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT companystatuslist_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: documentlist_doliid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.documentlist_doliid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: documentlist; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.documentlist (
    doliid smallint DEFAULT nextval('list.documentlist_doliid_seq'::regclass) NOT NULL,
    name character varying(150) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT documentlist_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: documenttype_dotyid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.documenttype_dotyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: documenttype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.documenttype (
    dotyid smallint DEFAULT nextval('list.documenttype_dotyid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    code character varying(3) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    doliid integer,
    CONSTRAINT documenttype_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: entitystatus; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.entitystatus (
    enstid character(2) NOT NULL,
    name character varying(20) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT entitystatus_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: fueltype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.fueltype (
    futyid character(2) NOT NULL,
    name character varying(45),
    description text
);


--
-- Name: identitydocumenttype_iddoid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.identitydocumenttype_iddoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: identitydocumenttype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.identitydocumenttype (
    iddoid smallint DEFAULT nextval('list.identitydocumenttype_iddoid_seq'::regclass) NOT NULL,
    name character varying(30) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT identitydocumenttype_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: jobcategory_jocaid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.jobcategory_jocaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: jobcategory; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.jobcategory (
    jocaid smallint DEFAULT nextval('list.jobcategory_jocaid_seq'::regclass) NOT NULL,
    name character varying(100) NOT NULL,
    description text NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT jobcategory_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: moneymovetype_mmtyid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.moneymovetype_mmtyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: moneymovetype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.moneymovetype (
    mmtyid integer DEFAULT nextval('list.moneymovetype_mmtyid_seq'::regclass) NOT NULL,
    name character varying(225),
    movetype character(1),
    description text
);


--
-- Name: producttype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.producttype (
    prtyid character(2) NOT NULL,
    name character varying(50) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT producttype_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: reasonforcancellation_recaid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.reasonforcancellation_recaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: reasonforcancellation; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.reasonforcancellation (
    recaid smallint DEFAULT nextval('list.reasonforcancellation_recaid_seq'::regclass) NOT NULL,
    description character varying(255)
);


--
-- Name: status; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.status (
    statid character(2) NOT NULL,
    name character varying(50) NOT NULL
);


--
-- Name: sunatelectronicanswer; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.sunatelectronicanswer (
    suea character(2) NOT NULL,
    name character varying(50),
    description text
);


--
-- Name: taxpayertypelist_tptlid_seq; Type: SEQUENCE; Schema: list; Owner: -
--

CREATE SEQUENCE list.taxpayertypelist_tptlid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: taxpayertypelist; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.taxpayertypelist (
    tptlid smallint DEFAULT nextval('list.taxpayertypelist_tptlid_seq'::regclass) NOT NULL,
    name character varying(100) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT taxpayertypelist_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: vehicletype; Type: TABLE; Schema: list; Owner: -
--

CREATE TABLE list.vehicletype (
    vetyid character(2) NOT NULL,
    name character varying(25) NOT NULL,
    description text
);


--
-- Name: action_catalog_acatid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.action_catalog_acatid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: action_catalog; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.action_catalog (
    acatid integer DEFAULT nextval('maintenance.action_catalog_acatid_seq'::regclass) NOT NULL,
    altoid smallint NOT NULL,
    name character varying(200) NOT NULL,
    category character varying(80),
    recommended_product character varying(200),
    recommended_quantity character varying(50),
    unit_of_measure character varying(30),
    useful_life_km integer,
    expires_by_time boolean DEFAULT false NOT NULL,
    useful_life_days integer,
    description text,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT action_catalog_life_check CHECK ((((expires_by_time = false) AND (useful_life_days IS NULL)) OR ((expires_by_time = true) AND (useful_life_days IS NOT NULL) AND (useful_life_days > 0)))),
    CONSTRAINT action_catalog_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: action_list_type_altoid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.action_list_type_altoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: action_list_type; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.action_list_type (
    altoid smallint DEFAULT nextval('maintenance.action_list_type_altoid_seq'::regclass) NOT NULL,
    name character varying(80) NOT NULL,
    description text,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT action_list_type_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: alert_config_alcoid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.alert_config_alcoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: alert_config; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.alert_config (
    alcoid integer DEFAULT nextval('maintenance.alert_config_alcoid_seq'::regclass) NOT NULL,
    alert_type character varying(50) NOT NULL,
    description text,
    enabled boolean DEFAULT true NOT NULL,
    threshold_value character varying(50),
    threshold_unit character varying(30),
    CONSTRAINT alert_config_type_check CHECK (((alert_type)::text <> ''::text))
);


--
-- Name: alert_log_alloid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.alert_log_alloid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: alert_log; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.alert_log (
    alloid integer DEFAULT nextval('maintenance.alert_log_alloid_seq'::regclass) NOT NULL,
    alcoid integer NOT NULL,
    prcoid integer,
    mateid integer,
    maloid integer,
    incoid integer,
    message text NOT NULL,
    alert_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    read boolean DEFAULT false NOT NULL,
    read_at timestamp without time zone,
    read_by integer,
    resolved boolean DEFAULT false NOT NULL,
    resolved_at timestamp without time zone,
    resolved_by integer,
    CONSTRAINT al_message_check CHECK ((message <> ''::text))
);


--
-- Name: config_system_cosyid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.config_system_cosyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: config_system; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.config_system (
    cosyid integer DEFAULT nextval('maintenance.config_system_cosyid_seq'::regclass) NOT NULL,
    key character varying(100) NOT NULL,
    value character varying(255) NOT NULL,
    description text,
    data_type character varying(20) DEFAULT 'string'::character varying NOT NULL,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_by integer,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT config_system_key_check CHECK (((key)::text <> ''::text)),
    CONSTRAINT config_system_value_check CHECK (((value)::text <> ''::text))
);


--
-- Name: diagnosis_diagid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.diagnosis_diagid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: diagnosis; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.diagnosis (
    diagid integer DEFAULT nextval('maintenance.diagnosis_diagid_seq'::regclass) NOT NULL,
    mainid integer NOT NULL,
    general_status character varying(100) NOT NULL,
    observations text,
    vehicle_operative boolean DEFAULT true NOT NULL,
    future_recommendations text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT diagnosis_status_check CHECK (((general_status)::text <> ''::text))
);


--
-- Name: installed_component_incoid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.installed_component_incoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: installed_component; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.installed_component (
    incoid integer DEFAULT nextval('maintenance.installed_component_incoid_seq'::regclass) NOT NULL,
    prcoid integer NOT NULL,
    acatid integer NOT NULL,
    mainid integer NOT NULL,
    maloid integer,
    installation_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    installation_km integer NOT NULL,
    expiration_date date,
    active boolean DEFAULT true NOT NULL,
    replaced_by_incoid integer,
    CONSTRAINT ic_km_check CHECK ((installation_km >= 0))
);


--
-- Name: maintenance_mainid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.maintenance_mainid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: maintenance; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.maintenance (
    mainid integer DEFAULT nextval('maintenance.maintenance_mainid_seq'::regclass) NOT NULL,
    prcoid integer NOT NULL,
    matyid smallint NOT NULL,
    setyid smallint,
    order_number character varying(30),
    maintenance_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    mileage integer NOT NULL,
    km_since_last integer,
    additional_work text,
    oil_brand character varying(100),
    oil_viscosity_sae character varying(20),
    climate_season character varying(50),
    show_oil_in_next_maintenance boolean DEFAULT false NOT NULL,
    origin_service character varying(50) DEFAULT 'Taller propio'::character varying NOT NULL,
    signature_seal text,
    is_emergency_complete boolean,
    workid integer NOT NULL,
    note text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    statid character(2) DEFAULT 'AC'::bpchar NOT NULL,
    assigned_to integer,
    CONSTRAINT maintenance_km_since_check CHECK (((km_since_last IS NULL) OR (km_since_last >= 0))),
    CONSTRAINT maintenance_mileage_check CHECK ((mileage >= 0)),
    CONSTRAINT maintenance_origin_check CHECK (((origin_service)::text = ANY (ARRAY['Taller propio'::text, 'Taller externo'::text])))
);


--
-- Name: maintenance_action_detail_madeid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.maintenance_action_detail_madeid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: maintenance_action_detail; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.maintenance_action_detail (
    madeid integer DEFAULT nextval('maintenance.maintenance_action_detail_madeid_seq'::regclass) NOT NULL,
    mainid integer NOT NULL,
    acatid integer NOT NULL,
    completed boolean DEFAULT false NOT NULL,
    action_performed character(1),
    product_used character varying(200),
    quantity_used character varying(50),
    origin_product character varying(50),
    observation text,
    maloid integer,
    CONSTRAINT mad_action_check CHECK (((action_performed IS NULL) OR ((action_performed)::text = ANY (ARRAY['A'::text, 'C'::text, 'I'::text, 'R'::text])))),
    CONSTRAINT mad_origin_check CHECK (((origin_product IS NULL) OR ((origin_product)::text = ANY (ARRAY['Stock propio'::text, 'Externo'::text]))))
);


--
-- Name: maintenance_type_matyid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.maintenance_type_matyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: maintenance_type; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.maintenance_type (
    matyid smallint DEFAULT nextval('maintenance.maintenance_type_matyid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT maintenance_type_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: managed_vehicle; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.managed_vehicle (
    mv_id integer NOT NULL,
    prcoid integer,
    license_plate character varying(20) NOT NULL,
    vehicle_name character varying(200) NOT NULL,
    brand character varying(100),
    model character varying(100),
    year smallint,
    color character varying(50),
    vin character varying(50),
    engine_number character varying(50),
    source character varying(20) DEFAULT 'managed'::character varying NOT NULL,
    status boolean DEFAULT true NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    updated_at timestamp with time zone DEFAULT now() NOT NULL,
    CONSTRAINT managed_vehicle_source_check CHECK (((source)::text = ANY ((ARRAY['legacy'::character varying, 'managed'::character varying])::text[])))
);


--
-- Name: managed_vehicle_mv_id_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.managed_vehicle_mv_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: managed_vehicle_mv_id_seq; Type: SEQUENCE OWNED BY; Schema: maintenance; Owner: -
--

ALTER SEQUENCE maintenance.managed_vehicle_mv_id_seq OWNED BY maintenance.managed_vehicle.mv_id;


--
-- Name: material_mateid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_mateid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material (
    mateid integer DEFAULT nextval('maintenance.material_mateid_seq'::regclass) NOT NULL,
    macaid smallint NOT NULL,
    name character varying(200) NOT NULL,
    unit_of_measure character varying(30) NOT NULL,
    stock_total numeric(12,3) DEFAULT 0 NOT NULL,
    stock_minimum numeric(12,3) DEFAULT 0 NOT NULL,
    description text,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by integer NOT NULL,
    status boolean DEFAULT true NOT NULL,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT material_minimum_check CHECK ((stock_minimum >= (0)::numeric)),
    CONSTRAINT material_name_check CHECK (((name)::text <> ''::text)),
    CONSTRAINT material_stock_check CHECK ((stock_total >= (0)::numeric)),
    CONSTRAINT material_unit_check CHECK (((unit_of_measure)::text <> ''::text))
);


--
-- Name: material_category_macaid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_category_macaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material_category; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material_category (
    macaid smallint DEFAULT nextval('maintenance.material_category_macaid_seq'::regclass) NOT NULL,
    name character varying(100) NOT NULL,
    description text,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT material_category_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: material_consumption_macoid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_consumption_macoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material_consumption; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material_consumption (
    macoid integer DEFAULT nextval('maintenance.material_consumption_macoid_seq'::regclass) NOT NULL,
    mainid integer NOT NULL,
    mateid integer NOT NULL,
    maloid integer,
    quantity numeric(12,3) NOT NULL,
    origin character varying(50) DEFAULT 'Stock propio'::character varying NOT NULL,
    consumed_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT mc_origin_check CHECK (((origin)::text = ANY (ARRAY['Stock propio'::text, 'Externo'::text]))),
    CONSTRAINT mc_quantity_check CHECK ((quantity > (0)::numeric))
);


--
-- Name: material_discard_madiid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_discard_madiid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material_discard; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material_discard (
    madiid integer DEFAULT nextval('maintenance.material_discard_madiid_seq'::regclass) NOT NULL,
    maloid integer NOT NULL,
    discarded_quantity numeric(12,3) NOT NULL,
    discard_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    reason character varying(50) NOT NULL,
    note text,
    discarded_by integer NOT NULL,
    CONSTRAINT md_quantity_check CHECK ((discarded_quantity > (0)::numeric)),
    CONSTRAINT md_reason_check CHECK (((reason)::text = ANY (ARRAY['Vencimiento'::text, 'Daño'::text, 'Otro'::text])))
);


--
-- Name: material_lot_maloid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_lot_maloid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material_lot; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material_lot (
    maloid integer DEFAULT nextval('maintenance.material_lot_maloid_seq'::regclass) NOT NULL,
    mateid integer NOT NULL,
    initial_quantity numeric(12,3) NOT NULL,
    current_quantity numeric(12,3) NOT NULL,
    unit_cost numeric(12,4) DEFAULT 0 NOT NULL,
    entry_date timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    expiration_date date,
    provid integer,
    supplier_lot_number character varying(100),
    note text,
    lot_status character varying(20) DEFAULT 'activo'::character varying NOT NULL,
    created_by integer NOT NULL,
    CONSTRAINT material_lot_current_check CHECK ((current_quantity >= (0)::numeric)),
    CONSTRAINT material_lot_qty_check CHECK ((initial_quantity > (0)::numeric)),
    CONSTRAINT material_lot_status_check CHECK (((lot_status)::text = ANY (ARRAY['activo'::text, 'agotado'::text, 'vencido'::text, 'descartado'::text]))),
    CONSTRAINT material_lot_unit_cost_check CHECK ((unit_cost >= (0)::numeric))
);


--
-- Name: material_rating_matraid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.material_rating_matraid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: material_rating; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.material_rating (
    matraid integer DEFAULT nextval('maintenance.material_rating_matraid_seq'::regclass) NOT NULL,
    mateid integer NOT NULL,
    mainid integer NOT NULL,
    rating smallint NOT NULL,
    observation text,
    rated_by integer NOT NULL,
    rated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT mr_observation_required CHECK (((rating > 3) OR ((rating <= 3) AND (observation IS NOT NULL) AND (observation <> ''::text)))),
    CONSTRAINT mr_rating_range CHECK (((rating >= 1) AND (rating <= 5)))
);


--
-- Name: schedule_action_shacid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.schedule_action_shacid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: schedule_action; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.schedule_action (
    shacid integer DEFAULT nextval('maintenance.schedule_action_shacid_seq'::regclass) NOT NULL,
    veshid integer NOT NULL,
    acatid integer NOT NULL,
    scheduled_km integer NOT NULL,
    action_code character(1) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT schedule_action_code_check CHECK (((action_code)::text = ANY (ARRAY['A'::text, 'C'::text, 'I'::text, 'R'::text]))),
    CONSTRAINT schedule_action_km_check CHECK ((scheduled_km > 0))
);


--
-- Name: service_type_setyid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.service_type_setyid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: service_type; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.service_type (
    setyid smallint DEFAULT nextval('maintenance.service_type_setyid_seq'::regclass) NOT NULL,
    code character(1) NOT NULL,
    name character varying(50) NOT NULL,
    description text,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT service_type_code_check CHECK (((code)::text = ANY (ARRAY['A'::text, 'B'::text])))
);


--
-- Name: technician_assignment_teasid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.technician_assignment_teasid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: technician_assignment; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.technician_assignment (
    teasid integer DEFAULT nextval('maintenance.technician_assignment_teasid_seq'::regclass) NOT NULL,
    mainid integer NOT NULL,
    workid integer NOT NULL,
    role_in_job character varying(50) DEFAULT 'Principal'::character varying,
    assigned_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    assigned_by integer NOT NULL
);


--
-- Name: vehicle_allowed_action; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.vehicle_allowed_action (
    vaacid integer NOT NULL,
    prcoid integer,
    acatid integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    mv_id integer
);


--
-- Name: vehicle_allowed_action_vaacid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.vehicle_allowed_action_vaacid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: vehicle_allowed_action_vaacid_seq; Type: SEQUENCE OWNED BY; Schema: maintenance; Owner: -
--

ALTER SEQUENCE maintenance.vehicle_allowed_action_vaacid_seq OWNED BY maintenance.vehicle_allowed_action.vaacid;


--
-- Name: vehicle_allowed_component; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.vehicle_allowed_component (
    vacoid integer NOT NULL,
    prcoid integer,
    acatid integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    mv_id integer
);


--
-- Name: vehicle_allowed_component_vacoid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.vehicle_allowed_component_vacoid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: vehicle_allowed_component_vacoid_seq; Type: SEQUENCE OWNED BY; Schema: maintenance; Owner: -
--

ALTER SEQUENCE maintenance.vehicle_allowed_component_vacoid_seq OWNED BY maintenance.vehicle_allowed_component.vacoid;


--
-- Name: vehicle_allowed_material; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.vehicle_allowed_material (
    vamid integer NOT NULL,
    prcoid integer,
    mateid integer NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    mv_id integer
);


--
-- Name: vehicle_allowed_material_vamid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.vehicle_allowed_material_vamid_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: vehicle_allowed_material_vamid_seq; Type: SEQUENCE OWNED BY; Schema: maintenance; Owner: -
--

ALTER SEQUENCE maintenance.vehicle_allowed_material_vamid_seq OWNED BY maintenance.vehicle_allowed_material.vamid;


--
-- Name: vehicle_schedule_veshid_seq; Type: SEQUENCE; Schema: maintenance; Owner: -
--

CREATE SEQUENCE maintenance.vehicle_schedule_veshid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: vehicle_schedule; Type: TABLE; Schema: maintenance; Owner: -
--

CREATE TABLE maintenance.vehicle_schedule (
    veshid integer DEFAULT nextval('maintenance.vehicle_schedule_veshid_seq'::regclass) NOT NULL,
    prcoid integer NOT NULL,
    interval_km integer DEFAULT 5000 NOT NULL,
    next_km integer NOT NULL,
    alert_km_threshold integer DEFAULT 800,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    created_by integer NOT NULL,
    updated_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    status boolean DEFAULT true NOT NULL,
    next_service_type_code character(1) DEFAULT 'A'::bpchar,
    CONSTRAINT vehicle_schedule_alert_check CHECK ((alert_km_threshold >= 0)),
    CONSTRAINT vehicle_schedule_interval_check CHECK ((interval_km > 0)),
    CONSTRAINT vehicle_schedule_next_km_check CHECK ((next_km >= 0)),
    CONSTRAINT vehicle_schedule_svc_type_check CHECK (((next_service_type_code IS NULL) OR (next_service_type_code = ANY (ARRAY['A'::character(1), 'B'::character(1)]))))
);


--
-- Name: company_prcoid_seq; Type: SEQUENCE; Schema: product; Owner: -
--

CREATE SEQUENCE product.company_prcoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.company (
    prcoid integer DEFAULT nextval('product.company_prcoid_seq'::regclass) NOT NULL,
    prodid integer NOT NULL,
    description text,
    serial_number character varying(150),
    qty integer DEFAULT 1,
    status boolean DEFAULT true,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    workid integer NOT NULL,
    prstid character(1) DEFAULT 'A'::bpchar
);


--
-- Name: vehicle; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.vehicle (
    license_plate_number character varying(50),
    vetyid character(2),
    year_of_manufacture smallint,
    engine_number character varying(50),
    futyid character(2),
    number_of_passengers smallint,
    row_account smallint,
    gross_vehicle_weight numeric,
    net_vehicle_weight numeric,
    number_of_axles smallint,
    number_of_wheels smallint,
    color character varying(35),
    mileage integer,
    power character varying(20) DEFAULT ''::character varying,
    wheel_formula character varying(10) DEFAULT ''::character varying,
    version character varying(100) DEFAULT ''::character varying,
    cylinder_count smallint DEFAULT 0,
    cylinder_capacity numeric(12,4) DEFAULT 0.00,
    length numeric(12,4) DEFAULT 0.00,
    height numeric(12,4) DEFAULT 0.00,
    width numeric(12,4) DEFAULT 0.00,
    payload numeric(12,4) DEFAULT 0.00,
    verification_code character varying(25) DEFAULT ''::character varying,
    publish_number character varying(25) DEFAULT ''::character varying,
    document_date timestamp without time zone,
    registry_entry character varying(25) DEFAULT ''::character varying,
    dua_dam character varying(30) DEFAULT ''::character varying,
    title character varying(25) DEFAULT ''::character varying,
    title_date timestamp without time zone,
    condition character(2) DEFAULT 'NU'::bpchar,
    seat_count smallint DEFAULT 0,
    vin_number character varying(50),
    category character varying(50),
    CONSTRAINT vehicle_mileage_check CHECK ((mileage >= 0))
)
INHERITS (product.company);


--
-- Name: product_prodid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.product_prodid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: product; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.product (
    prodid integer DEFAULT nextval('public.product_prodid_seq'::regclass) NOT NULL,
    name character varying(255) NOT NULL,
    prtyid character(2) NOT NULL,
    description text,
    coinid smallint NOT NULL,
    workid integer NOT NULL,
    cost numeric(12,3),
    registrationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    status boolean DEFAULT true NOT NULL,
    webvisible boolean DEFAULT false NOT NULL,
    icon text,
    image text,
    smalldescription text,
    tax numeric(4,4) DEFAULT 0.00,
    account character varying(255),
    locked boolean DEFAULT false,
    modifyprice boolean DEFAULT false,
    CONSTRAINT product_cost_check CHECK ((cost >= (0)::numeric)),
    CONSTRAINT product_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: rentexecute_seexid_seq; Type: SEQUENCE; Schema: service; Owner: -
--

CREATE SEQUENCE service.rentexecute_seexid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: rentexecute; Type: TABLE; Schema: service; Owner: -
--

CREATE TABLE service.rentexecute (
    seexid integer DEFAULT nextval('service.rentexecute_seexid_seq'::regclass) NOT NULL,
    coceid integer NOT NULL,
    workid integer NOT NULL,
    clieid integer NOT NULL,
    delivered_date timestamp without time zone NOT NULL,
    delivered_workid integer NOT NULL,
    received_cowoid integer NOT NULL,
    note_start text,
    kilometer_start integer,
    kilometer_end integer,
    delivery_cowoid integer,
    received_workid integer,
    return_date timestamp without time zone,
    note_end text,
    made_sell_document boolean DEFAULT false,
    checklist integer NOT NULL,
    sereid integer,
    statid character(2) DEFAULT 'AC'::bpchar,
    tank_start public.tank_level,
    tank_end public.tank_level,
    CONSTRAINT rentexecute_kilometer_start_check CHECK ((kilometer_start >= 0))
);


--
-- Name: rentrequest_sereid_seq; Type: SEQUENCE; Schema: service; Owner: -
--

CREATE SEQUENCE service.rentrequest_sereid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: rentrequest; Type: TABLE; Schema: service; Owner: -
--

CREATE TABLE service.rentrequest (
    sereid integer DEFAULT nextval('service.rentrequest_sereid_seq'::regclass) NOT NULL,
    coceid integer NOT NULL,
    persid integer NOT NULL,
    driver integer NOT NULL,
    prodid integer NOT NULL,
    price numeric(7,2) DEFAULT 0,
    pricecoin integer NOT NULL,
    guarantee numeric(7,2) DEFAULT 0,
    frecuency character varying(1) NOT NULL,
    guaranteecoin integer NOT NULL,
    deliverydate timestamp without time zone NOT NULL,
    returndate timestamp without time zone NOT NULL,
    exactreturn boolean NOT NULL,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    compid integer,
    statid character(2) DEFAULT 'AC'::bpchar,
    CONSTRAINT rentrequest_guarantee_check CHECK ((guarantee >= (0)::numeric)),
    CONSTRAINT rentrequest_price_check CHECK ((price >= (0)::numeric))
);


--
-- Name: vw_vehicle_current_km; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_vehicle_current_km AS
 SELECT v.prcoid,
    v.license_plate_number,
    v.vin_number,
    p.name AS vehicle_name,
    vt.name AS vehicle_type,
    v.year_of_manufacture,
    COALESCE(( SELECT re.kilometer_end
           FROM (service.rentexecute re
             JOIN service.rentrequest rr ON ((re.sereid = rr.sereid)))
          WHERE ((rr.prodid = v.prodid) AND (re.kilometer_end IS NOT NULL) AND (re.statid <> 'CA'::bpchar))
          ORDER BY re.return_date DESC NULLS LAST
         LIMIT 1), v.mileage) AS current_km,
    v.mileage AS registered_mileage
   FROM ((product.vehicle v
     JOIN public.product p ON ((v.prodid = p.prodid)))
     LEFT JOIN list.vehicletype vt ON ((v.vetyid = vt.vetyid)))
  WHERE (v.status = true);


--
-- Name: vw_active_alerts; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_active_alerts AS
 SELECT al.alloid,
    ac.alert_type,
    al.message,
    al.alert_date,
    al.prcoid,
    vk.license_plate_number,
    al.mateid,
    mat.name AS material_name,
    al.maloid,
    al.incoid,
    al.read,
    al.resolved
   FROM (((maintenance.alert_log al
     JOIN maintenance.alert_config ac ON ((al.alcoid = ac.alcoid)))
     LEFT JOIN maintenance.vw_vehicle_current_km vk ON ((al.prcoid = vk.prcoid)))
     LEFT JOIN maintenance.material mat ON ((al.mateid = mat.mateid)))
  WHERE (al.resolved = false)
  ORDER BY al.alert_date DESC;


--
-- Name: vw_cost_per_km; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_cost_per_km AS
 SELECT m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    count(m.mainid) AS total_services,
    COALESCE(sum(mc_cost.cost_total), (0)::numeric) AS total_material_cost,
    vk.current_km,
        CASE
            WHEN (vk.current_km > 0) THEN round((COALESCE(sum(mc_cost.cost_total), (0)::numeric) / (vk.current_km)::numeric), 4)
            ELSE (0)::numeric
        END AS cost_per_km
   FROM ((maintenance.maintenance m
     JOIN maintenance.vw_vehicle_current_km vk ON ((m.prcoid = vk.prcoid)))
     LEFT JOIN LATERAL ( SELECT sum((mc.quantity * COALESCE(ml.unit_cost, (0)::numeric))) AS cost_total
           FROM (maintenance.material_consumption mc
             LEFT JOIN maintenance.material_lot ml ON ((mc.maloid = ml.maloid)))
          WHERE ((mc.mainid = m.mainid) AND ((mc.origin)::text = 'Stock propio'::text))) mc_cost ON (true))
  WHERE (m.statid = 'AC'::bpchar)
  GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name, vk.current_km;


--
-- Name: vw_expiring_lots; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_expiring_lots AS
 SELECT ml.maloid,
    mat.mateid,
    mat.name AS material_name,
    mc.name AS category,
    ml.current_quantity,
    mat.unit_of_measure,
    ml.expiration_date,
    (ml.expiration_date - CURRENT_DATE) AS days_until_expiry,
    ml.unit_cost,
    round((ml.current_quantity * ml.unit_cost), 2) AS at_risk_cost,
    ml.lot_status
   FROM ((maintenance.material_lot ml
     JOIN maintenance.material mat ON ((ml.mateid = mat.mateid)))
     JOIN maintenance.material_category mc ON ((mat.macaid = mc.macaid)))
  WHERE (((ml.lot_status)::text = 'activo'::text) AND (ml.expiration_date IS NOT NULL) AND (ml.expiration_date <= (CURRENT_DATE + '30 days'::interval)))
  ORDER BY ml.expiration_date;


--
-- Name: vw_low_stock; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_low_stock AS
 SELECT mat.mateid,
    mat.name,
    mc.name AS category,
    mat.unit_of_measure,
    mat.stock_total,
    mat.stock_minimum,
    (mat.stock_total - mat.stock_minimum) AS deficit
   FROM (maintenance.material mat
     JOIN maintenance.material_category mc ON ((mat.macaid = mc.macaid)))
  WHERE ((mat.stock_total < mat.stock_minimum) AND (mat.status = true));


--
-- Name: vw_bi_dashboard_summary; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_bi_dashboard_summary AS
 SELECT ( SELECT count(*) AS count
           FROM product.vehicle
          WHERE (vehicle.status = true)) AS total_vehicles,
    ( SELECT count(*) AS count
           FROM maintenance.maintenance
          WHERE ((maintenance.statid = 'AC'::bpchar) AND (maintenance.maintenance_date >= date_trunc('month'::text, (CURRENT_DATE)::timestamp with time zone)))) AS services_this_month,
    ( SELECT round((((count(*) FILTER (WHERE ((mt.name)::text = 'Emergencia'::text)))::numeric / (NULLIF(count(*), 0))::numeric) * (100)::numeric), 2) AS round
           FROM (maintenance.maintenance m
             JOIN maintenance.maintenance_type mt ON ((m.matyid = mt.matyid)))
          WHERE (m.statid = 'AC'::bpchar)) AS global_emergency_rate_percent,
    ( SELECT count(*) AS count
           FROM maintenance.vw_low_stock) AS low_stock_materials,
    ( SELECT count(*) AS count
           FROM maintenance.alert_log
          WHERE (alert_log.resolved = false)) AS unresolved_alerts,
    ( SELECT count(*) AS count
           FROM maintenance.vw_expiring_lots) AS expiring_lots,
    ( SELECT round(avg(vw_cost_per_km.cost_per_km), 4) AS round
           FROM maintenance.vw_cost_per_km
          WHERE (vw_cost_per_km.cost_per_km > (0)::numeric)) AS fleet_avg_cost_per_km;


--
-- Name: vw_calendar_compliance; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_calendar_compliance AS
 SELECT m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    m.mainid,
    m.maintenance_date,
    m.mileage AS service_km,
    (vs.next_km - vs.interval_km) AS scheduled_km,
    (m.mileage - (vs.next_km - vs.interval_km)) AS km_deviation,
        CASE
            WHEN (m.mileage <= ((vs.next_km - vs.interval_km) + vs.alert_km_threshold)) THEN 'A tiempo'::text
            ELSE 'Retrasado'::text
        END AS compliance_status
   FROM (((maintenance.maintenance m
     JOIN maintenance.maintenance_type mt ON ((m.matyid = mt.matyid)))
     JOIN maintenance.vehicle_schedule vs ON ((m.prcoid = vs.prcoid)))
     JOIN maintenance.vw_vehicle_current_km vk ON ((m.prcoid = vk.prcoid)))
  WHERE ((m.statid = 'AC'::bpchar) AND ((mt.name)::text = 'Calendarizado'::text))
  ORDER BY m.prcoid, m.maintenance_date DESC;


--
-- Name: vw_component_useful_life; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_component_useful_life AS
 SELECT ac.acatid,
    ac.name AS component_name,
    ac.category,
    count(ic.incoid) AS total_installations,
    round(avg(
        CASE
            WHEN (ic2.installation_km IS NOT NULL) THEN (ic.installation_km - ic2.installation_km)
            ELSE NULL::integer
        END), 0) AS avg_km_between_changes,
    ac.useful_life_km AS recommended_life_km
   FROM ((maintenance.installed_component ic
     JOIN maintenance.action_catalog ac ON ((ic.acatid = ac.acatid)))
     LEFT JOIN maintenance.installed_component ic2 ON ((ic2.replaced_by_incoid = ic.incoid)))
  WHERE ((ic.active = false) OR (ic.replaced_by_incoid IS NOT NULL))
  GROUP BY ac.acatid, ac.name, ac.category, ac.useful_life_km
 HAVING (count(ic.incoid) >= 2);


--
-- Name: vw_discard_cost; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_discard_cost AS
 SELECT date_trunc('month'::text, md.discard_date) AS month,
    mat.mateid,
    mat.name AS material_name,
    mc_cat.name AS category,
    sum(md.discarded_quantity) AS total_discarded_qty,
    mat.unit_of_measure,
    round(sum((md.discarded_quantity * ml.unit_cost)), 2) AS discard_cost,
    md.reason
   FROM (((maintenance.material_discard md
     JOIN maintenance.material_lot ml ON ((md.maloid = ml.maloid)))
     JOIN maintenance.material mat ON ((ml.mateid = mat.mateid)))
     JOIN maintenance.material_category mc_cat ON ((mat.macaid = mc_cat.macaid)))
  GROUP BY (date_trunc('month'::text, md.discard_date)), mat.mateid, mat.name, mc_cat.name, mat.unit_of_measure, md.reason
  ORDER BY (date_trunc('month'::text, md.discard_date)) DESC, (round(sum((md.discarded_quantity * ml.unit_cost)), 2)) DESC;


--
-- Name: vw_emergency_rate; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_emergency_rate AS
 SELECT m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    count(*) FILTER (WHERE ((mt.name)::text = 'Calendarizado'::text)) AS scheduled_count,
    count(*) FILTER (WHERE ((mt.name)::text = 'Emergencia'::text)) AS emergency_count,
    count(*) AS total_count,
        CASE
            WHEN (count(*) > 0) THEN round((((count(*) FILTER (WHERE ((mt.name)::text = 'Emergencia'::text)))::numeric / (count(*))::numeric) * (100)::numeric), 2)
            ELSE (0)::numeric
        END AS emergency_rate_percent
   FROM ((maintenance.maintenance m
     JOIN maintenance.maintenance_type mt ON ((m.matyid = mt.matyid)))
     JOIN maintenance.vw_vehicle_current_km vk ON ((m.prcoid = vk.prcoid)))
  WHERE (m.statid = 'AC'::bpchar)
  GROUP BY m.prcoid, vk.license_plate_number, vk.vehicle_name;


--
-- Name: vw_expiring_components; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_expiring_components AS
 SELECT ic.incoid,
    ic.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    ac.name AS component_name,
    ic.installation_date,
    ic.installation_km,
    ic.expiration_date,
    (ic.expiration_date - CURRENT_DATE) AS days_until_expiry
   FROM ((maintenance.installed_component ic
     JOIN maintenance.action_catalog ac ON ((ic.acatid = ac.acatid)))
     JOIN maintenance.vw_vehicle_current_km vk ON ((ic.prcoid = vk.prcoid)))
  WHERE ((ic.active = true) AND (ic.expiration_date IS NOT NULL) AND (ic.expiration_date <= (CURRENT_DATE + '30 days'::interval)))
  ORDER BY ic.expiration_date;


--
-- Name: vw_maintenance_history; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_maintenance_history AS
 SELECT m.mainid,
    m.prcoid,
    vk.license_plate_number,
    vk.vehicle_name,
    mt.name AS maintenance_type,
    st.name AS service_type,
    m.maintenance_date,
    m.mileage,
    m.km_since_last,
    m.origin_service,
    m.oil_brand,
    m.oil_viscosity_sae,
    m.is_emergency_complete,
    d.general_status AS diagnosis_status,
    d.vehicle_operative,
    COALESCE(cost_sub.total_cost, (0)::numeric) AS total_material_cost,
    m.note
   FROM (((((maintenance.maintenance m
     JOIN maintenance.maintenance_type mt ON ((m.matyid = mt.matyid)))
     JOIN maintenance.vw_vehicle_current_km vk ON ((m.prcoid = vk.prcoid)))
     LEFT JOIN maintenance.service_type st ON ((m.setyid = st.setyid)))
     LEFT JOIN maintenance.diagnosis d ON ((m.mainid = d.mainid)))
     LEFT JOIN LATERAL ( SELECT sum((mc.quantity * COALESCE(ml.unit_cost, (0)::numeric))) AS total_cost
           FROM (maintenance.material_consumption mc
             LEFT JOIN maintenance.material_lot ml ON ((mc.maloid = ml.maloid)))
          WHERE ((mc.mainid = m.mainid) AND ((mc.origin)::text = 'Stock propio'::text))) cost_sub ON (true))
  WHERE (m.statid = 'AC'::bpchar)
  ORDER BY m.prcoid, m.maintenance_date DESC;


--
-- Name: vw_material_rating_summary; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_material_rating_summary AS
 SELECT mat.mateid,
    mat.name AS material_name,
    mc_cat.name AS category,
    count(mr.matraid) AS total_ratings,
    round(avg(mr.rating), 2) AS avg_rating,
    count(*) FILTER (WHERE (mr.rating <= 2)) AS poor_ratings,
    count(*) FILTER (WHERE (mr.rating = 3)) AS regular_ratings,
    count(*) FILTER (WHERE (mr.rating >= 4)) AS good_ratings,
    ( SELECT mr2.observation
           FROM maintenance.material_rating mr2
          WHERE ((mr2.mateid = mat.mateid) AND (mr2.rating <= 3))
          ORDER BY mr2.rated_at DESC
         LIMIT 1) AS last_critical_observation
   FROM ((maintenance.material mat
     JOIN maintenance.material_category mc_cat ON ((mat.macaid = mc_cat.macaid)))
     LEFT JOIN maintenance.material_rating mr ON ((mr.mateid = mat.mateid)))
  WHERE (mat.status = true)
  GROUP BY mat.mateid, mat.name, mc_cat.name;


--
-- Name: vw_monthly_cost; Type: VIEW; Schema: maintenance; Owner: -
--

CREATE VIEW maintenance.vw_monthly_cost AS
 SELECT date_trunc('month'::text, m.maintenance_date) AS month,
    m.prcoid,
    vk.license_plate_number,
    count(m.mainid) AS services_count,
    COALESCE(sum((mc.quantity * COALESCE(ml.unit_cost, (0)::numeric))), (0)::numeric) AS monthly_cost
   FROM (((maintenance.maintenance m
     JOIN maintenance.vw_vehicle_current_km vk ON ((m.prcoid = vk.prcoid)))
     LEFT JOIN maintenance.material_consumption mc ON (((mc.mainid = m.mainid) AND ((mc.origin)::text = 'Stock propio'::text))))
     LEFT JOIN maintenance.material_lot ml ON ((mc.maloid = ml.maloid)))
  WHERE (m.statid = 'AC'::bpchar)
  GROUP BY (date_trunc('month'::text, m.maintenance_date)), m.prcoid, vk.license_plate_number
  ORDER BY (date_trunc('month'::text, m.maintenance_date)) DESC, COALESCE(sum((mc.quantity * COALESCE(ml.unit_cost, (0)::numeric))), (0)::numeric) DESC;


--
-- Name: productbrand_prbrid_seq; Type: SEQUENCE; Schema: product; Owner: -
--

CREATE SEQUENCE product.productbrand_prbrid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: productbrand; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.productbrand (
    prbrid smallint DEFAULT nextval('product.productbrand_prbrid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT productbrand_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: productcategory_prcaid_seq; Type: SEQUENCE; Schema: product; Owner: -
--

CREATE SEQUENCE product.productcategory_prcaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: productcategory; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.productcategory (
    prcaid smallint DEFAULT nextval('product.productcategory_prcaid_seq'::regclass) NOT NULL,
    name character varying(255),
    state boolean,
    status boolean DEFAULT true
);


--
-- Name: productmerchandise_prmeid_seq; Type: SEQUENCE; Schema: product; Owner: -
--

CREATE SEQUENCE product.productmerchandise_prmeid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: productmerchandise; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.productmerchandise (
    prmeid integer DEFAULT nextval('product.productmerchandise_prmeid_seq'::regclass) NOT NULL,
    prodid integer NOT NULL,
    prcaid smallint NOT NULL,
    prmoid integer,
    kardex boolean DEFAULT true NOT NULL,
    utility numeric(12,4),
    utilitybypercent boolean DEFAULT false,
    comercial boolean DEFAULT false NOT NULL
);


--
-- Name: productmodel_prmoid_seq; Type: SEQUENCE; Schema: product; Owner: -
--

CREATE SEQUENCE product.productmodel_prmoid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: productmodel; Type: TABLE; Schema: product; Owner: -
--

CREATE TABLE product.productmodel (
    prmoid integer DEFAULT nextval('product.productmodel_prmoid_seq'::regclass) NOT NULL,
    prbrid smallint NOT NULL,
    name character varying(150) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT productmodel_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: accountingplan; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.accountingplan (
    account character varying(255) NOT NULL,
    name character varying(255),
    fatheraccount character varying(255),
    commentary character varying(500),
    kind character varying(3),
    status boolean DEFAULT true,
    aetyid character(2),
    rcd boolean,
    level smallint NOT NULL
);


--
-- Name: address_addrid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.address_addrid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: agency_agenid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.agency_agenid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: agency; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.agency (
    agenid smallint DEFAULT nextval('public.agency_agenid_seq'::regclass) NOT NULL,
    zoneid smallint NOT NULL,
    code character(3),
    name character varying(255) NOT NULL,
    startdate date DEFAULT CURRENT_DATE NOT NULL,
    phonenumber character(13),
    resiid integer,
    color character(9),
    enabled boolean DEFAULT true NOT NULL,
    workid integer,
    status boolean DEFAULT true NOT NULL,
    haswhatsapp boolean DEFAULT false,
    vaultlimit numeric DEFAULT 0,
    vaultworkid integer,
    vaultalertworkid integer,
    serialnumber character varying(5),
    email character varying(255),
    CONSTRAINT agency_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: bankaccounts_baacid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.bankaccounts_baacid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: bankaccounts; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.bankaccounts (
    baacid smallint DEFAULT nextval('public.bankaccounts_baacid_seq'::regclass) NOT NULL,
    bankid smallint NOT NULL,
    coinid smallint NOT NULL,
    account character varying(30) NOT NULL,
    interbankaccount character varying(30) NOT NULL,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    status boolean DEFAULT true NOT NULL,
    persid integer,
    compid integer
);


--
-- Name: baucher_baucid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.baucher_baucid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: baucheritems_baitid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.baucheritems_baitid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: client_clieid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.client_clieid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: client; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.client (
    clieid integer DEFAULT nextval('public.client_clieid_seq'::regclass) NOT NULL,
    enstid character(2) DEFAULT 'AC'::bpchar NOT NULL,
    persid integer,
    compid integer,
    partid integer,
    code character(15),
    availablecredit numeric(12,3) DEFAULT 0 NOT NULL,
    username character varying(50),
    password character varying(32),
    anaid integer,
    agenid smallint NOT NULL,
    workid integer NOT NULL,
    origin character(1) DEFAULT 'I'::bpchar NOT NULL,
    startdate date DEFAULT CURRENT_DATE NOT NULL,
    status boolean DEFAULT true NOT NULL,
    registernumber integer,
    blacklist boolean DEFAULT false,
    partner boolean DEFAULT true,
    clientnumber integer,
    risk smallint DEFAULT 0,
    CONSTRAINT check_one_non_null CHECK ((((persid IS NOT NULL) AND (compid IS NULL) AND (partid IS NULL)) OR ((persid IS NULL) AND (compid IS NOT NULL) AND (partid IS NULL)) OR ((persid IS NULL) AND (compid IS NULL) AND (partid IS NOT NULL)))),
    CONSTRAINT client_availablecredit_check CHECK ((availablecredit >= (0)::numeric)),
    CONSTRAINT client_origin_check CHECK (((origin)::text = ANY (ARRAY['I'::text, 'W'::text])))
);


--
-- Name: company_compid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.company_compid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: company; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.company (
    compid integer DEFAULT nextval('public.company_compid_seq'::regclass) NOT NULL,
    enstid character(2) DEFAULT 'PA'::bpchar NOT NULL,
    name character varying(200) NOT NULL,
    ruc character varying(11) NOT NULL,
    tradename character varying(200),
    tptlid smallint NOT NULL,
    coslid smallint NOT NULL,
    coclid smallint NOT NULL,
    operationstart date,
    numberworkers smallint,
    observation text,
    anaid integer,
    recorderid integer,
    registrationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    registrationtime time without time zone DEFAULT CURRENT_TIME NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT company_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: costcenter_coceid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.costcenter_coceid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: costcenter; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.costcenter (
    coceid integer DEFAULT nextval('public.costcenter_coceid_seq'::regclass) NOT NULL,
    name character varying(150) NOT NULL,
    father_coceid integer,
    status boolean DEFAULT true NOT NULL,
    isexpensive boolean DEFAULT false,
    workid integer NOT NULL,
    prcoid integer,
    manager integer,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    code character varying(20),
    description text,
    color character(8)
);


--
-- Name: country_counid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.country_counid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: country; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.country (
    counid smallint DEFAULT nextval('public.country_counid_seq'::regclass) NOT NULL,
    name character varying(50) NOT NULL,
    demonym character varying(50),
    phoneprefix character varying(7),
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT country_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: department_depaid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.department_depaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: department; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.department (
    depaid smallint DEFAULT nextval('public.department_depaid_seq'::regclass) NOT NULL,
    counid smallint NOT NULL,
    name character varying(100) NOT NULL,
    isocode character varying(3),
    phoneprefix character varying(3),
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT department_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: district_distid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.district_distid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: district; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.district (
    distid integer DEFAULT nextval('public.district_distid_seq'::regclass) NOT NULL,
    provid integer NOT NULL,
    name character varying(100) NOT NULL,
    ubigeo character(6) NOT NULL,
    latitude numeric(15,12),
    longitude numeric(15,12),
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT district_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: job_jobid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.job_jobid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: job; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.job (
    jobid smallint DEFAULT nextval('public.job_jobid_seq'::regclass) NOT NULL,
    joarid smallint,
    name character varying(100) NOT NULL,
    jocaid smallint,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    status boolean DEFAULT true NOT NULL,
    cansignature boolean DEFAULT false,
    canarrearsdiscount boolean DEFAULT false,
    CONSTRAINT job_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: jobarea_joarid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.jobarea_joarid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: jobarea; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.jobarea (
    joarid smallint DEFAULT nextval('public.jobarea_joarid_seq'::regclass) NOT NULL,
    name character varying(100) NOT NULL,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT jobarea_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: payments_paymid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.payments_paymid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: payments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.payments (
    paymid integer DEFAULT nextval('public.payments_paymid_seq'::regclass) NOT NULL,
    amount numeric(11,3) NOT NULL,
    coinid smallint NOT NULL,
    paydate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    checknumber character varying(25),
    operationcode character varying(25),
    targetbaacid integer,
    chargebaacid integer,
    workid integer NOT NULL,
    windid integer,
    agenid integer NOT NULL,
    recaid smallint,
    status boolean DEFAULT true NOT NULL,
    validated boolean DEFAULT true NOT NULL,
    validatedworkid integer,
    note character varying,
    persid integer,
    CONSTRAINT payments_amount_check CHECK ((amount >= (0)::numeric))
);


--
-- Name: person_persid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.person_persid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: person; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.person (
    persid integer DEFAULT nextval('public.person_persid_seq'::regclass) NOT NULL,
    enstid character(2) DEFAULT 'PA'::bpchar NOT NULL,
    iddoid smallint NOT NULL,
    document character varying(15) NOT NULL,
    fln character varying(50) NOT NULL,
    mln character varying(50),
    name character varying(100) NOT NULL,
    sex character(1) NOT NULL,
    birthdate date NOT NULL,
    cistid smallint,
    nationality smallint,
    peruvian boolean,
    birthplace integer,
    sons smallint,
    observation text,
    imagepath text,
    anaid integer,
    recorderid integer,
    registrationdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    registrationtime time without time zone DEFAULT CURRENT_TIME NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT person_document_check CHECK (((document)::text <> ''::text)),
    CONSTRAINT person_fln_check CHECK (((fln)::text <> ''::text)),
    CONSTRAINT person_name_check CHECK (((name)::text <> ''::text)),
    CONSTRAINT person_sex_check CHECK (((sex)::text = ANY (ARRAY['M'::text, 'F'::text, 'O'::text, '-'::text, 'U'::text]))),
    CONSTRAINT person_sons_check CHECK ((sons >= 0))
);


--
-- Name: provider_provid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.provider_provid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: provider; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.provider (
    provid integer DEFAULT nextval('public.provider_provid_seq'::regclass) NOT NULL,
    persid integer,
    compid integer,
    code character varying(15),
    username character varying(25),
    password character varying(32),
    workid integer NOT NULL,
    startdate date DEFAULT CURRENT_DATE,
    status boolean DEFAULT true,
    note character varying,
    enstid character(2) DEFAULT 'AC'::bpchar
);


--
-- Name: province_provid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.province_provid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: province; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.province (
    provid integer DEFAULT nextval('public.province_provid_seq'::regclass) NOT NULL,
    depaid smallint NOT NULL,
    name character varying(100) NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT province_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: residence; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.residence (
    resiid integer DEFAULT nextval('public.address_addrid_seq'::regclass) NOT NULL,
    persid integer,
    compid integer,
    partid integer,
    distid integer NOT NULL,
    zotyid smallint,
    zonename character varying(255),
    rotyid smallint,
    roadname character varying(255),
    nro character varying(10),
    km character varying(10),
    mz character varying(10),
    inside character varying(10),
    flat character varying(10),
    lot character varying(10),
    address character varying(255),
    reference character varying(255),
    waterbill character varying(30),
    lightbill character varying(30),
    latitude numeric(15,12),
    longitude numeric(15,12),
    main boolean DEFAULT true NOT NULL,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    registrationtime time without time zone DEFAULT CURRENT_TIME NOT NULL,
    status boolean DEFAULT true NOT NULL,
    wamaid smallint,
    flmaid smallint,
    romaid smallint,
    retyid smallint,
    cobaid smallint,
    owreid character(2),
    adtyid integer
);


--
-- Name: sale_saleid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.sale_saleid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: sale; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.sale (
    saleid integer DEFAULT nextval('public.sale_saleid_seq'::regclass) NOT NULL,
    clieid integer NOT NULL,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    dotyid smallint NOT NULL,
    coinid smallint NOT NULL,
    serialnumber character varying(5) NOT NULL,
    correlativenumber integer NOT NULL,
    persid integer,
    subtotal numeric(12,3),
    igv numeric(12,3),
    total numeric(12,3),
    workid integer NOT NULL,
    sellerid integer NOT NULL,
    resiid integer,
    agenid integer NOT NULL,
    windid integer NOT NULL,
    scpaid integer,
    statid character(2) DEFAULT 'AC'::bpchar,
    payed boolean DEFAULT false,
    description character varying(255),
    documentdate timestamp without time zone,
    sunat character(2) DEFAULT 'NS'::bpchar,
    sunatanswer text DEFAULT 'NS'::text,
    smallbox boolean DEFAULT false,
    note character varying,
    coceid integer,
    cowoid integer,
    chliid integer,
    payeddate date
);


--
-- Name: saleitems_saitid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.saleitems_saitid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: saleitems; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.saleitems (
    saitid integer DEFAULT nextval('public.saleitems_saitid_seq'::regclass) NOT NULL,
    saleid integer,
    prodid integer NOT NULL,
    description character varying,
    qty numeric(12,3) DEFAULT 1,
    unitprice numeric(12,3),
    total numeric(12,3),
    discount numeric(12,3),
    "position" smallint,
    tax numeric(12,3) DEFAULT 0
);


--
-- Name: salepayments_sapaid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.salepayments_sapaid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: salepayments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.salepayments (
    sapaid integer DEFAULT nextval('public.salepayments_sapaid_seq'::regclass) NOT NULL,
    saleid integer NOT NULL,
    paymid integer NOT NULL
);


--
-- Name: voucher; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.voucher (
    voucid integer DEFAULT nextval('public.baucher_baucid_seq'::regclass) NOT NULL,
    clieid integer,
    loscid integer,
    satrid integer,
    contid integer,
    inscid integer,
    coinid integer NOT NULL,
    windid integer NOT NULL,
    workid integer NOT NULL,
    correlativenumber integer NOT NULL,
    paymentdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    registerdate timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    statid character(2) DEFAULT 'AC'::bpchar,
    total numeric NOT NULL,
    serialnumber character(2),
    loanid integer,
    operationnumber smallint DEFAULT 0 NOT NULL,
    saleid integer,
    purcid integer,
    provid integer,
    note text,
    payment_type character(2) DEFAULT 'EF'::bpchar,
    mmtyid integer,
    updater_workid integer,
    latitude numeric(11,8),
    longitude numeric(11,8),
    CONSTRAINT baucher_total_check CHECK ((total >= (0)::numeric))
);


--
-- Name: voucheritems; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.voucheritems (
    voitid integer DEFAULT nextval('public.baucheritems_baitid_seq'::regclass) NOT NULL,
    voucid integer NOT NULL,
    prodid integer NOT NULL,
    description character varying,
    total numeric NOT NULL,
    CONSTRAINT baucheritems_total_check CHECK ((total >= (0)::numeric))
);


--
-- Name: voucherpayments; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.voucherpayments (
    voucid integer NOT NULL,
    paymid integer NOT NULL
);


--
-- Name: windows_windid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.windows_windid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: windows; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.windows (
    windid integer DEFAULT nextval('public.windows_windid_seq'::regclass) NOT NULL,
    agenid smallint NOT NULL,
    name character varying(255) NOT NULL,
    workid integer NOT NULL,
    main boolean DEFAULT false NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    status boolean DEFAULT true NOT NULL,
    locked boolean DEFAULT false,
    issmall boolean DEFAULT false,
    active boolean DEFAULT false,
    isworkerbox boolean DEFAULT false,
    CONSTRAINT windows_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: worker_workid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.worker_workid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: worker; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.worker (
    workid integer DEFAULT nextval('public.worker_workid_seq'::regclass) NOT NULL,
    wenstid character(2) DEFAULT 'AC'::bpchar NOT NULL,
    persid integer NOT NULL,
    jobid smallint NOT NULL,
    startdate date DEFAULT CURRENT_DATE NOT NULL,
    username character varying(25),
    password character varying(32),
    webreception boolean DEFAULT false NOT NULL,
    wrecorderid integer,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    registrationtime time without time zone DEFAULT CURRENT_TIME NOT NULL,
    status boolean DEFAULT true NOT NULL,
    email character varying(255),
    locked boolean DEFAULT false,
    CONSTRAINT worker_username_password_check CHECK ((((username IS NULL) AND (password IS NULL)) OR ((username IS NOT NULL) AND (password IS NOT NULL))))
);


--
-- Name: zone_zoneid_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.zone_zoneid_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- Name: zone; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.zone (
    zoneid smallint DEFAULT nextval('public.zone_zoneid_seq'::regclass) NOT NULL,
    name character varying(255) NOT NULL,
    color character(9),
    enabled boolean DEFAULT true NOT NULL,
    workid integer,
    registrationdate date DEFAULT CURRENT_DATE NOT NULL,
    status boolean DEFAULT true NOT NULL,
    CONSTRAINT zone_name_check CHECK (((name)::text <> ''::text))
);


--
-- Name: managed_vehicle mv_id; Type: DEFAULT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.managed_vehicle ALTER COLUMN mv_id SET DEFAULT nextval('maintenance.managed_vehicle_mv_id_seq'::regclass);


--
-- Name: vehicle_allowed_action vaacid; Type: DEFAULT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_action ALTER COLUMN vaacid SET DEFAULT nextval('maintenance.vehicle_allowed_action_vaacid_seq'::regclass);


--
-- Name: vehicle_allowed_component vacoid; Type: DEFAULT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_component ALTER COLUMN vacoid SET DEFAULT nextval('maintenance.vehicle_allowed_component_vacoid_seq'::regclass);


--
-- Name: vehicle_allowed_material vamid; Type: DEFAULT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_material ALTER COLUMN vamid SET DEFAULT nextval('maintenance.vehicle_allowed_material_vamid_seq'::regclass);


--
-- Name: vehicle prcoid; Type: DEFAULT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle ALTER COLUMN prcoid SET DEFAULT nextval('product.company_prcoid_seq'::regclass);


--
-- Name: vehicle qty; Type: DEFAULT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle ALTER COLUMN qty SET DEFAULT 1;


--
-- Name: vehicle status; Type: DEFAULT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle ALTER COLUMN status SET DEFAULT true;


--
-- Name: vehicle registerdate; Type: DEFAULT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle ALTER COLUMN registerdate SET DEFAULT CURRENT_TIMESTAMP;


--
-- Name: vehicle prstid; Type: DEFAULT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle ALTER COLUMN prstid SET DEFAULT 'A'::bpchar;


--
-- Name: worker worker_pkey; Type: CONSTRAINT; Schema: company; Owner: -
--

ALTER TABLE ONLY company.worker
    ADD CONSTRAINT worker_pkey PRIMARY KEY (cowoid);


--
-- Name: accountingplanelementtype accountingplanelementtype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.accountingplanelementtype
    ADD CONSTRAINT accountingplanelementtype_pkey PRIMARY KEY (aetyid);


--
-- Name: banks banks_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.banks
    ADD CONSTRAINT banks_name_key UNIQUE (name);


--
-- Name: banks banks_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.banks
    ADD CONSTRAINT banks_pkey PRIMARY KEY (bankid);


--
-- Name: civilstatus civilstatus_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.civilstatus
    ADD CONSTRAINT civilstatus_name_key UNIQUE (name);


--
-- Name: civilstatus civilstatus_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.civilstatus
    ADD CONSTRAINT civilstatus_pkey PRIMARY KEY (cistid);


--
-- Name: coin coin_isocode_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.coin
    ADD CONSTRAINT coin_isocode_key UNIQUE (isocode);


--
-- Name: coin coin_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.coin
    ADD CONSTRAINT coin_name_key UNIQUE (name);


--
-- Name: coin coin_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.coin
    ADD CONSTRAINT coin_pkey PRIMARY KEY (coinid);


--
-- Name: companyconditionlist companyconditionlist_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.companyconditionlist
    ADD CONSTRAINT companyconditionlist_name_key UNIQUE (name);


--
-- Name: companyconditionlist companyconditionlist_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.companyconditionlist
    ADD CONSTRAINT companyconditionlist_pkey PRIMARY KEY (coclid);


--
-- Name: companystatuslist companystatuslist_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.companystatuslist
    ADD CONSTRAINT companystatuslist_name_key UNIQUE (name);


--
-- Name: companystatuslist companystatuslist_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.companystatuslist
    ADD CONSTRAINT companystatuslist_pkey PRIMARY KEY (coslid);


--
-- Name: documentlist documentlist_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documentlist
    ADD CONSTRAINT documentlist_name_key UNIQUE (name);


--
-- Name: documentlist documentlist_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documentlist
    ADD CONSTRAINT documentlist_pkey PRIMARY KEY (doliid);


--
-- Name: documenttype documenttype_code_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documenttype
    ADD CONSTRAINT documenttype_code_key UNIQUE (code);


--
-- Name: documenttype documenttype_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documenttype
    ADD CONSTRAINT documenttype_name_key UNIQUE (name);


--
-- Name: documenttype documenttype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documenttype
    ADD CONSTRAINT documenttype_pkey PRIMARY KEY (dotyid);


--
-- Name: entitystatus entitystatus_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.entitystatus
    ADD CONSTRAINT entitystatus_name_key UNIQUE (name);


--
-- Name: entitystatus entitystatus_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.entitystatus
    ADD CONSTRAINT entitystatus_pkey PRIMARY KEY (enstid);


--
-- Name: fueltype fueltype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.fueltype
    ADD CONSTRAINT fueltype_pkey PRIMARY KEY (futyid);


--
-- Name: identitydocumenttype identitydocumenttype_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.identitydocumenttype
    ADD CONSTRAINT identitydocumenttype_name_key UNIQUE (name);


--
-- Name: identitydocumenttype identitydocumenttype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.identitydocumenttype
    ADD CONSTRAINT identitydocumenttype_pkey PRIMARY KEY (iddoid);


--
-- Name: jobcategory jobcategory_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.jobcategory
    ADD CONSTRAINT jobcategory_name_key UNIQUE (name);


--
-- Name: jobcategory jobcategory_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.jobcategory
    ADD CONSTRAINT jobcategory_pkey PRIMARY KEY (jocaid);


--
-- Name: moneymovetype moneymovetype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.moneymovetype
    ADD CONSTRAINT moneymovetype_pkey PRIMARY KEY (mmtyid);


--
-- Name: producttype producttype_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.producttype
    ADD CONSTRAINT producttype_name_key UNIQUE (name);


--
-- Name: producttype producttype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.producttype
    ADD CONSTRAINT producttype_pkey PRIMARY KEY (prtyid);


--
-- Name: reasonforcancellation reasonforcancellation_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.reasonforcancellation
    ADD CONSTRAINT reasonforcancellation_pkey PRIMARY KEY (recaid);


--
-- Name: status status_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.status
    ADD CONSTRAINT status_pkey PRIMARY KEY (statid);


--
-- Name: sunatelectronicanswer sunatelectronicanswer_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.sunatelectronicanswer
    ADD CONSTRAINT sunatelectronicanswer_pkey PRIMARY KEY (suea);


--
-- Name: taxpayertypelist taxpayertypelist_name_key; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.taxpayertypelist
    ADD CONSTRAINT taxpayertypelist_name_key UNIQUE (name);


--
-- Name: taxpayertypelist taxpayertypelist_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.taxpayertypelist
    ADD CONSTRAINT taxpayertypelist_pkey PRIMARY KEY (tptlid);


--
-- Name: vehicletype vehicletype_pkey; Type: CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.vehicletype
    ADD CONSTRAINT vehicletype_pkey PRIMARY KEY (vetyid);


--
-- Name: action_catalog action_catalog_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.action_catalog
    ADD CONSTRAINT action_catalog_pkey PRIMARY KEY (acatid);


--
-- Name: action_list_type action_list_type_name_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.action_list_type
    ADD CONSTRAINT action_list_type_name_unique UNIQUE (name);


--
-- Name: action_list_type action_list_type_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.action_list_type
    ADD CONSTRAINT action_list_type_pkey PRIMARY KEY (altoid);


--
-- Name: alert_config alert_config_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_config
    ADD CONSTRAINT alert_config_pkey PRIMARY KEY (alcoid);


--
-- Name: alert_config alert_config_type_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_config
    ADD CONSTRAINT alert_config_type_unique UNIQUE (alert_type);


--
-- Name: alert_log alert_log_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT alert_log_pkey PRIMARY KEY (alloid);


--
-- Name: config_system config_system_key_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.config_system
    ADD CONSTRAINT config_system_key_unique UNIQUE (key);


--
-- Name: config_system config_system_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.config_system
    ADD CONSTRAINT config_system_pkey PRIMARY KEY (cosyid);


--
-- Name: diagnosis diagnosis_mainid_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.diagnosis
    ADD CONSTRAINT diagnosis_mainid_unique UNIQUE (mainid);


--
-- Name: diagnosis diagnosis_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.diagnosis
    ADD CONSTRAINT diagnosis_pkey PRIMARY KEY (diagid);


--
-- Name: installed_component installed_component_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT installed_component_pkey PRIMARY KEY (incoid);


--
-- Name: maintenance_action_detail maintenance_action_detail_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_action_detail
    ADD CONSTRAINT maintenance_action_detail_pkey PRIMARY KEY (madeid);


--
-- Name: maintenance maintenance_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_pkey PRIMARY KEY (mainid);


--
-- Name: maintenance_type maintenance_type_name_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_type
    ADD CONSTRAINT maintenance_type_name_unique UNIQUE (name);


--
-- Name: maintenance_type maintenance_type_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_type
    ADD CONSTRAINT maintenance_type_pkey PRIMARY KEY (matyid);


--
-- Name: managed_vehicle managed_vehicle_license_plate_key; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.managed_vehicle
    ADD CONSTRAINT managed_vehicle_license_plate_key UNIQUE (license_plate);


--
-- Name: managed_vehicle managed_vehicle_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.managed_vehicle
    ADD CONSTRAINT managed_vehicle_pkey PRIMARY KEY (mv_id);


--
-- Name: managed_vehicle managed_vehicle_prcoid_key; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.managed_vehicle
    ADD CONSTRAINT managed_vehicle_prcoid_key UNIQUE (prcoid);


--
-- Name: material_category material_category_name_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_category
    ADD CONSTRAINT material_category_name_unique UNIQUE (name);


--
-- Name: material_category material_category_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_category
    ADD CONSTRAINT material_category_pkey PRIMARY KEY (macaid);


--
-- Name: material_consumption material_consumption_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_consumption
    ADD CONSTRAINT material_consumption_pkey PRIMARY KEY (macoid);


--
-- Name: material_discard material_discard_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_discard
    ADD CONSTRAINT material_discard_pkey PRIMARY KEY (madiid);


--
-- Name: material_lot material_lot_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_lot
    ADD CONSTRAINT material_lot_pkey PRIMARY KEY (maloid);


--
-- Name: material material_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material
    ADD CONSTRAINT material_pkey PRIMARY KEY (mateid);


--
-- Name: material_rating material_rating_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_rating
    ADD CONSTRAINT material_rating_pkey PRIMARY KEY (matraid);


--
-- Name: schedule_action schedule_action_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.schedule_action
    ADD CONSTRAINT schedule_action_pkey PRIMARY KEY (shacid);


--
-- Name: service_type service_type_code_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.service_type
    ADD CONSTRAINT service_type_code_unique UNIQUE (code);


--
-- Name: service_type service_type_name_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.service_type
    ADD CONSTRAINT service_type_name_unique UNIQUE (name);


--
-- Name: service_type service_type_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.service_type
    ADD CONSTRAINT service_type_pkey PRIMARY KEY (setyid);


--
-- Name: technician_assignment ta_mainid_workid_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.technician_assignment
    ADD CONSTRAINT ta_mainid_workid_unique UNIQUE (mainid, workid);


--
-- Name: technician_assignment technician_assignment_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.technician_assignment
    ADD CONSTRAINT technician_assignment_pkey PRIMARY KEY (teasid);


--
-- Name: vehicle_allowed_action vehicle_allowed_action_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_action
    ADD CONSTRAINT vehicle_allowed_action_pkey PRIMARY KEY (vaacid);


--
-- Name: vehicle_allowed_component vehicle_allowed_component_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_component
    ADD CONSTRAINT vehicle_allowed_component_pkey PRIMARY KEY (vacoid);


--
-- Name: vehicle_allowed_material vehicle_allowed_material_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_material
    ADD CONSTRAINT vehicle_allowed_material_pkey PRIMARY KEY (vamid);


--
-- Name: vehicle_schedule vehicle_schedule_pkey; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_schedule
    ADD CONSTRAINT vehicle_schedule_pkey PRIMARY KEY (veshid);


--
-- Name: vehicle_schedule vehicle_schedule_prcoid_unique; Type: CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_schedule
    ADD CONSTRAINT vehicle_schedule_prcoid_unique UNIQUE (prcoid);


--
-- Name: company company_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.company
    ADD CONSTRAINT company_pkey PRIMARY KEY (prcoid);


--
-- Name: productbrand productbrand_name_key; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productbrand
    ADD CONSTRAINT productbrand_name_key UNIQUE (name);


--
-- Name: productbrand productbrand_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productbrand
    ADD CONSTRAINT productbrand_pkey PRIMARY KEY (prbrid);


--
-- Name: productcategory productcategory_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productcategory
    ADD CONSTRAINT productcategory_pkey PRIMARY KEY (prcaid);


--
-- Name: productmerchandise productmerchandise_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmerchandise
    ADD CONSTRAINT productmerchandise_pkey PRIMARY KEY (prmeid);


--
-- Name: productmerchandise productmerchandise_prodid_unique; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmerchandise
    ADD CONSTRAINT productmerchandise_prodid_unique UNIQUE (prodid);


--
-- Name: productmodel productmodel_name_key; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmodel
    ADD CONSTRAINT productmodel_name_key UNIQUE (name);


--
-- Name: productmodel productmodel_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmodel
    ADD CONSTRAINT productmodel_pkey PRIMARY KEY (prmoid);


--
-- Name: vehicle vehicle_pkey; Type: CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle
    ADD CONSTRAINT vehicle_pkey PRIMARY KEY (prcoid);


--
-- Name: accountingplan accountingplan_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.accountingplan
    ADD CONSTRAINT accountingplan_pkey PRIMARY KEY (account);


--
-- Name: residence address_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.residence
    ADD CONSTRAINT address_pkey PRIMARY KEY (resiid);


--
-- Name: agency agency_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_pkey PRIMARY KEY (agenid);


--
-- Name: bankaccounts bankaccounts_bankid_account_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_bankid_account_key UNIQUE (bankid, account);


--
-- Name: bankaccounts bankaccounts_interbankaccount_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_interbankaccount_key UNIQUE (interbankaccount);


--
-- Name: bankaccounts bankaccounts_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_pkey PRIMARY KEY (baacid);


--
-- Name: voucher baucher_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT baucher_pkey PRIMARY KEY (voucid);


--
-- Name: client client_compid_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_compid_key UNIQUE (compid);


--
-- Name: client client_partid_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_partid_key UNIQUE (partid);


--
-- Name: client client_persid_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_persid_key UNIQUE (persid);


--
-- Name: client client_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_pkey PRIMARY KEY (clieid);


--
-- Name: client client_username_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_username_key UNIQUE (username);


--
-- Name: company company_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_name_key UNIQUE (name);


--
-- Name: company company_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_pkey PRIMARY KEY (compid);


--
-- Name: company company_ruc_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_ruc_key UNIQUE (ruc);


--
-- Name: company company_tradename_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_tradename_key UNIQUE (tradename);


--
-- Name: costcenter costcenter_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.costcenter
    ADD CONSTRAINT costcenter_pkey PRIMARY KEY (coceid);


--
-- Name: country country_demonym_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.country
    ADD CONSTRAINT country_demonym_key UNIQUE (demonym);


--
-- Name: country country_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.country
    ADD CONSTRAINT country_name_key UNIQUE (name);


--
-- Name: country country_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.country
    ADD CONSTRAINT country_pkey PRIMARY KEY (counid);


--
-- Name: department department_counid_name_unique; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.department
    ADD CONSTRAINT department_counid_name_unique UNIQUE (counid, name);


--
-- Name: department department_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.department
    ADD CONSTRAINT department_pkey PRIMARY KEY (depaid);


--
-- Name: district district_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.district
    ADD CONSTRAINT district_pkey PRIMARY KEY (distid);


--
-- Name: district district_provid_name_unique; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.district
    ADD CONSTRAINT district_provid_name_unique UNIQUE (provid, name);


--
-- Name: job job_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.job
    ADD CONSTRAINT job_name_key UNIQUE (name);


--
-- Name: job job_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.job
    ADD CONSTRAINT job_pkey PRIMARY KEY (jobid);


--
-- Name: jobarea jobarea_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.jobarea
    ADD CONSTRAINT jobarea_name_key UNIQUE (name);


--
-- Name: jobarea jobarea_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.jobarea
    ADD CONSTRAINT jobarea_pkey PRIMARY KEY (joarid);


--
-- Name: payments payments_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_pkey PRIMARY KEY (paymid);


--
-- Name: person person_document_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_document_key UNIQUE (document);


--
-- Name: person person_imagepath_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_imagepath_key UNIQUE (imagepath);


--
-- Name: person person_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_pkey PRIMARY KEY (persid);


--
-- Name: product product_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_pkey PRIMARY KEY (prodid);


--
-- Name: provider provider_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.provider
    ADD CONSTRAINT provider_pkey PRIMARY KEY (provid);


--
-- Name: province province_depaid_name_unique; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.province
    ADD CONSTRAINT province_depaid_name_unique UNIQUE (depaid, name);


--
-- Name: province province_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.province
    ADD CONSTRAINT province_pkey PRIMARY KEY (provid);


--
-- Name: sale sale_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_pkey PRIMARY KEY (saleid);


--
-- Name: saleitems saleitems_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.saleitems
    ADD CONSTRAINT saleitems_pkey PRIMARY KEY (saitid);


--
-- Name: salepayments salepayments_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.salepayments
    ADD CONSTRAINT salepayments_pkey PRIMARY KEY (sapaid);


--
-- Name: agency unique_name; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT unique_name UNIQUE (name);


--
-- Name: voucheritems voucheritems_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucheritems
    ADD CONSTRAINT voucheritems_pkey PRIMARY KEY (voitid);


--
-- Name: voucherpayments voucherpayments_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucherpayments
    ADD CONSTRAINT voucherpayments_pkey PRIMARY KEY (voucid, paymid);


--
-- Name: windows windows_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.windows
    ADD CONSTRAINT windows_pkey PRIMARY KEY (windid);


--
-- Name: worker worker_persid_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_persid_key UNIQUE (persid);


--
-- Name: worker worker_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_pkey PRIMARY KEY (workid);


--
-- Name: worker worker_username_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_username_key UNIQUE (username);


--
-- Name: zone zone_name_key; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.zone
    ADD CONSTRAINT zone_name_key UNIQUE (name);


--
-- Name: zone zone_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.zone
    ADD CONSTRAINT zone_pkey PRIMARY KEY (zoneid);


--
-- Name: rentexecute rentexecute_pkey; Type: CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_pkey PRIMARY KEY (seexid);


--
-- Name: rentrequest rentrequest_pkey; Type: CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_pkey PRIMARY KEY (sereid);


--
-- Name: idx_alert_date; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_alert_date ON maintenance.alert_log USING btree (alert_date);


--
-- Name: idx_alert_prcoid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_alert_prcoid ON maintenance.alert_log USING btree (prcoid);


--
-- Name: idx_alert_unread; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_alert_unread ON maintenance.alert_log USING btree (read) WHERE (read = false);


--
-- Name: idx_consumption_mainid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_consumption_mainid ON maintenance.material_consumption USING btree (mainid);


--
-- Name: idx_consumption_maloid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_consumption_maloid ON maintenance.material_consumption USING btree (maloid);


--
-- Name: idx_consumption_mateid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_consumption_mateid ON maintenance.material_consumption USING btree (mateid);


--
-- Name: idx_ic_acatid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_ic_acatid ON maintenance.installed_component USING btree (acatid);


--
-- Name: idx_ic_expiration; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_ic_expiration ON maintenance.installed_component USING btree (expiration_date) WHERE ((active = true) AND (expiration_date IS NOT NULL));


--
-- Name: idx_ic_prcoid_active; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_ic_prcoid_active ON maintenance.installed_component USING btree (prcoid) WHERE (active = true);


--
-- Name: idx_lot_expiration; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_lot_expiration ON maintenance.material_lot USING btree (expiration_date) WHERE ((lot_status)::text = 'activo'::text);


--
-- Name: idx_lot_mateid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_lot_mateid ON maintenance.material_lot USING btree (mateid);


--
-- Name: idx_lot_status_mateid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_lot_status_mateid ON maintenance.material_lot USING btree (mateid, lot_status);


--
-- Name: idx_maintenance_assigned_date; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_assigned_date ON maintenance.maintenance USING btree (assigned_to, maintenance_date DESC);


--
-- Name: idx_maintenance_assigned_to; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_assigned_to ON maintenance.maintenance USING btree (assigned_to);


--
-- Name: idx_maintenance_date; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_date ON maintenance.maintenance USING btree (maintenance_date);


--
-- Name: idx_maintenance_matyid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_matyid ON maintenance.maintenance USING btree (matyid);


--
-- Name: idx_maintenance_prcoid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_prcoid ON maintenance.maintenance USING btree (prcoid);


--
-- Name: idx_maintenance_prcoid_date; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_prcoid_date ON maintenance.maintenance USING btree (prcoid, maintenance_date);


--
-- Name: idx_maintenance_statid_matyid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_maintenance_statid_matyid ON maintenance.maintenance USING btree (statid, matyid);


--
-- Name: idx_material_macaid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_material_macaid ON maintenance.material USING btree (macaid);


--
-- Name: idx_rating_mainid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_rating_mainid ON maintenance.material_rating USING btree (mainid);


--
-- Name: idx_rating_mateid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_rating_mateid ON maintenance.material_rating USING btree (mateid);


--
-- Name: idx_ta_mainid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_ta_mainid ON maintenance.technician_assignment USING btree (mainid);


--
-- Name: idx_ta_workid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_ta_workid ON maintenance.technician_assignment USING btree (workid);


--
-- Name: idx_vehicle_allowed_action_prcoid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_vehicle_allowed_action_prcoid ON maintenance.vehicle_allowed_action USING btree (prcoid);


--
-- Name: idx_vehicle_allowed_component_prcoid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_vehicle_allowed_component_prcoid ON maintenance.vehicle_allowed_component USING btree (prcoid);


--
-- Name: idx_vehicle_allowed_material_prcoid; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE INDEX idx_vehicle_allowed_material_prcoid ON maintenance.vehicle_allowed_material USING btree (prcoid);


--
-- Name: uq_vaa_legacy; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vaa_legacy ON maintenance.vehicle_allowed_action USING btree (prcoid, acatid) WHERE ((prcoid IS NOT NULL) AND (mv_id IS NULL));


--
-- Name: uq_vaa_managed; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vaa_managed ON maintenance.vehicle_allowed_action USING btree (mv_id, acatid) WHERE (mv_id IS NOT NULL);


--
-- Name: uq_vac_legacy; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vac_legacy ON maintenance.vehicle_allowed_component USING btree (prcoid, acatid) WHERE ((prcoid IS NOT NULL) AND (mv_id IS NULL));


--
-- Name: uq_vac_managed; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vac_managed ON maintenance.vehicle_allowed_component USING btree (mv_id, acatid) WHERE (mv_id IS NOT NULL);


--
-- Name: uq_vam_legacy; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vam_legacy ON maintenance.vehicle_allowed_material USING btree (prcoid, mateid) WHERE ((prcoid IS NOT NULL) AND (mv_id IS NULL));


--
-- Name: uq_vam_managed; Type: INDEX; Schema: maintenance; Owner: -
--

CREATE UNIQUE INDEX uq_vam_managed ON maintenance.vehicle_allowed_material USING btree (mv_id, mateid) WHERE (mv_id IS NOT NULL);


--
-- Name: worker worker_compid_fkey; Type: FK CONSTRAINT; Schema: company; Owner: -
--

ALTER TABLE ONLY company.worker
    ADD CONSTRAINT worker_compid_fkey FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: worker worker_persid_fkey; Type: FK CONSTRAINT; Schema: company; Owner: -
--

ALTER TABLE ONLY company.worker
    ADD CONSTRAINT worker_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: documenttype documenttype_doliid_fkey; Type: FK CONSTRAINT; Schema: list; Owner: -
--

ALTER TABLE ONLY list.documenttype
    ADD CONSTRAINT documenttype_doliid_fkey FOREIGN KEY (doliid) REFERENCES list.documentlist(doliid);


--
-- Name: action_catalog action_catalog_altoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.action_catalog
    ADD CONSTRAINT action_catalog_altoid_fkey FOREIGN KEY (altoid) REFERENCES maintenance.action_list_type(altoid);


--
-- Name: alert_log al_alcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_alcoid_fkey FOREIGN KEY (alcoid) REFERENCES maintenance.alert_config(alcoid);


--
-- Name: alert_log al_incoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_incoid_fkey FOREIGN KEY (incoid) REFERENCES maintenance.installed_component(incoid);


--
-- Name: alert_log al_maloid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_maloid_fkey FOREIGN KEY (maloid) REFERENCES maintenance.material_lot(maloid);


--
-- Name: alert_log al_mateid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_mateid_fkey FOREIGN KEY (mateid) REFERENCES maintenance.material(mateid);


--
-- Name: alert_log al_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid);


--
-- Name: alert_log al_read_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_read_by_fkey FOREIGN KEY (read_by) REFERENCES public.worker(workid);


--
-- Name: alert_log al_resolved_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.alert_log
    ADD CONSTRAINT al_resolved_by_fkey FOREIGN KEY (resolved_by) REFERENCES public.worker(workid);


--
-- Name: config_system config_system_updated_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.config_system
    ADD CONSTRAINT config_system_updated_by_fkey FOREIGN KEY (updated_by) REFERENCES public.worker(workid);


--
-- Name: diagnosis diagnosis_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.diagnosis
    ADD CONSTRAINT diagnosis_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: installed_component ic_acatid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT ic_acatid_fkey FOREIGN KEY (acatid) REFERENCES maintenance.action_catalog(acatid);


--
-- Name: installed_component ic_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT ic_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: installed_component ic_maloid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT ic_maloid_fkey FOREIGN KEY (maloid) REFERENCES maintenance.material_lot(maloid);


--
-- Name: installed_component ic_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT ic_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid);


--
-- Name: installed_component ic_replaced_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.installed_component
    ADD CONSTRAINT ic_replaced_by_fkey FOREIGN KEY (replaced_by_incoid) REFERENCES maintenance.installed_component(incoid);


--
-- Name: maintenance_action_detail mad_acatid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_action_detail
    ADD CONSTRAINT mad_acatid_fkey FOREIGN KEY (acatid) REFERENCES maintenance.action_catalog(acatid);


--
-- Name: maintenance_action_detail mad_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_action_detail
    ADD CONSTRAINT mad_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: maintenance_action_detail mad_maloid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance_action_detail
    ADD CONSTRAINT mad_maloid_fkey FOREIGN KEY (maloid) REFERENCES maintenance.material_lot(maloid);


--
-- Name: maintenance maintenance_assigned_to_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_assigned_to_fkey FOREIGN KEY (assigned_to) REFERENCES public.worker(workid);


--
-- Name: maintenance maintenance_matyid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_matyid_fkey FOREIGN KEY (matyid) REFERENCES maintenance.maintenance_type(matyid);


--
-- Name: maintenance maintenance_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid);


--
-- Name: maintenance maintenance_setyid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_setyid_fkey FOREIGN KEY (setyid) REFERENCES maintenance.service_type(setyid);


--
-- Name: maintenance maintenance_statid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_statid_fkey FOREIGN KEY (statid) REFERENCES list.status(statid);


--
-- Name: maintenance maintenance_workid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.maintenance
    ADD CONSTRAINT maintenance_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: managed_vehicle managed_vehicle_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.managed_vehicle
    ADD CONSTRAINT managed_vehicle_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid);


--
-- Name: material material_created_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material
    ADD CONSTRAINT material_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.worker(workid);


--
-- Name: material_lot material_lot_created_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_lot
    ADD CONSTRAINT material_lot_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.worker(workid);


--
-- Name: material_lot material_lot_mateid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_lot
    ADD CONSTRAINT material_lot_mateid_fkey FOREIGN KEY (mateid) REFERENCES maintenance.material(mateid);


--
-- Name: material_lot material_lot_provid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_lot
    ADD CONSTRAINT material_lot_provid_fkey FOREIGN KEY (provid) REFERENCES public.provider(provid);


--
-- Name: material material_macaid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material
    ADD CONSTRAINT material_macaid_fkey FOREIGN KEY (macaid) REFERENCES maintenance.material_category(macaid);


--
-- Name: material_consumption mc_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_consumption
    ADD CONSTRAINT mc_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: material_consumption mc_maloid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_consumption
    ADD CONSTRAINT mc_maloid_fkey FOREIGN KEY (maloid) REFERENCES maintenance.material_lot(maloid);


--
-- Name: material_consumption mc_mateid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_consumption
    ADD CONSTRAINT mc_mateid_fkey FOREIGN KEY (mateid) REFERENCES maintenance.material(mateid);


--
-- Name: material_discard md_discarded_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_discard
    ADD CONSTRAINT md_discarded_by_fkey FOREIGN KEY (discarded_by) REFERENCES public.worker(workid);


--
-- Name: material_discard md_maloid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_discard
    ADD CONSTRAINT md_maloid_fkey FOREIGN KEY (maloid) REFERENCES maintenance.material_lot(maloid);


--
-- Name: material_rating mr_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_rating
    ADD CONSTRAINT mr_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: material_rating mr_mateid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_rating
    ADD CONSTRAINT mr_mateid_fkey FOREIGN KEY (mateid) REFERENCES maintenance.material(mateid);


--
-- Name: material_rating mr_rated_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.material_rating
    ADD CONSTRAINT mr_rated_by_fkey FOREIGN KEY (rated_by) REFERENCES public.worker(workid);


--
-- Name: schedule_action schedule_action_acatid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.schedule_action
    ADD CONSTRAINT schedule_action_acatid_fkey FOREIGN KEY (acatid) REFERENCES maintenance.action_catalog(acatid);


--
-- Name: schedule_action schedule_action_veshid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.schedule_action
    ADD CONSTRAINT schedule_action_veshid_fkey FOREIGN KEY (veshid) REFERENCES maintenance.vehicle_schedule(veshid);


--
-- Name: technician_assignment ta_assigned_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.technician_assignment
    ADD CONSTRAINT ta_assigned_by_fkey FOREIGN KEY (assigned_by) REFERENCES public.worker(workid);


--
-- Name: technician_assignment ta_mainid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.technician_assignment
    ADD CONSTRAINT ta_mainid_fkey FOREIGN KEY (mainid) REFERENCES maintenance.maintenance(mainid) ON DELETE CASCADE;


--
-- Name: technician_assignment ta_workid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.technician_assignment
    ADD CONSTRAINT ta_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: vehicle_allowed_action vehicle_allowed_action_acatid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_action
    ADD CONSTRAINT vehicle_allowed_action_acatid_fkey FOREIGN KEY (acatid) REFERENCES maintenance.action_catalog(acatid) ON DELETE CASCADE;


--
-- Name: vehicle_allowed_action vehicle_allowed_action_mv_id_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_action
    ADD CONSTRAINT vehicle_allowed_action_mv_id_fkey FOREIGN KEY (mv_id) REFERENCES maintenance.managed_vehicle(mv_id);


--
-- Name: vehicle_allowed_action vehicle_allowed_action_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_action
    ADD CONSTRAINT vehicle_allowed_action_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid) ON DELETE CASCADE;


--
-- Name: vehicle_allowed_component vehicle_allowed_component_acatid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_component
    ADD CONSTRAINT vehicle_allowed_component_acatid_fkey FOREIGN KEY (acatid) REFERENCES maintenance.action_catalog(acatid) ON DELETE CASCADE;


--
-- Name: vehicle_allowed_component vehicle_allowed_component_mv_id_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_component
    ADD CONSTRAINT vehicle_allowed_component_mv_id_fkey FOREIGN KEY (mv_id) REFERENCES maintenance.managed_vehicle(mv_id);


--
-- Name: vehicle_allowed_component vehicle_allowed_component_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_component
    ADD CONSTRAINT vehicle_allowed_component_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid) ON DELETE CASCADE;


--
-- Name: vehicle_allowed_material vehicle_allowed_material_mateid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_material
    ADD CONSTRAINT vehicle_allowed_material_mateid_fkey FOREIGN KEY (mateid) REFERENCES maintenance.material(mateid) ON DELETE CASCADE;


--
-- Name: vehicle_allowed_material vehicle_allowed_material_mv_id_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_material
    ADD CONSTRAINT vehicle_allowed_material_mv_id_fkey FOREIGN KEY (mv_id) REFERENCES maintenance.managed_vehicle(mv_id);


--
-- Name: vehicle_allowed_material vehicle_allowed_material_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_allowed_material
    ADD CONSTRAINT vehicle_allowed_material_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid) ON DELETE CASCADE;


--
-- Name: vehicle_schedule vehicle_schedule_created_by_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_schedule
    ADD CONSTRAINT vehicle_schedule_created_by_fkey FOREIGN KEY (created_by) REFERENCES public.worker(workid);


--
-- Name: vehicle_schedule vehicle_schedule_prcoid_fkey; Type: FK CONSTRAINT; Schema: maintenance; Owner: -
--

ALTER TABLE ONLY maintenance.vehicle_schedule
    ADD CONSTRAINT vehicle_schedule_prcoid_fkey FOREIGN KEY (prcoid) REFERENCES product.company(prcoid);


--
-- Name: company company_prodid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.company
    ADD CONSTRAINT company_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: company company_workid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.company
    ADD CONSTRAINT company_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: productmerchandise productmerchandise_prcaid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmerchandise
    ADD CONSTRAINT productmerchandise_prcaid_fkey FOREIGN KEY (prcaid) REFERENCES product.productcategory(prcaid);


--
-- Name: productmerchandise productmerchandise_prmoid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmerchandise
    ADD CONSTRAINT productmerchandise_prmoid_fkey FOREIGN KEY (prmoid) REFERENCES product.productmodel(prmoid);


--
-- Name: productmerchandise productmerchandise_prodid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmerchandise
    ADD CONSTRAINT productmerchandise_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: productmodel productmodel_prbrid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.productmodel
    ADD CONSTRAINT productmodel_prbrid_fkey FOREIGN KEY (prbrid) REFERENCES product.productbrand(prbrid);


--
-- Name: vehicle vehicle_futyid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle
    ADD CONSTRAINT vehicle_futyid_fkey FOREIGN KEY (futyid) REFERENCES list.fueltype(futyid);


--
-- Name: vehicle vehicle_prodid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle
    ADD CONSTRAINT vehicle_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: vehicle vehicle_vetyid_fkey; Type: FK CONSTRAINT; Schema: product; Owner: -
--

ALTER TABLE ONLY product.vehicle
    ADD CONSTRAINT vehicle_vetyid_fkey FOREIGN KEY (vetyid) REFERENCES list.vehicletype(vetyid);


--
-- Name: accountingplan accountingplan_aetyid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.accountingplan
    ADD CONSTRAINT accountingplan_aetyid_fkey FOREIGN KEY (aetyid) REFERENCES list.accountingplanelementtype(aetyid);


--
-- Name: accountingplan accountingplan_fatheraccount_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.accountingplan
    ADD CONSTRAINT accountingplan_fatheraccount_fkey FOREIGN KEY (fatheraccount) REFERENCES public.accountingplan(account);


--
-- Name: residence address_fkey_compid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.residence
    ADD CONSTRAINT address_fkey_compid FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: residence address_fkey_distid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.residence
    ADD CONSTRAINT address_fkey_distid FOREIGN KEY (distid) REFERENCES public.district(distid);


--
-- Name: residence address_fkey_persid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.residence
    ADD CONSTRAINT address_fkey_persid FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: agency agency_fkey_workid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_fkey_workid FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: agency agency_fkey_zoneid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_fkey_zoneid FOREIGN KEY (zoneid) REFERENCES public.zone(zoneid);


--
-- Name: agency agency_resiid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_resiid_fkey FOREIGN KEY (resiid) REFERENCES public.residence(resiid);


--
-- Name: agency agency_vaultalertworkid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_vaultalertworkid_fkey FOREIGN KEY (vaultalertworkid) REFERENCES public.worker(workid);


--
-- Name: agency agency_vaultworkid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.agency
    ADD CONSTRAINT agency_vaultworkid_fkey FOREIGN KEY (vaultworkid) REFERENCES public.worker(workid);


--
-- Name: bankaccounts bankaccounts_compid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_compid_fkey FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: bankaccounts bankaccounts_fkey_bankid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_fkey_bankid FOREIGN KEY (bankid) REFERENCES list.banks(bankid);


--
-- Name: bankaccounts bankaccounts_fkey_coinid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_fkey_coinid FOREIGN KEY (coinid) REFERENCES list.coin(coinid);


--
-- Name: bankaccounts bankaccounts_persid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.bankaccounts
    ADD CONSTRAINT bankaccounts_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: voucher baucher_clieid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT baucher_clieid_fkey FOREIGN KEY (clieid) REFERENCES public.client(clieid);


--
-- Name: voucher baucher_statid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT baucher_statid_fkey FOREIGN KEY (statid) REFERENCES list.status(statid);


--
-- Name: voucher baucher_windid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT baucher_windid_fkey FOREIGN KEY (windid) REFERENCES public.windows(windid);


--
-- Name: voucher baucher_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT baucher_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: client client_enstid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_enstid_fkey FOREIGN KEY (enstid) REFERENCES list.entitystatus(enstid);


--
-- Name: client client_fkey_agenid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_fkey_agenid FOREIGN KEY (agenid) REFERENCES public.agency(agenid);


--
-- Name: client client_fkey_anaid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_fkey_anaid FOREIGN KEY (anaid) REFERENCES public.worker(workid);


--
-- Name: client client_fkey_compid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_fkey_compid FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: client client_fkey_workid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.client
    ADD CONSTRAINT client_fkey_workid FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: company company_fkey_anaid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_anaid FOREIGN KEY (anaid) REFERENCES public.worker(workid);


--
-- Name: company company_fkey_coclid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_coclid FOREIGN KEY (coclid) REFERENCES list.companyconditionlist(coclid);


--
-- Name: company company_fkey_coslid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_coslid FOREIGN KEY (coslid) REFERENCES list.companystatuslist(coslid);


--
-- Name: company company_fkey_enstid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_enstid FOREIGN KEY (enstid) REFERENCES list.entitystatus(enstid);


--
-- Name: company company_fkey_recorderid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_recorderid FOREIGN KEY (recorderid) REFERENCES public.worker(workid);


--
-- Name: company company_fkey_tptlid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.company
    ADD CONSTRAINT company_fkey_tptlid FOREIGN KEY (tptlid) REFERENCES list.taxpayertypelist(tptlid);


--
-- Name: costcenter costcenter_fkey_father; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.costcenter
    ADD CONSTRAINT costcenter_fkey_father FOREIGN KEY (father_coceid) REFERENCES public.costcenter(coceid);


--
-- Name: costcenter costcenter_manager_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.costcenter
    ADD CONSTRAINT costcenter_manager_fkey FOREIGN KEY (manager) REFERENCES public.worker(workid);


--
-- Name: costcenter costcenter_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.costcenter
    ADD CONSTRAINT costcenter_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: department department_fkey_counid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.department
    ADD CONSTRAINT department_fkey_counid FOREIGN KEY (counid) REFERENCES public.country(counid);


--
-- Name: district district_fkey_provid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.district
    ADD CONSTRAINT district_fkey_provid FOREIGN KEY (provid) REFERENCES public.province(provid);


--
-- Name: job job_fkey_joarid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.job
    ADD CONSTRAINT job_fkey_joarid FOREIGN KEY (joarid) REFERENCES public.jobarea(joarid);


--
-- Name: job job_fkey_jocaid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.job
    ADD CONSTRAINT job_fkey_jocaid FOREIGN KEY (jocaid) REFERENCES list.jobcategory(jocaid);


--
-- Name: payments payments_agenid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_agenid_fkey FOREIGN KEY (agenid) REFERENCES public.agency(agenid);


--
-- Name: payments payments_chargebaacid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_chargebaacid_fkey FOREIGN KEY (chargebaacid) REFERENCES public.bankaccounts(baacid);


--
-- Name: payments payments_coinid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_coinid_fkey FOREIGN KEY (coinid) REFERENCES list.coin(coinid);


--
-- Name: payments payments_persid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: payments payments_recaid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_recaid_fkey FOREIGN KEY (recaid) REFERENCES list.reasonforcancellation(recaid);


--
-- Name: payments payments_targetbaacid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_targetbaacid_fkey FOREIGN KEY (targetbaacid) REFERENCES public.bankaccounts(baacid);


--
-- Name: payments payments_validatedworkid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_validatedworkid_fkey FOREIGN KEY (validatedworkid) REFERENCES public.worker(workid);


--
-- Name: payments payments_windid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_windid_fkey FOREIGN KEY (windid) REFERENCES public.windows(windid);


--
-- Name: payments payments_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.payments
    ADD CONSTRAINT payments_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: person person_fkey_anaid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_anaid FOREIGN KEY (anaid) REFERENCES public.worker(workid);


--
-- Name: person person_fkey_birthplace; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_birthplace FOREIGN KEY (birthplace) REFERENCES public.district(distid);


--
-- Name: person person_fkey_cistid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_cistid FOREIGN KEY (cistid) REFERENCES list.civilstatus(cistid);


--
-- Name: person person_fkey_enstid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_enstid FOREIGN KEY (enstid) REFERENCES list.entitystatus(enstid);


--
-- Name: person person_fkey_iddoid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_iddoid FOREIGN KEY (iddoid) REFERENCES list.identitydocumenttype(iddoid);


--
-- Name: person person_fkey_nationality; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_nationality FOREIGN KEY (nationality) REFERENCES public.country(counid);


--
-- Name: person person_fkey_recorderid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.person
    ADD CONSTRAINT person_fkey_recorderid FOREIGN KEY (recorderid) REFERENCES public.worker(workid);


--
-- Name: product product_account_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_account_fkey FOREIGN KEY (account) REFERENCES public.accountingplan(account);


--
-- Name: product product_fkey_coinid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_fkey_coinid FOREIGN KEY (coinid) REFERENCES list.coin(coinid);


--
-- Name: product product_fkey_prtyid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_fkey_prtyid FOREIGN KEY (prtyid) REFERENCES list.producttype(prtyid);


--
-- Name: product product_fkey_workid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.product
    ADD CONSTRAINT product_fkey_workid FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: provider provider_compid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.provider
    ADD CONSTRAINT provider_compid_fkey FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: provider provider_enstid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.provider
    ADD CONSTRAINT provider_enstid_fkey FOREIGN KEY (enstid) REFERENCES list.entitystatus(enstid);


--
-- Name: provider provider_persid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.provider
    ADD CONSTRAINT provider_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: provider provider_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.provider
    ADD CONSTRAINT provider_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: province province_fkey_depaid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.province
    ADD CONSTRAINT province_fkey_depaid FOREIGN KEY (depaid) REFERENCES public.department(depaid);


--
-- Name: sale sale_agenid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_agenid_fkey FOREIGN KEY (agenid) REFERENCES public.agency(agenid);


--
-- Name: sale sale_clieid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_clieid_fkey FOREIGN KEY (clieid) REFERENCES public.client(clieid);


--
-- Name: sale sale_coceid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_coceid_fkey FOREIGN KEY (coceid) REFERENCES public.costcenter(coceid);


--
-- Name: sale sale_coinid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_coinid_fkey FOREIGN KEY (coinid) REFERENCES list.coin(coinid);


--
-- Name: sale sale_dotyid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_dotyid_fkey FOREIGN KEY (dotyid) REFERENCES list.documenttype(dotyid);


--
-- Name: sale sale_persid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: sale sale_resiid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_resiid_fkey FOREIGN KEY (resiid) REFERENCES public.residence(resiid);


--
-- Name: sale sale_sellerid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_sellerid_fkey FOREIGN KEY (sellerid) REFERENCES public.worker(workid);


--
-- Name: sale sale_statid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_statid_fkey FOREIGN KEY (statid) REFERENCES list.status(statid);


--
-- Name: sale sale_sunat_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_sunat_fkey FOREIGN KEY (sunat) REFERENCES list.sunatelectronicanswer(suea);


--
-- Name: sale sale_windid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_windid_fkey FOREIGN KEY (windid) REFERENCES public.windows(windid);


--
-- Name: sale sale_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.sale
    ADD CONSTRAINT sale_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: saleitems saleitems_prodid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.saleitems
    ADD CONSTRAINT saleitems_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: saleitems saleitems_saleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.saleitems
    ADD CONSTRAINT saleitems_saleid_fkey FOREIGN KEY (saleid) REFERENCES public.sale(saleid);


--
-- Name: salepayments salepayments_paymid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.salepayments
    ADD CONSTRAINT salepayments_paymid_fkey FOREIGN KEY (paymid) REFERENCES public.payments(paymid);


--
-- Name: salepayments salepayments_saleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.salepayments
    ADD CONSTRAINT salepayments_saleid_fkey FOREIGN KEY (saleid) REFERENCES public.sale(saleid);


--
-- Name: voucher voucher_mmtyid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT voucher_mmtyid_fkey FOREIGN KEY (mmtyid) REFERENCES list.moneymovetype(mmtyid);


--
-- Name: voucher voucher_provid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT voucher_provid_fkey FOREIGN KEY (provid) REFERENCES public.provider(provid);


--
-- Name: voucher voucher_saleid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT voucher_saleid_fkey FOREIGN KEY (saleid) REFERENCES public.sale(saleid);


--
-- Name: voucher voucher_updater_workid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucher
    ADD CONSTRAINT voucher_updater_workid_fkey FOREIGN KEY (updater_workid) REFERENCES public.worker(workid);


--
-- Name: voucheritems voucheritems_prodid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucheritems
    ADD CONSTRAINT voucheritems_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: voucheritems voucheritems_voucid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucheritems
    ADD CONSTRAINT voucheritems_voucid_fkey FOREIGN KEY (voucid) REFERENCES public.voucher(voucid) ON DELETE CASCADE;


--
-- Name: voucherpayments voucherpayments_paymid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucherpayments
    ADD CONSTRAINT voucherpayments_paymid_fkey FOREIGN KEY (paymid) REFERENCES public.payments(paymid);


--
-- Name: voucherpayments voucherpayments_voucid_fkey; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.voucherpayments
    ADD CONSTRAINT voucherpayments_voucid_fkey FOREIGN KEY (voucid) REFERENCES public.voucher(voucid);


--
-- Name: windows windows_fkey_agenid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.windows
    ADD CONSTRAINT windows_fkey_agenid FOREIGN KEY (agenid) REFERENCES public.agency(agenid);


--
-- Name: windows windows_fkey_workid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.windows
    ADD CONSTRAINT windows_fkey_workid FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: worker worker_fkey_jobid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_fkey_jobid FOREIGN KEY (jobid) REFERENCES public.job(jobid);


--
-- Name: worker worker_fkey_persid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_fkey_persid FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: worker worker_fkey_wenstid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_fkey_wenstid FOREIGN KEY (wenstid) REFERENCES list.entitystatus(enstid);


--
-- Name: worker worker_fkey_wrecorderid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.worker
    ADD CONSTRAINT worker_fkey_wrecorderid FOREIGN KEY (wrecorderid) REFERENCES public.worker(workid);


--
-- Name: zone zone_fkey_workid; Type: FK CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.zone
    ADD CONSTRAINT zone_fkey_workid FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: rentexecute rentexecute_clieid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_clieid_fkey FOREIGN KEY (clieid) REFERENCES public.client(clieid);


--
-- Name: rentexecute rentexecute_coceid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_coceid_fkey FOREIGN KEY (coceid) REFERENCES public.costcenter(coceid);


--
-- Name: rentexecute rentexecute_delivered_workid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_delivered_workid_fkey FOREIGN KEY (delivered_workid) REFERENCES public.worker(workid);


--
-- Name: rentexecute rentexecute_delivery_cowoid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_delivery_cowoid_fkey FOREIGN KEY (delivery_cowoid) REFERENCES company.worker(cowoid);


--
-- Name: rentexecute rentexecute_received_cowoid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_received_cowoid_fkey FOREIGN KEY (received_cowoid) REFERENCES company.worker(cowoid);


--
-- Name: rentexecute rentexecute_received_workid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_received_workid_fkey FOREIGN KEY (received_workid) REFERENCES public.worker(workid);


--
-- Name: rentexecute rentexecute_sereid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_sereid_fkey FOREIGN KEY (sereid) REFERENCES service.rentrequest(sereid);


--
-- Name: rentexecute rentexecute_statid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_statid_fkey FOREIGN KEY (statid) REFERENCES list.status(statid);


--
-- Name: rentexecute rentexecute_workid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentexecute
    ADD CONSTRAINT rentexecute_workid_fkey FOREIGN KEY (workid) REFERENCES public.worker(workid);


--
-- Name: rentrequest rentrequest_coceid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_coceid_fkey FOREIGN KEY (coceid) REFERENCES public.costcenter(coceid);


--
-- Name: rentrequest rentrequest_compid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_compid_fkey FOREIGN KEY (compid) REFERENCES public.company(compid);


--
-- Name: rentrequest rentrequest_driver_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_driver_fkey FOREIGN KEY (driver) REFERENCES public.person(persid);


--
-- Name: rentrequest rentrequest_persid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_persid_fkey FOREIGN KEY (persid) REFERENCES public.person(persid);


--
-- Name: rentrequest rentrequest_prodid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_prodid_fkey FOREIGN KEY (prodid) REFERENCES public.product(prodid);


--
-- Name: rentrequest rentrequest_statid_fkey; Type: FK CONSTRAINT; Schema: service; Owner: -
--

ALTER TABLE ONLY service.rentrequest
    ADD CONSTRAINT rentrequest_statid_fkey FOREIGN KEY (statid) REFERENCES list.status(statid);


--
-- PostgreSQL database dump complete
--

\unrestrict W7BbFqRnwjDfbHZBcKGsfodrxtgbEfVZfW5vI28Xc9QlbbjxxOrMguOqklsmhnM


-- ============================================




-- ==========================================

-- Seed: 04_seed_components_materials.sql

-- ==========================================
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


-- ==========================================

-- Seed: 05_seed_dashboard_data.sql

-- ==========================================
-- ============================================================
-- Seed Data: Poblar Dashboard BI con datos reales
-- Fecha: 2026-05-21
-- Proyecto: MaintManager
-- ============================================================
-- Este script agrega:
--   1. Consumos de material para órdenes ACTIVAS (mainid=36,37,38)
--   2. Una orden de emergencia con consumo
--   3. Lotes por vencer (próximos 30 días)
--   4. Instalaciones de componentes con vida útil
-- ============================================================

BEGIN;

-- ============================================================
-- 1. LOTES POR VENCER (próximos 30 días)
-- ============================================================
-- Estos lotes expiran pronto para que aparezcan en el dashboard

UPDATE maintenance.material_lot
SET expiration_date = CURRENT_DATE + 15
WHERE maloid = 82 AND expiration_date IS DISTINCT FROM CURRENT_DATE + 15;

UPDATE maintenance.material_lot
SET expiration_date = CURRENT_DATE + 25
WHERE maloid = 83 AND expiration_date IS DISTINCT FROM CURRENT_DATE + 25;

-- ============================================================
-- 2. CONSUMOS DE MATERIAL para órdenes ACTIVAS
-- ============================================================
-- mainid=36, 37, 38 son las únicas órdenes activas (statid='AC')
-- Se consumen materiales con costo real para que el dashboard muestre datos

-- mainid=36: Consumo de aceite (mateid=61, maloid=77, costo 25.00/L)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (36, 61, 4.5, 77, 'Stock propio', NOW() - INTERVAL '5 days')
ON CONFLICT DO NOTHING;

-- mainid=37: Consumo de filtro aceite (mateid=67, maloid=82, costo 45.00/uni)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (37, 67, 1, 82, 'Stock propio', NOW() - INTERVAL '3 days')
ON CONFLICT DO NOTHING;

-- mainid=38: Consumo de aceite transmisión (mateid=63, maloid=80, costo 38.50/L)
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (38, 63, 2, 80, 'Stock propio', NOW() - INTERVAL '1 day')
ON CONFLICT DO NOTHING;

-- mainid=36: Consumo adicional de filtro
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
VALUES (36, 67, 1, 82, 'Stock propio', NOW() - INTERVAL '5 days')
ON CONFLICT DO NOTHING;

-- ============================================================
-- 3. NUEVA ORDEN DE EMERGENCIA (activa)
-- ============================================================
-- Para que la tasa de emergencia no sea 0%

INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
SELECT v.prcoid, 2, NULL, v.mileage + 500, 'Falla en sistema eléctrico - Emergencia en ruta', 'Taller propio', 17, 16, 'AC', NOW() - INTERVAL '2 days', NOW(), NOW()
FROM product.vehicle v WHERE v.prcoid = 90 AND NOT EXISTS (SELECT 1 FROM maintenance.maintenance m WHERE m.prcoid = 90 AND m.statid = 'AC')
LIMIT 1;

-- Consumo de material para la orden de emergencia
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 73, 1, ml.maloid, 'Stock propio', NOW() - INTERVAL '2 days'
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 73 AND lot_status = 'activo' LIMIT 1) ml
WHERE m.prcoid = 90 AND m.statid = 'AC' AND m.matyid = 2
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid)
LIMIT 1;

-- Instalar batería como componente en la emergencia
INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, installation_km, installation_date, active)
SELECT m.prcoid, 201, m.mainid, m.mileage, NOW() - INTERVAL '2 days', true
FROM maintenance.maintenance m
WHERE m.prcoid = 90 AND m.statid = 'AC' AND m.matyid = 2
AND NOT EXISTS (SELECT 1 FROM maintenance.installed_component ic WHERE ic.mainid = m.mainid)
LIMIT 1;

-- ============================================================
-- 4. AGREGAR OTRA ORDEN ACTIVA para más variedad en dashboard
-- ============================================================
-- Para vehículo 81 (VDG-361), una orden Calendarizado activa
INSERT INTO maintenance.maintenance (prcoid, matyid, setyid, mileage, note, origin_service, assigned_to, workid, statid, maintenance_date, created_at, updated_at)
SELECT v.prcoid, 1, 2, v.mileage, 'Mantenimiento programado', 'Taller propio', 17, 16, 'AC', NOW(), NOW(), NOW()
FROM product.vehicle v WHERE v.prcoid = 81
AND NOT EXISTS (SELECT 1 FROM maintenance.maintenance m WHERE m.prcoid = 81 AND m.statid = 'AC')
LIMIT 1;

-- Consumo de materiales para esta nueva orden
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 73, 1, ml.maloid, 'Stock propio', NOW()
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 73 AND lot_status = 'activo' LIMIT 1) ml
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid)
LIMIT 1;

-- Instalar batería como componente
INSERT INTO maintenance.installed_component (prcoid, acatid, mainid, installation_km, installation_date, active)
SELECT m.prcoid, 201, m.mainid, m.mileage, NOW(), true
FROM maintenance.maintenance m
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.installed_component ic WHERE ic.mainid = m.mainid)
LIMIT 1;

-- Consumo adicional de aceite
INSERT INTO maintenance.material_consumption (mainid, mateid, quantity, maloid, origin, consumed_at)
SELECT m.mainid, 61, 4.5, ml.maloid, 'Stock propio', NOW()
FROM maintenance.maintenance m
CROSS JOIN LATERAL (SELECT maloid FROM maintenance.material_lot WHERE mateid = 61 AND lot_status = 'activo' ORDER BY expiration_date NULLS LAST LIMIT 1) ml
WHERE m.prcoid = 81 AND m.statid = 'AC'
AND NOT EXISTS (SELECT 1 FROM maintenance.material_consumption mc WHERE mc.mainid = m.mainid AND mc.mateid = 61)
LIMIT 1;

-- ============================================================
-- 5. CALENDAR COMPLIANCE: actualizar vehicle_schedule
-- ============================================================
-- Asegurar que los vehículos activos tengan schedule configurado

INSERT INTO maintenance.vehicle_schedule (prcoid, interval_km, next_km, alert_km_threshold, next_service_type_code, created_by, status)
SELECT v.prcoid, 5000, v.mileage + 5000, 800, 'A', 16, true
FROM product.vehicle v
WHERE NOT EXISTS (SELECT 1 FROM maintenance.vehicle_schedule vs WHERE vs.prcoid = v.prcoid)
LIMIT 5
ON CONFLICT DO NOTHING;

COMMIT;

-- ============================================================
-- VERIFICACIONES
-- ============================================================
-- SELECT * FROM maintenance.vw_bi_dashboard_summary;
-- SELECT * FROM maintenance.vw_cost_per_km;
-- SELECT * FROM maintenance.vw_emergency_rate;
-- SELECT * FROM maintenance.vw_monthly_cost ORDER BY month;
-- SELECT * FROM maintenance.vw_calendar_compliance;


-- ==========================================

-- Seed: 07_vehicle_config.sql

-- ==========================================
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


-- ==========================================

-- Seed: 08_managed_vehicle.sql

-- ==========================================
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
