using System.Collections.Concurrent;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class GameLoopService(IServiceScopeFactory scopeFactory, ILogger<GameLoopService> logger)
        : IGameLoopService
    {
        // Almacena las sesiones activas que deben recibir ticks. Thread-safe.
        private readonly ConcurrentDictionary<Guid, bool> _activeSessions = new();
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<GameLoopService> _logger = logger;

        public void StartGameLoop(Guid sessionId)
        {
            _activeSessions.TryAdd(sessionId, true);
            _logger.LogInformation("GameLoop iniciado para Sesión: {SessionId}", sessionId);
        }

        public void StopGameLoop(Guid sessionId)
        {
            _activeSessions.TryRemove(sessionId, out _);
        }

        public async Task ProcessTick()
        {
            // Crear una lista de tareas (Task) para procesar cada sesión en paralelo.
            List<Guid> sessionsToProcess = _activeSessions.Keys.ToList();

            if (sessionsToProcess.Count == 0)
                return;

            _logger.LogDebug(
                "Iniciando ProcessTick. Sesiones activas: {Count}",
                sessionsToProcess.Count
            );

            // Esperar a que todas las tareas de procesamiento terminen. Esto es lo que permite el procesamiento PARALELO.
            await Task.WhenAll(sessionsToProcess.Select(ProcessSessionTick));

            _logger.LogDebug("ProcessTick completado.");
        }

        private async Task ProcessSessionTick(Guid sessionId)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            IGameService gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
            IBehaviorService behaviourService =
                scope.ServiceProvider.GetRequiredService<IBehaviorService>();
            IArenaService arenaService = scope.ServiceProvider.GetRequiredService<IArenaService>();

            // Chequeo de seguridad: la sesión pudo ser eliminada justo ahora.
            if (!_activeSessions.ContainsKey(sessionId))
                return;

            try
            {
                _logger.LogDebug("Iniciando tick para Sesión: {SessionId}", sessionId);

                // Obtener estado
                Game game = await gameService.GetGame(sessionId);
                Arena arena = game.GameArena;

                // Logica
                List<ArenaEntity> entities = arenaService.GetEntities(arena);
                List<Tower> towers = arenaService.GetTowers(arena);

                _logger.LogDebug(
                    "Sesión {SessionId}: Encontradas {EntityCount} entidades para procesar.",
                    sessionId,
                    entities.Count + towers.Count
                );

                // Procesar entidades EN PARALELO
                List<Task> entityTasks = entities
                    .Select(entity =>
                        Task.Run(() =>
                        {
                            try
                            {
                                behaviourService.ExecuteAction(sessionId, arena, entity);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(
                                    ex,
                                    "Error procesando entidad {EntityId} en sesión {SessionId}",
                                    entity.Id,
                                    sessionId
                                );
                            }
                        })
                    )
                    .ToList();

                // Procesar torres EN PARALELO también
                List<Task> towerTasks = towers
                    .Select(tower =>
                        Task.Run(() =>
                        {
                            try
                            {
                                behaviourService.ExecuteAction(sessionId, arena, tower);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(
                                    ex,
                                    "Error procesando torre {TowerId} en sesión {SessionId}",
                                    tower.Id,
                                    sessionId
                                );
                            }
                        })
                    )
                    .ToList();

                // Esperar todas las tareas de esta sesión
                await Task.WhenAll(entityTasks.Concat(towerTasks));

                // Actualizar elixir y guardar
                await gameService.UpdateElixir(game);
                await gameService.SaveGame(game);

                _logger.LogDebug("Sesión {SessionId}: Tick completado y guardado.", sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "FATAL ERROR: El procesamiento de la Sesión {SessionId} ha fallado y se detendrá el GameLoop.",
                    sessionId
                );
                StopGameLoop(sessionId);
            }
        }
    }
}
