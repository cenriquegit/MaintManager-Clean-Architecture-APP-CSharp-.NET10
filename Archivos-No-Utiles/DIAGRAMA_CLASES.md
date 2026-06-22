# Diagrama de Clases — MaintManager (Base de Datos PostgreSQL)

## Tablas del esquema `maintenance` (24 tablas)

```
┌─────────────────────────────────┐   ┌─────────────────────────────────┐
│ maintenance                     │   │ maintenance_type                │
│─────────────────────────────────│   │─────────────────────────────────│
│ mainid          PK  INTEGER     │   │ matyid          PK  SMALLINT    │
│ prcoid              INTEGER FK  │──▶│ name                 VARCHAR     │
│ matyid              SMALLINT FK │   │ description          TEXT        │
│ setyid              SMALLINT FK │   │ status               BOOLEAN     │
│ order_number        VARCHAR     │   └─────────────────────────────────┘
│ maintenance_date    TIMESTAMP   │   ┌─────────────────────────────────┐
│ mileage             INTEGER     │   │ service_type                    │
│ km_since_last       INTEGER     │   │─────────────────────────────────│
│ additional_work     TEXT        │   │ setyid          PK  SMALLINT    │
│ oil_brand           VARCHAR     │   │ code                CHAR         │
│ oil_viscosity_sae   VARCHAR     │   │ name                VARCHAR      │
│ climate_season      VARCHAR     │   │ description         TEXT         │
│ show_oil_next       BOOLEAN     │   │ status              BOOLEAN      │
│ origin_service      VARCHAR     │   └─────────────────────────────────┘
│ signature_seal      TEXT        │
│ is_emergency_compl  BOOLEAN     │
│ workid              INTEGER FK  │──▶ public.worker
│ assigned_to         INTEGER FK  │──▶ public.worker
│ note                TEXT        │
│ created_at          TIMESTAMP   │
│ updated_at          TIMESTAMP   │
│ statid              CHAR FK     │──▶ list.status
└─────────────────────────────────┘
         │1           │1           │1           │1
         │            │            │            │
         ▼            ▼            ▼            ▼

┌────────────────┐ ┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ diagnosis      │ │maintenance_action│ │material_consumpt │ │installed_        │
│ (1:1)          │ │_detail (1:N)     │ │ion (1:N)         │ │component (1:N)   │
│────────────────│ │──────────────────│ │──────────────────│ │──────────────────│
│ diagid  PK INT │ │ madeid  PK INT   │ │ macoid  PK INT   │ │ incoid  PK INT   │
│ mainid  FK INT │ │ mainid  FK INT   │ │ mainid  FK INT   │ │ prcoid  FK INT   │──▶product.company
│ general_stat   │ │ acatid  FK INT   │ │ mateid  FK INT   │ │ acatid  FK INT   │──▶action_catalog
│   VARCHAR      │ │ completed BOOL   │ │ maloid  FK INT   │ │ mainid  FK INT   │
│ observations   │ │ action_perf CHAR │ │ quantity NUMERIC │ │ maloid  FK INT   │──▶material_lot
│   TEXT         │ │ product_used     │ │ origin  VARCHAR  │ │ install_date     │
│ vehicle_op BOOL│ │   VARCHAR        │ │ consumed_at      │ │   TIMESTAMP      │
│ future_recomm  │ │ quantity_used    │ │   TIMESTAMP      │ │ install_km INT   │
│   TEXT         │ │   VARCHAR        │ └──────────────────┘ │ expiration_date  │
│ created_at     │ │ origin_product   │                      │   DATE           │
│   TIMESTAMP    │ │   VARCHAR        │                      │ active BOOL      │
└────────────────┘ │ observation TEXT │                      │ replaced_by      │
                   │ maloid  FK INT──▶│material_lot          │   FK INT──▶sí mismo
                   └──────────────────┘                     └──────────────────┘

┌─────────────────────────────────┐   ┌─────────────────────────────────┐
│ action_catalog                  │   │ action_list_type                │
│─────────────────────────────────│   │─────────────────────────────────│
│ acatid           PK  INTEGER    │   │ altoid           PK  SMALLINT   │
│ altoid               SMALLINT FK│──▶│ name                  VARCHAR    │
│ name                 VARCHAR     │   │ description           TEXT       │
│ category             VARCHAR     │   │ status                BOOLEAN    │
│ recommended_product  VARCHAR     │   └─────────────────────────────────┘
│ recommended_quantity VARCHAR     │
│ unit_of_measure      VARCHAR     │
│ useful_life_km       INTEGER     │
│ expires_by_time      BOOLEAN     │
│ useful_life_days     INTEGER     │
│ description          TEXT        │
│ status               BOOLEAN     │
└─────────────────────────────────┘

┌─────────────────────────────────┐   ┌─────────────────────────────────┐
│ material                        │   │ material_category               │
│─────────────────────────────────│   │─────────────────────────────────│
│ mateid           PK  INTEGER    │   │ macaid           PK  SMALLINT   │
│ macaid               SMALLINT FK│──▶│ name                  VARCHAR    │
│ name                 VARCHAR     │   │ description           TEXT       │
│ unit_of_measure      VARCHAR     │   │ status                BOOLEAN    │
│ stock_total          NUMERIC     │   └─────────────────────────────────┘
│ stock_minimum        NUMERIC     │
│ description          TEXT        │
│ created_at           TIMESTAMP   │
│ created_by           INTEGER FK──│──▶public.worker
│ status               BOOLEAN     │
│ updated_at           TIMESTAMP   │
└─────────────────────────────────┘
         │1           │1           │1
         │            │            │
         ▼            ▼            ▼

┌────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ material_lot   │ │material_consumpt │ │material_rating   │
│ (1:N)          │ │ion (1:N)         │ │ (1:N)            │
│────────────────│ │──────────────────│ │──────────────────│
│ maloid PK INT  │ │ macoid  PK INT   │ │ matraid PK INT   │
│ mateid FK INT  │ │ mainid  FK INT   │ │ mateid  FK INT   │
│ initial_qty    │ │ mateid  FK INT   │ │ mainid  FK INT   │
│   NUMERIC      │ │ maloid  FK INT   │ │ rating   SMALLINT│
│ current_qty    │ │ quantity NUMERIC │ │ observation TEXT │
│   NUMERIC      │ │ origin  VARCHAR  │ │ rated_by FK INT──│──▶public.worker
│ unit_cost      │ │ consumed_at      │ │ rated_at TIMESTMP│
│   NUMERIC      │ │   TIMESTAMP      │ └──────────────────┘
│ entry_date     │ └──────────────────┘
│   TIMESTAMP    │ ┌──────────────────┐
│ expiration_date│ │material_discard  │
│   DATE         │ │ (1:N)            │
│ provid FK INT──│▶public.provider   ││──────────────────│
│ supplier_lot   │ │ madiid  PK INT   │
│   VARCHAR      │ │ maloid  FK INT   │
│ note TEXT      │ │ discarded_qty    │
│ lot_status     │ │   NUMERIC        │
│   VARCHAR      │ │ discard_date     │
│ created_by FK──│▶public.worker     ││   TIMESTAMP      │
└────────────────┘ │ reason  VARCHAR  │
                   │ note    TEXT     │
                   │ discarded_by ───│▶public.worker
                   └──────────────────┘

┌─────────────────────────────────┐   ┌─────────────────────────────────┐
│ vehicle_schedule                │   │ schedule_action                 │
│─────────────────────────────────│   │─────────────────────────────────│
│ veshid           PK  INTEGER    │   │ shacid           PK  INTEGER    │
│ prcoid               INTEGER FK │──▶│ veshid               INTEGER FK │──▶vehicle_schedule
│ interval_km          INTEGER    │   │ acatid               INTEGER FK │──▶action_catalog
│ next_km              INTEGER    │   │ scheduled_km         INTEGER    │
│ alert_km_threshold   INTEGER    │   │ action_code          CHAR       │
│ created_at           TIMESTAMP  │   │ status               BOOLEAN    │
│ created_by           INTEGER FK │──▶└─────────────────────────────────┘
│ updated_at           TIMESTAMP  │
│ status               BOOLEAN    │
│ next_service_typecode CHAR     │
└─────────────────────────────────┘

┌─────────────────────────────────┐   ┌─────────────────────────────────┐
│ managed_vehicle                 │   │ technician_assignment           │
│─────────────────────────────────│   │─────────────────────────────────│
│ mv_id             PK  INTEGER   │   │ teasid           PK  INTEGER    │
│ prcoid                INTEGER FK│──▶│ mainid               INTEGER FK │──▶maintenance
│ license_plate         VARCHAR    │   │ workid               INTEGER FK │──▶public.worker
│ vehicle_name          VARCHAR    │   │ role_in_job          VARCHAR    │
│ brand                 VARCHAR    │   │ assigned_at          TIMESTAMP  │
│ model                 VARCHAR    │   │ assigned_by          INTEGER FK │──▶public.worker
│ year                  SMALLINT   │   └─────────────────────────────────┘
│ color                 VARCHAR    │   ┌─────────────────────────────────┐
│ vin                   VARCHAR    │   │ config_system                   │
│ engine_number         VARCHAR    │   │─────────────────────────────────│
│ source                VARCHAR    │   │ cosyid           PK  INTEGER    │
│ status                BOOLEAN    │   │ key                  VARCHAR     │
│ created_at            TIMESTAMPTZ│   │ value                VARCHAR     │
│ updated_at            TIMESTAMPTZ│   │ description          TEXT        │
└─────────────────────────────────┘   │ data_type            VARCHAR     │
         │                            │ updated_at           TIMESTAMP   │
         │ (herencia parcial)          │ updated_by           INTEGER FK──│▶public.worker
         ▼                            │ status               BOOLEAN     │
┌─────────────────────────────────┐   └─────────────────────────────────┘
│ product.vehicle       (hereda de product.company vía prcoid)                       │
│────────────────────────────────────────────────────────────────────────────────────│
│ prcoid               PK FK INTEGER  ← hereda de product.company                     │
│ prodid                  FK INTEGER  → public.product                                │
│ license_plate_number      VARCHAR    ← PLACA (dato clave)                            │
│ vin_number                VARCHAR    ← VIN / N° de serie                            │
│ engine_number             VARCHAR    ← N° de motor                                  │
│ year_of_manufacture       SMALLINT   ← Año de fabricación                           │
│ color                     VARCHAR    ← Color                                        │
│ mileage                   INTEGER    ← KILOMETRAJE ACTUAL                           │
│ vetyid               FK   CHAR       → list.vehicletype                             │
│ futyid               FK   CHAR       → list.fueltype                                │
│ description               TEXT       ← Descripción ERP                              │
│ power                     VARCHAR    ← Potencia                                     │
│ cylinder_count            SMALLINT   ← N° cilindros                                 │
│ cylinder_capacity         NUMERIC    ← Cilindrada                                   │
│ number_of_passengers      SMALLINT   ← Pasajeros                                    │
│ number_of_axles/wheels    SMALLINT   ← Ejes / Ruedas                               │
│ gross_vehicle_weight      NUMERIC    ← Peso bruto                                   │
│ net_vehicle_weight        NUMERIC    ← Peso neto                                    │
│ payload                   NUMERIC    ← Carga útil                                   │
│ length/height/width       NUMERIC    ← Dimensiones                                  │
│ seat_count                SMALLINT   ← Asientos                                     │
│ category / condition      VARCHAR    ← Categoría / Condición                        │
│ verification_code         VARCHAR    ← Código verificación                          │
│ publish_number            VARCHAR    ← N° publicación                              │
│ document_date             TIMESTAMP  ← Fecha documento                              │
│ registry_entry            VARCHAR    ← Partida registral                            │
│ dua_dam                   VARCHAR    ← DUA/DAM (aduana)                             │
│ title / title_date        VARCHAR/TS ← Título propiedad                             │
│ status                    BOOLEAN    ← Estado                                       │
│ workid               FK   INTEGER    → public.worker                                │
│────────────────────────────────────────────────────────────────────────────────────│
│ (FK: prcoid = product.company.prcoid — herencia de tabla)                           │
├────────────────────────────────────────────────────────────────────────────────────┤
│ product.company (ERP legacy)                                                        │
│ prcoid           PK  INTEGER                                                        │
│ prodid               INTEGER FK──│▶public.product                                    │
│ description          VARCHAR     ││ serial_number   VARCHAR │ qty INTEGER           │
│ status               INTEGER     ││ registerdate  TIMESTAMP│ workid FK──▶worker     │
└────────────────────────────────────────────────────────────────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│            TABLAS DE CONFIGURACIÓN POR VEHÍCULO                 │
├────────────────────────────────────────────────────────────────┤

┌───────────────────────────┐ ┌───────────────────────────┐ ┌───────────────────────────┐
│ vehicle_allowed_action    │ │ vehicle_allowed_material  │ │ vehicle_allowed_component │
│───────────────────────────│ │───────────────────────────│ │───────────────────────────│
│ vaacid  PK  INTEGER       │ │ vamid   PK  INTEGER       │ │ vacoid  PK  INTEGER       │
│ prcoid      INTEGER FK ──▶│ │ prcoid      INTEGER FK ──▶│ │ prcoid      INTEGER FK ──▶│
│ mv_id       INTEGER FK ──▶│ │ mv_id       INTEGER FK ──▶│ │ mv_id       INTEGER FK ──▶│
│ acatid      INTEGER FK ──▶│ │ mateid      INTEGER FK    │ │ acatid      INTEGER FK ──▶│
│ created_at  TIMESTAMPTZ   │ │ created_at  TIMESTAMPTZ   │ │ created_at  TIMESTAMPTZ   │
│                           │ │                           │ │                           │
│ FK prcoid→product.company │ │ FK mateid→material        │ │ FK acatid→action_catalog  │
│ FK mv_id →managed_vehicle │ │ FK mv_id →managed_vehicle │ │ FK mv_id →managed_vehicle │
│ FK acatid→action_catalog  │ │ FK prcoid→product.company │ │ FK prcoid→product.company │
└───────────────────────────┘   └───────────────────────────┘   └───────────────────────────┘

┌────────────────────────────────────────────────────────────────┐
│                        ALERTAS                                 │
├────────────────────────────────────────────────────────────────┤

┌─────────────────────────┐     ┌─────────────────────────────────┐
│ alert_config            │     │ alert_log                       │
│─────────────────────────│     │─────────────────────────────────│
│ alcoid  PK  INTEGER     │     │ alloid       PK  INTEGER        │
│ alert_type   VARCHAR    │     │ alcoid           INTEGER FK ───▶│
│ description  TEXT       │     │ prcoid           INTEGER FK ───▶│
│ enabled      BOOLEAN    │     │ mateid           INTEGER FK ───▶│
│ threshold_val VARCHAR   │     │ maloid           INTEGER FK ───▶│
│ threshold_unit VARCHAR  │     │ incoid           INTEGER FK ───▶│
└─────────────────────────┘     │ message          TEXT           │
                                │ alert_date       TIMESTAMP      │
                                │ read             BOOLEAN        │
                                │ read_at          TIMESTAMP      │
                                │ read_by          INTEGER FK────▶│
                                │ resolved         BOOLEAN        │
                                │ resolved_at      TIMESTAMP      │
                                │ resolved_by      INTEGER FK────▶│
                                └─────────────────────────────────┘
```

