# MaintManager — Sistema de Gestión de Mantenimiento Vehicular con Business Intelligence

## Neo Plus Business S.A.C.

**Stack tecnológico:** .NET 10 · ASP.NET Web API · EF Core · PostgreSQL 16 · .NET MAUI 10.0.0 · Clean Architecture  
**Arquitectura frontend:** MVVM (CommunityToolkit.Mvvm)  
**Inteligencia de negocio:** Vistas SQL en tiempo real + LiveChartsCore  
**Autenticación:** JWT Bearer (HMAC‑SHA256, 8 h)  
**Exportaciones:** QuestPDF (órdenes) · ClosedXML (Excel)  

---

## Resumen del proyecto

**Problema:** Los datos generados durante el mantenimiento de la flota vehicular de Neo Plus Business S.A.C. estaban dispersos en registros manuales y hojas de cálculo, impidiendo su análisis y la toma de decisiones basada en evidencia.

**Solución:** Desarrollo e implementación de un sistema que centraliza la gestión de mantenimientos, controla el inventario con lotes y fechas de vencimiento, realiza seguimiento de componentes instalados y ofrece un **dashboard de Business Intelligence** con KPIs calculados en tiempo real.

**Resultado esperado:** Transformar datos operativos en información estratégica que permita a la gerencia optimizar recursos, reducir costos y mejorar la disponibilidad de la flota.

**Objetivos SMART del proyecto:**
- **Reducir ≥ 40 %** el tiempo total del proceso de mantenimiento (de 332 min a ≈140 min).
- **Eliminar el 100 %** de las actividades manuales de archivo y búsqueda de historial.
- **Reducir ≥ 50 %** las compras de emergencia en los primeros 2 meses.
- **Reducir ≥ 80 %** las pérdidas por caducidad de materiales.
- **Disponer de KPIs en tiempo real** desde el primer día de operación.

---

## Arquitectura del sistema (Clean Architecture + MVVM)

```
┌─────────────────────────────────────────────────┐
│              MaintManager.MAUI                  │
│       (MVVM · CommunityToolkit.Mvvm)            │
├─────────────────────────────────────────────────┤
│              MaintManager.API                   │
│   (ASP.NET Web API · JWT · Swagger · Serilog)   │
├──────────────┬──────────────────────────────────┤
│  Application │  Domain (Entities, Interfaces)   │
│  (DTOs,      │                                  │
│   Services,  │                                  │
│   Validators)│                                  │
├──────────────┴──────────────────────────────────┤
│          Infrastructure (EF Core · Npgsql)      │
├─────────────────────────────────────────────────┤
│              PostgreSQL 16                      │
│   • Esquemas existentes (solo lectura)          │
│   • Nuevo esquema: maintenance.* (20 tablas)    │
│   • Vistas BI (5 vistas)                        │
└─────────────────────────────────────────────────┘
```

**Principios aplicados:**
- **Domain** no depende de ningún framework externo.
- **Infrastructure** implementa las interfaces definidas en Domain.
- **API** solo orquesta; la lógica de negocio reside en las entidades y servicios de Application.
- **MAUI** consume la API REST mediante `HttpClient` con JWT.

---

## Estructura del repositorio

```
MaintManager/
├── MaintManager.Domain/          ← Entidades, interfaces de repositorio, lógica de negocio
├── MaintManager.Application/     ← DTOs, servicios, validadores FluentValidation, mappings
├── MaintManager.Infrastructure/  ← EF Core, repositorios, configuraciones Fluent API, seed data
├── MaintManager.API/             ← Controladores REST, middleware (JWT, CORS, excepciones), Swagger
├── MaintManager.MAUI/            ← App Android/Windows, ViewModels, Pages, Services, Converters, Controls
└── MaintManager.Shared/          ← Constantes (ApiRoutes, ErrorMessages, RoleNames, AlertTypes) + DTOs compartidos (Shared/Models)
```

