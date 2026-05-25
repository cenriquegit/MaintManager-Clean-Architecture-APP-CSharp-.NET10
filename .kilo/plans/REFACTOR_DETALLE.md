# PLAN DE REFACTORIZACIÓN — MaintManager DetailPage

> **Objetivo General:** Rediseñar y corregir la página de detalle de orden de mantenimiento para que sea funcional, usable, visualmente ordenada, y que todos los procesos (acciones, consumo de materiales, instalación de componentes, diagnóstico, cierre, exportación) funcionen correctamente y con coherencia lógica.
> 
> **Contexto:** La DetailPage actual tiene múltiples problemas: datos que no se muestran, layouts desordenados, lógica incorrecta (orden finalizada aún permite editar), exportaciones vacías, crash en rating de materiales, falta de seed data para componentes con vida útil, y ausencia de eliminación de items.
> 
> **Fechas:** Plan creado el 2026-05-21. Cada fase se ejecutará bajo demanda del usuario.
>
> **Reglas ABSOLUTAS que NUNCA deben violarse:**
> 1. **NO tocar el Dashboard/Home:** `HomePage.xaml`, `HomeViewModel.cs`, `BiDashboardPage.xaml`, `BiDashboardViewModel.cs`
> 2. **NO tocar LiveCharts/SkiaSharp:** `MauiProgram.cs` (configuración de gráficos), paquetes NuGet de LiveCharts
> 3. **NO tocar la configuración de compilación:** `.csproj`, `AndroidManifest.xml`, `launchSettings.json`
> 4. **NO tocar Login/Auth:** `AuthController.cs`, `AuthService.cs`, `LoginPage.xaml`, `LoginViewModel.cs`
> 5. **NO tocar el Wizard de nueva orden:** `MaintenanceWizardViewModel.cs`, `MaintenanceWizardPage.xaml`
> 6. **NO modificar la BD directamente** sin especificarlo en la fase
> 7. **Cada fase DEBE compilar** (`dotnet build -f net10.0-android`) antes de pasar a la siguiente

---

## FASE 1 — Fix crítico: RateMaterial crash (columna MaterialMateid)

### Objetivo
Corregir el `DbUpdateException` al calificar un material: `no existe la columna «MaterialMateid» en la relación «material_rating»`.

### ¿Por qué ocurre?
EF Core detecta una relación por convención entre `MaterialRating.Mateid` y `Material.Mateid` y crea una shadow FK `MaterialMateid`. La tabla `material_rating` NO tiene esa columna. Como no hay configuración explícita de la relación, EF agrega la shadow property automáticamente.

### ¿Qué se hará?
Agregar configuración explícita de la relación `MaterialRating → Material` en `InventoryConfiguration.cs`.

### Archivo a modificar
- `MaintManager.Infrastructure/Data/Configurations/InventoryConfiguration.cs`

### Cambio exacto
En `MaterialRatingConfiguration.Configure`, después de los índices (línea 135), agregar:
```csharp
builder.HasOne<Material>().WithMany().HasForeignKey(mr => mr.Mateid)
       .OnDelete(DeleteBehavior.Restrict);
```

### NO tocar
- Ninguna otra entidad, configuración o controlador
- No modificar la BD
- No modificar el MAUI

### Validación
1. `dotnet build MaintManager.Infrastructure`
2. `dotnet build MaintManager.API`
3. Probar `POST /api/v1/inventory/materials/{id}/ratings` → 200 OK
4. Confirmar que el INSERT en `material_rating` NO incluya la columna `MaterialMateid`

---

## FASE 2 — Seed data: Componentes con vida útil + lotes de materiales

### Objetivo
Agregar datos de semilla para que existan componentes con `useful_life_days`/`useful_life_km` en `action_catalog`, y materiales con lotes para consumir.

### ¿Por qué?
No hay componentes con vida útil, la pestaña "Componentes instalados" no tiene datos, y el sistema de alertas no tiene base.

### ¿Qué se hará?
Crear script SQL con inserts en `action_catalog`, `material`, `material_lot`.

### Archivo
- Nuevo: `database/04_seed_components_materials.sql`

### Data
**Componentes con vida útil:**
| acatid | name | category | useful_life_days | useful_life_km | expires_by_time |
|--------|------|----------|-----------------|----------------|-----------------|
| 91 | Batería 12V 70Ah | Componente Eléctrico | 1095 | NULL | true |
| 92 | Neumáticos 205/55R16 | Componente Rodaje | NULL | 50000 | false |
| 93 | Pastillas de Freno | Componente Frenos | NULL | 30000 | false |
| 94 | Kit de Distribución | Componente Motor | NULL | 90000 | false |
| 95 | Filtro de Aire | Componente Motor | 365 | 20000 | true |
| 96 | Correa de Alternador | Componente Motor | NULL | 60000 | false |

**Acciones para checklist:**
| acatid | name | category |
|--------|------|----------|
| 100 | Revisión general | Acción |
| 101 | Cambio de aceite | Acción |
| 102 | Alineación y balanceo | Acción |
| 103 | Calibración de neumáticos | Acción |
| 104 | Revisión de frenos | Acción |
| 105 | Revisión de luces | Acción |
| 106 | Revisión de suspensión | Acción |
| 107 | Escaneo electrónico | Acción |
| 201 | Inspección general (Emergencia) | Acción Emergencia |
| 202 | Reparación en ruta | Acción Emergencia |