---

## Conexiones (Relaciones)

### COMPOSICIÓN (rombo negro) — El hijo no existe sin el padre

| Padre | Hijo | Cardinalidad | Descripción |
|-------|------|-------------|-------------|
| `maintenance` | `diagnosis` | 1:1 | Una orden tiene un diagnóstico |
| `maintenance` | `maintenance_action_detail` | 1:N | Acciones realizadas en la orden |
| `maintenance` | `material_consumption` | 1:N | Materiales consumidos |
| `maintenance` | `installed_component` | 1:N | Componentes instalados |
| `material` | `material_lot` | 1:N | Lotes de un material |
| `material` | `material_consumption` | 1:N | Consumos de un material |
| `material` | `material_rating` | 1:N | Calificaciones de material |
| `material_lot` | `material_discard` | 1:N | Descarte de lotes |
| `vehicle_schedule` | `schedule_action` | 1:N | Acciones programadas por vehículo |
| `alert_config` | `alert_log` | 1:N | Registro de alertas |

### AGREGACIÓN (rombo blanco) — El hijo puede existir sin el padre

| Padre | Hijo | Cardinalidad |
|-------|------|-------------|
| `action_list_type` | `action_catalog` | 1:N |
| `material_category` | `material` | 1:N |
| `maintenance_type` | `maintenance` | 1:N |
| `service_type` | `maintenance` | 1:N |

