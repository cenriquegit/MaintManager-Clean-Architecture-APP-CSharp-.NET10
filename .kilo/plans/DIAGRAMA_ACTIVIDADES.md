# ESPECIFICACIÓN PARA DIAGRAMA DE ACTIVIDADES — MaintManager

**Propósito:** Proveer toda la información necesaria para construir el Diagrama de Actividades UML del sistema MaintManager, incluyendo actores (swimlanes), actividades secuenciales, nodos de decisión, bifurcaciones/convergencias y flujos de objetos.

---

## 1. ACTORES (SWIMLANES)

| Swimlane | Actor | Responsabilidad |
|----------|-------|-----------------|
| **Jefe de Mantenimiento** | Admin | Gestiona inventario, crea materiales/lotes, revisa dashboard BI, gestiona alertas, asigna técnicos, exporta reportes, visualiza perfil, configuración |
| **Mecánico** | Técnico | Crea órdenes de mantenimiento, ejecuta servicios, consume materiales, instala componentes, registra diagnóstico, cierra órdenes |
| **Sistema de Alertas** | Automático | Verifica alertas, recalendariza servicios, actualiza stock vía FIFO, genera PDF, actualiza KPIs del dashboard, gestiona autenticación JWT |

---

## 2. SÍMBOLOS A UTILIZAR

| Símbolo | Representación | Uso |
|---------|---------------|-----|
| ● | **Nodo inicial** | Círculo sólido — inicio del flujo |
| ◉ | **Nodo final** | Círculo con borde grueso — fin del flujo |
| ▭ (redondeado) | **Actividad/Acción** | Rectángulo de esquinas redondeadas — acción realizada por un actor |
| ◇ | **Decisión** | Rombo — bifurcación condicional (sí/no) |
| ◇ (varias entradas) | **Fusión (Merge)** | Rombo con múltiples entradas y 1 salida — unión de caminos alternativos |
| ⎯ (barra gruesa) | **Bifurcación (Fork)** | Barra horizontal/vertical — divide en flujos paralelos |
| ⎯ (barra gruesa) | **Convergencia (Join)** | Barra horizontal/vertical — sincroniza flujos paralelos |
| ▭ (rectángulo) | **Objeto** | Datos que fluyen entre actividades |
| → (discontinua) | **Flujo de objeto** | Flecha punteada — transferencia de datos |
| → (sólida) | **Flujo de actividad** | Flecha sólida — transición entre actividades |
| ▭ (con forma de hoja) | **Señal de recepción** | Actividad que recibe un evento externo |

---

## 3. DIAGRAMA PRINCIPAL: CICLO DE MANTENIMIENTO COMPLETO

### Descripción general
El flujo principal del sistema es el ciclo de mantenimiento vehicular. 
Inicia cuando el Jefe o el Sistema detecta un vehículo próximo a mantenimiento,
pasa por la creación de la orden, ejecución del servicio con consumo de materiales
y componentes, registro de diagnóstico, y finaliza con el cierre de la orden
y la recalendarización automática.

### Swimlanes involucrados: Jefe, Mecánico, Sistema

### Flujo detallado:

