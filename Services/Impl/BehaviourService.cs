using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BehaviourService(IArenaService arenaService, IBattleService battleService, IPathfindingService pathfindingService, ILogger<BehaviourService> logger) : IBehaviorService
    {
        private readonly IArenaService _arenaService = arenaService;
        private readonly IBattleService _battleService = battleService;
        private readonly IPathfindingService _pathFindingService = pathfindingService;
        private readonly ILogger<BehaviourService> _logger = logger;

        public void ExecuteTroopAction(Arena arena, TroopEntity troop, List<ArenaEntity> changedEntities, List<Cell> changedCells)
        {
            if (troop.Card.Card is not TroopCard troopCard) return;

            // --- 1. COMPROBACIÓN RÁPIDA DE ATAQUE (Salida Temprana - Prioridad Alta) ---
            if (troop.CurrentTarget is not null && troop.CurrentTarget.IsAlive())
            {
                double attackDistance = _arenaService.CalculateDistance(troop, troop.CurrentTarget);

                if (attackDistance <= troopCard.Range)
                {
                    // La tropa está en rango y enfrascada. Solo ATACA.
                    _battleService.HandleAttack(troop, troop.CurrentTarget);
                    return; // ¡Salida Temprana! NO recalcula ruta ni busca objetivos.
                }
            }

            // --- 2. LÓGICA DE BÚSQUEDA Y RECALCULO DE RUTA ---

            // Buscar el objetivo de MAYOR prioridad: Tropa/Edificio (Visión) O Torre (Global).
            var (target, distance) = _arenaService.FindClosestTarget(arena, troop, troopCard.VisionRange);

            if (target is null)
            {
                _logger.LogWarning("Troop {TroopId} NO encontró objetivo y se detuvo.", troop.Id);
                troop.CurrentTarget = null;
                return; // No hay objetivos válidos.
            }

            _logger.LogDebug("Troop {TroopId} encontró Target {TargetId} (Tipo: {TargetType}) a Distancia: {Distance}.", troop.Id, target.Id, target.GetType().Name, distance);

            // Evaluar si es necesario recalcular la ruta:
            // 1. El objetivo de mayor prioridad es NUEVO (PathTarget es nulo o diferente).
            // 2. La ruta actual ha finalizado (Path.Count == 0).
            bool requiresRecalculation = target != troop.PathTarget || troop.Path.Count == 0;

            troop.CurrentTarget = target; // El objetivo actual es el de mayor prioridad.

            // --- 3. PATHFINDING (Cálculo costoso: Solo si requiresRecalculation es true) ---
            if (requiresRecalculation)
            {
                _logger.LogInformation("Troop {TroopId} RECALCULANDO ruta a ({TargetX},{TargetY}).", troop.Id, troop.CurrentTarget!.PosX, troop.CurrentTarget.PosY);

                // Limpiar y recalcular
                troop.Path.Clear();
                troop.PathTarget = troop.CurrentTarget;

                var path = _pathFindingService.FindPath(arena, troop, troop.PosX, troop.PosY, troop.CurrentTarget.PosX, troop.CurrentTarget.PosY);
                troop.Path = new Queue<(int X, int Y)>(path);

                _logger.LogInformation("Troop {TroopId} PATHFINDING completado. Nodos: {NodeCount}.", troop.Id, troop.Path.Count);
            }

            // --- 4. MOVIMIENTO (Seguimiento de Ruta Persistente) ---
            if (troop.Path.Count > 0)
            {
                (int nextX, int nextY) = troop.Path.Dequeue(); // Toma un único paso

                // Se añade CanExecuteMovement para verificar bloqueos antes de mover
                if (_battleService.CanExecuteMovement(arena, troop, nextX, nextY))
                {
                    _logger.LogDebug("Troop {TroopId} MOVIÉNDOSE de ({OldX},{OldY}) a ({NewX},{NewY}).", troop.Id, troop.PosX, troop.PosY, nextX, nextY);

                    _battleService.HandleMovement(troop, nextX, nextY, arena, changedEntities, changedCells);
                }
                else
                {
                    // Movimiento bloqueado: forzar recálculo en el siguiente tick.
                    _logger.LogWarning("Troop {TroopId} MOVIMIENTO BLOQUEADO a ({X},{Y}). Forzando recálculo.", troop.Id, nextX, nextY);

                    troop.Path.Clear();
                    troop.PathTarget = null;
                }
            }
        }
    }
}
