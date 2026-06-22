# Guión de Exposición — MaintManager
## Reemplazo de Diagramas de Secuencia y Actividades con Demo en Vivo

> **Tiempo total:** 15 min teoría + 5 min demo = 20 min  
> **Objetivo:** Explicar los flujos del sistema mostrando la app funcionando,  
> sin necesidad de diagramas de secuencia ni actividades.

---

## ESTRUCTURA GENERAL (15 min teoría)

| Bloque | Tiempo | Contenido |
|--------|--------|-----------|
| 1. Contexto | 2 min | Empresa, problema, objetivos |
| 2. Arquitectura | 2 min | Clean Architecture + MVVM, stack |
| 3. Flujo Login + Dashboard | 3 min | Caso de uso: autenticación y KPIs |
| 4. Flujo Mantenimiento | 4 min | Ciclo completo: crear → diagnosticar → cerrar |
| 5. Flujo Alertas + Inventario | 2 min | Automatización y control |
| 6. Business Intelligence | 2 min | Dashboard BI, KPIs dinámicos |
| **DEMO** | **5 min** | App en vivo |

---

## BLOQUE 1 — CONTEXTO (2 min)

**Diapositiva:** Logo NeoPlus + problema + objetivos

**Lo que dices:**
> Neo Plus Business S.A.C. alquila vehículos. Su problema: los datos de mantenimiento están en cuadernos, hojas de cálculo y comunicaciones verbales — no hay trazabilidad. No saben cuánto cuesta mantener cada vehículo por kilómetro, qué componentes fallan más, ni si el mantenimiento preventivo se está cumpliendo.
>
> El objetivo del proyecto MaintManager es centralizar, estructurar y analizar los datos de mantenimiento para convertirlos en inteligencia de negocio accionable.

---

## BLOQUE 2 — ARQUITECTURA (2 min)

**Diapositiva:** Diagrama de arquitectura (Clean Architecture + MVVM)

**Lo que dices:**
> El sistema usa Clean Architecture en el backend (API .NET 10 con PostgreSQL) y MVVM en el frontend (.NET MAUI). Clean Architecture separa el código en capas: Domain (entidades de negocio), Application (casos de uso), Infrastructure (acceso a BD) y API (controllers). En el frontend, MVVM separa la UI (XAML) de la lógica (ViewModels) y los servicios.
>
> Las vistas de BD (vw_*) están en Infrastructure y son consultadas directamente por los controladores para generar los reportes BI sin procesamiento adicional.

---

## BLOQUE 3 — FLUJO LOGIN + DASHBOARD (3 min)
*→ Reemplaza al Diagrama de Secuencia "Dashboard BI en tiempo real"*

**Diapositiva:** Mockup de Login + Dashboard o capturas de pantalla

**Lo que dices:**
> El flujo comienza con el login. El usuario ingresa credenciales, el API valida contra PostgreSQL y devuelve un token JWT con rol (Admin o Técnico). El token se almacena en SecureStorage con expiración de 8 horas.
>
> Al ingresar al Dashboard, el sistema carga:
> 1. **KPIs** desde `vw_bi_dashboard_summary` — total vehículos, servicios del mes, stock bajo, alertas
> 2. **Estadísticas** desde el endpoint `/maintenances/stats`
> 3. **Acciones rápidas** que varían según el rol (Admin ve "Nuevo Mantenimiento" directo al wizard, Técnico ve la lista)
>
> **EN VIVO:** Voy a mostrar el login, el dashboard con KPIs y las acciones rápidas.

---

## BLOQUE 4 — FLUJO MANTENIMIENTO (4 min)
*→ Reemplaza al Diagrama de Secuencia "Ciclo de Mantenimiento Calendarizado"*

**Diapositiva:** Capturas del wizard, detalle de orden, cierre

**Lo que dices:**
> Este es el flujo principal del sistema. Tiene 4 etapas:

> **Etapa 1 — Crear orden:** El usuario selecciona vehículo, tipo de mantenimiento (Calendarizado o Emergencia), técnico asignado, kilometraje y nota. Se guarda vía POST a `/api/v1/maintenances`.

> **Etapa 2 — Ejecutar:** En el detalle de la orden, el técnico puede:
>   - **Agregar acciones** del checklist (Picker + botón +)
>   - **Consumir materiales** con selección FIFO del lote más antiguo y cálculo automático de costo
>   - **Instalar componentes** con seguimiento de vida útil
>   - Cada item se acumula localmente hasta guardar diagnóstico

> **Etapa 3 — Diagnosticar:** El técnico registra estado general (Picker: Excelente a Malo), operatividad del vehículo, observaciones y recomendaciones futuras. Al guardar, se persisten también las acciones pendientes.

> **Etapa 4 — Cerrar:** Una vez diagnosticado, se habilita el botón "Cerrar Orden". El sistema actualiza el schedule del vehículo (próximo kilometraje de servicio) y genera alertas si es necesario.
>
> **EN VIVO:** Voy a crear una orden, agregar acciones, consumir material, guardar diagnóstico y cerrar la orden.

---

## BLOQUE 5 — FLUJO ALERTAS + INVENTARIO (2 min)
*→ Reemplaza al Diagrama de Secuencia "Verificación Automática de Alertas"*

**Diapositiva:** Captura de pantalla de Alertas y/o Inventario

