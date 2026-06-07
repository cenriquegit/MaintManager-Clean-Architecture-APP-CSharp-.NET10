# PLAN DEFINITIVO: Reestructuración del Módulo de Reportes
> Basado en análisis de coherencia, negocio, BD y datos existentes

---

## Análisis previo: R2 vs R4 (¿coexisten o se reemplazan?)

### R2 — Órdenes de Mantenimiento (PDF)
| Aspecto | Descripción |
|---------|-------------|
| **Propósito** | Visión GENERAL de todas las órdenes en un período |
| **Alcance** | Multi-vehículo (todos los vehículos filtrados) |
| **Contenido** | Lista/tabla: placa, vehículo, tipo, fecha, km, técnico, estado |
| **Formato** | PDF resumido, tabla paginada |
| **Usuario** | Jefe de mantenimiento — revisión semanal/mensual |

### R4 — Historial por Vehículo (PDF)
| Aspecto | Descripción |
|---------|-------------|
| **Propósito** | Detalle COMPLETO de la vida de UN vehículo específico |
| **Alcance** | Mono-vehículo (solo 1 vehículo) |
| **Contenido** | Por cada orden: fecha, km, tipo, técnico, CHECKLIST de acciones realizadas, materiales consumidos (con cantidades), componentes instalados, diagnóstico final |
| **Formato** | PDF detallado, tipo formulario, varias páginas |
| **Usuario** | Jefe o Mecánico — trazabilidad de un vehículo específico |

### Conclusión: AMBOS SE QUEDAN
- R2 = resumen ejecutivo (macro) → "¿cuántas órdenes hubo este mes?"
- R4 = ficha clínica del vehículo (micro) → "¿qué se le hizo al SWM G05 en todo el año?"

Son complementarios, no duplicados.

---

## Los 4 reportes definitivos

### R1 — Costo por Km (Excel)
**Endpoint:** `GET /api/v1/reports/cost-excel` (ya existe)
**Mejora:** Agregar pantalla de filtros antes de exportar
**Filtros:**
- 📅 Período: desde / hasta (DatePicker)
- 🚗 Vehículo: Picker con "Todos" + lista de vehículos
**Columnas Excel:** Placa, Vehículo, Servicios, Costo Materiales, Km Actual, $/km

### R2 — Órdenes de Mantenimiento (PDF)
**Endpoint nuevo:** `POST /api/v1/reports/maintenance-orders`
**Filtros:**
- 📅 Período: desde / hasta
- 🚗 Vehículo: "Todos" o específico
- 📌 Estado: "Todas", "Activas (AC)", "Finalizadas (FI)"
- 🔧 Tipo: "Todos", "Calendarizado", "Emergencia"
**Columnas PDF:**
| Placa | Vehículo | Tipo | Servicio | Fecha | Km | Técnico | Estado |
|-------|----------|------|----------|-------|----|---------|--------|

### R3 — Alertas (PDF)
**Endpoint nuevo:** `POST /api/v1/reports/alerts`
**Filtros:**
- 📅 Período: desde / hasta
- 📌 Estado: "No resueltas", "Resueltas", "Todas"
- ⚠️ Tipo de alerta: Picker con tipos disponibles
**Columnas PDF:**
| Fecha | Tipo | Vehículo/Material | Mensaje | Leída | Resuelta |
|-------|------|-------------------|---------|-------|----------|

### R4 — Historial de Mantenimiento por Vehículo (PDF) — 🆕 NUEVO
**Endpoint nuevo:** `POST /api/v1/reports/vehicle-history`
**Filtros:**
- 🚗 Vehículo: **Picker obligatorio** (sin opción "Todos")
- 📅 Período: desde / hasta (opcional)
**Contenido del PDF por cada orden del vehículo:**

```
┌─────────────────────────────────────────────┐
│  MANTENIMIENTO DE UNIDADES                   │
│  Vehículo: SWM G05 — VDG-361                │
│  Período: 01/01/2026 - 30/06/2026           │
├─────────────────────────────────────────────┤
│                                              │
│  ──── Orden #1 — 15/05/2026 ────            │
│  Kilometraje: 15,000 km   Tipo: Calendarizado│
│  Técnico: Juan Quispe     Estado: Finalizado │
│                                              │
│  ✅ Acciones realizadas:                     │
│     ☑ Revisión general                      │
│     ☑ Cambio de aceite                      │
│     ☐ Calibración de neumáticos             │
│                                              │
│  ✅ Materiales consumidos:                   │
│     • Aceite 5W-30 (4.5 L × S/25.00 = 112.50)│
│     • Filtro de Aceite Premium (1 uni × S/50)│
│                                              │
│  ✅ Componentes instalados:                  │
│     • Batería 12V 70Ah (inst. en 15,000 km) │
│                                              │
│  ✅ Diagnóstico final:                       │
│     Estado: Bueno  Operativo: Sí             │
│     Obs: Cambio de aceite y filtros          │
│     Recomendación: Próximo service a 20,000km│
│                                              │
│  ──── Orden #2 — 10/03/2026 ────            │
│  ...                                         │
└─────────────────────────────────────────────┘
```

