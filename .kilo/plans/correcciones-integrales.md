# Plan Integral de Correcciones — MaintManager

> Basado en `cambios_errores-rf-rnf-en.txt`
> Enfoque: solo arreglar lo que está roto, no tocar nada que funcione.

---

## DIAGNÓSTICO GENERAL

| Área | Estado |
|------|--------|
| **Regresiones por cambios previos** | 🔴 3 bugs activos (botones mal posicionados) |
| **Datos incorrectos** | 🔴 KM muestra valor equivocado |
| **Funcionalidad faltante** | 🟡 10 RF cumplidos parcialmente |
| **Rendimiento** | 🔴 Sin paginación en lista de mantenimientos |
| **BI Dashboard** | 🟡 4 KPIs con datos incompletos |

---

## FASE 1 — 🔴 CRÍTICO: Regresiones (bugs que yo causé)

### Error 1: Botón Cancelar mal posicionado
- **Síntoma:** "Cancelar Orden" aparece en lugares donde no debe (consumir material, abajo duplicado)
- **Causa:** Al mover el botón Cancelar al header, usé `replaceAll` que cambió `CanClose` por `CanCancel` en múltiples sitios. También dupliqué botones.
- **Fix:** Restaurar `MaintenanceDetailPage.xaml` a su estado funcional. El botón Cancelar debe ir en el header negro (donde dice "Orden #XXX"), alineado a la derecha, con texto "✕ Cancelar". NO en secciones internas. NO duplicado.
- **Archivos:** Solo `MaintenanceDetailPage.xaml`

### Error 2: Dos botones "Cerrar Orden" tras diagnóstico
- **Síntoma:** Aparecen 2 botones de cerrar abajo
- **Causa:** Duplicación durante la edición del XAML
- **Fix:** Restaurar a un solo botón "Cerrar Orden" y un solo "Exportar PDF" en el bottom bar
- **Archivos:** Solo `MaintenanceDetailPage.xaml`

---

## FASE 2 — 🔴 ALTO: Datos incorrectos

### Error 3: KM actual no es correcto (RF-02)
- **Síntoma:** Muestra 26000km cuando debería ser cada 5000. Próximo servicio muestra 25000 y 15100 inconsistentes.
- **Causa:** El sistema usa `product.vehicle.mileage` (KM de rentas) pero NO compara con el KM registrado en el último mantenimiento. Debe tomar el mayor de ambos.
- **Fix:** En el endpoint que devuelve KM actual, comparar `product.vehicle.mileage` con el `mileage` del último `maintenance` cerrado para ese vehículo, y usar el mayor.
- **Archivos:** `VehicleManagementController.GetAll`, `AgendaController.GetAgenda`

### Error 4: Alternar tipo A/B cada 10000km (RF-06)
- **Síntoma:** Siempre sugiere el mismo tipo de servicio
- **Causa:** El `SchedulingService` recalendariza pero no alterna entre A y B
- **Fix:** Al recalendarizar, si el vehículo tiene un historial de servicio A → próximo es B, si es B → próximo es A. La lógica: cada 5000km = servicio A, cada 10000km = servicio B (completo).
- **Archivos:** `SchedulingService.cs`

---

## FASE 3 — 🟡 MEDIO: Funcionalidad faltante

### Error 5: VehicleHistoryPage desde Dashboard (RF-07)
- **Síntoma:** No se puede ver historial del vehículo desde el panel principal al hacer click
- **Causa:** Las cards de flota no tienen navegación asignada
- **Fix:** Agregar `TapGestureRecognizer` en las cards del Dashboard que naveguen a `///Maintenances/VehicleHistory?prcoid=X`. La página VehicleHistory ya existe.
- **Archivos:** `HomePage.xaml`, `HomePage.xaml.cs`

### Error 6: Lotes separados en MaterialDetail (RF-11)
- **Síntoma:** Stock se muestra junto, no hay cards individuales por lote. Edit/Eliminar no funcionan.
- **Causa:** `MaterialDetailPage` no carga los lotes reales del material. Solo muestra un resumen.
- **Fix:** Cargar los lotes desde `GET /api/v1/inventory/materials/{id}/lots` y mostrar cada lote como un card individual con cantidad, fecha, proveedor. Agregar botones editar/eliminar por lote.
- **Archivos:** `MaterialDetailViewModel.cs`, `MaterialDetailPage.xaml`

