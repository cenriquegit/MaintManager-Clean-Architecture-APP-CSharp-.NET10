# Relaciones del Diagrama de Clases — MaintManager

> Extraídas del archivo `Diagrama de clases.mdj` (StarUML)
> 27 relaciones totales: 13 composiciones + 14 asociaciones

---

## COMPOSICIÓN (Dueño ◇ → Parte)

| # | Dueño (◇) | Parte (→) | Mensaje en StarUML | Cardinalidad |
|---|----------|----------|-------------------|-------------|
| 1 | Vehicle ◇ | → vehicle_allowed_action | accion permitida | 1 → 0..* |
| 2 | Vehicle ◇ | → vehicle_allowed_component | componente permitida | 1 → 0..* |
| 3 | Vehicle ◇ | → vehicle_allowed_material | material permitido | 1 → 0..* |
| 4 | Vehicle ◇ | → VehicleSchedule | — | 1 → 0..1 |
| 5 | Material ◇ | → MaterialLot | — | 1 → 1..* |
| 6 | MaterialLot ◇ | → MaterialDiscard | — | 1 → * |
| 7 | MaterialLot ◇ | → MaterialConsumption | — | 0..1 → * |
| 8 | Material ◇ | → MaterialRating | Calificación de materiales | 1 → 0..* |
| 9 | Maintenance ◇ | → Diagnosis | Diagnosis | 1 → 0..1 |
| 10 | Maintenance ◇ | → MaintenanceActionDetail | ActionDetails | 1 → 1..* |
| 11 | Maintenance ◇ | → TechnicianAssignment | — | 1 → 0..* |
| 12 | Maintenance ◇ | → MaterialConsumption | consumo de material | 1 → 0..* |
| 13 | Maintenance ◇ | → InstalledComponent | componentes instalados | 1 → 0..* |

## ASOCIACIÓN SIMPLE

| # | Origen | Destino | Mensaje en StarUML | Cardinalidad |
|---|--------|--------|-------------------|-------------|
| 14 | Maintenance | → Vehicle | — | * → 1 |
| 15 | Maintenance | → Worker | Registrador | * → 1 |
| 16 | Maintenance | → Worker | MecanicoAsignado | * → 1 |
| 17 | Maintenance | → Status | — | * → 1 |
| 18 | Maintenance | → MaterialRating | Calificación de Materiales | 1 → 0..* |
| 19 | MaterialLot | → Provider | — | 0..1 → * |
| 20 | InstalledComponent | → AlertLog | alerta componente por caducar | 0..1 → * |
| 21 | InstalledComponent | → InstalledComponent | remplazadoPor | * → 0..1 |
| 22 | InstalledComponent | → MaterialLot | — | * → 1 |
| 23 | InstalledComponent | → Vehicle | — | 1 → * |
| 24 | AlertLog | → Material | alerta stock bajo | * → 0..1 |
| 25 | AlertLog | → MaterialLot | alerta lote por vencer | * → 0..1 |
| 26 | Vehicle | → AlertLog | alertas del vehículo | 0..1 → 0..* |
| 27 | ConfigSystem | → Worker | ActualizadoPor | * → 0..1 |
