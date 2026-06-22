## Figura X. Diagrama de Actividades de Análisis de Business Intelligence — MaintManager

**Descripción:**
Representa el flujo de análisis del Jefe de Mantenimiento al ingresar al BI Dashboard. El diagrama muestra cómo el usuario observa los indicadores clave de rendimiento y analiza los cinco gráficos interactivos para identificar problemas de costos, tasas de emergencia, tendencias de gasto, riesgos de caducidad de inventario y cumplimiento del calendario de mantenimientos. A partir de este análisis, toma decisiones correctivas o preventivas navegando hacia los módulos operativos correspondientes.

---

## 2. Tipo de diagrama

Es un:

**Diagrama de Actividades UML**

Su objetivo es mostrar:

- Observación de KPIs financieros y operativos
- Análisis visual mediante gráficos interactivos
- Identificación de anomalías y patrones
- Decisiones basadas en datos
- Navegación hacia acciones correctivas

---

## 3. Participantes (Swimlanes)

El diagrama está dividido en **un carril** porque es una actividad realizada exclusivamente por un actor.

### Carril 1: Jefe de Mantenimiento (Admin)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de:**
- Acceder al módulo de BI Dashboard (exclusivo para Admin)
- Observar los seis KPIs de resumen
- Analizar cinco gráficos interactivos con datos históricos de la flota
- Detectar anomalías: vehículos costosos, alta tasa de emergencia, lotes críticos por vencer, servicios tardíos
- Tomar decisiones de gestión basadas en el análisis
- Navegar hacia los módulos operativos para ejecutar acciones

---

## 4. Elementos gráficos utilizados

### A. Nodo Inicial

**Figura:**
- ● Círculo negro sólido

**Ubicación:**
Parte superior izquierda.

### B. Actividades

**Figura:**
- Rectángulo con esquinas redondeadas
- Fondo blanco, borde azul

### C. Decisiones

**Figura:**
- Rombo, borde naranja

### D. Flujo de Control

**Figura:**
- Flechas negras continuas

### E. Nodo Final

**Figura:**
- ◎ Círculo negro dentro de un círculo

---

## 5. Explicación paso a paso

---

### Fase 1: Acceso al BI Dashboard

**Actividad 1**
`Ingresar al BI Dashboard`

**Actor:** Jefe de Mantenimiento (Admin)

**Objetivo:**
El Jefe accede al módulo de inteligencia de negocio desde el menú lateral o desde la acción rápida del Panel Principal. Este módulo está restringido exclusivamente a su rol.

---

### Fase 2: Observación de KPIs de Resumen

**Actividad 2**
`Observar indicadores clave de resumen`

**Actor:** Jefe de Mantenimiento

**Indicadores mostrados en la parte superior del BI Dashboard:**

| KPI | Qué mide | Interpretación |
|---|---|---|
| **Total Vehículos** | Cantidad de vehículos activos en la flota | Dimensión de la operación |
| **Servicios del Mes** | Mantenimientos realizados en el mes actual | Carga de trabajo del taller |
| **Stock Bajo** | Materiales por debajo del mínimo | Riesgo de desabastecimiento |
| **Alertas sin Resolver** | Alertas pendientes de atención | Problemas no atendidos |
| **Costo Promedio por Km** | Costo en soles por kilómetro recorrido (toda la flota) | Eficiencia económica general |
| **Tasa de Emergencia Global** | Porcentaje de mantenimientos que fueron emergencias | Confiabilidad de la flota |

**Decisión** `¿Algún KPI está fuera de lo esperado?`

- **Si el Costo Promedio por Km es elevado:**
  - → Pasar al **Gráfico 1: Costo por Km** para identificar qué vehículos elevan el promedio

- **Si la Tasa de Emergencia es alta:**
  - → Pasar al **Gráfico 2: Tasa de Emergencia** para identificar los vehículos problemáticos

- **Si Stock Bajo o Alertas están elevados:**
  - → Ir directamente a **Inventario** o **Alertas**

---

### Fase 3: Análisis del Gráfico 1 — Costo por Kilómetro

