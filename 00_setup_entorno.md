# Configuración del entorno — MaintManager
# Neo Plus Business S.A.C.
# Ejecutar comandos EN ORDEN. Leer cada sección antes de ejecutar.

---

## REQUISITOS PREVIOS (instalar si no tienes)

### 1. .NET 10 SDK
```
winget install Microsoft.DotNet.SDK.10
```
Verificar:
```
dotnet --version
# debe mostrar 10.x.x
```

### 2. PostgreSQL 16 (opción A — instalador directo)
Descargar de: https://www.postgresql.org/download/windows/
- Durante instalación: password para usuario postgres → anótala (la usarás en appsettings)
- Puerto default: 5432

### 2b. PostgreSQL (opción B — Docker, si prefieres)
```
docker run --name neoplus-db -e POSTGRES_PASSWORD=NeoPlus2026! -e POSTGRES_DB=neoplus_maintenance -p 5432:5432 -d postgres:16
```

### 3. Herramientas EF Core (global)
```
dotnet tool install --global dotnet-ef
dotnet tool update --global dotnet-ef
```
Verificar:
```
dotnet ef --version
# debe mostrar 9.x o 10.x
```

---

## PASO 1 — Crear la base de datos en PostgreSQL

Abre pgAdmin o psql y ejecuta:

```sql
-- En psql:
CREATE DATABASE neoplus_maintenance
    WITH ENCODING 'UTF8'
    LC_COLLATE = 'es_PE.UTF-8'
    LC_CTYPE   = 'es_PE.UTF-8'
    TEMPLATE   = template0;
```

Si el locale da error en Windows, usar:
```sql
CREATE DATABASE neoplus_maintenance WITH ENCODING 'UTF8';
```

Luego ejecuta el script SQL completo que tienes (el archivo de la BD con todas las tablas).
Orden de ejecución del script:
1. CREATE SCHEMA (list, product, company, service, maintenance)
2. Secuencias
3. Tablas list.*
4. Tablas public.*
5. Tablas product.*
6. Tablas company.* y service.*
7. Tablas maintenance.*
8. Vistas BI

---

## PASO 2 — Crear la solución y proyectos

Ejecutar en tu carpeta de proyectos (ej: C:\Proyectos\):

```bash
# Crear carpeta raíz y solución
mkdir MaintManager
cd MaintManager
dotnet new sln -n MaintManager

# Crear proyectos
dotnet new classlib -n MaintManager.Domain        -f net10.0
dotnet new classlib -n MaintManager.Application   -f net10.0
dotnet new classlib -n MaintManager.Infrastructure -f net10.0
dotnet new webapi   -n MaintManager.API           -f net10.0
dotnet new classlib -n MaintManager.Shared        -f net10.0
dotnet new maui     -n MaintManager.MAUI

# Agregar proyectos a la solución
dotnet sln add MaintManager.Domain/MaintManager.Domain.csproj
dotnet sln add MaintManager.Application/MaintManager.Application.csproj
dotnet sln add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj
dotnet sln add MaintManager.API/MaintManager.API.csproj
dotnet sln add MaintManager.Shared/MaintManager.Shared.csproj
dotnet sln add MaintManager.MAUI/MaintManager.MAUI.csproj
```

---

## PASO 3 — Referencias entre proyectos

```bash
# Application referencia Domain y Shared
dotnet add MaintManager.Application/MaintManager.Application.csproj reference MaintManager.Domain/MaintManager.Domain.csproj
dotnet add MaintManager.Application/MaintManager.Application.csproj reference MaintManager.Shared/MaintManager.Shared.csproj

# Infrastructure referencia Application y Domain
dotnet add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj reference MaintManager.Application/MaintManager.Application.csproj
dotnet add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj reference MaintManager.Domain/MaintManager.Domain.csproj

# API referencia Application e Infrastructure
dotnet add MaintManager.API/MaintManager.API.csproj reference MaintManager.Application/MaintManager.Application.csproj
dotnet add MaintManager.API/MaintManager.API.csproj reference MaintManager.Infrastructure/MaintManager.Infrastructure.csproj
dotnet add MaintManager.API/MaintManager.API.csproj reference MaintManager.Shared/MaintManager.Shared.csproj

# MAUI referencia Shared (solo para constantes y rutas)
dotnet add MaintManager.MAUI/MaintManager.MAUI.csproj reference MaintManager.Shared/MaintManager.Shared.csproj
```

