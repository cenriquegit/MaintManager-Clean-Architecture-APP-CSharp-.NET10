# GUION DE EXPOSICIÓN — MaintManager
> 15 min teoría + 5 min demo  
> Sin diagramas de secuencia/actividades — los flujos se muestran en la app

---

## ESTRUCTURA (20 min)

| Tiempo | Sección | Contenido |
|--------|---------|-----------|
| 0-3 | Problema + Solución | Contexto, stack tecnológico |
| 3-7 | Arquitectura | Clean Architecture, flujo de datos |
| 7-10 | Funcionalidades core | Login, Dashboard BI, Wizard, Detalle |
| 10-13 | Casos de uso clave | Flujo completo de mantenimiento |
| 13-15 | Retos técnicos | .NET 10 AOT, LiveCharts, Shell routing |
| 15-20 | Demo en vivo | App funcionando en dispositivo |

---

## PARTE 1 — Problema + Solución (3 min)

**Problema:** Empresa con flota de 16 vehículos, mantenimiento en Excel, sin trazabilidad de componentes con vida útil, sin alertas predictivas, sin dashboard de costos.

**Solución:** App móvil MAUI + API .NET 10 + PostgreSQL. 3 roles (Admin, Técnico, Consultor). Clean Architecture.

**Stack:**
- Frontend: .NET MAUI 10 + CommunityToolkit.Mvvm + LiveChartsCore (SkiaSharp)
- Backend: .NET 10 + EF Core + PostgreSQL 16
- Reportes: QuestPDF + ClosedXML

---

## PARTE 2 — Arquitectura (4 min)

**Diagrama de capas:**
```
MAUI (Android) → ViewModels → Services (MVVM)
API .NET 10 → Controllers → Middleware (JWT)
Application → DTOs → Validators → Services
Infrastructure → EF Core → Repositories → DbContext
PostgreSQL 16 → 74 tablas, 14 vistas BI
```

**Flujo de datos:** Usuario toca en MAUI → ViewModel llama ApiService → API valida JWT → Service usa Repository → EF Core → PostgreSQL → Respuesta viaja de vuelta → UI se refresca.

**Seguridad:** JWT en SecureStorage, 8h expiración, roles Admin/Técnico, PIN 1234 en Settings.

---

## PARTE 3 — Funcionalidades Core (3 min)

**1. Login + Sesión:** JWT en SecureStorage, `TryRestoreSessionAsync` al iniciar, 8h expiración.

**2. Dashboard BI:** KPIs + 5 gráficos (Costo/Km, Tasa Emergencia, Costo Mensual, Lotes por Vencer, Desviación Km). Datos desde vistas `vw_*` en PostgreSQL. Dinámicos.

**3. Wizard Nueva Orden:** 4 pasos (Vehículo → Servicio → Técnico → Nota). Guarda con `PostAndUnwrapAsync<int>`.

**4. Detalle de Orden:** Info General → Acciones → Consumo Material → Componentes → Reasignar → Diagnóstico → Cerrar. Modo solo lectura en FI. Export PDF.

---

## PARTE 4 — Casos de Uso Clave (3 min)

**Flujo completo de mantenimiento (demostrar en vivo):**
```
Login → Dashboard → Nueva Orden (wizard 4 pasos) → DetailPage →
Agregar acciones → Consumir material (FIFO) → Calificar →
Instalar componente → Guardar Diagnóstico → Cerrar Orden → PDF
```

**Cómo explicar SIN diagramas:** "En lugar de mostrar un diagrama de secuencia, voy a mostrar el flujo en la app. Cada paso que el usuario da, el sistema responde. Por ejemplo, al guardar el diagnóstico se persisten primero las acciones pendientes, luego el diagnóstico, y se habilita el botón Cerrar."

**Dashboard BI:** Las vistas SQL ejecutan consultas agregadas en tiempo real. Cada apertura del dashboard = datos frescos.

**Alertas:** El sistema genera alertas automáticas al login. Técnico marca como leída, Admin resuelve. Histórico con Switch "Mostrar resueltas".

---

## PARTE 5 — Retos Técnicos (2 min)

| Reto | Problema | Solución |
|------|----------|----------|
| LiveCharts en .NET 10 AOT | CPURenderMode sin handler | `.UseSkiaSharp()` + versión dev-570 |
| Shell routing .NET 10 | Rutas relativas rotas | Prefijo `///` absoluto |
| AOT + JsonSerializer | Crash con `data.GetType()` | `JsonContent.Create` con tipos concretos |
| EF Core shadow FK | Columna MaterialMateid inexistente | `HasOne<Material>().WithMany()` explícito |

---

## PARTE 6 — Demo en Vivo (5 min)

**Secuencia:**
```
0:00 Login (herror.ortiz / herror.ortiz)
0:30 Dashboard: KPIs + 5 gráficos con datos reales
1:00 Alertas: lista, marcar leída, Switch "Mostrar resueltas"
1:30 Mantenimientos: lista con filtros
2:00 Click orden activa → DetailPage: Info General, acciones, consumir material, calificar, instalar componente
3:00 Guardar diagnóstico → Cerrar orden
3:30 Orden finalizada → solo lectura, Exportar PDF
4:30 Share dialog del PDF
5:00 Cierre
```

**Qué decir durante la demo:**
> "Muestro el login con credenciales Admin. El Dashboard carga KPIs en tiempo real: 16 vehículos, servicios del mes, tasa de emergencia. Los gráficos se alimentan de vistas PostgreSQL. Creo un mantenimiento: selecciono vehículo, tipo Calendarizado, servicio B, asigno técnico y guardo. La API responde 201 Created. En el detalle registro acciones, consumo material FIFO con costo por lote, califico con estrellas, instalo un componente. Guardo diagnóstico, cierro la orden, y la app cambia a solo lectura. Finalmente exporto el PDF con todos los datos."

---

## POSIBLES PREGUNTAS

**1. ¿Por qué MAUI y no React Native/Flutter?**
Integración nativa con .NET (mismo lenguaje backend/frontend), AOT para rendimiento Android, LiveChartsCore sin WebView.

**2. ¿Por qué PostgreSQL?**
La empresa ya lo usaba, vistas materializadas para BI, costo cero.

**3. ¿Sesión?**
JWT en SecureStorage, 8h expiración, restaura automática al abrir la app.

**4. ¿Dashboard en tiempo real?**
Sí, cada apertura ejecuta las vistas `vw_*`, datos frescos de BD, sin caché.

**5. ¿Requiere internet?**
Sí, online-first. Se podría agregar offline con SQLite local (futuro).

---

## Si preguntan por los diagramas de secuencia/actividades

> "Los diagramas de secuencia y actividades los usamos durante el diseño para modelar los flujos. Para la presentación preferí mostrar los flujos directamente en la aplicación funcionando, que comunica mejor la experiencia real del usuario. Si lo desean, puedo mostrar los diagramas que tenemos como documentación interna."

---

## SLIDES MÍNIMOS

1. Portada: MaintManager
2. Problema + Solución
3. Stack Tecnológico
4. Arquitectura (diagrama capas)
5. Funcionalidades (iconos)
6. Flujo Mantenimiento (capturas app)
7. Dashboard BI (captura gráficos)
8. Retos técnicos (tabla)
9. Demo (QR APK)
10. Conclusiones + Q&A