### ASOCIACIÓN (línea simple) — Ambos existen independientemente

| Origen | Destino | Cardinalidad |
|--------|---------|-------------|
| `product.company` | `managed_vehicle` | 1:1 opcional |
| `product.company` | `vehicle_schedule` | 1:N |
| `product.company` | `vehicle_allowed_action/material/component` | 1:N |
| `product.company` | `maintenance` | 1:N |
| `product.company` | `installed_component` | 1:N |
| `managed_vehicle` | `vehicle_allowed_action/material/component` | 1:N |
| `action_catalog` | `maintenance_action_detail` | 1:N |
| `action_catalog` | `installed_component` | 1:N |
| `action_catalog` | `vehicle_allowed_action/component` | 1:N |
| `action_catalog` | `schedule_action` | 1:N |
| `material_lot` | `maintenance_action_detail` | 1:N |
| `material_lot` | `material_consumption` | 1:N |
| `material_lot` | `installed_component` | 1:N |
| `material_lot` | `alert_log` | 1:N |
| `public.worker` | `maintenance` (workid, assigned_to) | 1:N |
| `public.worker` | `technician_assignment` | 1:N |
| `public.worker` | `material` (created_by) | 1:N |
| `public.worker` | `material_lot` (created_by) | 1:N |
| `public.worker` | `material_rating` (rated_by) | 1:N |
| `public.worker` | `material_discard` (discarded_by) | 1:N |
| `public.worker` | `config_system` (updated_by) | 1:N |
| `public.worker` | `vehicle_schedule` (created_by) | 1:N |
| `public.worker` | `alert_log` (read_by, resolved_by) | 1:N |
| `public.provider` | `material_lot` | 1:N |

