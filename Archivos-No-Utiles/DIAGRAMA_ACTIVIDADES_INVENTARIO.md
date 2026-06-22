## Figura X. Diagrama de Actividades de Almacenamiento y Control de Inventario — MaintManager

**Descripción:**
Representa el flujo completo de gestión de inventario del sistema MaintManager. Cubre el ingreso de materiales y lotes al almacén, el consumo durante mantenimientos aplicando el criterio de agotar primero lo que vence antes, el descarte por vencimiento o merma, la calificación de materiales por los mecánicos, y el monitoreo automático de stock bajo y caducidad.

---

## 2. Tipo de diagrama

Es un:

**Diagrama de Actividades UML**

Su objetivo es mostrar:

- Ingreso de materiales al almacén (creación de material, registro de lote)
- Salida de materiales (consumo en mantenimiento, descarte por merma)
- Criterio de consumo: priorizar lotes con fecha de vencimiento más cercana
- Control de stock total contra stock mínimo
- Calificación de materiales por parte de los mecánicos
- Monitoreo automático y generación de alertas
- Diferenciación de permisos por rol

---

## 3. Participantes (Swimlanes)

El diagrama está dividido en **dos columnas verticales**.

### Carril 1: Jefe de Mantenimiento (Admin)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de:**
- Crear nuevos materiales en el catálogo
- Registrar lotes de ingreso al almacén (cantidad, costo, vencimiento, proveedor)
- Descartar lotes por vencimiento, merma o daño
- Revisar el estado general del inventario
- Resolver alertas de stock bajo y lotes por vencer
- Planificar reabastecimiento

### Carril 2: Mecánico (Técnico)

**Color:**
- Fondo blanco
- Encabezado gris claro

**Responsable de:**
- Consultar disponibilidad de materiales y componentes
- Consumir materiales durante un mantenimiento
- Decidir si el consumo descuenta del inventario o es externo
- Calificar materiales usados (1 a 5 estrellas)

---

## 4. Elementos gráficos utilizados

### A. Nodo Inicial

**Figura:**
- ● Círculo negro sólido

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

### E. Flujo Automático

**Figura:**
- Línea punteada

**Representa:**
Procesos que el sistema ejecuta por sí solo.

### F. Nodo Final

**Figura:**
- ◎ Círculo negro dentro de un círculo

---

## 5. Explicación paso a paso

---

## RAMAL A: Ingreso de Materiales al Almacén

---

### Fase A1: Acceso al Inventario

**Actividad A1**
`Consultar lista de inventario`

**Actor:** Jefe o Mecánico
**Pantalla:** Inventario

**Objetivo:**
El usuario ingresa al módulo de inventario. La pantalla muestra la lista de materiales y componentes, permite alternar entre ambos, filtrar solo los que están en stock bajo, y buscar por nombre o categoría.

**Decisión** `¿El usuario es Admin o Técnico?`

- **Admin:** Puede crear materiales, registrar ingreso de lotes y descartar
- **Técnico:** Puede consultar la lista, buscar y ver el detalle

---

### Fase A2: Creación de Material

**Decisión** `¿El material ya existe en el catálogo?`

- **Si NO:**

  **Actividad A2a**
  `Crear nuevo material`

  **Actor:** Jefe (Admin)
  **Pantalla:** Crear Material

  **Datos que se registran:**
  - Nombre del material
  - Categoría (lubricantes, filtros, fluidos, repuestos)
  - Unidad de medida (litros, unidad, par, set)
  - Stock mínimo (cantidad que dispara la alerta de stock bajo)
  - Tipo: material o componente

  El material se crea con stock inicial en cero.

- **Si SÍ:**
  - → Pasar directamente al registro de lote

---

### Fase A3: Registro de Lote (Ingreso al Almacén)

**Actividad A3**
`Registrar lote de ingreso`

**Actor:** Jefe (Admin)
**Pantalla:** Ingresar Lote

**Datos que se registran:**
- Material al que pertenece
- Cantidad que ingresa
- Costo unitario
- Fecha de vencimiento (opcional)
- Proveedor
- Identificador del lote del proveedor (opcional)

**Qué ocurre automáticamente:**
- El lote queda disponible con estado activo
- El stock total del material se incrementa en la cantidad ingresada
- Si el material estaba en stock bajo y ahora supera el mínimo, la condición se normaliza

**Regla:** El stock total del material es la suma de las cantidades de todos sus lotes activos.

---

**Decisión** `¿Es un componente (no un material)?`

- **Si es Componente:**
  - Queda disponible para ser instalado en vehículos durante mantenimientos
  - → Fin del ingreso

- **Si es Material:**
  - Queda disponible para consumo en órdenes de mantenimiento
  - → Fin del ingreso

