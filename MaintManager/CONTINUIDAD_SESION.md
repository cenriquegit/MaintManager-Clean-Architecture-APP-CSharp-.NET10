# 🚛 MaintManager — Archivo de Continuidad para Nueva Sesión

> **Propósito:** Todo el contexto necesario para retomar el proyecto exactamente donde se quedó.
> **Leer:** 1) Este archivo → 2) KILO_SESSION_CONTEXT.md → 3) BUGS_HISTORY.md → 4) PROXIMOS_PASOS.md
> **Fecha:** 2026-06-05

---

## ⚠️ REGLAS DE ORO — Lo que NO debes repetir

### 1. NO toques LabelsRotation, LabelsPaint ni DataLabels en el dashboard
- LabelsRotation=-90° comprime el chart y oculta las barras. Usar -20° máximo.
- LabelsPaint y DataLabels en LiveChartsCore v2.1.0-dev-570 causan conflictos de renderizado.

### 2. NO uses rutas relativas en Shell.GoToAsync
- ❌ `"Maintenances/Detail"` → ✅ `"///Maintenances/Detail"`
- ❌ `"Inventory/CreateLot"` → ✅ `"///Inventory/CreateLot"`
- Siempre prefijo `///`.

### 3. NO envíes `new { }` vacío al cerrar orden de emergencia
- Usar `new { IsEmergencyComplete = false }` siempre.

### 4. NO hagas ediciones masivas sin backup
- Una edición masiva corrompió BiDashboardViewModel.cs. Restaurado con `git checkout c95af1c`.

---

## 📋 Estado actual del proyecto

| Componente | Estado | Último cambio |
|------------|--------|---------------|
| Login + JWT + 8h sesión | ✅ | Sesiones anteriores |
| Dashboard principal (KPIs, stats) | ✅ | Sesiones anteriores |
| Wizard de mantenimiento | ✅ | Sesiones anteriores |
| Detalle de orden (acciones, mat, comp, diag) | ✅ | Sesión 2026-05-21 |
| Cerrar orden + recalendarización | ✅ | Sesión 2026-05-21 |
| BI Dashboard (5 gráficos) | ✅ | Restaurado c95af1c + x1000 |
| Reportes (4 tipos, generación directa) | ✅ | Sesión 2026-06-05 |
| Inventario (consulta, FIFO, lotes) | ✅ | Sesión 2026-06-04 |
| Alertas (automáticas, Switch históricas) | ✅ | Sesión 2026-06-04 |
| Historial vehículo (prompt + PDF) | ✅ | Sesión 2026-06-05 |
| Permisos role-based (AppShell + API) | ✅ | Sesión 2026-06-04 |
| Calendario | ✅ | Sesiones anteriores |

---

## 🗄️ BD — Datos actuales
- 127 órdenes (20 activas), 137 consumos, 109 diagnósticos, 40 componentes, 77 ratings
- Scripts SQL en `database/` (04 al 08)
- Conexión: `Host=localhost;Port=5432;Database=neoplus_maintenance;User=postgres;Password=postgres;`

## 🔑 Credenciales
| Usuario | Password | Rol |
|---------|----------|-----|
| herror.ortiz | Admin2026! | Admin |
| juan.quispe | Tecnico2026! | Técnico |

## 🚀 Comandos rápidos
```bash
dotnet build -f net10.0-android                                      # Compilar MAUI Debug
dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormats=apk  # APK
adb install -r bin/Release/net10.0-android/publish/*-Signed.apk     # Instalar
```

## 📊 Mega Prompt para nueva sesión
```
Estoy en MaintManager (C:\Users\carlo\Desktop\proyect\MaintManager).
Clean Architecture: API .NET 10 + PostgreSQL, MAUI .NET 10 MVVM.
Leer: CONTINUIDAD_SESION.md, KILO_SESSION_CONTEXT.md, BUGS_HISTORY.md, PROXIMOS_PASOS.md, Todo el codigo de todo el proyecto iniciando del Application,Domain,Infrastructure, de ahi los ams importantes que necesitan tu ate nciaopn completa paso a paso API, Share y MAUI, vas leer todo detalel a detalle palabbra a palabra, y entenderes su sintaxis, su significado, que representa y el contexto completo,entendiendo todo de manera completa pare que puedas seguier el proeyto de manera correcta ,secuencial, completa, lineal y todop lo que consigamos despeus lo hagamos rapido y bien.
REGLAS: No tocar LabelsRotation/LabelsPaint/DataLabels del dashboard.
Siempre usar /// en Shell.GoToAsync.
CloseOrder: new { IsEmergencyComplete = false }.
Mega seed en database/ (04-08).
```
