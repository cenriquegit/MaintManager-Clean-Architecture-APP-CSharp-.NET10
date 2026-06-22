## Figura X. Diagrama de Actividades del Ciclo Completo de Mantenimiento de Vehículos — MaintManager

**Descripción:**
Representa el flujo de trabajo completo del sistema MaintManager, desde el inicio de sesión y la detección de un mantenimiento próximo, pasando por la creación de la orden mediante el Wizard de 4 pasos, la ejecución de los checklists de acciones, materiales y componentes, el diagnóstico del mecánico, el cierre de la orden con recalendarización automática, hasta la generación de reportes, actualización de indicadores BI y gestión de alertas automáticas.

---

## 2. Tipo de diagrama

Es un:

**Diagrama de Actividades UML**

Su objetivo es mostrar:

- Procesos
- Tareas
- Decisiones
- Flujo de información
- Responsables
- Actividades automáticas del sistema
- Puntos de integración entre capas (app móvil, API, base de datos)

---

## 3. Participantes (Swimlanes)

El diagrama está dividido en **cuatro columnas verticales** llamadas carriles o Swimlanes.

Cada carril representa quién realiza las actividades.

### Carril 1: Jefe de Mantenimiento (Admin)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de:**
- Iniciar sesión en la aplicación MAUI
- Consultar el Dashboard (KPIs, vehículos, estadísticas)
- Revisar la Agenda de servicios (Vencidos, Próximos, En Servicio, Al día)
- Crear órdenes de mantenimiento mediante el Wizard de 4 pasos
- Configurar acciones, materiales y componentes permitidos por vehículo
- Gestionar inventario (crear materiales, registrar lotes, descartar lotes)
- Revisar y resolver alertas del sistema
- Consultar el BI Dashboard con gráficos interactivos
- Exportar reportes (Excel de costo/km, PDF de órdenes, alertas, historial)
- Configurar parámetros del sistema (intervalo km, umbral de alerta)
- Gestionar vehículos (CRUD, ingreso manual de datos)
- Crear nuevos usuarios (trabajadores)
- Reasignar técnico en órdenes activas

### Carril 2: Mecánico (Técnico)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de:**
- Iniciar sesión en la aplicación MAUI
- Ver el Dashboard y KPIs generales
- Ver lista de mantenimientos y detalle de órdenes
- Crear órdenes de mantenimiento mediante el Wizard de 4 pasos
- Ejecutar el checklist de acciones del mantenimiento
- Consumir materiales del inventario (con calificación de 1 a 5 estrellas)
- Instalar componentes en el vehículo
- Registrar el diagnóstico final del vehículo
- Cerrar órdenes de mantenimiento (calendarizadas o emergencia)
- Cancelar órdenes activas
- Ver inventario (materiales, componentes, lotes)
- Marcar alertas como leídas
- Exportar reportes en PDF

### Carril 3: Aplicación MAUI (Frontend)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de procesos del cliente:**
- Validación de kilometraje ingresado (advertencia si es menor al último registrado)
- Presentación de checklists con filtrado por vehículo
- Control de cantidad disponible en consumo de materiales
- Envío de peticiones HTTP con token JWT a la API
- Almacenamiento seguro de sesión (SecureStorage + Preferences)
- Protección de rutas por rol (Flyout restringido)
- Compartir archivos PDF/Excel generados

### Carril 4: Sistema (Backend API + Base de Datos)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de procesos automáticos:**
- Autenticación JWT (validación de credenciales, generación de token)
- Determinación automática del rol (Admin/Técnico según puesto en BD)
- Cálculo del kilometraje actual del vehículo
- Recalendarización automática al cerrar orden (NextKm + alternancia A/B)
- Consumo de inventario con algoritmo FIFO (primero lotes que vencen)
- Actualización automática de stock_total al consumir o descartar
- Verificación y generación de alertas (4 tipos)
- Actualización de vistas SQL para BI
- Generación de PDFs con QuestPDF y Excel con ClosedXML
- Control de duplicados de alertas (solo una no resuelta por referencia)

---

## 4. Elementos gráficos utilizados

### A. Nodo Inicial

**Figura:**
- ● Círculo negro sólido

**Ubicación:**
Parte superior izquierda, dentro del carril del Jefe o Mecánico.

