# MaintManager — Sistema de Gestión de Mantenimiento Vehicular con BI
## Neo Plus Business S.A.C.

Stack: .NET 10 · ASP.NET Web API · EF Core · PostgreSQL · .NET MAUI · Clean Architecture

---

## Requisitos previos

| Herramienta | Versión mínima |
|-------------|---------------|
| .NET SDK | 10.0 |
| PostgreSQL | 16.x |
| dotnet-ef (global) | 9.x / 10.x |
| Visual Studio 2022 / VS Code | Latest |
| Android SDK (para MAUI Android) | API 24+ |

---

## Configuración inicial

### 1. Clonar y restaurar

```bash
git clone <repo>
cd MaintManager
dotnet restore MaintManager.sln
```

### 2. Crear la base de datos

Conectarse a PostgreSQL y ejecutar en orden:

```
1. BD-FINAL-COMPLETAMENTE-CORREGIDA.sql   ← script principal
2. 02_ajustes_fase1.sql                   ← ajustes del sistema de mantenimiento
3. 03_seed_data.sql                       ← datos de ejemplo
```

### 3. Configurar variables de entorno

Crear `MaintManager.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=neoplus_maintenance;Username=postgres;Password=TU_PASSWORD;"
  },
  "Jwt": {
    "Key": "NeoPlus2026_SuperSecretKey_MantVehicular_32chars!!",
    "Issuer": "MaintManager.API",
    "Audience": "MaintManager.MAUI",
    "ExpirationHours": "8"
  }
}
```

> **Importante:** `appsettings.Development.json` está en `.gitignore`. Nunca commitear credenciales.

---

## Ejecución

### API Backend

```bash
cd MaintManager.API
dotnet run
# API disponible en: http://localhost:5000
# Swagger UI en: http://localhost:5000 (raíz)
```

### App MAUI (Windows)

```bash
cd MaintManager.MAUI
dotnet run -f net10.0-windows10.0.19041.0
```

### App MAUI (Android — emulador)

```bash
dotnet run -f net10.0-android
# La URL de la API en Android usa: http://10.0.2.2:5000/
# (configurado automáticamente en MauiProgram.cs según DeviceInfo.Platform)
```

---

## Usuarios del sistema (seed data)

| Usuario | Contraseña | Rol | Descripción |
|---------|-----------|-----|-------------|
| `christian.ortiz` | `Admin2026!` | Admin | Gerente General |
| `herror.ortiz` | `Admin2026!` | Admin | Jefe de Mantenimiento |
| `juan.quispe` | `Tecnico2026!` | Tecnico | Mecánico 1 |
| `pedro.mamani` | `Tecnico2026!` | Tecnico | Mecánico 2 |

---

## Estructura de proyectos

```
MaintManager/
├── MaintManager.Domain/          ← Entidades, interfaces, enums
├── MaintManager.Application/     ← DTOs, servicios, validaciones, mappings
├── MaintManager.Infrastructure/  ← EF Core, repositorios, seed
├── MaintManager.API/             ← Controllers, middleware, Program.cs
├── MaintManager.MAUI/            ← App multiplataforma
└── MaintManager.Shared/          ← Constantes, rutas de API
```

---

## Migraciones EF Core

El proyecto usa enfoque **híbrido**:
- Tablas existentes (`list`, `public`, `product`, `company`, `service`): scaffolded, excluidas de migraciones
- Tablas nuevas (`maintenance.*`): Code First

```bash
# Desde la raíz de la solución:
dotnet ef migrations add InitialMaintenance \
  --project MaintManager.Infrastructure \
  --startup-project MaintManager.API \
  --context FleetMaintenanceContext

dotnet ef database update \
  --project MaintManager.Infrastructure \
  --startup-project MaintManager.API
```

> Las migraciones EF Core solo afectan el esquema `maintenance.*`.
> Las tablas existentes de la empresa **nunca** se modifican desde EF Core.

---

## Endpoints principales

| Método | Ruta | Rol | Descripción |
|--------|------|-----|-------------|
| POST | `/api/v1/auth/login` | Público | Autenticación JWT |
| GET | `/api/v1/vehicles` | Admin/Técnico | Lista de vehículos |
| GET | `/api/v1/maintenances` | Admin/Técnico | Lista paginada |
| POST | `/api/v1/maintenances` | Admin/Técnico | Crear orden |
| GET | `/api/v1/inventory/materials` | Admin/Técnico | Inventario |
| POST | `/api/v1/inventory/materials/{id}/lots` | Admin | Ingresar lote |
| GET | `/api/v1/alerts` | Admin/Técnico | Alertas sin resolver |
| GET | `/api/v1/reports/dashboard` | Admin | KPIs del dashboard |
| GET | `/api/v1/reports/cost-per-km` | Admin | Costo por km |
| GET | `/api/v1/reports/emergency-rate` | Admin | Tasa de emergencias |
| GET | `/api/v1/reports/monthly-cost` | Admin | Costo mensual |
| GET | `/api/v1/reports/maintenances/{id}/pdf` | Admin/Técnico | Exportar PDF |
| GET | `/api/v1/reports/cost-excel` | Admin | Exportar Excel |

---

## Lógica de negocio importante

### Recalendarización automática
- Mantenimiento **calendarizado** completado → `VehicleSchedule.next_km = mileage + interval_km`
- Emergencia **completa** → también recalendariza
- Emergencia **parcial** → NO recalendariza (solo se atendió lo urgente)

### FIFO por vencimiento
- Al consumir material del stock, se consume primero el lote más próximo a vencer
- Si un consumo abarca 2 lotes, se generan 2 registros en `material_consumption`

### Alertas automáticas
- Se generan al llamar `POST /api/v1/alerts/check`
- Se recomienda configurar una tarea programada (ej: cada hora) que llame a este endpoint
- Tipos: servicio próximo, componente por caducar, lote por vencer, stock bajo

---

## Notas de escalabilidad futura

Cuando Neo Plus crezca, agregar en este orden:

1. **Más usuarios / roles**: agregar nuevos `jobid` en la BD y mapearlos a roles JWT en `AuthController.DetermineRoleAsync`
2. **Múltiples técnicos por orden**: `maintenance.technician_assignment` ya existe — actualizar el Controller para aceptar un array de `workid`
3. **Más sucursales**: agregar campo `agency_id` en `maintenance.maintenance` → filtrar vistas por agencia
4. **Notificaciones push**: integrar Firebase Cloud Messaging en MAUI usando el sistema de `alert_log` existente
5. **Predicción de fallas**: la vista `vw_component_useful_life` ya calcula vida útil real vs recomendada — conectar a un modelo ML en Python vía endpoint externo
6. **Integración con módulo de rentas**: la vista `vw_vehicle_current_km` ya lee `service.rentexecute` — no requiere cambios
7. **Módulo de proveedores**: `public.provider` ya existe — agregar endpoints CRUD en la API

---

## Logs

Los logs de Serilog se escriben en:
- Consola (durante desarrollo)
- `MaintManager.API/logs/maintmanager-YYYYMMDD.log` (archivos diarios, retención 30 días)

---

*Generado para proyecto final académico — SENATI Arequipa 2026*
*Aprendiz: Carlos Enrique Tarazona Medrano*