```
[INICIO]
  │
  ▼
┌─────────────────────────────────────────────────────┐
│ 1. Jefe consulta Dashboard BI / Lista Mantenimientos│
│    - Identifica vehículos próximos a mantenimiento  │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
                 ┌─────────────────┐     NO
                 │ ¿Hay vehículo   │────────► [FIN]
                 │ próximo a       │
                 │ mantenimiento?  │
                 └────────┬────────┘
                          │ SÍ
                          ▼
┌─────────────────────────────────────────────────────┐
│ 2. Jefe notifica al Mecánico (verbal o vía app)     │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ 3. Esperar disponibilidad del vehículo              │
│    (vehículo en ruta, debe retornar al taller)      │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ 4. Trasladar vehículo al taller                     │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────┐
│ 5. Inspección visual inicial del vehículo           │
└─────────────────────────┬───────────────────────────┘
                          │
                          ▼
                  ┌──────────────────┐
                  │ [NUEVA ORDEN]     │
                  │ Mecánico crea     │
                  │ orden en la app   │
                  └────────┬─────────┘
                           │
                           ▼
                  ┌──────────────────┐          ┌─────────────────────┐
                  │ Tipo:            │── Emerg. →│ ¿Es emergencia     │
                  │ ¿Calendarizado   │          │ completa o parcial? │
                  │ o Emergencia?    │          └─────────┬───────────┘
                  └────────┬─────────┘                    │
                  │ Calend.                               │
                  ▼                                       ▼
         ┌──────────────────┐                  ┌──────────────────┐
         │ 6. Acciones del  │                  │ 6. Acciones      │
         │    checklist se  │                  │    solo urgente  │
         │    cargan por km │                  │    (parcial)     │
         └────────┬─────────┘                  └────────┬─────────┘
                  │                                      │
                  └──────────────┬───────────────────────┘
                                 ▼
                  ┌─────────────────────────────────────┐
                  │ 7. Mecánico ejecuta mantenimiento   │
                  │    siguiendo el checklist de accione │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 8. Verificación de funcionamiento   │
                  │    post-servicio                    │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 9. Mecánico registra materiales     │
                  │    consumidos (FIFO automático:     │
                  │    el sistema selecciona el lote    │
                  │    más próximo a vencer)            │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 10. ¿Instala componente?            │
                  │     (Picker de componentes con      │
                  │      vida útil)                     │
                  └────────┬────────────────────────────┘
                           │ SÍ
                           ▼
                  ┌─────────────────────────────────────┐
                  │ 11. Mecánico instala componente     │
                  │     (se registra en la BD con km    │
                  │      y fecha de instalación)        │
                  └─────────────────┬───────────────────┘
                           │ NO (o después de instalar)
                           ▼
                  ┌─────────────────────────────────────┐
                  │ 12. Mecánico registra diagnóstico   │
                  │      - Estado general (Picker)      │
                  │      - Vehículo operativo (Switch)  │
                  │      - Observaciones (Editor)       │
                  │      - Recomendaciones futuras      │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 13. Mecánico cierra la orden        │
                  │     - Sistema recalendariza:        │
                  │       next_km = km_servicio +       │
                  │       interval_km                   │
                  │     - Alterna tipo servicio A↔B     │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 14. Sistema genera PDF automático   │
                  │     de la orden finalizada          │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                  ┌─────────────────────────────────────┐
                  │ 15. Jefe verifica nuevo cronograma  │
                  │     en Dashboard BI                 │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                              ┌──────────┐
                              │ [FIN]    │
                              └──────────┘

### Actividades en paralelo (Fork/Join)
En los pasos 8-9-11-12, el Sistema ejecuta 3 procesos paralelos:

        ┌─────────────────────────────────────────────────────┐
        │ 9a. Sistema actualiza stock del inventario (FIFO)   │ ←─ Fork
        │     (resta cantidad consumida del lote, si llega    │
        │      a 0 cambia estado a "agotado")                 │
        └─────────────────────────────────────────────────────┘
        ┌─────────────────────────────────────────────────────┐
        │ 9b. Sistema verifica alertas de stock bajo          │
        │     (si stock_total < stock_minimum → alerta)       │
        └─────────────────────────────────────────────────────┘
        ┌─────────────────────────────────────────────────────┐
        │ 9c. Sistema actualiza KPIs del dashboard BI         │
        │     (costo/km, tasa emergencia, servicios del mes)  │
        └─────────────────────────────────────────────────────┘
                                                                   ←─ Join
### Flujo de objetos entre actividades
- "Orden" (objeto) fluye de "Crear orden" → "Ejecutar mantenimiento" → "Cerrar orden"
- "Diagnóstico" fluye de "Registrar diagnóstico" → "Cerrar orden"
- "PDF" fluye de "Generar PDF" → (disponible para descarga)
- "Alerta" fluye de "Verificar alertas" → "Notificar al jefe"
```

---

## 4. DIAGRAMA SECUNDARIO: VERIFICACIÓN AUTOMÁTICA DE ALERTAS

### Swimlanes: Sistema, Jefe

