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
                sessionId, troop.Id, troop.PosX, troop.PosY, troop.State);

            var enemies = _arenaService.GetEnemiesInVision(arena, troop);
            _logger.LogDebug("Found {Count} enemies in vision for troop {TroopId}", enemies.Count(), troop.Id);

            // 1️⃣ Verificar enemigos en visión (otras tropas)
            if (enemies.Any())
            {
                var nearest = enemies.OrderBy(e => _arenaService.CalculateDistance(troop, e)).First();
                var distance = _arenaService.CalculateDistance(troop, nearest);
                double attackRange = (troop.Card.Card as TroopCard)!.Range;

                _logger.LogDebug("Nearest enemy for troop {TroopId}: enemyId={EnemyId}, distance={Distance}, enemyPos=({EnemyX},{EnemyY})",
                    troop.Id, nearest.Id, distance, nearest.PosX, nearest.PosY);

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
                    MoveTowardsTarget(sessionId, troop, nearest.PosX, nearest.PosY, arena);
                    return;
                }
            }

            // 2️⃣ Si no hay enemigos, buscar torre enemiga
            var tower = _arenaService.GetNearestEnemyTower(arena, troop);

            if (tower == null)
            {
                _logger.LogWarning("No enemy tower found for troop {TroopId} in session {SessionId}", troop.Id, sessionId);
                return;
            }

            double towerDistance = _arenaService.CalculateDistance(troop, tower);
            double towerRange = (troop.Card.Card as TroopCard)!.Range;

            // 3️⃣ Si la torre está en rango, atacar
            if (towerDistance <= towerRange)
            {
                _logger.LogInformation("Troop {TroopId} attacking tower {TowerId} (distance={Distance}, range={Range})",
                    troop.Id, tower.Id, towerDistance, towerRange);

                troop.State = TroopState.Attacking;
                _battleService.HandleAttack(troop, tower);
                return;
            }

            // 4️⃣ Si no está en rango, moverse hacia la torre
            MoveTowardsTarget(sessionId, troop, tower.PosX, tower.PosY, arena);
        }

        private void MoveTowardsTarget(Guid sessionId, TroopEntity troop, int targetX, int targetY, Arena arena)
        {
            if ((troop.TargetPosition.X, troop.TargetPosition.Y) != (targetX, targetY))
            {
                _logger.LogDebug("Troop {TroopId} updating path to target at ({TargetX},{TargetY})",
                    troop.Id, targetX, targetY);

                troop.TargetPosition = new Point()
                {
                    X = targetX,
                    Y = targetY
                };

                var path = _pathFindingService.FindPath(arena, troop, troop.PosX, troop.PosY, targetX, targetY);
                troop.Path = new Queue<Point>(path);

                _logger.LogDebug("New path generated for troop {TroopId}: waypoints={WaypointCount}, path=[{Path}]",
                    troop.Id, path.Count(), FormatPath(path));
            }

            _battleService.HandleMovement(sessionId, troop, arena);
        }
    }
}
