using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BattleService(IGameService gameService, IArenaService arenaService, IHubContext<GameHub> hubContext, ILogger<BattleService> logger) : IBattleService
    {
        private readonly IGameService _gameService = gameService;
        private readonly IArenaService _arenaService = arenaService;
        private readonly IHubContext<GameHub> _hubContext = hubContext;
        private readonly ILogger<BattleService> _logger = logger;

        public async Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            _logger.LogInformation("SpawnCard called: sessionId={SessionId}, userId={UserId}, cardId={CardId}, x={X}, y={Y}", sessionId, userId, cardId, x, y);

            Game game = await _gameService.GetGame(sessionId);
            PlayerState player = game.PlayerStates.FirstOrDefault(p => p.Id == userId)
                ?? throw new PlayerNotInGameException(userId);

            PlayerCard card = player.Cards.FirstOrDefault(c => c.Id == cardId)
                ?? throw new InvalidCardException(cardId);

            if (player.CurrentElixir < card.Card.ElixirCost)
            {
                _logger.LogWarning("Not enough elixir: required={Required}, available={Available}, player={PlayerId}", card.Card.ElixirCost, player.CurrentElixir, player.Id);
                throw new NotEnoughElixirException(card.Card.ElixirCost, player.CurrentElixir);
            }

            ArenaEntity entity = _arenaService.CreateEntity(game.GameArena, player, card, x, y);
            player.CurrentElixir -= card.Card.ElixirCost;

            _logger.LogInformation("Spawned entity of card {CardId} for player {PlayerId} at ({X},{Y})", cardId, player.Id, x, y);

            await _gameService.SaveGame(game);
            _logger.LogDebug("Game saved after spawning card for session {SessionId}", sessionId);

            await NotifyPlayers(sessionId, entity);
            _logger.LogDebug("NotifyPlayers called for session {SessionId} with entity type {Type}", sessionId, entity?.GetType().Name);
        }

        public void HandleAttack(Guid sessionId, Arena arena, Positioned attacker, Positioned target)
        {
            _logger.LogInformation(
                "[{SessionId}] HandleAttack called: attacker={AttackerId} ({AttackerType}), target={TargetId} ({TargetType})",
                sessionId,
                attacker?.Id,
                attacker?.GetType().Name,
                target?.Id,
                target?.GetType().Name
            );

            int damage = 0;

            if (attacker is ArenaEntity arenaEntity)
            {
                damage = arenaEntity.PlayerCard.Card.Damage;
                _logger.LogDebug(
                    "[{SessionId}] Damage from ArenaEntity {AttackerId} to target {TargetId}: {Damage}",
                    sessionId,
                    attacker.Id,
                    target.Id,
                    damage
                );
            }
            else if (attacker is Tower tower)
            {
                damage = tower.TowerTemplate.Damage;
                _logger.LogDebug(
                    "[{SessionId}] Damage from Tower {AttackerId} to target {TargetId}: {Damage}",
                    sessionId,
                    attacker.Id,
                    target.Id,
                    damage
                );
            }

            int oldHealth = target.Health;
            target.TakeDamage(damage);

            _logger.LogInformation(
                "[{SessionId}] Target {TargetId} health: {OldHealth} -> {NewHealth}",
                sessionId,
                target.Id,
                oldHealth,
                target.Health
            );

            if (!target.IsAlive())
            {
                if (target is ArenaEntity entityTarget)
                {
                    arena.KillArenaEntity(entityTarget);

                    _logger.LogWarning(
                        "[{SessionId}] ArenaEntity {TargetId} was killed by {AttackerId}",
                        sessionId,
                        target.Id,
                        attacker.Id
                    );
                }
                else if (target is Tower towerTarget)
                {
                    arena.KillTower(towerTarget);
                    _logger.LogWarning(
                        "[{SessionId}] Tower {TargetId} was destroyed by {AttackerId}",
                        sessionId,
                        target.Id,
                        attacker.Id
                    );
                }
            }
        }

        public async Task HandleMovement(Guid sessionId, TroopEntity troop, Arena arena)
        {
            _logger.LogDebug("HandleMovement called: session={SessionId}, troopId={TroopId}, currentPos=({PosX},{PosY}), pathCount={PathCount}", sessionId, troop?.Id, troop?.X, troop?.Y, troop?.Path.Count);

            if (troop.Path.Count == 0)
            {
                _logger.LogDebug("Troop {TroopId} has empty path, skipping movement", troop?.Id);
                return;
            }

            var prevX = troop.X;
            var prevY = troop.Y;

            _logger.LogTrace("Removing troop {TroopId} from arena at ({PrevX},{PrevY})", troop?.Id, prevX, prevY);
            _arenaService.RemoveEntity(arena, troop);

            Point point = troop.Path.Dequeue();
            troop.X = point.X;
            troop.Y = point.Y;
            troop.State = TroopState.Moving;

            _logger.LogTrace("Placing troop {TroopId} to new position ({X},{Y})", troop?.Id, point.X, point.Y);
            _arenaService.PlaceEntity(arena, troop);

            _logger.LogInformation("Troop {TroopId} moved from ({PrevX},{PrevY}) to ({X},{Y})", troop?.Id, prevX, prevY, point.X, point.Y);

            await NotifyPlayers(sessionId, troop);
        }

        public bool CanExecuteMovement(Arena arena, TroopEntity troop, int x, int y)
        {
            bool result = arena.IsInsideBounds(x, y) && arena.Grid[y][x].IsWalkable(troop);
            _logger.LogTrace("CanExecuteMovement check: troop={TroopId}, target=({X},{Y}), result={Result}", troop?.Id, x, y, result);
            return result;
        }

        private async Task NotifyPlayers(Guid sessionId, object obj)
        {
            _logger.LogDebug("NotifyPlayers sending GameSyncDelta to session {SessionId} with payload type {Type}", sessionId, obj?.GetType().Name);
            await _hubContext.Clients.Group(sessionId.ToString()).SendAsync("GameSyncDelta", obj);
        }
    }
}