```
[INICIO] (automático al iniciar sesión o al marcar alerta como leída)
  │
  ▼
┌─────────────────────────────────────────────────────┐
│ Sistema ejecuta 4 verificaciones en paralelo:       │
└────────┬────────┬────────┬──────────────────────────┘
         │        │        │
         ▼        ▼        ▼
┌──────────────┐┌──────────────┐┌──────────────┐┌──────────────┐
│ Verificar    ││ Verificar    ││ Verificar    ││ Verificar    │
│ MANTENIMIENTO││ STOCK BAJO   ││ LOTES POR    ││ COMPONENTES  │
│ PRÓXIMO KM   ││              ││ VENCER       ││ POR CADUCAR  │
│ (vs schedule)││ (stock_total ││ (expir. ≤30d)││ (expir. ≤30d)│
└──────┬───────┘│ < stock_min) │└──────┬───────┘└──────┬───────┘
       │        └──────┬───────┘       │               │
       ▼               ▼               ▼               ▼
  ┌────────┐      ┌────────┐      ┌────────┐      ┌────────┐
  │¿Hay    │      │¿Hay    │      │¿Hay    │      │¿Hay    │
  │alerta  │      │alerta  │      │alerta  │      │alerta  │
  │activa? │      │activa? │      │activa? │      │activa? │
  └───┬────┘      └───┬────┘      └───┬────┘      └───┬────┘
      │ SÍ            │ SÍ            │ SÍ            │ SÍ
      ▼               ▼               ▼               ▼
┌──────────────┐┌──────────────┐┌──────────────┐┌──────────────┐
│Insertar      ││Insertar      ││Insertar      ││Insertar      │
│alerta en     ││alerta en     ││alerta en     ││alerta en     │
│alert_log     ││alert_log     ││alert_log     ││alert_log     │
└──────┬───────┘└──────┬───────┘└──────┬───────┘└──────┬───────┘
       │                │               │               │
       └────────────────┴───────────────┴───────────────┘
                            │ (Join de todas las alertas)
                            ▼
                  ┌─────────────────────────────────────┐
                  │ Sistema retorna lista de alertas    │
                  │ activas al Jefe                     │
                  └─────────────────┬───────────────────┘
                                    │
                                    ▼
                          ┌───────────────────────┐
                          │ Jefe visualiza        │
                          │ alertas en la app     │
                          │ (pantalla Alertas)    │
                          └───────────┬───────────┘
                                      │
                                      ▼
                              ┌───────────────┐     NO
                              │ ¿Hay          │────────► [FIN]
                              │ alertas       │
                              │ nuevas?       │
                              └───────┬───────┘
                                      │ SÍ
                                      ▼
                          ┌───────────────────────┐
                          │ Jefe marca alerta     │
                          │ como leída            │
                          └───────────┬───────────┘
                                      │
                                      ▼
                          ┌───────────────────────┐
                          │ ¿Jefe resuelve        │
                          │ la alerta?            │── NO ──► [FIN]
                          │ (Admin only)          │
                          └───────┬───────────────┘
                                  │ SÍ
                                  ▼
                          ┌───────────────────────┐
                          │ Jefe resuelve alerta  │
                          │ (updated_at, resolved │
                          │  = true)              │
                          └───────────┬───────────┘
                                      │
                                      ▼
                                  ┌──────┐
                                  │ [FIN]│
                                  └──────┘
```

---

## 5. DIAGRAMA TERCIARIO: INVENTARIO — CICLO DE MATERIAL

### Swimlanes: Jefe, Sistema

```
[INICIO]
  │
  ▼
┌─────────────────────────────────┐
│ Jefe accede a pantalla          │
│ Inventario                      │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│ ¿Crear material o               │
│ ingresar lote?                  │── Crear ──► ┌─────────────────┐
│ (Picker en UI)                  │             │ Jefe crea       │
└────────┬────────────────────────┘             │ nuevo material  │
         │ Ingresar lote                        │ (nombre, cat,   │
         ▼                                      │  unidad, stock  │
┌─────────────────────────────────┐             │  mínimo)        │
│ Jefe selecciona material        │             └────────┬────────┘
│ existente                       │                      │
└────────┬────────────────────────┘                      ▼
         │                                      ┌─────────────────┐
         ▼                                      │ Sistema guarda  │
┌─────────────────────────────────┐             │ material en BD  │
│ Jefe ingresa datos del lote:    │             └────────┬────────┘
│ - Cantidad inicial                                    │
│ - Costo unitario                    ◄──────────────────┘
│ - Fecha de vencimiento (opcional)
│ - Número de lote (proveedor)
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│ Sistema registra lote y         │
│ actualiza stock_total del       │
│ material                        │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐     NO
│ ¿Descartar lote existente       │────────► [FIN]
│ por vencimiento/deterioro?      │
└────────┬────────────────────────┘
         │ SÍ
         ▼
┌─────────────────────────────────┐
│ Jefe selecciona lote a          │
│ descartar (cantidad, motivo)    │
└────────┬────────────────────────┘
         │
         ▼
┌─────────────────────────────────┐
│ Sistema ejecuta descarte:       │
│ - Inserta en material_discard   │
│ - Cambia lote a "descartado"    │
│ - Genera alerta de descarte     │
└────────┬────────────────────────┘
         │
         ▼
     ┌──────┐
     │ [FIN]│
     └──────┘
```