### Error 7: Proveedor como nombre, no número (RF-09)
- **Síntoma:** Solo se ingresa número de lote del proveedor, no el nombre
- **Causa:** `LotCreatePage` tiene campo `SupplierLotNumber` pero no `SupplierName`
- **Fix:** Agregar campo `SupplierName` (texto libre) junto al número de lote. El número de lote se auto-genera si está vacío.
- **Archivos:** `LotCreatePage.xaml`, `LotCreateViewModel.cs`

### Error 8: Auto-llenar material al crear lote desde MaterialDetail (RF-11)
- **Síntoma:** Al crear lote desde la página de detalle de material, hay que volver a seleccionar el material
- **Causa:** `LotCreateViewModel` recibe `mateid` pero no lo preselecciona en el picker
- **Fix:** En `ApplyQueryAttributes`, si viene `mateid`, seleccionar automáticamente ese material y deshabilitar el picker (mostrar label en su lugar)
- **Archivos:** `LotCreateViewModel.cs`, `LotCreatePage.xaml`

---

## FASE 4 — 🟡 MEDIO: Dashboard BI

### Error 9: Costo por km no muestra todos los vehículos (RF-16)
- **Síntoma:** El gráfico de costo/km solo muestra algunos vehículos
- **Causa:** La vista `vw_cost_per_km` puede estar filtrando incorrectamente o el endpoint tiene un límite
- **Fix:** Revisar la vista SQL y el endpoint. Incluir vehículos con 0 costo.
- **Archivos:** `BiReportService.cs`, `ReportsController.cs`

### Error 10: Proporción emergencia vs calendarizado por vehículo (RF-17)
- **Síntoma:** No se muestra la proporción por vehículo
- **Causa:** El endpoint `GET /api/v1/reports/emergency-rate` no devuelve datos por vehículo
- **Fix:** Modificar para agrupar por vehículo, no solo total global
- **Archivos:** `ReportsController.cs`, `vw_emergency_rate`

### Error 11: Colores de cumplimiento + leyenda (RF-19)
- **Síntoma:** No hay distinción visual entre puntual/tardío/anticipado
- **Causa:** La vista de compliance muestra datos pero sin formato condicional
- **Fix:** Agregar colores: verde (puntual ±500km), naranja (anticipado >500km antes), rojo (tardío >500km después). Agregar leyenda explicativa.
- **Archivos:** `ReportsPage.xaml`, `ReportsViewModel.cs`

### Error 12: KPI "lotes por vencer" (RF-15)
- **Síntoma:** Falta este KPI en el dashboard
- **Causa:** No está incluido en `vw_bi_dashboard_summary`
- **Fix:** Agregar query a `material_lot WHERE expiration_date <= NOW() + 30 days` y mostrar en el dashboard
- **Archivos:** `HomeViewModel.cs`, `HomePage.xaml`, `BiReportService.cs`

---

## FASE 5 — 🟡 MEDIO: Rendimiento

### Error 13: Sin paginación en lista de mantenimientos (RNF-04)
- **Síntoma:** La lista carga todos los mantenimientos sin paginar
- **Causa:** `MaintenanceListViewModel.Load` no usa paginación. Carga todos y filtra client-side.
- **Fix:** Implementar scroll infinito o botones de página. El endpoint ya soporta `page` y `pageSize`.
- **Archivos:** `MaintenanceListViewModel.cs`, `MaintenanceListPage.xaml`

---

## FASE 6 — 🟢 BAJO: Verificación

### Error 14: Simular RF-21 (ConfigSystem)
- **Acción:** Probar `GET /api/v1/config` y `PUT /api/v1/config` con valores reales
- **Verificar:** Que los valores se lean correctamente en `SchedulingService`

---

## REGLAS ESTRICTAS

1. ❌ NO modificar nada que ya funcione
2. ❌ NO usar `replaceAll` en XAML
3. ❌ NO tocar Dashboard KPIs labels/rotation/paint
4. ✅ Solo modificar archivos listados en cada error
5. ✅ Probar cada fix individualmente antes de pasar al siguiente
6. ✅ Mantener el patrón de diseño unificado (SearchBar + Filter + blue bottom button)