**Actividad 3**
`Analizar gráfico de Costo por Kilómetro`

**Tipo de gráfico:** Barras verticales
**Qué muestra:** Los 10 vehículos con mayor costo de materiales por kilómetro recorrido

**Análisis:**

Cada barra representa un vehículo. La altura indica cuántos soles gasta en materiales por cada kilómetro que recorre. Un vehículo con barra alta es más caro de mantener que uno con barra baja.

**Decisión** `¿Hay vehículos con costo por km anormalmente alto?`

- **Si SÍ:**
  - El vehículo está generando un gasto excesivo en materiales
  - → Investigar el **Historial del Vehículo** para revisar sus órdenes de mantenimiento
  - → Determinar si el gasto se debe a fallas recurrentes, uso de materiales inadecuados o emergencias frecuentes
  - → Tomar acción: programar revisión exhaustiva, cambiar tipo de material, analizar causa raíz

- **Si NO:**
  - Los costos están distribuidos de manera uniforme
  - → Continuar con el siguiente gráfico

---

### Fase 4: Análisis del Gráfico 2 — Tasa de Emergencia

**Actividad 4**
`Analizar gráfico de Tasa de Emergencia`

**Tipo de gráfico:** Barras horizontales
**Qué muestra:** Los 8 vehículos con mayor porcentaje de mantenimientos por emergencia frente al total de sus servicios

**Análisis:**

Cada barra horizontal representa un vehículo. Muestra qué porcentaje de todos sus mantenimientos fueron emergencias (no programados). Un vehículo con más del 50% de emergencias indica poca confiabilidad.

**Decisión** `¿Hay vehículos con tasa de emergencia superior al 30%?`

- **Si SÍ:**
  - El vehículo está fallando con frecuencia inesperada
  - → Revisar el **Historial del Vehículo** para ver qué tipo de emergencias ha tenido
  - → Determinar patrón: ¿siempre es el mismo sistema? ¿frenos, motor, eléctrico?
  - → Tomar acción: programar mantenimiento integral, evaluar reemplazo del vehículo, reforzar inspecciones preventivas

- **Si NO:**
  - La flota tiene buena confiabilidad general
  - → Continuar con el siguiente gráfico

---

### Fase 5: Análisis del Gráfico 3 — Costo Mensual

**Actividad 5**
`Analizar gráfico de Costo Mensual`

**Tipo de gráfico:** Línea de tiempo
**Qué muestra:** La evolución del gasto mensual en materiales de los últimos 6 meses, con una línea por cada vehículo

**Análisis:**

La línea asciende o desciende según cuánto se gastó en mantenimiento cada mes. Permite ver tendencias: ¿el gasto está aumentando, disminuyendo o es estable?

**Decisión** `¿La tendencia de costo es creciente?`

- **Si SÍ (la línea sube mes a mes):**
  - El gasto en mantenimiento está aumentando
  - → Identificar en qué mes comenzó la tendencia alcista
  - → Cruzar con los otros gráficos para ver si coincide con algún vehículo específico o con aumento de emergencias
  - → Tomar acción: revisar políticas de mantenimiento preventivo, negociar precios de materiales, investigar causas

- **Si NO (la línea es estable o decreciente):**
  - El gasto está controlado
  - → Continuar con el siguiente gráfico

---

### Fase 6: Análisis del Gráfico 4 — Lotes por Vencer

**Actividad 6**
`Analizar gráfico de Lotes por Vencer`

**Tipo de gráfico:** Circular (torta)
**Qué muestra:** La distribución del inventario según su urgencia de vencimiento: crítico (7 días o menos), próximo (entre 8 y 30 días) y normal (más de 30 días)

**Análisis:**

El gráfico de torta muestra tres porciones. Una porción roja grande significa que hay mucho material a punto de perderse.

**Decisión** `¿Hay una porción significativa en estado crítico o próximo?`

