# Manual de Pruebas — MaintManager

## Configuración inicial

### 1. Recrear BD desde cero
```bash
psql -U postgres -c "DROP DATABASE IF EXISTS neoplus_maintenance;"
psql -U postgres -c "CREATE DATABASE neoplus_maintenance;"
psql -U postgres -d neoplus_maintenance -f bd-final.sql
psql -U postgres -d neoplus_maintenance -f 02_ajustes_fase1.sql
psql -U postgres -d neoplus_maintenance -f 05_seed_final.sql
psql -U postgres -d neoplus_maintenance -f 06_fix_bi_views.sql
```

### 2. Compilar e instalar
```bash
# API
dotnet build MaintManager.API/MaintManager.API.csproj -c Release
dotnet run --project MaintManager.API/MaintManager.API.csproj --urls "http://0.0.0.0:5056"

# MAUI (otra terminal)
dotnet publish MaintManager.MAUI/MaintManager.MAUI.csproj -f net10.0-android -c Release -p:AndroidPackageFormats=apk
adb install -r MaintManager.MAUI/bin/Release/net10.0-android/publish/*-Signed.apk
```

### 3. Usuarios
| Usuario | Contraseña | Rol |
|---|---|---|
| `herror.ortiz` | `Admin2026!` | Admin (Gerente) |
| `juan.quispe` | `Tecnico2026!` | Mecánico |
| `pedro.mamani` | `Tecnico2026!` | Mecánico |

---

## Flujo 1: Login + Home

### Paso 1.1: Configurar URL
1. Abrir app → ver login
2. Tocar "▸ Cambiar URL de conexión"
3. Ingresar: `http://IP_DEL_SERVIDOR:5056` (ej: `http://192.168.1.10:5056`)
4. Tocar "Guardar URL"

### Paso 1.2: Login Admin
1. Usuario: `herror.ortiz`
2. Contraseña: `Admin2026!`
3. Tocar "Ingresar"

### Paso 1.3: Verificar Home
- **KPIs**: 4 tarjetas con números
  - Vehículos: 16 ✅
  - Servicios del Mes: 0 (no hay servicios activos este mes)
  - Stock Bajo: 0
  - Alertas: 0
- **Flota**: lista de vehículos con placa y km
- **Resumen Mantenimientos**: todos en 0 (solo histórico)
- **Acciones Rápidas**: 4 botones funcionales

---

## Flujo 2: Nueva Orden de Mantenimiento

### Paso 2.1: Iniciar wizard
1. Home → "🚗 Nuevo Mantenimiento" (o menú "Nuevo")

### Paso 2.2: Seleccionar vehículo (Step 1/4)
- Picker: seleccionar cualquier vehículo (ej: VW Gol - V0U-053)
- Ingresar km: `155500` (mayor al último registrado)
- Verificar: aparece "Último km registrado: 155198 km"
- Tocar "Siguiente"

### Paso 2.3: Tipo de servicio (Step 2/4)
- Seleccionar: "Servicio A"
- Tocar "Siguiente"

### Paso 2.4: Asignar técnico (Step 3/4)
- Seleccionar: "Juan Carlos Quispe" (juan.quispe)
- Nota (opcional): "Revisar presión de neumáticos"
- Tocar "Siguiente"

### Paso 2.5: Confirmar (Step 4/4)
- Verificar resumen: vehículo, km, servicio, técnico
- Tocar "Guardar"

### Verificar resultado:
- ✅ Orden creada
- ✅ Navega automáticamente al detalle de la orden
- ✅ Ver: número de orden, tipo, técnico, estado "Activo"

---

## Flujo 3: Detalle de Orden

### Paso 3.1: Información general
- Ver placa, tipo, fecha, km, técnico
- Tocar "Ver historial completo del vehículo" → lista de mantenimientos del VW Gol

### Paso 3.2: Completar acciones (checklist)
- Ver lista de acciones en "Operaciones"
- Tocar "Completar" en "Aceite de Motor" y "Filtro de Aceite"
- ✅ Acción marcada como completada

