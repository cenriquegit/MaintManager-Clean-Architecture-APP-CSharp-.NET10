# Explicación detallada de las conexiones del Diagrama de Clases

> Basado en el diagrama presentado (13 composiciones + 14 asociaciones)
> Solo incluye las relaciones del diagrama actual

---

## Conexiones de Maintenance (Mantenimiento / Orden de Servicio)

| Origen | Destino | Tipo | Texto explicativo |
|--------|--------|------|-------------------|
| Maintenance | Vehicle | Asociación simple | Vehículo atendido. Cada orden se registra sobre un vehículo de la flota (FK prcoid → product.company). |
| Maintenance | Worker (Registrador) | Asociación simple | Quién registró la orden. El campo workid identifica al trabajador que creó la orden en el sistema. |
| Maintenance | Worker (MecanicoAsignado) | Asociación simple | Mecánico ejecutor. El campo assigned_to señala al técnico que realiza físicamente el trabajo. |
| Maintenance | Status | Asociación simple | Estado de la orden (list.status): AC=Activo, FI=Finalizado, CA=Cancelado. |

### Composiciones desde Maintenance (todo ◇ → parte, cascade delete)

| Origen (◇) | Destino (→) | Tipo | Texto explicativo |
|------------|------------|------|-------------------|
| Maintenance ◇ | → MaintenanceActionDetail | Composición | Checklist de acciones (1..*). Cada fila detalla qué se hizo, producto usado y lote de origen. Cascade delete. |
| Maintenance ◇ | → Diagnosis | Composición | Diagnóstico final (0..1). Estado general, observaciones, operatividad. Obligatorio para cerrar. |
| Maintenance ◇ | → MaterialConsumption | Composición | Consumos de material (0..*). Materiales extraídos del inventario durante el servicio. |
| Maintenance ◇ | → MaterialRating | Asociación simple | Calificación de materiales (1→0..*). El mecánico puntúa de 1 a 5 estrellas. Si ≤3, observación obligatoria. |
| Maintenance ◇ | → InstalledComponent | Composición | Componentes instalados (0..*). Qué componentes se colocaron y su fecha de caducidad. |
| Maintenance ◇ | → TechnicianAssignment | Composición | Asignación de técnicos (0..*). Soporta múltiples técnicos por orden. |

---

## Conexiones del módulo de Inventario

| Origen | Destino | Tipo | Texto explicativo |
|--------|--------|------|-------------------|
| Material ◇ | → MaterialLot | Composición (1→1..*) | Lotes del material. Cada material tiene múltiples lotes con cantidad, costo y vencimiento propios. |
| MaterialLot ◇ | → MaterialConsumption | Composición | Consumo FIFO. Se consume primero el lote más próximo a vencer. |
| MaterialLot ◇ | → MaterialDiscard | Composición (1→*) | Descarte de lote. Por vencimiento, daño u otro motivo. Reduce el stock. |
| MaterialLot | Provider | Asociación simple | Proveedor del lote (public.provider, solo lectura). |
| Material ◇ | → MaterialRating | Composición | Calificaciones recibidas por el material a lo largo de las órdenes. |

---

## Conexiones del módulo de Programación (KM-based)

| Origen | Destino | Tipo | Texto explicativo |
|--------|--------|------|-------------------|
| Vehicle ◇ | → VehicleSchedule | Composición (1→0..1) | Cronograma por vehículo. Cada vehículo tiene un único cronograma con el próximo KM de servicio, intervalo y tipo esperado (A/B). |

---

## Conexiones del módulo de Alertas

| Origen | Destino | Tipo | Texto explicativo |
|--------|--------|------|-------------------|
| Vehicle | AlertLog | Asociación simple | Alertas del vehículo. Una alerta puede asociarse a un vehículo cuando se acerca su KM de servicio. |
| AlertLog | Material | Asociación simple | Alerta de stock bajo. Se genera cuando el stock cae por debajo del mínimo. |
| AlertLog | MaterialLot | Asociación simple | Alerta de lote por vencer. Lote próximo a su fecha de vencimiento. |
| InstalledComponent | AlertLog | Asociación simple | Alerta de componente por caducar. Componente instalado próximo a caducar. |

---

## Conexiones de Configuración por Vehículo (Join Tables)

| Origen (◇) | Destino (→) | Tipo | Texto explicativo |
|------------|------------|------|-------------------|
| Vehicle ◇ | → vehicle_allowed_action | Composición (1→0..*) | Acciones permitidas. Define qué acciones del catálogo pueden realizarse en cada vehículo. |
| Vehicle ◇ | → vehicle_allowed_material | Composición (1→0..*) | Materiales permitidos. Define qué materiales pueden consumirse en cada vehículo. |
| Vehicle ◇ | → vehicle_allowed_component | Composición (1→0..*) | Componentes permitidos. Define qué componentes pueden instalarse en cada vehículo. |

---

## Otras conexiones relevantes

| Origen | Destino | Tipo | Texto explicativo |
|--------|--------|------|-------------------|
| ConfigSystem | Worker | Asociación simple | Auditoría. Registra qué trabajador modificó los parámetros del sistema. |
| InstalledComponent | InstalledComponent | Auto-asociación (reflexiva) | Historial de reemplazos. `replaced_by_incoid` rastrea qué componente sustituyó a cuál. |
| InstalledComponent | Vehicle | Asociación simple | Componente instalado en un vehículo. |
| InstalledComponent | MaterialLot | Asociación simple | Lote del material usado para el componente instalado. |

---

## Clases externas («external» — Solo lectura)

| Clase | Tabla original | Descripción |
|-------|---------------|-------------|
| Vehicle | product.vehicle (hereda product.company) | Vehículos de la flota. El sistema solo lee: placa, kilometraje, estado. |
| Worker | public.worker | Trabajadores de la empresa. Autenticación y roles según public.job. |
| Provider | public.provider | Proveedores de materiales. Vinculados a lotes para trazabilidad. |
| Status | list.status | Catálogo de estados (AC, FI, CA). |
