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

            _logger.LogDebug(
                "Iniciando ProcessTick. Sesiones activas: {Count}",
                sessionsToProcess.Count
            );

            List<Task> processingTasks = sessionsToProcess.Select(ProcessSessionTick).ToList();

            // Esperar a que todas las tareas de procesamiento terminen. Esto es lo que permite el procesamiento PARALELO.
            await Task.WhenAll(processingTasks);

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

                // 1. Obtener estado
                Game game = await gameService.GetGame(sessionId);
                Arena arena = game.GameArena;

                // 2. Logica
                List<ArenaEntity> entities = arenaService.GetEntities(arena);
                List<Tower> towers = arenaService.GetTowers(arena);

                _logger.LogDebug(
                    "Sesión {SessionId}: Encontradas {EntityCount} entidades para procesar.",
                    sessionId,
                    entities.Count + towers.Count
                );

                foreach (ArenaEntity entity in entities)
                {
                    behaviourService.ExecuteAction(sessionId, arena, entity);
                }

                foreach (Tower tower in towers)
                {
                    behaviourService.ExecuteAction(sessionId, arena, tower);
                }

                await gameService.UpdateElixir(game);

                // 3. Persistir estado
                await gameService.SaveGame(game);

                _logger.LogDebug("Sesión {SessionId}: Estado guardado en Redis.", sessionId);
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