---

## 6. DIAGRAMA: LOGIN + SESIÓN

### Swimlanes: Trabajador, Sistema

```
[INICIO]
  │
  ▼
┌─────────────────────────────────────┐
│ Trabajador abre la app              │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐     ┌────────────────────┐
│ Sistema verifica token almacenado   │── ¿Token válido y no    │
│ en SecureStorage                    │    expirado (8h)?       │
└─────────────────┬───────────────────┘    └─────────┬──────────┘
                  │ NO (no hay token)                 │ SÍ
                  ▼                                   ▼
┌─────────────────────────────┐         ┌───────────────────────────┐
│ Pantalla de Login:          │         │ Sistema restaura sesión:  │
│ Trabajador ingresa:         │         │ - Carga token             │
│ - Username                  │         │ - Configura HttpClient    │
│ - Password                  │         │ - Navega a Dashboard     │
│ (desde dispositivo Android) │         └──────────┬────────────────┘
└────────┬────────────────────┘                     │
         │                                          ▼
         ▼                                    ┌──────────┐
┌─────────────────────────────┐                │ Dashboard│
│ Sistema valida contra API:  │                │ con KPIs │
│ POST /api/v1/auth/login     │                └──────────┘
│ - Busca en public.worker    │
│ - Verifica password hash    │
└────────┬────────────────────┘
         │
         ▼
          ┌──────────────┐     NO
          │ ¿Credenciales├──────► ┌──────────────────┐
          │ válidas?     │        │ Mostrar error:    │
          └──────┬───────┘        │ "Credenciales     │
                 │ SÍ             │ incorrectas"     │
                 ▼                └────────┬─────────┘
┌──────────────────────────────┐           │
│ Sistema genera JWT con rol:  │           ▼
│ - Admin (jefe)               │      ┌──────────┐
│ - Técnico (mecánico)         │      │ [FIN]    │
│ Expiración: 8h               │      └──────────┘
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Sistema guarda en dispositivo│
│ - SecureStorage: token       │
│ - Preferences: expires_at    │
└────────┬─────────────────────┘
         │
         ▼
┌──────────────────────────────┐
│ Sistema redirige según rol:  │
│ - Admin → Dashboard          │
│ - Técnico → Dashboard        │
└────────┬─────────────────────┘
         │
         ▼
    ┌──────────┐
    │ Dashboard│
    └──────────┘
```

---

## 7. DIAGRAMA: CONSUMO DE MATERIAL (FIFO)

### Actividad detallada dentro del flujo de mantenimiento

```
[INICIO] (dentro del detalle de orden)
  │
  ▼
┌─────────────────────────────────────┐
│ Mecánico selecciona material        │
│ del Picker (cargado desde API)      │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ Sistema consulta lotes activos      │
│ del material (FIFO por vencimiento) │
│ GET /api/v1/inventory/materials/{id}│
│ /lots                               │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ Sistema muestra lote más próximo    │
│ a vencer: número, costo, vencimiento│
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ Mecánico ingresa cantidad a consumir │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐     ┌─────────────────┐
│ Sistema verifica stock suficiente   │──NO─► Mostrar error:  │
│ (current_quantity >= cantidad)      │     │ "Stock          │
└─────────────────┬───────────────────┘     │ insuficiente"  │
                  │ SÍ                      └────────┬────────┘
                  ▼                                  │
┌─────────────────────────────────────┐              │
│ Sistema ejecuta POST /consume:      │              │
│ - Crea registro material_consumption│              │
│ - Resta cantidad del lote           │              ▼
│ - Si current_quantity = 0,          │         ┌──────────┐
│   marca lote como "agotado"         │         │ [FIN]    │
│ - Actualiza stock_total del material│         └──────────┘
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ Sistema agrega item a lista local   │
│ de consumidos (con costo total)     │
└─────────────────┬───────────────────┘
                  │
                  ▼
┌─────────────────────────────────────┐
│ ¿Mecánico quiere calificar         │
│ el material?                        │── NO ──► ┌──────────┐
│ (Rating 1-5 estrellas +             │          │ [FIN]    │
│  observación opcional)              │          └──────────┘
└─────────────────┬───────────────────┘
                  │ SÍ
                  ▼
┌─────────────────────────────────────┐
│ Rating guardado LOCALMENTE          │
│ (se envía al API al guardar         │
│  diagnóstico: PersistPendingActions)│
└─────────────────┬───────────────────┘
                  │
                  ▼
              ┌──────────┐
              │ [FIN]    │
              └──────────┘
```