**Lo que dices:**
> El sistema de alertas es automático. Se ejecuta:
> - Al iniciar sesión
> - Al marcar una alerta como leída (se elimina la condición y se re-evalúa)
> - Manualmente con el botón "Verificar alertas ahora"
>
> Las alertas verifican:
> 1. **Mantenimiento próximo** — compara kilometraje actual + schedule del vehículo
> 2. **Stock bajo** — materiales con stock_total < stock_minimum
> 3. **Lotes por vencer** — lotes con expiration_date ≤ 30 días
> 4. **Componentes por caducar** — componentes instalados próximos a vencer
>
> En el inventario, los materiales tienen lotes con costo unitario, fecha de vencimiento y cantidad. El consumo se hace con método FIFO (First In, First Out).
>
> **EN VIVO:** Voy a mostrar las alertas activas, marcar una como leída, y ver el inventario con lotes.

---

## BLOQUE 6 — BUSINESS INTELLIGENCE (2 min)

**Diapositiva:** Captura del Dashboard BI con gráficos

**Lo que dices:**
> El Dashboard BI tiene 6 indicadores y 5 gráficos que se actualizan en tiempo real con cada operación:
>
> **KPIs:** Vehículos totales, servicios del mes, stock bajo, alertas, costo flota/km, tasa de emergencia
>
> **Gráficos:**
> 1. **Costo por Km** — Top 10 vehículos, columnas (datos de `vw_cost_per_km`)
> 2. **Tasa de Emergencia** — Vehículos con mayor porcentaje de emergencias (`vw_emergency_rate`)
> 3. **Costo Mensual** — Línea de tendencia últimos 6 meses (`vw_monthly_cost`)
> 4. **Lotes por Vencer** — Pastel: crítico (≤7d), próximo (≤30d), normal (>30d)
> 5. **Desviación Km** — Top vehículos que más se desvían del schedule (`vw_calendar_compliance`)
>
> **EN VIVO:** Voy a mostrar el dashboard BI y explicar cómo cada gráfico se relaciona con los datos operativos.

---

## BLOQUE 7 — DEMO EN VIVO (5 min)

| Tiempo | Qué mostrar | Qué decir |
|--------|-------------|-----------|
| 0:00-0:30 | Login + Dashboard | "Ingreso con credenciales, ven los KPIs cargados desde la BD" |
| 0:30-1:00 | Acciones rápidas | "Admin puede ir directo a crear orden, Técnico va a lista" |
| 1:00-2:00 | Crear orden | "Selecciono vehículo, tipo Calendarizado, técnico, guardo" |
| 2:00-3:00 | Detalle + acciones + material | "Agrego acción del checklist, consumo material con FIFO" |
| 3:00-3:30 | Guardar diagnóstico | "Estado Bueno, operativo, observaciones, guardo" |
| 3:30-4:00 | Cerrar orden | "Se cierra la orden, cambia a FI, queda solo lectura" |
| 4:00-4:30 | Dashboard BI | "Muestro los 5 gráficos con datos actualizados" |
| 4:30-5:00 | Alertas | "Muestro alertas activas, explico las verificaciones automáticas" |

---

## MAPA: Diagramas del PDF → Demo en Vivo

| Diagrama en PDF | Dónde se ve en la app | Lo que demuestras |
|-----------------|----------------------|-------------------|
| DOP Actual (Cuadro 1) | No aplica (proceso manual) | "Así era antes: 15 pasos manuales" |
| DAP Actual (Cuadro 2) | No aplica (proceso manual) | "Operaciones, transportes, demoras" |
| Casos de Uso (Cuadro 8) | Toda la app | "Cada caso de uso es una pantalla: Login, Dashboard, Crear orden, etc." |
| Diagrama de Clases (Cuadro 9) | Backend (entidades) | "Estas 20+ entidades se mapean a tablas PostgreSQL" |
| Secuencia 1: Ciclo Mantenimiento (Cuadro 10) | **Demo completa**: Wizard → Detalle → Diagnóstico → Cerrar | "El flujo completo en vivo, paso a paso" |
| Secuencia 2: Dashboard BI (Cuadro 11) | **Demo**: Dashboard BI | "Los 5 gráficos cargan datos desde vistas SQL en tiempo real" |
| Secuencia 3: Alertas (Cuadro 12) | **Demo**: Alertas | "Verificación automática al iniciar, al leer, o manual" |
| Diagrama Arquitectura (Cuadro 13) | Diapositiva | "Clean Architecture + MVVM, explico capas" |
| UI Mockups (Imágenes 3-8) | **La app real** | "Cada mockup se implementó tal cual en MAUI" |

---

## TIPS PARA LA EXPOSICIÓN

1. **Cuando te pregunten "¿y el diagrama de secuencia?"**
   > "El diagrama de secuencia describe el flujo teórico. En lugar de mostrarlo en una diapositiva, prefiero ejecutarlo en vivo en la app, porque así se ve el comportamiento real del sistema, incluyendo los casos borde y la interacción con la base de datos."

2. **Cuando te pregunten "¿y el diagrama de actividades?"**
   > "El diagrama de actividades describe las decisiones del flujo. En la app, eso se traduce en: si el mantenimiento es Calendarizado o Emergencia, si el usuario es Admin o Técnico, si la orden está Activa o Finalizada. Prefiero mostrarlo en vivo porque se entiende mejor visualmente."

3. **Para la pregunta "¿cómo sabes que funciona?"**
   > "Los 5 gráficos del Dashboard BI se alimentan de vistas SQL que consultan los datos reales. Cada mantenimiento que hacemos en la demo actualiza los KPIs automáticamente — eso es la prueba de que el flujo completo funciona."

4. **Qué hacer si falla la demo:**
   - Muestra capturas de pantalla del dashboard con datos (prepara 3-4 capturas)
   - Sigue la explicación con las diapositivas
   - Di: "En el dispositivo la app funciona, permítanme mostrarles las capturas de los resultados reales"