### REFLEXIVA (a sí mismo)

| Tabla | Columna | Descripción |
|-------|---------|-------------|
| `installed_component` | `replaced_by_incoid` | Un componente reemplaza a otro |

---

## Tablas externas referenciadas

| Esquema | Tabla | Columna FK | Usada por |
|---------|-------|-----------|-----------|
| `public` | `worker` | `workid` | maintenance (workid, assigned_to), technician_assignment, material, material_lot, material_rating, material_discard, config_system, vehicle_schedule, alert_log |
| `public` | `person` | `persid` | worker, client, provider, residence, sale |
| `public` | `product` | `prodid` | product.company, product.vehicle |
| `public` | `provider` | `provid` | material_lot |
| `public` | `job` | `jobid` | worker |
| `product` | `company` | `prcoid` | maintenance, vehicle_schedule, installed_component, alert_log, managed_vehicle, vehicle_allowed_* |
| `product` | `vehicle` | `prcoid` | hereda de product.company |
| `list` | `status` | `statid` | maintenance |
| `list` | `fueltype` | `futyid` | product.vehicle |
| `list` | `vehicletype` | `vetyid` | product.vehicle |

---

**Resumen:** 24 tablas `maintenance` + 6 `product` + 26 `public` + 19 `list` + 2 `service` + 1 `company` = **78 tablas**. El núcleo es `maintenance` (orden de servicio) que conecta vehículos, acciones, materiales, componentes, diagnóstico y trabajadores.