### Paso 3.3: Consumir material (FIFO)
- Seleccionar material: cualquier material del picker
- **Verificar**: aparece recuadro informativo con:
  - Número de lote FIFO
  - Fecha de vencimiento
  - Costo unitario
- Ingresar cantidad: `4.5`
- Tocar "Consumir"
- ✅ Aparece diálogo de calificación
- Seleccionar "No, gracias" (o calificar para probar rating)

### Paso 3.4: Instalar componente
- Seleccionar componente del picker (ej: "Pastillas de Freno Delanteras")
- Tocar "Instalar"
- ✅ Aparece en la lista de Componentes Instalados

### Paso 3.5: Guardar diagnóstico
- En la sección "Registrar Diagnóstico"
- Ingresar observaciones: "Cambio de aceite realizado, motor en buen estado. Se recomienda revisar filtro de aire en próximo servicio."
- Tocar "Guardar Diagnóstico"
- ✅ El formulario desaparece, muestra el diagnóstico guardado

### Paso 3.6: Cerrar orden
- Botón "Cerrar Orden" ahora visible (solo si hay diagnóstico)
- Tocar "Cerrar Orden"
- ✅ Orden cerrada, navega atrás
- ✅ En lista de mantenimientos: esta orden ahora aparece con estado "Finalizado"

---

## Flujo 4: Lista de Mantenimientos

### Paso 4.1: Ver lista
- Menú → "Mantenimientos"
- ✅ Lista de todas las órdenes (seed + recién creada)

### Paso 4.2: Filtrar
- Picker de filtros: probar cada uno
  - "Todas" → todas las órdenes
  - "Pendientes" / "En progreso" → órdenes con estado 'AC'
  - "Completadas" → órdenes con estado 'FI' (28+)
  - "Canceladas" → órdenes con estado 'CA' (1)

### Paso 4.3: Buscar
- Ingresar texto: `VW` → filtra por nombre o placa

---

## Flujo 5: Calendario

### Paso 5.1: Ver calendario
- Menú → "Calendario"
- ✅ Lista de mantenimientos ordenados por fecha

### Paso 5.2: Filtrar calendario
- Tipo: "Calendarizado" o "Emergencia"
- Estado: "Programado", "En Progreso", "Completado"
- Tocar "Filtrar"

### Paso 5.3: Ir a detalle
- Tocar cualquier item → navega al detalle de la orden

---

## Flujo 6: Inventario

### Paso 6.1: Ver materiales
- Menú → "Inventario"
- ✅ Lista de 12 materiales con stock total

### Paso 6.2: Filtrar stock bajo
- Marcar "Solo stock bajo mínimo" → filtra materiales con stock bajo

### Paso 6.3: Ver lotes de un material
- Tocar cualquier material → navega a lista de lotes
- ✅ Cada lote muestra: número de lote, ingreso, vencimiento, cantidad (actual/inicial), costo unitario, estado
- ✅ Orden FIFO: el primer lote es el que vence primero

### Paso 6.4: Descartar lote (Admin)
- Seleccionar un lote activo
- Tocar botón rojo "Descartar"
- Confirmar: "Sí, descartar"
- Ingresar motivo: "Vencido"
- ✅ Lote marcado como descartado, desaparece de FIFO

### Paso 6.5: Crear material (Admin)
- En inventario, barra inferior → "+ Nuevo material"
- Categoría: "Lubricantes"
- Nombre: "Aceite Motor 0W-20 Sintético"
- Unidad: "Litros"
- Stock mínimo: "8"
- Tocar "Guardar Material"
- ✅ Material creado, aparece en la lista

### Paso 6.6: Ingresar lote (Admin)
- En inventario, barra inferior → "+ Ingresar lote"
- Seleccionar material del picker
- Cantidad: `20`
- Costo unitario: `35.00`
- Activar "Tiene vencimiento" → fecha: `2027-06-01`
- N° de lote: `LOT-TEST-001`
- Tocar "Ingresar Lote"
- ✅ Lote creado, stock del material aumentado

---

## Flujo 7: Dashboard BI

### Paso 7.1: Cargar dashboard
- Menú → "BI Dashboard"
- ✅ KPIs cargan con datos:
  - Vehículos: 16
  - Servicios del Mes: 0 (no hay servicios este mes en seed)
  - Costo Promedio por Km: valor calculado de consumos
  - Tasa de Emergencia: porcentaje calculado

