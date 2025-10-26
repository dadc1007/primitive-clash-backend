using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class GameLoopService(IServiceScopeFactory scopeFactory, IHubContext<GameHub> hubContext, ILogger<GameLoopService> logger) : IGameLoopService
    {
        // Almacena las sesiones activas que deben recibir ticks. Thread-safe.
        private readonly ConcurrentDictionary<Guid, bool> _activeSessions = new();
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IHubContext<GameHub> _hubContext = hubContext;
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
            var processingTasks = new List<Task>();

            // Convertir el IEnumerable de SessionIds a una lista para iteración segura
            var sessionsToProcess = _activeSessions.Keys.ToList();

            _logger.LogDebug("Iniciando ProcessTick. Sesiones activas: {Count}", sessionsToProcess.Count);

            foreach (var sessionId in sessionsToProcess)
            {
                // Añadir el procesamiento de esta sesión como una tarea a la lista.
                // NO usamos 'await' aquí, simplemente creamos la tarea.
                processingTasks.Add(ProcessSessionTick(sessionId));
            }

            // Esperar a que TODAS las tareas de procesamiento terminen.
            // Esto es lo que permite el procesamiento PARALELO.
            await Task.WhenAll(processingTasks);

            _logger.LogDebug("ProcessTick completado.");
        }

        // Contiene toda la lógica de un solo tick para una sesión.
        private async Task ProcessSessionTick(Guid sessionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var _gameService = scope.ServiceProvider.GetRequiredService<IGameService>();
            var _behaviourService = scope.ServiceProvider.GetRequiredService<IBehaviorService>();

            if (!_activeSessions.ContainsKey(sessionId))
            {
                return; // Chequeo de seguridad: la sesión pudo ser eliminada justo ahora.
            }

            try
            {
                _logger.LogDebug("Iniciando tick para Sesión: {SessionId}", sessionId);

                // 1. OBTENER ESTADO
                // Las operaciones I/O (como GetGame) se ejecutarán en paralelo gracias a Task.WhenAll
                Game game = await _gameService.GetGame(sessionId);
                Arena arena = game.GameArena;

                // 2. LÓGICA DE JUEGO

                // a) Regenerar Elixir

                // b) Ejecutar Comportamiento de Tropas y Edificios
                var entities = arena.GetAttackEntities().ToList();

                _logger.LogDebug("Sesión {SessionId}: Encontradas {EntityCount} entidades para procesar.", sessionId, entities.Count);

                foreach (var entity in entities)
                {
                    if (entity is TroopEntity troop)
                    {
                        _behaviourService.ExecuteTroopAction(sessionId, arena, troop);
                    }
                }

                // 3. PERSISTIR ESTADO
                await _gameService.SaveGame(game);

                _logger.LogDebug("Sesión {SessionId}: Estado guardado en Redis.", sessionId);
            }
            catch (Exception ex)
            {
                // Manejar errores de una sesión específica sin detener las otras.
                _logger.LogError(ex, "FATAL ERROR: El procesamiento de la Sesión {SessionId} ha fallado y se detendrá el GameLoop.", sessionId);
                StopGameLoop(sessionId);
            }
        }

    }
}