**Representa:**
Inicio del proceso. En UML significa que aquí comienza el flujo de actividades.

### B. Actividades

**Figura:**
- Rectángulo con esquinas redondeadas

**Color:**
- Fondo blanco
- Borde azul

**Ejemplo:**
- `1. Iniciar sesión (Login)`
- `2. Consultar Dashboard Principal`
- `3. Consultar Agenda de Servicios`

**Representan:**
Acciones o tareas ejecutadas por un actor.

### C. Decisiones

**Figura:**
- Rombo

**Color:**
- Borde naranja

**Ejemplos:**
- `¿Credenciales válidas?`
- `Tipo de servicio: ¿Calendarizado o Emergencia?`
- `¿Emergencia completa o parcial?`
- `¿Instala componentes?`
- `¿Quedan acciones pendientes en checklist?`

**Representan:**
Puntos donde el flujo toma diferentes caminos según una condición.

### D. Flujo de Control

**Figura:**
- Flechas negras continuas

**Representan:**
La secuencia de ejecución de actividades.

**Ejemplo:**
`Actividad A → Actividad B`

### E. Flujo Automático o Dependencia

**Figura:**
- Línea punteada

**Color:**
- Negro

**Ejemplo:**
`Registrar materiales consumidos` ⤓ `Procesos automáticos del sistema`

**Representa:**
Una acción que dispara procesos automáticos del backend.

### F. Fork (Bifurcación Paralela)

**Figura:**
- Barra negra horizontal gruesa

**Ubicación:**
Sección del Sistema.

**Representa:**
Ejecución simultánea de varias tareas automáticas.

### G. Join (Unión)

**Figura:**
- Otra barra negra horizontal gruesa

**Representa:**
La sincronización de procesos paralelos antes de continuar.

### H. Nodo Final

**Figura:**
- ◎ Círculo negro dentro de un círculo

**Representa:**
Finalización completa del proceso.

---

## 5. Explicación paso a paso

---

### Fase 1: Autenticación

**Actividad 1**
`Iniciar sesión (Login)`

**Actor:** Jefe o Mecánico
**Pantalla:** `LoginPage`

**Objetivo:**
El usuario ingresa su nombre de usuario y contraseña en la aplicación MAUI. La app envía las credenciales al backend.

**Decisión** `¿Credenciales válidas?`

- **Si NO:**
  - El backend responde con error 401 ("Usuario o contraseña incorrectos" o "Cuenta bloqueada")
  - → Regresa a la pantalla de login
- **Si SÍ:**
  - El backend genera un token JWT con claims: `workid`, `username`, `fullname`, `role`
  - El rol se determina automáticamente: si el puesto contiene "Mecánico" → `Tecnico`, caso contrario → `Admin`
  - La app guarda el token en `SecureStorage` y las preferencias en `Preferences`
  - → Navega al `Dashboard Principal`

---

### Fase 2: Monitoreo y Detección

**Actividad 2**
`Consultar Dashboard Principal`

**Actor:** Jefe o Mecánico
**Pantalla:** `HomePage`

**Objetivo:**
Revisar el panel principal que muestra:
- **KPIs:** Total de vehículos, servicios del mes, stock bajo, alertas sin resolver, lotes por vencer
- **Tarjetas de vehículos:** Placa, nombre, km actual, próximo servicio, barra de progreso
- **Estadísticas:** Programados, en progreso, completados este mes, emergencias este mes
- **Acciones rápidas:** contextuales según el rol

**Actividad 3**
`Consultar Agenda de Servicios (Solo Admin)`

**Actor:** Jefe
**Pantalla:** `AgendaPage`

**Objetivo:**
La agenda clasifica todos los vehículos en 4 categorías:
- **Vencidos:** km actual ≥ próximo km programado
- **Próximos:** km actual dentro del umbral de alerta (default 800 km)
- **En Servicio:** tienen una orden activa (statid = "AC")
- **Al día:** km actual lejos del próximo servicio

**Decisión** `¿Hay vehículo que necesita mantenimiento?`

- **Si NO:**
  - → El flujo puede continuar con revisión de alertas, inventario o reportes
- **Si SÍ:**
  - → El usuario puede tocar un vehículo vencido o próximo para iniciar la creación de orden

---

### Fase 3: Creación de la Orden — Wizard de 4 Pasos