---

## RAMAL B: Consumo de Materiales durante Mantenimiento

---

### Fase B1: Solicitud de Consumo

**Actividad B1**
`Iniciar consumo desde orden de mantenimiento`

**Actor:** Mecánico o Jefe
**Pantalla:** Detalle de Orden → Checklist de Materiales

**Objetivo:**
Durante la ejecución de un mantenimiento, el usuario necesita consumir materiales (aceites, filtros, líquidos, repuestos). El checklist muestra los materiales permitidos para ese vehículo, con su nombre, unidad de medida y stock disponible.

---

### Fase B2: Configuración del Consumo

**Actividad B2**
`Configurar detalle del consumo`

**Actor:** Mecánico

**El usuario define para cada material:**
- **Cantidad:** cuánto va a consumir (debe ser mayor a cero)
- **Origen:** Stock propio (descuenta del inventario) o Externo (no descuenta)
- **Calificación:** 1 a 5 estrellas (opcional)

---

**Decisión** `¿Origen del consumo: Stock propio o Externo?`

- **Stock propio:**
  - Se descuenta del inventario
  - Se consume primero de los lotes que vencen antes
  - → Continuar a validación de stock

- **Externo:**
  - No se descuenta del inventario
  - Se registra para trazabilidad pero no afecta el stock
  - → Saltar directamente al registro del consumo

---

### Fase B3: Validación y Consumo

**Decisión** `¿Hay stock suficiente?`

- **Si NO:**
  - El sistema rechaza el consumo
  - El usuario puede: reducir la cantidad, cambiar a origen externo, o esperar reabastecimiento

- **Si SÍ:**
  - → Ejecutar el consumo

---

**Actividad B3**
`Ejecutar consumo`

**El sistema aplica el siguiente criterio automáticamente:**

1. Toma todos los lotes activos del material, ordenados del que vence primero al que vence después
2. Consume primero del lote con fecha de vencimiento más cercana
3. Si ese lote no alcanza, continúa con el siguiente, y así hasta cubrir la cantidad solicitada
4. Cada lote reduce su cantidad disponible. Si un lote llega a cero, queda como agotado
5. El stock total del material se reduce en la cantidad consumida

**Ejemplo concreto:**

Material: Aceite Motor 5W-30, stock total 48 litros.

| Lote | Cantidad | Vencimiento |
|---|---|---|
| Lote A | 24 L | Abril 2027 |
| Lote B | 24 L | Julio 2027 |

Si el mecánico consume 30 litros:
1. Toma 24 L del Lote A (vence primero) → Lote A se agota
2. Toma 6 L del Lote B → Lote B queda con 18 L
3. Stock total del material: 48 → 18 litros

---

### Fase B4: Procesos Automáticos Post-Consumo

**Qué ocurre automáticamente después del consumo:**

- El stock del material se actualiza en la lista de inventario
- Si el stock total quedó por debajo del mínimo configurado, el sistema genera una alerta de stock bajo
- El sistema no genera alertas duplicadas: si ya existe una alerta no resuelta para ese material, no crea otra
- El indicador de Stock Bajo en el Panel Principal se actualiza

---

### Fase B5: Calificación del Material (Opcional)

**Decisión** `¿El mecánico califica el material usado?`

- **Si NO:**
  - → Fin del consumo

- **Si SÍ:**

  **Actividad B5**
  `Registrar calificación del material`

  **Actor:** Mecánico

  **Opciones:** 1 ⭐ | 2 ⭐⭐ | 3 ⭐⭐⭐ | 4 ⭐⭐⭐⭐ | 5 ⭐⭐⭐⭐⭐

  **Decisión** `¿La calificación es menor o igual a 3 estrellas?`

  - **Si SÍ:**
    - La observación es **obligatoria**. El mecánico debe explicar por qué la calificación es baja.

  - **Si NO (4 o 5 estrellas):**
    - La observación es opcional.

  La calificación queda vinculada al material, a la orden de mantenimiento y al mecánico que la registró. El promedio de calificaciones del material se actualiza.

---

## RAMAL C: Descarte de Lotes (Merma)

---

### Fase C1: Identificación del Lote a Descartar

**Actividad C1**
`Revisar lotes de un material`

**Actor:** Jefe (Admin)
**Pantalla:** Detalle de Material → Lista de lotes

El usuario revisa cada lote. Ve: cantidad inicial, cantidad actual, costo unitario, fecha de vencimiento y estado.

**Decisión** `¿El lote debe descartarse?`

Razones para descartar:
- Fecha de vencimiento superada
- Material contaminado o dañado
- Merma por manipulación
- Devolución a proveedor

- **Si NO:**
  - → Fin

- **Si SÍ:**

---