### Paso 7.2: Gráficos
- ✅ Costo por km: barras por vehículo
- ✅ Tasa de Emergencia: barras horizontales
- ✅ Costos Mensuales: línea de los últimos 6 meses
- ✅ Lotes por Vencer: gráfico de dona
- ✅ Desviación de Calendario: barras de desviación

---

## Flujo 8: Alertas

### Paso 8.1: Verificar alertas
- Menú → "Alertas"
- ✅ Página carga (vacía porque alert_log = 0)
- Tocar "Verificar alertas ahora"
- ✅ POST /api/v1/alerts/check ejecutado
- ❗ Si no hay condiciones (stock no bajo, sin lotes próximos), no genera alertas

### Paso 8.2: Marcar como leída/resolver
- (Requeriría alertas existentes)
- Tocar "Leída" → marca como leída
- Admin puede tocar "Resolver" → elimina de la lista

---

## Flujo 9: Reportes

### Paso 9.1: Exportar PDF
- Menú → "Reportes"
- Seleccionar una orden → "Exportar PDF"
- ✅ PDF generado y compartido

### Paso 9.2: Exportar Excel
- Tocar "Exportar Reporte de Costos"
- ✅ Excel descargado con datos de costo por km

---

## Flujo 10: Configuración (post-login)

### Paso 10.1: Parámetros del sistema
- Menú → "Configuración"
- ✅ Intervalo entre mantenimientos: 5000
- ✅ Umbral de alerta: 800
- Cambiar intervalo: `5500`
- Tocar "Guardar parámetros"
- ✅ Parámetros actualizados en config_system

### Paso 10.2: Cerrar sesión
- Tocar "Cerrar sesión"
- Confirmar
- ✅ Vuelve al login

---

## Flujo 11: Login como Mecánico

### Paso 11.1: Login
- Usuario: `juan.quispe`
- Contraseña: `Tecnico2026!`

### Paso 11.2: Verificar limitaciones
- Inventario: ver materiales OK
- Inventario: **no** ver botones "+ Nuevo material" ni "+ Ingresar lote"
- Alertas: **no** ver botón "Resolver"
- Dashboard BI: no accesible (solo Admin)

---

## Tabla de verificación rápida

| # | Funcionalidad | Estado esperado |
|---|---|---|
| 1a | Login Admin | ✅ |
| 1b | Login Mecánico | ✅ |
| 2 | Home KPIs (16 vehículos) | ✅ |
| 3 | Wizard (crear orden) | ✅ |
| 4 | Asignar técnico en wizard | ✅ |
| 5 | Detalle: checklist acciones | ✅ |
| 6 | Detalle: consumir material FIFO | ✅ |
| 7 | Detalle: ver info del lote FIFO | ✅ |
| 8 | Detalle: calificar material | ✅ |
| 9 | Detalle: instalar componente | ✅ |
| 10 | Detalle: guardar diagnóstico | ✅ |
| 11 | Detalle: cerrar orden | ✅ |
| 12 | Historial por vehículo | ✅ |
| 13 | Filtros en lista mantenimientos | ✅ |
| 14 | Calendario + filtros | ✅ |
| 15 | Inventario: materiales | ✅ |
| 16 | Inventario: lotes por material | ✅ |
| 17 | Inventario: descartar lote (Admin) | ✅ |
| 18 | Inventario: crear material (Admin) | ✅ |
| 19 | Inventario: ingresar lote (Admin) | ✅ |
| 20 | BI Dashboard KPIs | ✅ |
| 21 | BI Dashboard gráficos | ✅ |
| 22 | Alertas: verificar ahora | ✅ |
| 23 | Alertas: marcar leída | ✅ |
| 24 | Alertas: resolver (Admin) | ✅ |
| 25 | Reportes: PDF | ✅ |
| 26 | Reportes: Excel | ✅ |
| 27 | Config: parámetros sistema | ✅ |
| 28 | Config: cerrar sesión | ✅ |
| 29 | Login: cambiar URL | ✅ |