---

## PASO 4 — Instalar paquetes NuGet

```bash
# ── Infrastructure ──────────────────────────────────────────────
dotnet add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add MaintManager.Infrastructure/MaintManager.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# ── Application ──────────────────────────────────────────────────
dotnet add MaintManager.Application/MaintManager.Application.csproj package FluentValidation --version 11.11.0
dotnet add MaintManager.Application/MaintManager.Application.csproj package FluentValidation.DependencyInjectionExtensions --version 11.11.0

# ── API ──────────────────────────────────────────────────────────
dotnet add MaintManager.API/MaintManager.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
dotnet add MaintManager.API/MaintManager.API.csproj package Swashbuckle.AspNetCore --version 7.3.1
dotnet add MaintManager.API/MaintManager.API.csproj package Serilog.AspNetCore --version 9.0.0
dotnet add MaintManager.API/MaintManager.API.csproj package Serilog.Sinks.Console --version 6.0.0
dotnet add MaintManager.API/MaintManager.API.csproj package Serilog.Sinks.File --version 6.0.0
dotnet add MaintManager.API/MaintManager.API.csproj package QuestPDF --version 2025.4.0
dotnet add MaintManager.API/MaintManager.API.csproj package ClosedXML --version 0.104.2
dotnet add MaintManager.API/MaintManager.API.csproj reference MaintManager.Infrastructure/MaintManager.Infrastructure.csproj

# ── MAUI ─────────────────────────────────────────────────────────
dotnet add MaintManager.MAUI/MaintManager.MAUI.csproj package CommunityToolkit.Mvvm --version 8.4.0
dotnet add MaintManager.MAUI/MaintManager.MAUI.csproj package LiveChartsCore.SkiaSharpView.Maui --version 2.0.0-rc4
dotnet add MaintManager.MAUI/MaintManager.MAUI.csproj package Microsoft.Extensions.Http --version 10.0.0
```

---

## PASO 5 — Limpiar archivos de plantilla innecesarios

```bash
# Eliminar clases de ejemplo que genera dotnet new
rm MaintManager.Domain/Class1.cs
rm MaintManager.Application/Class1.cs
rm MaintManager.Infrastructure/Class1.cs
rm MaintManager.Shared/Class1.cs

# En API: eliminar el WeatherForecast de ejemplo
rm MaintManager.API/WeatherForecast.cs
rm MaintManager.API/Controllers/WeatherForecastController.cs
```

---

## PASO 6 — Verificar que todo compila

```bash
dotnet build MaintManager.sln
# Debe mostrar: Build succeeded. 0 Error(s)
```

---

## PASO 7 — Variables de entorno (NO hardcodear en código)

Crear archivo `MaintManager.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=neoplus_maintenance;Username=postgres;Password=TU_PASSWORD_AQUI;"
  },
  "Jwt": {
    "Key": "NeoPlus2026_SuperSecretKey_MantVehicular_32chars!!",
    "Issuer": "MaintManager.API",
    "Audience": "MaintManager.MAUI",
    "ExpirationHours": 8
  },
  "Serilog": {
    "MinimumLevel": "Debug"
  }
}
```

IMPORTANTE: agregar a .gitignore:
```
appsettings.Development.json
appsettings.Production.json
```

---

## RESUMEN DE VERSIONES

| Tecnología         | Versión     |
|--------------------|-------------|
| .NET               | 10.0        |
| EF Core            | 10.0.0      |
| Npgsql EF Provider | 10.0.0      |
| FluentValidation   | 11.11.0     |
| JWT Bearer         | 10.0.0      |
| Swashbuckle        | 7.3.1       |
| Serilog            | 9.0.0       |
| QuestPDF           | 2025.4.0    |
| ClosedXML          | 0.104.2     |
| CommunityToolkit   | 8.4.0       |
| LiveChartsCore     | 2.0.0-rc4   |
| PostgreSQL         | 16.x        |