### Fase C2: Ejecución del Descarte

**Actividad C2**
`Registrar descarte del lote`

**Actor:** Jefe (Admin)
**Pantalla:** Descartar Lote

**Datos que se registran:**
- Cantidad a descartar
- Motivo: vencido, dañado, merma, devolución
- Nota adicional (opcional)

**Qué ocurre automáticamente:**
- Si el lote ya estaba vencido o descartado, el sistema lo rechaza
- La cantidad del lote se reduce. Si llega a cero, el lote queda como descartado
- El stock total del material se reduce en la cantidad descartada
- Si el material cae por debajo del stock mínimo, se genera alerta

---

## RAMAL D: Monitoreo Automático del Inventario

---

### Fase D1: Verificación de Stock Bajo

**El sistema verifica periódicamente:**

Para cada material, compara el stock total contra el stock mínimo configurado.

Si el stock total es menor o igual al mínimo, y no existe ya una alerta pendiente para ese material, genera una alerta de tipo "Stock Bajo".

**Ejemplo de mensaje generado:**
"Stock bajo de Aceite Motor 5W-30 Sintético. Actual: 10.0 Litros. Mínimo: 12.0."

---

### Fase D2: Verificación de Lotes Próximos a Vencer

**El sistema verifica periódicamente:**

Para cada lote activo con fecha de vencimiento, calcula cuántos días faltan. Si faltan 30 días o menos, y no existe ya una alerta pendiente para ese lote, genera una alerta de tipo "Lote por Vencer".

**Ejemplo de mensaje generado:**
"Lote de Aceite Motor 5W-30 vence el 01/04/2027. Cantidad restante: 24.0 Litros."

---

### Fase D3: Ciclo de Vida de una Alerta de Inventario

**Actividad D3**
`Gestionar alerta de inventario`

**Actor:** Jefe (Admin) o Mecánico
**Pantalla:** Alertas

1. **Alerta generada** por el sistema (no leída, no resuelta)
2. **Marcar como leída:** Cualquier usuario puede hacerlo. Significa que fue vista
3. **Resolver:** Solo el Jefe. Implica que se tomó una acción concreta

**Acciones correctivas típicas:**
- Alerta de stock bajo → Ir a registrar ingreso de lote para reabastecer
- Alerta de lote por vencer → Ir a inventario para usar el lote prioritariamente o descartarlo

---

## 6. Tabla de Estados de un Lote

| Estado | Significado | ¿Se puede consumir? |
|---|---|---|
| Activo | Lote vigente con cantidad disponible | Sí |
| Agotado | Se consumió toda su cantidad | No |
| Vencido | Superó su fecha de vencimiento | No |
| Descartado | Se retiró por merma, daño o devolución | No |

---

## 7. Flujo Completo Resumido

```
                      ┌──────────────────────┐
                      │       INICIO          │
                      └──────────┬───────────┘
                                 │
                      ┌──────────▼───────────┐
                      │  ¿Existe el material  │
                      │  en el catálogo?      │
                      └────┬────────────┬────┘
                           │NO          │SÍ
                           ▼            │
                    ┌────────────┐     │
                    │  Crear      │     │
                    │  material   │     │
                    └─────┬──────┘     │
                          └────┬───────┘
                               │
                    ┌──────────▼───────────┐
                    │  Registrar lote       │
                    │  (cantidad, costo,    │
                    │   vencimiento)        │
                    └──────────┬───────────┘
                               │
                    ┌──────────▼───────────┐
                    │  Stock del material   │
                    │  se incrementa        │
                    └──────────┬───────────┘
                               │
           ┌───────────────────┼───────────────────┐
           │                   │                   │
           ▼                   ▼                   ▼
  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
  │ CONSUMO          │ │ DESCARTE        │ │ MONITOREO       │
  │ (en mantenimiento)│ │ (merma/vencido) │ │ (automático)    │
  │                  │ │                 │ │                 │
  │ ¿Stock propio?   │ │ Registrar motivo│ │ ¿Stock bajo?    │
  │ ├─SÍ: priorizar  │ │ y cantidad      │ │ → alerta        │
  │ │   lotes que    │ │                 │ │                 │
  │ │   vencen antes │ │ Stock del       │ │ ¿Lote por       │
  │ │                 │ │ material se     │ │  vencer?        │
  │ └─NO: externo    │ │ reduce          │ │ → alerta        │
  │   (no descuenta) │ │                 │ │                 │
  │                  │ │                 │ │                 │
  │ ¿Calificar?      │ │                 │ │                 │
  │ ├─SÍ: 1-5 ⭐     │ │                 │ │                 │
  │ └─NO: fin        │ │                 │ │                 │
  └─────────────────┘ └─────────────────┘ └─────────────────┘
```
