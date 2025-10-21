#  Primitive Clash Backend

Backend API para un juego tipo **Clash Royale** desarrollado en **ASP.NET Core 9.0** con comunicación en tiempo real y arquitectura escalable.

##  **Arquitectura**

### **Clean Architecture + DDD (Domain-Driven Design)**
```
src/
├── PrimitiveClash.Domain/          # Entidades, Value Objects, Eventos
├── PrimitiveClash.Application/     # Casos de uso, Commands, Queries
├── PrimitiveClash.Infrastructure/  # Repositorios, Base de datos
└── PrimitiveClash.WebAPI/         # Controllers, SignalR Hubs
```

### **Patrones Implementados**

## Justificación

- Actor Pattern
	- Qué resuelve: Concurrencia por partida sin bloqueos complicados. Cada partida procesa acciones de forma serial (mailbox) y mantiene su estado aislado.
	- Dónde vive: `Application/Services` (GameSession/Engine) con acceso a `Domain`.
	- Beneficio en este juego: Evita condiciones de carrera cuando dos jugadores actúan casi al mismo tiempo; fácil de escalar con muchas partidas activas.

- CQRS (Command Query Responsibility Segregation)
	- Qué resuelve: Separar escritura (commands) de lectura (queries) permite optimizar cada camino y mantener la lógica de casos de uso clara y testeable.
	- Dónde vive: `Application/Commands`, `Application/Queries`, `Application/Handlers`.
	- Beneficio en este juego: Alta frecuencia de lecturas de estado vs. escrituras de acciones; facilita cachear/simplificar lecturas y auditar escrituras.

- Event Sourcing
	- Qué resuelve: Persistir eventos del juego para reconstruir el estado, permitir replays y auditorías.
	- Dónde vive: `Domain/Events` (contratos de evento) y almacenamiento en `Infrastructure` (si se implementa).
	- Beneficio en este juego: Replay de partidas, análisis de balance y anti-cheat.

- Repository Pattern
	- Qué resuelve: Desacoplar dominio de la persistencia; intercambiar EF/Dapper/mock sin tocar la lógica.
	- Dónde vive: Interfaces en `Domain/Interfaces`, implementaciones en `Infrastructure/Repositories`.
	- Beneficio en este juego: Cambiar motor de DB o añadir caché sin romper el dominio.

- Unit of Work
	- Qué resuelve: Agrupar operaciones de persistencia atómicas; transacciones coherentes.
	- Dónde vive: Interface en `Domain` o `Application`; implementación en `Infrastructure` (usualmente sobre DbContext).
	- Beneficio en este juego: Consistencia al aplicar múltiples cambios en un tick/acción.

- Strategy Pattern (Matchmaking)
	- Qué resuelve: Poder cambiar el algoritmo de emparejamiento (por trofeos, MMR, geografía) sin reescribir el servicio.
	- Dónde vive: `Application/Services` definiendo estrategias intercambiables.
	- Beneficio en este juego: Iterar en balance/criterios sin tocar consumidores.

- Observer Pattern (Eventos en tiempo real)
	- Qué resuelve: Notificar a interesados cuando el estado cambia (UI, analíticas, logs) sin acoplar capas.
	- Dónde vive: Emisión desde `Application`/`Domain` y publicación a clientes vía SignalR en `WebAPI`.
	- Beneficio en este juego: Broadcast de actualizaciones a los dos jugadores y espectadores en tiempo real.

##  Tiempo real: WebSockets con SignalR

- Transporte recomendado: SignalR (usa WebSockets cuando está disponible y fallback automático).
- Dónde está: `WebAPI/Hubs/GameHub.cs` y configuración en `WebAPI/Program.cs` con `app.MapHub("/gameHub")`.
- Por qué SignalR y no WebSocket crudo:
	- Manejo automático de reconexión, grupos (partidas), serialización y compatibilidad cross-browser.
	- Reduce código de infraestructura y te deja centrarte en la lógica del juego.

##  Mapeo carpeta → responsabilidad

- `PrimitiveClash.Domain`: Reglas de negocio puras (Entities, ValueObjects, Events, Interfaces, Services de dominio).
- `PrimitiveClash.Application`: Casos de uso (CQRS, Handlers, Services de aplicación, DTOs) y orquestación.
- `PrimitiveClash.Infrastructure`: Persistencia (DbContext, Repositories, Migrations) e integraciones externas.
- `PrimitiveClash.WebAPI`: Presentación (Controllers, Hubs, Contracts) y composición de dependencias.