---

## 8. RESUMEN DE DECISIONES (NODOS DIAMANTE)

| # | Decisión | Condición SÍ | Condición NO |
|---|----------|-------------|--------------|
| D1 | ¿Hay vehículo próximo a mantenimiento? | Iniciar flujo de mantenimiento | Permanecer en Dashboard |
| D2 | Tipo: Calendarizado o Emergencia? | Cargar checklist completo | Cargar checklist parcial (solo urgente) |
| D3 | ¿Emergencia completa o parcial? | Recalendarizar al cerrar | No recalendarizar |
| D4 | ¿Instala componente? | Mostrar picker de componentes | Continuar a diagnóstico |
| D5 | ¿Token almacenado válido? | Restaurar sesión → Dashboard | Mostrar pantalla Login |
| D6 | ¿Credenciales válidas? | Generar JWT → Dashboard | Mostrar error de login |
| D7 | ¿Stock suficiente para consumir? | Ejecutar consumo | Mostrar "stock insuficiente" |
| D8 | ¿Calificar material? | Guardar rating local | Continuar sin rating |
| D9 | ¿Descartar lote existente? | Mostrar formulario de descarte | Finalizar gestión inventario |
| D10 | ¿Hay alertas nuevas? | Mostrar en pantalla Alertas | Dashboard sin alertas |
| D11 | ¿Admin resuelve alerta? | Ejecutar resolución | Dejar como leída |

---

## 9. TRANSICIONES PARALELAS (FORK/JOIN)

### Fork 1: Al guardar diagnóstico + cerrar orden
Se ejecutan simultáneamente:
1. Persistir acciones pendientes (PendingActions)
2. Persistir ratings locales (PendingRatings)
3. Guardar diagnóstico en BD
4. Sistema recalendariza schedule

### Fork 2: Al verificar alertas
Se ejecutan simultáneamente:
1. Verificar MANTENIMIENTO_PRÓXIMO_KM (vs vehicle_schedule)
2. Verificar STOCK_BAJO (vs material.stock_minimum)
3. Verificar LOTE_POR_VENCER (vs material_lot.expiration_date)
4. Verificar COMPONENTE_POR_CADUCAR (vs installed_component.expiration_date)

---

## 10. CONDICIONES DE PROTECCIÓN (GUARDS)

| Transición | Guard |
|------------|-------|
| Login → Admin Dashboard | `rol == "Admin"` |
| Login → Técnico Dashboard | `rol == "Técnico"` |
| Crear material | `rol == "Admin"` |
| Resolver alerta | `rol == "Admin"` |
| Crear lote | `rol == "Admin"` |
| Cerrar orden | `diagnóstico_saved == true AND status == "AC"` |
| Exportar PDF | `status == "FI"` |
| Activar input | `status == "AC"` (no read-only) |
| Mostrar ✕ eliminar | `status == "AC"` (no read-only) |
| Consumir material | `stock_total >= cantidad_solicitada` |
| Instalar componente | `componente_seleccionado != null` |

---

## 11. ESPECIFICACIONES PARA EL DIBUJO

### Tamaño sugerido
- Diagrama principal (Ciclo de Mantenimiento): ~60x80 cm
- Diagrama secundario (Alertas): ~40x50 cm
- Diagrama terciario (Inventario): ~30x40 cm

### Agrupación
- Cada swimlane debe tener un color de fondo distinto:
  - Jefe: Azul claro (#E3F2FD)
  - Mecánico: Verde claro (#E8F5E9)
  - Sistema: Gris claro (#F5F5F5)
  - Trabajador: Naranja claro (#FFF3E0)

### Notas adicionales
- Las actividades automáticas del Sistema (actualizar stock, verificar alertas, recalendarizar) deben tener el estereotipo `«automático»` para distinguirlas de las actividades manuales.
- Los flujos de objeto (datos) deben usar flechas punteadas con el nombre del objeto entre corchetes, ej: `[Orden], [Diagnóstico], [PDF], [Alerta]`.
- Las actividades que son pantallas de la app pueden tener el estereotipo `«pantalla»`.