- **Si SÍ (hay material por vencer en menos de 30 días):**
  - Existe riesgo de pérdida económica por caducidad
  - → Ir a **Inventario** para identificar cuáles son los lotes afectados
  - → Planificar su uso prioritario en los próximos mantenimientos
  - → Si no se puede usar a tiempo, proceder al descarte

- **Si NO (todo está en estado normal):**
  - El inventario está saludable
  - → Continuar con el siguiente gráfico

---

### Fase 7: Análisis del Gráfico 5 — Cumplimiento del Calendario

**Actividad 7**
`Analizar gráfico de Cumplimiento del Calendario`

**Tipo de gráfico:** Barras verticales agrupadas
**Qué muestra:** Los 10 vehículos con mayor desviación en kilómetros entre el servicio programado y el servicio real. Colores: verde (puntual), naranja (anticipado), rojo (tardío)

**Análisis:**

Cada barra es un vehículo. Si la barra es roja, el mantenimiento se hizo muy por encima del kilometraje programado (tardío). Si es naranja, se hizo muy por debajo (anticipado, desperdiciando vida útil). Si es verde, se hizo en el momento correcto.

**Decisión** `¿Hay vehículos con desviación significativa en rojo o naranja?`

- **Si SÍ (hay servicios tardíos):**
  - Se está excediendo el kilometraje de servicio, lo que aumenta el desgaste y el riesgo de falla
  - → Revisar la **Agenda** para ver la situación actual de esos vehículos
  - → Reforzar el control de kilometraje y la puntualidad en las inspecciones

- **Si SÍ (hay servicios anticipados):**
  - Se está haciendo el mantenimiento antes de tiempo, desperdiciando vida útil de materiales
  - → Ajustar la planificación para optimizar el intervalo de servicio

- **Si NO (la mayoría están en verde):**
  - El taller está cumpliendo con el calendario
  - → El análisis ha concluido satisfactoriamente

---

### Fase 8: Toma de Decisiones y Navegación a la Acción

**Actividad 8**
`Decidir acción correctiva o preventiva`

**Actor:** Jefe de Mantenimiento

Basado en todo el análisis anterior, el Jefe elige hacia dónde dirigirse:

| Si el análisis reveló... | Navegar hacia... | Para... |
|---|---|---|
| Vehículo con costo/km elevado | Historial del Vehículo | Revisar órdenes pasadas y causas del gasto |
| Vehículo con alta tasa de emergencia | Historial del Vehículo | Investigar fallas recurrentes |
| Tendencia de costo mensual creciente | Lista de Mantenimientos | Revisar servicios recientes |
| Lotes críticos por vencer | Inventario | Usar prioritariamente o descartar |
| Servicios tardíos frecuentes | Agenda | Revisar estado actual de la flota |
| Múltiples anomalías detectadas | Panel Principal | Coordinar acciones con el equipo |

---

### Fase 9: Fin del Análisis

**Nodo Final**

**Figura:** ◎ Círculo negro dentro de un círculo

**Significado:**
El Jefe de Mantenimiento completó el ciclo de análisis de inteligencia de negocio. Los cinco gráficos fueron revisados, las anomalías fueron identificadas y se determinaron las acciones correctivas a ejecutar. El BI Dashboard cumplió su función de transformar datos operativos en decisiones de gestión.

---

## 6. Resumen: Los 5 gráficos y sus decisiones asociadas

| Gráfico | Tipo visual | Qué busca detectar | Decisión si es anómalo |
|---|---|---|---|
| **Costo por Km** | Barras verticales | Vehículos excesivamente caros de mantener | Investigar historial del vehículo |
| **Tasa de Emergencia** | Barras horizontales | Vehículos con muchas fallas inesperadas | Revisar causas raíz, evaluar reemplazo |
| **Costo Mensual** | Línea de tiempo | Tendencia de gasto creciente | Ajustar políticas de mantenimiento preventivo |
| **Lotes por Vencer** | Circular (torta) | Material próximo a caducar | Usar prioritariamente o descartar |
| **Cumplimiento Calendario** | Barras agrupadas (color) | Servicios tardíos o anticipados | Reforzar control de kilometraje y puntualidad |