---

## Flujo de navegación

```
[Página Reportes] (lista de 4 tarjetas)
    │
    ├── Toca "Costo por Km"
    │       → [Pantalla Filtros R1] (fechas + vehículo)
    │       → [Generar] → API → Excel → Share
    │
    ├── Toca "Órdenes de Mantenimiento"
    │       → [Pantalla Filtros R2] (fechas + vehículo + estado + tipo)
    │       → [Generar] → API → PDF → Share
    │
    ├── Toca "Alertas"
    │       → [Pantalla Filtros R3] (fechas + estado + tipo alerta)
    │       → [Generar] → API → PDF → Share
    │
    └── Toca "Historial por Vehículo"
            → [Pantalla Filtros R4] (vehículo + fechas)
            → [Generar] → API → PDF → Share
```

---

## API: Endpoints a crear

| Método | Endpoint | Body | Response |
|--------|----------|------|----------|
| `POST` | `/api/v1/reports/maintenance-orders` | `MaintenanceOrdersFilter` | `FileContentResult` (PDF) |
| `POST` | `/api/v1/reports/alerts` | `AlertsFilter` | `FileContentResult` (PDF) |
| `POST` | `/api/v1/reports/vehicle-history` | `VehicleHistoryFilter` | `FileContentResult` (PDF) |

### DTOs de filtros
```csharp
public sealed record MaintenanceOrdersFilter(
    DateOnly? DateFrom, DateOnly? DateTo,
    int? Prcoid, string? Status, short? Matyid
);

public sealed record AlertsFilter(
    DateOnly? DateFrom, DateOnly? DateTo,
    bool? Resolved, string? AlertType
);

public sealed record VehicleHistoryFilter(
    int Prcoid, DateOnly? DateFrom, DateOnly? DateTo
);
```

---

## Archivos a crear/modificar

| Archivo | Tipo | Descripción |
|---------|------|-------------|
| `ReportsViewModel.cs` | 🔧 Modificar | 4 comandos `GenerateR1`, `GenerateR2`, etc. |
| `ReportsPage.xaml` | 🔧 Modificar | Agregar R4 (Historial) a la lista |
| `FilterPage.xaml` | 🆕 Crear | Pantalla genérica de filtros (reutilizable) |
| `FilterViewModel.cs` | 🆕 Crear | Lógica de filtros según tipo de reporte |
| `ReportsController.cs` | 🔧 Modificar | Agregar 3 endpoints POST |
| `ReportService.cs` | 🆕 Crear | Servicio con métodos de generación PDF |
| `MaintenanceOrdersFilter.cs` | 🆕 Crear | DTO |
| `AlertsFilter.cs` | 🆕 Crear | DTO |
| `VehicleHistoryFilter.cs` | 🆕 Crear | DTO |
| `ApiRoutes.cs` | 🔧 Modificar | Nuevas rutas Reports.MaintenanceOrders, etc. |

---

## Puntos clave de diseño

1. **R4 usa datos REALES**: Las acciones, materiales y componentes se obtienen de `maintenance_action_detail`, `material_consumption` e `installed_component` vinculados a cada orden. No se editan manualmente.

2. **Los filtros son coherentes**: 
   - R1: fecha + vehículo (para ver evolución de costos por período/vehículo)
   - R2: estado + tipo + vehículo + fecha (para generar reportes específicos)
   - R3: resuelto + tipo + fecha (para auditoría de alertas)
   - R4: vehículo + fecha (obligatorio seleccionar un vehículo)

3. **Páginas separadas**: Cada reporte tiene su propia pantalla de filtros, no una página genérica.

4. **QuestPDF**: Los PDFs se generan con QuestPDF (ya configurado en el proyecto) con estilos profesionales: header con logo de NeoPlus, tablas con bordes, footer con numeración.
