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
            _logger.LogDebug(
                "Executing troop action: session={SessionId}, troopId={TroopId}, position=({X},{Y}), state={State}",
                sessionId, troop.Id, troop.X, troop.Y, troop.State
            );

            if (TryContinueCurrentAttack(sessionId, arena, troop))
                return;

            if (TryAttackVisibleEnemies(sessionId, arena, troop))
                return;

            TryAttackOrMoveToTower(sessionId, arena, troop);
        }

        private bool TryContinueCurrentAttack(Guid sessionId, Arena arena, TroopEntity troop)
        {
            if (troop.State != TroopState.Attacking || troop.CurrentTargetId == null)
                return false;

            _logger.LogDebug(
                "Troop {TroopId} currently attacking target {TargetId}, CurrentTargetIsTower={IsTower}",
                troop.Id, troop.CurrentTargetId, troop.CurrentTargetIsTower
            );

            IEnumerable<Positioned> candidates = troop.CurrentTargetIsTower
                ? arena.GetAllTowers().Cast<Positioned>()
                : arena.GetAttackEntities().Cast<Positioned>();

            var currentTarget = candidates.FirstOrDefault(t => t.Id == troop.CurrentTargetId);

            if (currentTarget != null && currentTarget.IsAlive())
            {
                double distance = _arenaService.CalculateDistance(troop, currentTarget);
                double attackRange = (troop.PlayerCard.Card as TroopCard)!.Range;

                if (distance <= attackRange)
                {
                    _battleService.HandleAttack(sessionId, arena, troop, currentTarget);
                    _logger.LogInformation(
                        "Troop {TroopId} continues attacking target {TargetId} (distance={Distance}, range={Range})",
                        troop.Id, currentTarget.Id, distance, attackRange
                    );
                    return true;
                }
            }

            _logger.LogDebug(
                "Current target {TargetId} is dead, missing or out of range, switching to Idle",
                troop.CurrentTargetId
            );
            troop.State = TroopState.Idle;
            troop.CurrentTargetId = null;
            troop.CurrentTargetPosition = null;

            return false;
        }

        private bool TryAttackVisibleEnemies(Guid sessionId, Arena arena, TroopEntity troop)
        {
            var enemies = _arenaService.GetEnemiesInVision(arena, troop)
                .Where(e => troop.PlayerCard.Card.Targets.Contains((e.PlayerCard.Card as AttackCard)!.UnitClass))
                .ToList();

            _logger.LogDebug("Found {Count} enemies in vision for troop {TroopId}", enemies.Count, troop.Id);

            if (enemies.Count == 0)
                return false;

            var nearest = enemies.OrderBy(e => _arenaService.CalculateDistance(troop, e)).First();
            double distance = _arenaService.CalculateDistance(troop, nearest);
            double attackRange = (troop.PlayerCard.Card as TroopCard)!.Range;

            if (distance <= attackRange)
            {
                _logger.LogInformation(
                    "Troop {TroopId} attacking enemy {EnemyId} (distance={Distance}, range={Range})",
                    troop.Id, nearest.Id, distance, attackRange
                );

                troop.State = TroopState.Attacking;
                troop.CurrentTargetId = nearest.Id;
                troop.CurrentTargetPosition = new Point(nearest.X, nearest.Y);
                troop.CurrentTargetIsTower = false;

                _battleService.HandleAttack(sessionId, arena, troop, nearest);
            }
            else
            {
                MoveTowardsTarget(sessionId, arena, troop, nearest);
            }

            return true;
        }

        private void TryAttackOrMoveToTower(Guid sessionId, Arena arena, TroopEntity troop)
        {
            var tower = _arenaService.GetNearestEnemyTower(arena, troop);
            if (tower == null)
            {
                _logger.LogWarning(
                    "No enemy tower found for troop {TroopId} in session {SessionId}",
                    troop.Id, sessionId
                );
                return;
            }

            double towerDistance = _arenaService.CalculateDistance(troop, tower);
            double towerRange = (troop.PlayerCard.Card as TroopCard)!.Range;

            if (towerDistance <= towerRange)
            {
                _logger.LogInformation(
                    "Troop {TroopId} attacking tower {TowerId} (distance={Distance}, range={Range})",
                    troop.Id, tower.Id, towerDistance, towerRange
                );

                troop.State = TroopState.Attacking;
                troop.CurrentTargetId = tower.Id;
                troop.CurrentTargetPosition = new Point(tower.X, tower.Y);
                troop.CurrentTargetIsTower = true;

                _battleService.HandleAttack(sessionId, arena, troop, tower);
            }
            else
            {
                _logger.LogInformation(
                    "Troop {TroopId} moving to {TowerId} (distance={Distance}, range={Range})",
                    troop.Id, tower.Id, towerDistance, towerRange
                );
                MoveTowardsTarget(sessionId, arena, troop, tower);
            }
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
