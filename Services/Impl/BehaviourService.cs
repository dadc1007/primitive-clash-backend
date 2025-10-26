using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BehaviourService(IArenaService arenaService, IBattleService battleService, IPathfindingService pathfindingService, ILogger<BehaviourService> logger) : IBehaviorService
    {
        private readonly IArenaService _arenaService = arenaService;
        private readonly IBattleService _battleService = battleService;
        private readonly IPathfindingService _pathFindingService = pathfindingService;
        private readonly ILogger<BehaviourService> _logger = logger;

        private string FormatPath(IEnumerable<Point> path)
        {
            if (path == null) return "null";
            return string.Join(" -> ", path.Select(p => $"({p.X},{p.Y})"));
        }

        public void ExecuteTroopAction(Guid sessionId, Arena arena, TroopEntity troop)
        {
            _logger.LogDebug("Executing troop action: session={SessionId}, troopId={TroopId}, position=({X},{Y}), state={State}",
                sessionId, troop.Id, troop.X, troop.Y, troop.State);

            // Si está atacando, no hace nada (espera a terminar su ataque)
            if (troop.State == TroopState.Attacking) return;

            var enemies = _arenaService.GetEnemiesInVision(arena, troop);
            _logger.LogDebug("Found {Count} enemies in vision for troop {TroopId}", enemies.Count(), troop.Id);

            // Filtrar enemigos que el troop puede atacar segun sus targets
            var validEnemies = enemies
                .Where(e => troop.PlayerCard.Card.Targets.Contains((e.PlayerCard.Card as AttackCard)!.UnitClass))
                .ToList();

            // 1️Verificar enemigos en visión (otras tropas)
            if (validEnemies.Count != 0)
            {
                var nearest = validEnemies.OrderBy(e => _arenaService.CalculateDistance(troop, e)).First();
                var distance = _arenaService.CalculateDistance(troop, nearest);
                double attackRange = (troop.PlayerCard.Card as TroopCard)!.Range;

                _logger.LogDebug("Nearest enemy for troop {TroopId}: enemyId={EnemyId}, distance={Distance}, enemyPos=({EnemyX},{EnemyY})",
                    troop.Id, nearest.Id, distance, nearest.X, nearest.Y);

                if (distance <= attackRange)
                {
                    _logger.LogInformation("Troop {TroopId} attacking enemy {EnemyId} (distance={Distance}, range={Range})",
                        troop.Id, nearest.Id, distance, attackRange);

                    troop.State = TroopState.Attacking;
                    _battleService.HandleAttack(troop, nearest);
                    return;
                }
                else
                {
                    MoveTowardsTarget(sessionId, arena, troop, nearest);
                    return;
                }
            }

            // Si no hay enemigos, buscar torre enemiga
            var tower = _arenaService.GetNearestEnemyTower(arena, troop);

            if (tower == null)
            {
                _logger.LogWarning("No enemy tower found for troop {TroopId} in session {SessionId}", troop.Id, sessionId);
                return;
            }

            double towerDistance = _arenaService.CalculateDistance(troop, tower);
            double towerRange = (troop.PlayerCard.Card as TroopCard)!.Range;
            _logger.LogInformation("Troop {TroopId} moving to {TowerId} (distance={Distance}, range={Range})",
                    troop.Id, tower.Id, towerDistance, towerRange);

            // 3️Si la torre está en rango, atacar
            if (towerDistance <= towerRange)
            {
                _logger.LogInformation("Troop {TroopId} attacking tower {TowerId} (distance={Distance}, range={Range})",
                    troop.Id, tower.Id, towerDistance, towerRange);

                troop.State = TroopState.Attacking;
                _battleService.HandleAttack(troop, tower);
                return;
            }

            // Si no está en rango, moverse hacia la torre
            MoveTowardsTarget(sessionId, arena, troop, tower);
        }

        private void MoveTowardsTarget(Guid sessionId, Arena arena, TroopEntity troop, Positioned target)
        {
            int targetX = target.X;
            int targetY = target.Y;

            if ((troop.TargetPosition.X, troop.TargetPosition.Y) != (targetX, targetY))
            {
                _logger.LogDebug("Troop {TroopId} updating path to target at ({TargetX},{TargetY})",
                    troop.Id, targetX, targetY);

                troop.TargetPosition = new Point(targetX, targetY);

                var path = _pathFindingService.FindPath(arena, troop, target);
                troop.Path = new Queue<Point>(path);

                _logger.LogDebug("New path generated for troop {TroopId}: waypoints={WaypointCount}, path=[{Path}]",
                    troop.Id, path.Count(), FormatPath(path));
            }

            _battleService.HandleMovement(sessionId, troop, arena);
        }
    }
}