---

## Requisitos previos

| Herramienta               | Versión mínima |
|---------------------------|---------------|
| .NET SDK                  | 10.0          |
| PostgreSQL                | 16.x          |
| EF Core CLI (`dotnet-ef`) | 9.x / 10.x    |
| Android SDK (MAUI)        | API 24+       |

---

## Configuración inicial

### 1. Clonar y restaurar

```bash
git clone <repositorio>
cd MaintManager
dotnet restore MaintManager.sln
```

### 2. Crear la base de datos

Ejecutar en orden los siguientes scripts (carpeta `database/` o `sql/`):

1. `BD-FINAL-COMPLETAMENTE-CORREGIDA.sql` — Script principal (esquemas, tablas, vistas)
2. `02_ajustes_fase1.sql` — Campos adicionales (`assigned_to`, `next_service_type_code`, `updated_at`, `technician_assignment`)
3. `03_seed_data.sql` — Datos de prueba (12 vehículos, 3 usuarios, 5 mantenimientos históricos, lotes, etc.)

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

> `appsettings.Development.json` está incluido en `.gitignore`. **No se deben versionar credenciales.**

### 4. Nota sobre HttpClient en MAUI

`ApiService.cs` usa `JsonContent.Create(data, options)` para serializar requests POST/PUT. Esto evita el bug de MAUI 10.0.0 donde `StringContent(..., "application/json")` no transmite correctamente el header `Content-Type` (415), y es compatible con AOT en Release. Los DTOs de request/response residen en `MaintManager.Shared.Models` como tipos concretos para que AOT genere serializadores.

---

## Compilación, ejecución e instalación

### Requisitos previos

| Herramienta | Versión | Propósito |
|------------|---------|-----------|
| .NET SDK | 10.0 | Compilar todo el proyecto |
| PostgreSQL | 16.x | Base de datos |
| Android SDK | API 24+ | Compilar APK de MAUI |
| ADB (Android Debug Bridge) | — | Instalar APK en emulador/dispositivo |

### 1. Base de datos

Ejecutar los scripts SQL en orden:

```bash
psql -h localhost -U postgres -d neoplus_maintenance -f ../bd-final.sql
psql -h localhost -U postgres -d neoplus_maintenance -f ../02_ajustes_fase1.sql
psql -h localhost -U postgres -d neoplus_maintenance -f ../03_seed_data.sql
```

> Los scripts están en `C:\Users\carlo\Desktop\proyect\`. Usar `psql` con `$env:PGPASSWORD="postgres"` si es necesario.

### 2. API Backend

```bash
cd MaintManager.API
dotnet build -c Release
dotnet run --urls "http://0.0.0.0:5056"
# API → http://localhost:5056
# Swagger → http://localhost:5056/swagger
```

> `0.0.0.0` expone la API en todas las interfaces de red. El emulador Android la alcanza desde `http://10.0.2.2:5056`. Para un celular físico, ver sección 6.

### 3. App MAUI (Android) — Compilar e instalar



```bash
cd MaintManager.MAUI
dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormats=apk
adb install -r bin/Release/net10.0-android/publish/com.companyname.maintmanager.maui-Signed.apk
```

### 4. App MAUI (Windows)

```bash
cd MaintManager.MAUI
dotnet run -f net10.0-windows10.0.19041.0
```

### 5. Después de modificar código fuente

Siempre seguir este orden:

```bash
# 1. Compilar API
dotnet build MaintManager.API/MaintManager.API.csproj -c Release

# 2. Compilar y publicar MAUI  
dotnet publish MaintManager.MAUI/MaintManager.MAUI.csproj -f net10.0-android -c Release -p:AndroidPackageFormats=apk

# 3. Instalar APK (el emulador debe estar corriendo)
adb install -r MaintManager.MAUI/bin/Release/net10.0-android/publish/com.companyname.maintmanager.maui-Signed.apk

# 4. Iniciar API
dotnet run --project MaintManager.API/MaintManager.API.csproj --urls "http://0.0.0.0:5056"
```