**Actividad 4**
`Iniciar Wizard de Nueva Orden`

**Actor:** Jefe o Mecánico
**Pantalla:** `MaintenanceWizardPage`

**Objetivo:**
Se abre el asistente de 4 pasos. Si se navegó desde la Agenda, el vehículo y tipo vienen pre-seleccionados.

---

**Paso 1 del Wizard: Vehículo + Kilometraje**

`Seleccionar vehículo de la flota`

- Se carga la lista de vehículos activos con su placa, nombre y km actual
- Si viene de la Agenda, el vehículo ya está seleccionado

**Decisión** `Validación de kilometraje`

El sistema (MAUI) compara el km ingresado contra el último km registrado (`VehicleLastKm`).

- **Si el km ingresado es menor al último registrado:**
  - La app muestra una advertencia: *"El kilometraje ingresado es menor al último registrado. Verifica."*
  - El usuario puede corregirlo o continuar
- **Si es mayor o igual:**
  - Continúa sin advertencia

---

**Paso 2 del Wizard: Tipo de Servicio**

`Seleccionar tipo de servicio`

**Actor:** Jefe o Mecánico

Opciones disponibles con descripción contextual:

| Tipo | Código | Descripción |
|---|---|---|
| **Servicio A** | Setyid=1 | Mantenimiento liviano cada 5,000 km. Cambio de aceite, filtros, revisión general |
| **Servicio B** | Setyid=2 | Mantenimiento completo cada 10,000 km. Incluye A + frenos, suspensión, rotación de neumáticos |
| **Emergencia** | Matyid=2 | Servicio no programado por falla inesperada. No tiene setyid |

**Decisión** `Tipo de servicio: ¿Calendarizado o Emergencia?`

- **Calendarizado (A o B):**
  - Se asigna `matyid = 1` y `setyid = 1 o 2`
  - → Continúa al Paso 3
- **Emergencia:**
  - Se asigna `matyid = 2` y `setyid = null`
  - → Continúa al Paso 3

---

**Paso 3 del Wizard: Asignar Técnico**

`Seleccionar técnico responsable`

**Actor:** Jefe o Mecánico

- Se carga la lista de técnicos disponibles (`GET /api/v1/workers/technicians`)
- Se puede agregar una nota opcional para el mecánico
- Si no se selecciona técnico, se asigna automáticamente el usuario actual

---

**Paso 4 del Wizard: Confirmación y Guardado**

`Revisar resumen y confirmar`

**Actor:** Jefe o Mecánico

- Se muestra un resumen de: vehículo, km, tipo de servicio, técnico asignado
- Al presionar **Guardar**:
  - `POST /api/v1/maintenances`
  - El backend calcula automáticamente `kmSinceLast` (km actual − km del último mantenimiento)
  - Si `kmSinceLast < 0`, se establece como `null`
  - La orden se crea con estado **"AC" (Activo)**
  - → Navega a la pantalla de detalle de la orden creada

---

### Fase 4: Ejecución del Mantenimiento

**Actividad 5**
`Abrir detalle de la orden de mantenimiento`

**Actor:** Jefe o Mecánico
**Pantalla:** `MaintenanceDetailPage`

**Objetivo:**
La pantalla de detalle muestra:
- Datos del vehículo, tipo de servicio, km, técnico asignado
- Información del aceite (marca, viscosidad) si aplica
- **Tres checklists ejecutables:** Acciones, Materiales, Componentes
- Formulario de diagnóstico
- Botones: Cerrar orden, Cancelar orden, Exportar PDF, Reasignar técnico

---

### Fase 4a: Checklist de Acciones

**Actividad 6**
`Cargar checklist de acciones`

**Actor:** Sistema (Backend)

- El backend consulta `VehicleAllowedAction` (acciones permitidas para ese vehículo)
- Filtra el catálogo maestro de acciones mostrando solo las autorizadas
- Si no hay configuración por vehículo, muestra el catálogo completo
- Las acciones ya completadas aparecen marcadas con ✅

**Decisión** `¿Quedan acciones por realizar?`

- **Si SÍ:**
  - El mecánico marca cada acción como realizada
  - `POST /api/v1/maintenances/{id}/actions` con `ActionCatalogId`
  - → Se crea `MaintenanceActionDetail` vinculado a la orden
  - → Repite hasta completar todas