**Materiales + lotes:** Aceite (2 lotes), Filtro (1), Batería (1), Neumático (1), Frenos (1), Kit Distribución (1), Correa (1).

### NO tocar
- Código C#, tablas existentes fuera de `maintenance.*`

---

## FASE 3 — DetailPage: Rediseño layout cards + listas

### Objetivo
Reordenar visualmente la página de detalle: cada sección (Acciones, Consumir Material, Instalar Componente) tendrá su input arriba y su lista de items debajo, con botón eliminar en cada item.

### Layout final:
```
Card: Info General (placa, vehículo, tipo, fecha, km, técnico, aceite)
Card: Acciones Realizadas
  [Picker Acción] [Agregar]
  ┌─ Lista ──────────────────────┐
  │ ☑️ Revisión general      [✕]│
  │ ☐ Cambio de aceite       [✕]│
  └──────────────────────────────┘
Card: Consumo de Materiales
  [Picker Material] [Cant] [Consumir]
  ┌─ Lista ──────────────────────┐
  │ Aceite 5W-30 (LOT-001)   [✕]│
  │   Costo: 25.00 x 2 = 50.00  │
  └──────────────────────────────┘
Card: Componentes Instalados
  [Picker Componente] [Instalar]
  ┌─ Lista ──────────────────────┐
  │ Batería 12V (km:15000)  [✕] │
  └──────────────────────────────┘
Card: Reasignar Técnico
Card: Diagnóstico
Footer: [Cerrar Orden] [Exportar PDF]
```

### Archivos
- `MaintenanceDetailPage.xaml`
- `MaintenanceDetailViewModel.cs`

---

## FASE 4 — Solo lectura para órdenes finalizadas (Status = "FI")

### Objetivo
Cuando Status == "FI", la página debe ser solo lectura: inputs deshabilitados, botones ocultos.

### ¿Qué se hará?
Propiedad `IsReadOnly` en ViewModel, bindeada a `IsEnabled`/`IsVisible` en XAML.

### Archivos
- `MaintenanceDetailViewModel.cs`
- `MaintenanceDetailPage.xaml`

### Comportamiento
- `IsReadOnly == true` cuando `Status == "FI"`
- Inputs: `IsEnabled="{Binding IsReadOnly, Converter={StaticResource InvertedBoolConverter}}"`
- Botón Cerrar: `IsVisible="{Binding IsReadOnly, Converter={StaticResource InvertedBoolConverter}}"`
- Botón Exportar PDF: `IsVisible="{Binding IsReadOnly}"` (solo en FI)

---

## FASE 5 — Verificar exportaciones (PDF/Excel con datos)

### Objetivo
Confirmar que los PDF y Excel exporten datos reales. Ya se verificó en logs que funcionan (200 OK con kilobytes de datos). Si algún reporte está vacío, corregir.

### Archivo
- `ReportsController.cs` (si requiere ajuste)

---

## FASE 6 — Buffering local + guardado batch (opcional)

### Objetivo
Acumular acciones/materiales/componentes en listas locales y guardar todo al cerrar.

### Archivos
- `MaintenanceDetailViewModel.cs`
- `MaintenanceDetailPage.xaml`
- (Opcional) Endpoints batch en `MaintenancesController.cs`

---

## FASE 7 — Endpoints DELETE para items

### Objetivo
Agregar endpoints para eliminar acciones, consumos y componentes.

### Endpoints
```csharp
DELETE /api/v1/maintenances/{id}/actions/{madeid}
DELETE /api/v1/maintenances/{id}/materials/{macoid}
DELETE /api/v1/maintenances/{id}/components/{incoid}
```

### Archivo
- `MaintenancesController.cs`

---

## FASE 8 — Diagnóstico: campos completos

### Objetivo
Mostrar todos los campos del diagnóstico: `generalStatus`, `vehicleOperative`, `observations`, `futureRecommendations`.

### Archivos
- `MaintenanceDetailPage.xaml`
- `MaintenanceDetailViewModel.cs`

---

## RESUMEN

| Fase | Descripción | Archivos | Prioridad | Estado |
|------|-------------|----------|-----------|--------|
| 1 | Fix RateMaterial crash | `InventoryConfiguration.cs` | 🔴 Crítica | ✅ |
| 2 | Seed data componentes + materiales | `04_seed_*.sql` | 🟡 Alta | ✅ |
| 3 | Rediseño layout DetailPage | `DetailPage.xaml`, `DetailViewModel.cs` | 🟡 Alta | ✅ |
| 4 | Solo lectura orden FI | `DetailPage.xaml`, `DetailViewModel.cs` | 🟡 Alta | ✅ |
| 5 | Verificar exportaciones | `ReportsController.cs` | 🟢 Media | ✅ |
| 6 | Buffering local + batch | `DetailViewModel.cs`, `DetailPage.xaml` | 🟢 Media | ✅ |
| 7 | Endpoints DELETE | `MaintenancesController.cs` | 🟢 Media | ✅ |
| 8 | Diagnóstico campos completos | `DetailPage.xaml`, `DetailViewModel.cs` | 🟢 Media | ✅ |