### 6. Ejecutar desde un celular físico Android

Para usar la app desde un **teléfono Android real** (no emulador), ambos dispositivos deben estar en la **misma red WiFi**.

#### 6.1 Obtener la IP local de tu PC

En la PC donde corre la API, abre una terminal y ejecuta:

```bash
ipconfig
```

Busca la dirección IPv4 de tu adaptador WiFi activo. Se ve similar a:

```
Dirección IPv4. . . . . . . . . . . : 192.168.3.134
```

Anota esa IP. La necesitarás en los pasos siguientes.

> 💡 Si cambias de red WiFi, la IP cambia. Repite este paso cada vez.

#### 6.2 Permitir la conexión en el Firewall de Windows

La primera vez que inicies la API con `--urls "http://0.0.0.0:5056"`, Windows Firewall mostrará una ventana preguntando si quieres permitir el acceso. Haz clic en **"Permitir acceso"**.

Si no apareció la ventana o la bloqueaste, hazlo manualmente:

```powershell
# Ejecutar como Administrador en PowerShell:
New-NetFirewallRule -DisplayName "MaintManager API 5056" `
    -Direction Inbound -Protocol TCP -LocalPort 5056 -Action Allow
```

#### 6.3 Compilar e instalar el APK

Con el teléfono conectado por USB (o el APK transferido):

```bash
cd MaintManager.MAUI
dotnet publish -f net10.0-android -c Release -p:AndroidPackageFormats=apk
adb install -r bin/Release/net10.0-android/publish/*-Signed.apk
```

> Si no usas ADB, transfiere el archivo `*-Signed.apk` al teléfono e instálalo manualmente.

#### 6.4 Iniciar la API

```bash
cd MaintManager.API
dotnet run --urls "http://0.0.0.0:5056"
```

> ⚠️ El flag **debe ser `--urls`** con doble guion. Si usas `-urls` (un guion), se ignora y se usa el puerto de `launchSettings.json`. El flag `0.0.0.0` es **obligatorio** para que la API acepte conexiones desde otros dispositivos en la red.

#### 6.5 Configurar la URL en la app

1. Abre la app en el teléfono.
2. Ve a la pantalla de **Configuración** (menú lateral).
3. En el campo **URL de la API**, ingresa: `http://<IP-DE-TU-PC>:5056`
   - Ejemplo: `http://192.168.1.100:5056`
4. Presiona **Guardar**.
5. Regresa al Login e ingresa tus credenciales.

#### 6.6 Verificar conectividad

Si sigue sin conectar, prueba estos pasos:

```bash
# 1. Confirma que la API responde localmente (en la PC):
curl http://localhost:5056/api/v1/auth/login -X POST ^
  -H "Content-Type: application/json" ^
  -d "{\"username\":\"herror.ortiz\",\"password\":\"Admin2026!\"}"

# 2. Confirma que responde desde otro dispositivo:
#    (En otro celular/tablet, abre el navegador y visita:)
#    http://<IP-DE-TU-PC>:5056/swagger

# 3. Verifica que no hay otro proceso en el puerto 5056:
netstat -ano | findstr :5056

# 4. Si el firewall bloquea, verifica la regla:
Get-NetFirewallRule -DisplayName "MaintManager API 5056" | fl
```

> ⚠️ **Importante:** En redes públicas (hoteles, centros comerciales) los dispositivos suelen estar aislados y no pueden comunicarse entre sí. Usa una red personal (ej. hotspot del celular) en esos casos.

---

### Configuración fija de compilación MAUI (NO MODIFICAR)

| Parámetro | Valor |
|-----------|-------|
| `TargetFramework` | `net10.0-android` |
| `MauiVersion` | `10.0.0` |
| `RuntimeIdentifiers` | `android-arm64` |
| `AndroidStoreUncompressedFileExtensions` | `.dll;.so;.pdb` |
| `AndroidEnableCompression` | `false` |
| `AndroidUseSharedRuntime` | `false` |
| Paquete `CommunityToolkit.Maui` | ❌ Eliminado (causa crash) |
| Paquete `CommunityToolkit.Mvvm` | ✅ Conservado |

### Credenciales de prueba

| Usuario | Contraseña | Rol |
|---------|-----------|-----|
| `herror.ortiz` | `Admin2026!` | Admin (Gerente General) |
| `juan.quispe` | `Tecnico2026!` | Técnico |
| `pedro.mamani` | `Tecnico2026!` | Técnico |

---

## Usuarios del sistema (seed data)

| Usuario         | Contraseña    | Rol     | Descripción                             |
|-----------------|--------------|---------|------------------------------------------|
| `herror.ortiz`  | `Admin2026!` | Admin   | Gerente General y Jefe de Mantenimiento |
| `juan.quispe`   | `Tecnico2026!` | Tecnico | Mecánico 1                             |
| `pedro.mamani`  | `Tecnico2026!` | Tecnico | Mecánico 2                             |

> Herror Ortiz es el dueño de la empresa y actúa como **Jefe de Mantenimiento / Admin** del sistema.

---

## Migraciones EF Core (enfoque híbrido)

*   **Tablas existentes** (`list.*`, `public.*`, `product.*`, `company.*`, `service.*`): **Database First**, marcadas con `ExcludeFromMigrations()`. Solo lectura (`AsNoTracking`).
*   **Nuevas tablas** (`maintenance.*`): **Code First** con migraciones de EF Core.

```bash
# Crear migración
dotnet ef migrations add InitialMaintenance \
  --project MaintManager.Infrastructure \
  --startup-project MaintManager.API \
  --context FleetMaintenanceContext

# Aplicar migración
dotnet ef database update \
  --project MaintManager.Infrastructure \
  --startup-project MaintManager.API
```

> Las migraciones **solo afectan** al esquema `maintenance.*`.

---

## Endpoints principales de la API

| Método | Ruta                                         | Rol           | Descripción                         |
|--------|----------------------------------------------|---------------|-------------------------------------|
| POST   | `/api/v1/auth/login`                         | Público       | Autenticación (devuelve JWT)        |
| GET    | `/api/v1/vehicles`                            | Admin/Tecnico | Lista de vehículos activos          |
| GET    | `/api/v1/maintenances`                        | Admin/Tecnico | Lista paginada de órdenes           |
| POST   | `/api/v1/maintenances`                        | Admin/Tecnico | Crear orden de mantenimiento        |
| PUT    | `/api/v1/maintenances/{id}/close`             | Admin/Tecnico | Cerrar orden                        |
| GET    | `/api/v1/inventory/materials`                 | Admin/Tecnico | Inventario de materiales            |
| POST   | `/api/v1/inventory/materials/{id}/lots`       | Admin         | Ingresar lote                       |
| GET    | `/api/v1/alerts`                              | Admin/Tecnico | Alertas sin resolver                |
| POST   | `/api/v1/alerts/check`                        | Admin         | Ejecutar verificación de alertas    |
| GET    | `/api/v1/reports/dashboard`                   | Admin         | KPIs del dashboard BI               |
| GET    | `/api/v1/reports/cost-per-km`                 | Admin         | Costo por km por vehículo           |
| GET    | `/api/v1/reports/maintenances/{id}/pdf`       | Admin/Tecnico | Exportar orden a PDF (QuestPDF)     |
| GET    | `/api/v1/reports/cost-excel`                  | Admin         | Exportar reporte de costos a Excel  |

---

## Lógica de negocio principal

### Recalendarización automática (SchedulingService)
- Orden **calendarizada** o **emergencia completa** → `VehicleSchedule.Reschedule(mileage)`
    - `nextKm = mileage + IntervalKm`
    - Alterna tipo A ↔ B automáticamente.
- Emergencia **parcial** → **NO** recalendariza.

### FIFO por vencimiento (InventoryService)
- Al consumir material del inventario, se consume primero el lote **más próximo a vencer**.
- Si un consumo abarca varios lotes, se genera un registro en `material_consumption` por cada lote afectado.

### Alertas automáticas (AlertService)
- Endpoint `POST /api/v1/alerts/check` (Admin o Sistema de Alertas).
- Verifica los 4 tipos: servicio próximo (km), componente por caducar, lote por vencer, stock bajo.
- **No crea duplicados** si ya existe una alerta activa (`resolved = false`) para la misma referencia.

---

## Diagramas UML y documentación del proyecto

Los siguientes artefactos se encuentran en la carpeta `docs/` o en el documento del proyecto (Capítulo IV):

- **Diagrama de Casos de Uso** (36 casos de uso, 3 actores, relaciones include/extend)
- **Especificación de Casos de Uso** (formato completo para los 36 CU)
- **Diagrama de Clases** (20 entidades del dominio `maintenance.*` + 4 clases externas)
- **Diagramas de Secuencia** (3 escenarios: Ciclo de Mantenimiento, Dashboard BI, Verificación de Alertas)
- **Diagrama de Actividades** (Flujo completo del sistema con particiones Actor/MAUI/Sistema)
- **Diagrama de Arquitectura** (Clean Architecture + MVVM)
- **Diagrama de Despliegue** (Servidor cloud Linux, MAUI Android, PostgreSQL)
- **DAP Mejorado** (15 actividades, reducción del 52 % respecto al proceso manual)

---

## Plan de acción (6 fases)

| Fase | Semanas | Actividades principales |
|------|---------|------------------------|
| 1. Análisis | S1‑S2 | Levantamiento de procesos, requerimientos funcionales/no funcionales |
| 2. Diseño | S3‑S4 | Diagramas UML, diseño de BD, mockups Figma |
| 3. Desarrollo | S5‑S12 | Implementación del dominio, API, BI, frontend MAUI |
| 4. Pruebas | S13‑S14 | Unitarias, integración, usuario (con datos seed) |
| 5. Implementación | S15‑S16 | Despliegue cloud, instalación MAUI, capacitación |
| 6. Evaluación | S17‑S18 | Medición de indicadores, informe comparativo antes/después |

---

## Indicadores de mejora esperados

| Código | Indicador | Meta |
|--------|-----------|------|
| IND‑01 | Reducción del tiempo de proceso | ≥ 40 % (de 332 min → ≈140 min) |
| IND‑02 | Disponibilidad de KPIs | Tiempo real (antes: horas/días) |
| IND‑03 | Reducción de compras de emergencia | ≥ 50 % en 2 meses |
| IND‑04 | Reducción de pérdidas por caducidad | ≥ 80 % (ahorro S/ 300 semestrales) |
| IND‑05 | Cumplimiento del calendario de mantenimiento | 0 vehículos fuera de rango |
| IND‑06 | Tiempo de acceso al historial | < 5 segundos (antes: 15 min) |
| IND‑07 | Disponibilidad del sistema | ≥ 95 % en horario laboral |

---

## Aspectos limitantes identificados

| Código | Limitante | Mitigación |
|--------|-----------|------------|
| AL‑01 | Dependencia de internet en taller | Garantizar WiFi estable + caché local básico |
| AL‑02 | Resistencia al cambio del personal | Capacitación práctica + período de marcha paralela |
| AL‑03 | Compatibilidad de versiones .NET 10 / MAUI | Fijar versiones exactas + pruebas tempranas en dispositivos |
| AL‑04 | Dependencia de estructura de BD de rentas | Excluir tablas existentes de migraciones + monitoreo |
| AL‑05 | Tiempo de desarrollo limitado (18 semanas) | Priorizar módulos core (RF‑01 a RF‑15) |
| AL‑06 | Costo del servidor cloud en producción | Alternativa: servidor local si no se aprueba presupuesto |
| AL‑07 | Contraseñas en MD5 (débiles) | Planificar migración futura a bcrypt/PBKDF2 |
| AL‑08 | Calidad del seed data | Verificar completitud antes de puesta en marcha |

---

## Costo total de implementación

| Concepto | Costo (S/) |
|----------|-----------|
| Materiales e insumos | 0.00 |
| Mano de obra (desarrollo integral) | 1,936.00 |
| Máquinas, servicios e infraestructura | 826.00 |
| Otros costos (movilidad) | 162.00 |
| **Total** | **2,924.00** |

> El costo de software es cero gracias al uso de herramientas open source (.NET, PostgreSQL, VS Community, etc.).

---

## Notas de escalabilidad futura

1. **Más roles**: agregar nuevos `jobid` en `public.job` y mapearlos en `AuthController.DetermineRoleAsync`.
2. **Múltiples técnicos por orden**: la tabla `technician_assignment` ya lo soporta.
3. **Más sucursales**: añadir `agency_id` en `maintenance.*` y filtrar por agencia.
4. **Notificaciones push**: integrar Firebase Cloud Messaging usando `alert_log`.
5. **Predicción de fallas**: usar datos de `vw_component_useful_life` + modelo ML externo.
6. **Módulo de proveedores**: `public.provider` ya existe; agregar endpoints CRUD.

---

##  Seguridad y logs

- **JWT** con clave ≥ 32 caracteres, almacenada en variables de entorno.
- **Serilog** escribe logs diarios en `MaintManager.API/logs/maintmanager-YYYYMMDD.log` (retención 30 días).
- **GlobalExceptionMiddleware** captura todas las excepciones no controladas y responde con `ApiResponse<T>` y códigos HTTP semánticos.

---

## Diseño de pantallas, experiencia de usuario y arquitectura frontend

El frontend de **MaintManager** fue diseñado bajo un enfoque de software enterprise operativo, priorizando rapidez de uso, claridad visual y toma de decisiones en tiempo real para personal técnico y administrativo.

La aplicación utiliza:
- **.NET MAUI 10** multiplataforma (Windows + Android)
- Arquitectura **MVVM**
- Navegación centralizada mediante `AppShell`
- Componentes reutilizables
- Diseño adaptativo para escritorio y dispositivos móviles

El diseño visual toma como referencia principios modernos inspirados en:
- Microsoft Fluent Design
- Azure Portal
- Fleet Management Systems
- Dashboards operativos modernos

---

## Arquitectura de navegación frontend

La navegación principal del sistema está organizada mediante `Shell Navigation`, permitiendo desacoplar módulos y mantener una experiencia consistente.

```text
AppShell
 ├── Dashboard
 ├── Calendario
 ├── Mantenimientos
 ├── Inventario
 ├── BI Dashboard
 ├── Reportes
 └── Configuración
````

### Objetivos del diseño de navegación

* Reducir el tiempo de acceso a funciones críticas.
* Minimizar clicks para tareas operativas.
* Mantener accesibles los módulos principales desde cualquier pantalla.
* Separar claramente operaciones, análisis y administración.

---

# HOME — Panel principal operativo

La pantalla principal funciona como centro de control operativo de la flota.

## Objetivo principal

Proporcionar al usuario una vista rápida del estado actual del sistema:

* mantenimientos próximos,
* vehículos críticos,
* accesos rápidos,
* métricas operativas,
* estado general de la flota.

## Elementos principales

* Vehículos de la flota
* Accesos rápidos:

    * Nuevo mantenimiento
    * Ver calendario
    * Ver inventario
    * Dashboard BI
* Resumen de módulos operativos
* Estado general de mantenimientos
* Indicadores rápidos

## Principios UX aplicados

* Priorización de acciones frecuentes.
* Accesos rápidos visibles sin navegación adicional.
* Jerarquía visual clara.
* Información resumida y accionable.

## Mejoras arquitectónicas definidas

El dashboard principal NO debe convertirse en una pantalla saturada de widgets y gráficos.
El objetivo es mantener el foco en:

> estado operativo inmediato de la flota.

Por ello:

* los gráficos complejos se concentran en el módulo BI,
* la HOME se mantiene orientada a operación rápida,
* las métricas mostradas son únicamente las críticas.

---

# CALENDARIO — Gestión de mantenimientos

Pantalla destinada al seguimiento completo de mantenimientos:

* realizados,
* próximos,
* calendarizados,
* emergencias,
* preventivos.

## Objetivo principal

Permitir visualizar rápidamente:

* qué vehículos requieren atención,
* cuáles están próximos a mantenimiento,
* cuáles presentan incidencias.

## Elementos principales

* Timeline de mantenimientos
* Estados visuales:

    * Próximo
    * Pendiente
    * Completado
    * Emergencia
* Filtros por:

    * Vehículo
    * Tipo
    * Estado

## Principios UX aplicados

* Estados altamente visuales mediante colores y badges.
* Lectura rápida sin abrir detalles.
* Priorización visual de emergencias.

## Mejoras definidas

El historial debe evolucionar hacia un formato tipo timeline vertical:

```text
● Servicio A
│
● Emergencia
│
● Preventivo
```

Esto mejora:

* lectura cronológica,
* comprensión histórica,
* identificación de eventos críticos.

---

# FORMULARIO — Registro de mantenimiento

Esta es la pantalla operativa más importante del sistema.

Actualmente integra:

* diagnóstico,
* operaciones,
* componentes,
* historial,
* planificación,
* consumo,
* estado del vehículo.

## Objetivo principal

Permitir al técnico registrar mantenimientos de forma rápida, clara y con el menor margen de error posible.

---

## Rediseño UX crítico aplicado

Se definió que el formulario NO debe implementarse como:

> una página gigante con scroll infinito.

En su lugar, se implementará mediante:

# Wizard / Stepper UX

---

## Flujo del formulario

### Paso 1 — Vehículo y contexto

* Información del vehículo
* Kilometraje actual
* Tipo de mantenimiento

### Paso 2 — Tipo de servicio

* Servicio A
* Servicio B
* Emergencia
* Preventivo

### Paso 3 — Componentes y materiales

* Materiales utilizados
* Consumo FIFO
* Origen del material
* Validaciones de stock

### Paso 4 — Operaciones realizadas

* Checklist técnico
* Actividades ejecutadas

### Paso 5 — Diagnóstico

* Resultado final
* Observaciones
* Alertas detectadas

### Paso 6 — Planificación siguiente servicio

* Recalendarización automática
* Próximo kilometraje
* Tipo siguiente

### Paso 7 — Resumen y confirmación

* Validación final
* Confirmación
* Guardado

---

## Beneficios del enfoque Wizard UX

* Reduce fatiga visual.
* Mejora concentración del técnico.
* Disminuye errores operativos.
* Mejora experiencia móvil Android.
* Facilita validaciones por etapa.
* Hace el sistema más escalable.

---

# INVENTARIO — Stock y materiales

Módulo orientado al control de materiales y consumos.

## Objetivo principal

Centralizar:

* materiales,
* lotes,
* vencimientos,
* movimientos,
* alertas de stock.

## Elementos principales

* Lista de materiales
* Historial de movimientos
* Alertas de stock bajo
* Información FIFO
* Consumos por mantenimiento

## Principios UX aplicados

* Visualización rápida de stock crítico.
* Diferenciación visual por estado.
* Priorización de materiales críticos.

## Mejoras definidas

Las tablas extensas deben adaptarse dinámicamente:

### Desktop

* tablas completas,
* grids avanzados,
* filtros laterales.

### Mobile

* cards verticales,
* diseño responsive,
* reducción de columnas visibles.

---

# DASHBOARD BI — Business Intelligence

Pantalla especializada en análisis operativo y toma de decisiones.

## Objetivo principal

Transformar datos operativos en métricas estratégicas en tiempo real.

## KPIs principales

* Costo por km
* Tasa de emergencias
* Servicios realizados
* Cumplimiento de calendario
* Predicción de fallas
* Vida útil de componentes
* Ahorro interno vs externo

---

## Principios UX aplicados

El dashboard BI se diseñó bajo:

> enfoque analítico y no decorativo.

Por ello:

* los gráficos representan métricas accionables,
* se evita saturación visual,
* se priorizan indicadores de impacto operacional.

---

## Estructura visual definida

### Fila 1

KPIs críticos

### Fila 2

Tendencias operativas

### Fila 3

Alertas inteligentes

### Fila 4

Predicciones y análisis futuros

---

## Feature diferencial del sistema

La predicción de fallas futuras se considera una funcionalidad estratégica del sistema.

Ejemplo:

* identificación de vehículo con mayor probabilidad de falla,
* cálculo estimado de tiempo restante,
* priorización preventiva.

---

# Sistema visual y Design System

Para garantizar consistencia visual y reutilización, el frontend implementará un sistema de diseño reusable.

## Tokens visuales

```csharp
AppColors.Primary
AppSpacing.Medium
AppRadius.Large
AppTypography.Title
```

---

## Componentes reutilizables definidos

### StatusBadge

Utilizado para:

* emergencias,
* stock bajo,
* calendarizados,
* preventivos,
* estados operativos.

### KpiCard

Utilizado en:

* dashboard principal,
* BI,
* inventario.

### VehicleCard

Componente reutilizable para:

* vehículos,
* resumen rápido,
* accesos contextuales.

### EmptyState

Pantallas sin información:

* sin alertas,
* sin mantenimientos,
* sin stock crítico.

### LoadingState

Carga de datos:

* skeleton loaders,
* indicadores visuales,
* placeholders.

---

# Diseño responsive y adaptativo

El frontend fue concebido para:

* escritorio,
* tablets,
* dispositivos Android.

## Estrategias adaptativas

### Desktop

* layouts tipo grid,
* paneles laterales,
* múltiples columnas.

### Mobile

* navegación simplificada,
* bottom tabs,
* cards verticales,
* formularios por pasos.

---

# Optimización UX y rendimiento

## Restricciones arquitectónicas definidas

Para mantener rendimiento estable en Android y Windows:

* Evitar `CollectionView` anidados dentro de `ScrollView`.
* Minimizar renderizados complejos.
* Utilizar carga diferida (`lazy loading`) cuando sea necesario.
* Mantener componentes desacoplados y reutilizables.

---

# Filosofía visual del sistema

MaintManager busca transmitir:

* claridad,
* rapidez,
* confiabilidad,
* entorno profesional.

Por ello:

* se evita saturación visual,
* se minimizan bordes innecesarios,
* se utilizan elevaciones suaves,
* se mantiene jerarquía visual clara,
* se prioriza legibilidad y foco operacional.

El objetivo final es que el sistema se perciba como:

> una plataforma enterprise moderna de gestión operativa vehicular.

---
# Tema consistente (Colores)
* Azul oscuro enterprise
* Gris neutro
* Verde éxito
* Naranja warning
* Rojo crítico

NO uses colores saturados tipo app escolar.

---

*Proyecto final académico — SENATI Arequipa 2026*  
*Aprendiz: Carlos Enrique Tarazona Medrano*  
*Asesor: M. Sc. César Rosas Aragón*