- **Si NO:**
  - → Continúa al siguiente checklist

---

### Fase 4b: Checklist de Materiales

**Actividad 7**
`Cargar checklist de materiales`

**Actor:** Sistema (Backend)

- El backend consulta `VehicleAllowedMaterial` (materiales permitidos para ese vehículo)
- Muestra: nombre, unidad de medida, stock disponible

**Decisión** `¿Se consumen materiales?`

- **Si NO:**
  - → Continúa al checklist de componentes
- **Si SÍ:**
  - El mecánico ingresa:
    - **Cantidad** consumida (debe ser ≤ stock disponible si es Stock propio)
    - **Origen:** `Stock propio` (descuenta del inventario) o `Externo` (no descuenta)
    - **Calificación:** 1 a 5 estrellas (opcional)

  **Actividad 7a**
  `Registrar consumo de material`

  - `POST /api/v1/maintenances/{id}/consume`
  - El backend ejecuta el **algoritmo FIFO**:
    1. Obtiene todos los lotes activos del material, ordenados por fecha de vencimiento ascendente
    2. Consume del lote que vence primero
    3. Si no alcanza, pasa al siguiente lote
    4. Reduce `current_quantity` de cada lote y `stock_total` del material
    5. Crea registros `MaterialConsumption` vinculados a la orden y a los lotes
  - Si `stock_total < cantidad solicitada` → Error: "Stock insuficiente"

  **Actividad 7b (Opcional)**
  `Calificar material`

  - `POST /api/v1/inventory/materials/{mateid}/ratings`
  - Rating de 1 a 5 estrellas
  - Si rating ≤ 3, la observación es obligatoria

  **Flujo automático:**
  - ⤓ Se actualiza el stock en inventario (FIFO)
  - ⤓ Se verifica si el material quedó en stock bajo
  - ⤓ El `AlertService` puede generar alerta `STOCK_BAJO` si `stock_total ≤ stock_minimum`

---

### Fase 4c: Checklist de Componentes

**Actividad 8**
`Cargar checklist de componentes`

**Actor:** Sistema (Backend)

- El backend consulta `VehicleAllowedComponent` (componentes permitidos para ese vehículo)
- Muestra componentes instalables

**Decisión** `¿Instala componentes?`

- **Si NO:**
  - → Continúa al diagnóstico
- **Si SÍ:**

  **Actividad 8a**
  `Instalar componente`

  - `POST /api/v1/maintenances/{id}/components`
  - Se registra:
    - `ActionCatalogId` (tipo de componente)
    - `Cantidad`
    - `InstallationKm` (km del vehículo al momento de instalación)
    - `UsefulLifeDays` (opcional, para calcular fecha de caducidad)
  - Se crea `InstalledComponent` vinculado al vehículo y a la orden

  **Flujo automático:**
  - ⤓ Si tiene `UsefulLifeDays`, el sistema calcula `ExpirationDate = InstallationDate + UsefulLifeDays`
  - ⤓ El `AlertService` puede generar alerta `COMPONENTE_POR_CADUCAR` cuando se acerque la fecha

---

### Fase 5: Diagnóstico

**Actividad 9**
`Registrar diagnóstico del vehículo`

**Actor:** Mecánico
**Pantalla:** `MaintenanceDetailPage`

**Campos del formulario:**
- **Estado general:** `Excelente` | `Bueno` | `Regular` | `Reparado` | `Malo`
- **¿Vehículo operativo?:** `Sí` / `No`
- **Observaciones:** texto libre
- **Recomendaciones futuras:** texto libre

**Regla de negocio:**
- El diagnóstico solo se puede guardar **una vez** por orden
- Si ya existe diagnóstico, el backend rechaza el segundo intento
- El diagnóstico es **obligatorio** para poder cerrar la orden

**POST** `/api/v1/maintenances/{id}/diagnosis`

- Si exitoso → `DiagnosisSaved = true` y `CanClose = true`
- → El botón "Cerrar Orden" se habilita

---

### Fase 6: Cierre de la Orden

**Decisión** `¿Cerrar la orden?`

- **Si NO (Cancelar):**

  **Actividad 10**
  `Cancelar orden de mantenimiento`

  - `PUT /api/v1/maintenances/{id}/cancel`
  - Solo se pueden cancelar órdenes en estado **"AC" (Activo)**
  - Cambia `statid` de `"AC"` a `"CA"` (Cancelado)
  - Los datos registrados se conservan
  - **No se recalendariza**
  - → Fin del proceso

