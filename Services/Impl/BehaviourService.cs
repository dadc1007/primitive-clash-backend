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

        public void ExecuteAction(Guid sessionId, Arena arena, Positioned unit)
        {
            _logger.LogDebug(
                "Executing action: session={SessionId}, unitId={UnitId}, position=({X},{Y}), state={State}",
                sessionId, unit.Id, unit.X, unit.Y, unit is TroopEntity t ? t.State.ToString() : "Tower"
            );

            if (!unit.IsAlive())
            {
                _logger.LogDebug("UnitId={UnitId} was destruyed", unit.Id);

                return;
            }

            if (TryContinueCurrentAttack(sessionId, arena, unit))
                return;

            switch (unit)
            {
                case TroopEntity troop:
                    if (TryAttackVisibleEnemies(sessionId, arena, troop)) return;
                    TryAttackOrMoveToTower(sessionId, arena, troop);
                    break;

                case Tower tower:
                    TryAttackNearestEnemyInRange(sessionId, arena, tower);
                    break;
            }
        }

        private bool TryContinueCurrentAttack(Guid sessionId, Arena arena, Positioned attacker)
        {
            if (attacker.CurrentTargetId == null) return false;

            Positioned? currentTarget = GetCurrentTarget(attacker.CurrentTargetId.Value, attacker.CurrentTargetIsTower, arena);
            if (currentTarget == null || !currentTarget.IsAlive())
            {
                _logger.LogDebug("Current target {TargetId} is dead or missing, switching to Idle", attacker.CurrentTargetId);

                attacker.State = PositionedState.Idle;
                attacker.CurrentTargetId = null;
                attacker.CurrentTargetPosition = null;
                return false;
            }

            double range = attacker switch
            {
                TroopEntity t => (t.PlayerCard.Card as TroopCard)!.Range,
                Tower tow => tow.TowerTemplate.Range,
                _ => 0
            };

            return TryAttackTargetIfInRange(sessionId, attacker, currentTarget, range, arena);
        }

        private bool TryAttackTargetIfInRange(Guid sessionId, Positioned attacker, Positioned target, double range, Arena arena)
        {
            double distance = _arenaService.CalculateDistance(attacker, target);
            if (distance > range) return false;

            attacker.State = PositionedState.Attacking;
            _battleService.HandleAttack(sessionId, arena, attacker, target);
            _logger.LogInformation(
                "{AttackerType} {AttackerId} attacked target {TargetId} (distance={Distance}, range={Range})",
                attacker.GetType().Name, attacker.Id, target.Id, distance, range
            );

            return true;
        }

        private Positioned? GetCurrentTarget(Guid targetId, bool isTower, Arena arena)
        {
            IEnumerable<Positioned> candidates = isTower
                ? _arenaService.GetTowers(arena)
                : _arenaService.GetEntities(arena);

            return candidates.FirstOrDefault(t => t.Id == targetId);
        }

        private bool TryAttackVisibleEnemies(Guid sessionId, Arena arena, TroopEntity troop)
        {
            List<ArenaEntity> enemies = _arenaService.GetEnemiesInVision(arena, troop)
             .Where(e => troop.PlayerCard.Card.Targets.Contains((e.PlayerCard.Card as AttackCard)!.UnitClass))
             .ToList();

            _logger.LogDebug("Found {Count} enemies in vision for troop {TroopId}", enemies.Count, troop.Id);

            if (enemies.Count == 0) return false;

            ArenaEntity nearest = enemies.OrderBy(e => _arenaService.CalculateDistance(troop, e)).First();
            double range = (troop.PlayerCard.Card as TroopCard)!.Range;

            if (!TryAttackTargetIfInRange(sessionId, troop, nearest, range, arena))
                MoveTowardsTarget(sessionId, arena, troop, nearest);

            return true;
        }

        private void TryAttackOrMoveToTower(Guid sessionId, Arena arena, TroopEntity troop)
        {
            Tower? tower = _arenaService.GetNearestEnemyTower(arena, troop);
            if (tower == null)
            {
                _logger.LogWarning("No enemy tower found for troop {TroopId} in session {SessionId}", troop.Id, sessionId);
                return;
            }

            double range = (troop.PlayerCard.Card as TroopCard)!.Range;
            
            if (TryAttackTargetIfInRange(sessionId, troop, tower, range, arena)) return;
            
            _logger.LogInformation("Troop {TroopId} moving to {TowerId} (distance={Distance}, range={Range})",
                troop.Id, tower.Id, _arenaService.CalculateDistance(troop, tower), range);

            MoveTowardsTarget(sessionId, arena, troop, tower);
        }

        private void TryAttackNearestEnemyInRange(Guid sessionId, Arena arena, Tower tower)
        {
            IEnumerable<ArenaEntity> enemies = _arenaService.GetEnemiesInVision(arena, tower).ToList();
            if (!enemies.Any()) return;

            ArenaEntity nearest = enemies.OrderBy(e => _arenaService.CalculateDistance(tower, e)).First();
            double range = tower.TowerTemplate.Range;

            TryAttackTargetIfInRange(sessionId, tower, nearest, range, arena);
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
                List<Point> path = _pathFindingService.FindPath(arena, troop, target);
                troop.Path = new Queue<Point>(path);

                _logger.LogDebug("New path generated for troop {TroopId}: waypoints={WaypointCount}, path=[{Path}]",
                    troop.Id, path.Count, FormatPath(path));
            }
            
            if (troop.Path.Count > 0)
            {
                Point next = troop.Path.Peek();
                
                if (!_arenaService.CanExecuteMovement(arena, troop, next.X, next.Y))
                {
                    _logger.LogDebug("Troop {TroopId} found next cell blocked, recalculating path...", troop.Id);

                    List<Point> newPath = _pathFindingService.FindPath(arena, troop, target);
                    troop.Path = new Queue<Point>(newPath);

                    _logger.LogDebug("Recalculated path for troop {TroopId}: waypoints={WaypointCount}, path=[{Path}]",
                        troop.Id, newPath.Count, FormatPath(newPath));
                }
            }

            troop.State = PositionedState.Moving;
            _battleService.HandleMovement(sessionId, troop, arena);
        }

        private static string FormatPath(IEnumerable<Point> path) => string.Join("->", path.Select(p => $"({p.X},{p.Y})"));

    }
}