- **Si SÍ:**

  **Decisión** `¿Es Emergencia?`

  - **Si es Calendarizado (matyid=1):**
    - → Cierre directo con recalendarización

  - **Si es Emergencia (matyid=2):**

    **Decisión** `¿Emergencia completa o parcial?`

    - La app muestra un diálogo con dos opciones:
      - **Completa:** Se realizó todo el servicio → recalendariza
      - **Parcial:** Solo se atendió lo urgente → NO recalendariza

  **Actividad 11**
  `Ejecutar cierre de orden`

  - `PUT /api/v1/maintenances/{id}/close` con `{ IsEmergencyComplete: true/false }`
  - El backend:
    1. Verifica que exista diagnóstico (obligatorio)
    2. Si es emergencia: guarda `is_emergency_complete`
    3. Cambia `statid` de `"AC"` a `"FI"` (Finalizado)
    4. Si aplica recalendarización, ejecuta el `SchedulingService`

  ---

  ### Fase 6a: Recalendarización Automática (Sistema)

  **Actividad 12**
  `Recalendarizar próximo mantenimiento`

  **Actor:** Sistema (Backend)

  **Lógica del `SchedulingService.RescheduleAsync()`:**
  1. Obtiene el `VehicleSchedule` activo del vehículo
  2. Calcula `NextKm = serviceKm + IntervalKm`
  3. Alterna el tipo de servicio:
     - Si el último fue **A** → próximo será **B**
     - Si el último fue **B** → próximo será **A**
     - Si no hay historial → se asigna **A**
  4. Actualiza `UpdatedAt`
  5. El intervalo de km se lee de `config_system.intervalo_km` (default 5000)
  6. El umbral de alerta se lee de `config_system.alerta_km_umbral` (default 800)

---

### Fase 7: Procesos Automáticos Post-Cierre

**Fork (Bifurcación Paralela)**

El sistema ejecuta simultáneamente **cinco procesos automáticos**:

**Actividad 13a**
`Actualizar indicadores del Dashboard BI`

- Se refrescan las vistas SQL:
  - `vw_bi_dashboard_summary` → KPIs principales
  - `vw_vehicle_current_km` → km actual por vehículo
  - `vw_monthly_cost` → costo mensual
  - `vw_cost_per_km` → costo por km
  - `vw_emergency_rate` → tasa de emergencia
  - `vw_calendar_compliance` → cumplimiento de calendario

**Actividad 13b**
`Verificar y generar alertas`

El `AlertService` ejecuta 4 verificaciones:

| Tipo de Alerta | Condición |
|---|---|
| `MANTENIMIENTO_PROXIMO_KM` | `currentKm ≥ nextKm − alertKmThreshold` |
| `COMPONENTE_POR_CADUCAR` | `expiration_date ≤ hoy + umbral_días` |
| `LOTE_POR_VENCER` | `expiration_date ≤ hoy + 30 días` y `current_quantity > 0` |
| `STOCK_BAJO` | `stock_total ≤ stock_minimum` |

- Se evitan duplicados: solo se genera una alerta no resuelta por referencia
- Las alertas aparecen en `AlertListPage`

**Actividad 13c**
`Verificar stock bajo de inventario`

- Materiales cuyo `stock_total ≤ stock_minimum` se marcan como stock bajo
- Visibles en `InventoryListPage` con toggle "Solo stock bajo"
- Contabilizados en KPIs del Dashboard

**Actividad 13d**
`Actualizar lista de vehículos en Agenda`

- El vehículo sale de la categoría "En Servicio"
- Se reclasifica según el nuevo `NextKm`:
  - Si `currentKm ≥ nextKm` → "Vencidos"
  - Si `currentKm ≥ nextKm − alertKmThreshold` → "Próximos"
  - Si `currentKm < nextKm − alertKmThreshold` → "Al día"

**Actividad 13e**
`Generar PDF de la orden automáticamente`

- `GET /api/v1/reports/maintenances/{id}/pdf`
- QuestPDF genera un documento A4 con:
  - Encabezado: "Neo Plus Business S.A.C. — Orden de Mantenimiento Vehicular"
  - Datos del vehículo, tipo de servicio, fecha, km
  - Acciones realizadas (con estado: Sí/No)
  - Materiales consumidos (ID, cantidad, origen)
  - Componentes instalados (nombre, km instalación, fecha de vencimiento)
  - Diagnóstico final (estado, operatividad, observaciones, recomendaciones)
  - Pie de página con fecha de generación y paginación

**Join (Unión)**

Los cinco procesos deben finalizar antes de continuar.

---

### Fase 8: Reportes y Consultas Posteriores

**Actividad 14**
`Consultar BI Dashboard`

**Actor:** Jefe (Admin)
**Pantalla:** `BiDashboardPage`

**Objetivo:**
Visualizar 5 gráficos interactivos con LiveCharts:
1. **Costo por Km** — barras verticales, top 10 vehículos
2. **Tasa de Emergencia** — barras horizontales, top 8 vehículos
3. **Costo Mensual** — línea de tiempo, últimos 6 meses
4. **Lotes por Vencer** — gráfico de torta (crítico ≤7d, próximo ≤30d, normal >30d)
5. **Cumplimiento de Calendario** — desviación en km coloreada (verde: puntual, naranja: anticipado, rojo: tardío)

**Actividad 15**
`Exportar reportes`

**Actor:** Jefe (Admin)
**Pantalla:** `ReportsPage`

Opciones de exportación:
- **Costo por Km** → Excel (`.xlsx`) con ClosedXML
- **Órdenes de Mantenimiento** → PDF con filtros (fechas, vehículo, estado)
- **Alertas** → PDF con filtros (fechas, tipo, resueltas/no resueltas)
- **Historial por Vehículo** → PDF detallado con todas las órdenes, acciones, materiales y diagnósticos

---

### Fase 9: Fin del Proceso

**Nodo Final**

**Figura:** ◎ Círculo negro dentro de un círculo

**Significado:**
El mantenimiento quedó completamente registrado, la orden está cerrada, el inventario está actualizado, el próximo servicio está programado, los indicadores BI están al día, y las alertas pertinentes fueron generadas. El vehículo está disponible para continuar su operación normal hasta el siguiente ciclo de mantenimiento.

---

## 6. Resumen de endpoints utilizados en el flujo

| Fase | Endpoint | Método | Actor |
|---|---|---|---|
| Autenticación | `api/v1/auth/login` | POST | Usuario |
| Dashboard | `api/v1/reports/dashboard` | GET | Sistema |
| Agenda | `api/v1/agenda` | GET | Sistema |
| Wizard | `api/v1/vehicles` | GET | MAUI |
| Wizard | `api/v1/vehicles/{id}/current-km` | GET | MAUI |
| Wizard | `api/v1/workers/technicians` | GET | MAUI |
| Wizard | `api/v1/maintenances` | POST | MAUI |
| Checklists | `api/v1/maintenances/actions/catalog` | GET | MAUI |
| Checklists | `api/v1/inventory/materials` | GET | MAUI |
| Acciones | `api/v1/maintenances/{id}/actions` | POST | Mecánico |
| Materiales | `api/v1/maintenances/{id}/consume` | POST | Mecánico |
| Calificación | `api/v1/inventory/materials/{id}/ratings` | POST | Mecánico |
| Componentes | `api/v1/maintenances/{id}/components` | POST | Mecánico |
| Diagnóstico | `api/v1/maintenances/{id}/diagnosis` | POST | Mecánico |
| Cierre | `api/v1/maintenances/{id}/close` | PUT | Mecánico |
| Cancelación | `api/v1/maintenances/{id}/cancel` | PUT | Mecánico |
| PDF Orden | `api/v1/reports/maintenances/{id}/pdf` | GET | Sistema |
| Alertas | `api/v1/alerts/check` | POST | Sistema |
| BI | `api/v1/reports/cost-per-km` | GET | Admin |
| BI | `api/v1/reports/emergency-rate` | GET | Admin |
| BI | `api/v1/reports/monthly-cost` | GET | Admin |
| BI | `api/v1/reports/calendar-compliance` | GET | Admin |
| Exportar | `api/v1/reports/cost-excel` | GET | Admin |
| Exportar | `api/v1/reports/maintenance-orders` | POST | Admin |
| Exportar | `api/v1/reports/vehicle-history` | POST | Admin |
