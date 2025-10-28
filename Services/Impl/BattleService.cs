using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BattleService(
        IGameService gameService,
        IArenaService arenaService,
        INotificationService notificationService,
        ILogger<BattleService> logger
    ) : IBattleService
    {
        private readonly IGameService _gameService = gameService;
        private readonly IArenaService _arenaService = arenaService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly ILogger<BattleService> _logger = logger;

        public async Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            _logger.LogInformation(
                "SpawnCard called: sessionId={SessionId}, userId={UserId}, cardId={CardId}, x={X}, y={Y}",
                sessionId,
                userId,
                cardId,
                x,
                y
            );

            Game game = await _gameService.GetGame(sessionId);
            PlayerState player =
                game.PlayerStates.FirstOrDefault(p => p.Id == userId)
                ?? throw new PlayerNotInGameException(userId);
            PlayerCard card =
                player.Cards.FirstOrDefault(c => c.Id == cardId)
                ?? throw new InvalidCardException(cardId);

            if (player.CurrentElixir < card.Card.ElixirCost)
            {
                _logger.LogWarning(
                    "Not enough elixir: required={Required}, available={Available}, player={PlayerId}",
                    card.Card.ElixirCost,
                    player.CurrentElixir,
                    player.Id
                );
                throw new NotEnoughElixirException(card.Card.ElixirCost, player.CurrentElixir);
            }

            ArenaEntity entity = _arenaService.CreateEntity(game.GameArena, player, card, x, y);
            player.CurrentElixir -= card.Card.ElixirCost;

            _logger.LogInformation(
                "Spawned entity of card {CardId} for player {PlayerId} at ({X},{Y})",
                cardId,
                player.Id,
                x,
                y
            );

            await _gameService.SaveGame(game);

            _logger.LogDebug("Game saved after spawning card for session {SessionId}", sessionId);

            await _notificationService.NotifyCardSpawned(
                sessionId,
                new CardSpawnedNotification(
                    entity.Id,
                    entity.UserId,
                    entity.PlayerCard.Card.Id,
                    entity.PlayerCard.Level,
                    entity.X,
                    entity.Y
                )
            );
        }

        public async Task HandleAttack(
            Guid sessionId,
            Arena arena,
            Positioned attacker,
            Positioned target
        )
        {
            int damage = attacker switch
            {
                ArenaEntity e => e.PlayerCard.Card.Damage,
                Tower t => t.TowerTemplate.Damage,
                _ => 0,
            };

            int oldHealth = target.Health;
            target.TakeDamage(damage);

            _logger.LogInformation(
                "[{SessionId}] Target {TargetId} health: {OldHealth} -> {NewHealth}",
                sessionId,
                target.Id,
                oldHealth,
                target.Health
            );

            bool died = !target.IsAlive();

            if (died)
            {
                attacker.CurrentTargetId = null;
                attacker.CurrentTargetPosition = null;
                attacker.State = PositionedState.Idle;

                _arenaService.KillPositioned(arena, target);
                _logger.LogWarning(
                    "[{SessionId}] {TargetType} {TargetId} was killed by {AttackerId}",
                    sessionId,
                    target.Id,
                    target.GetType().Name,
                    attacker.Id
                );
            }

            await _notificationService.NotifyUnitDamaged(
                sessionId,
                new UnitDamagedNotification(attacker.Id, target.Id, damage, target.Health)
            );

            if (died)
            {
                await _notificationService.NotifyUnitKilled(
                    sessionId,
                    new UnitKilledNotificacion(attacker.Id, target.Id)
                );

                if (target is Tower)
                    await _gameService.EndGame(sessionId, arena, attacker.UserId, target.UserId);
            }
        }

        public async Task HandleMovement(Guid sessionId, TroopEntity troop, Arena arena)
        {
            _logger.LogDebug(
                "HandleMovement called: session={SessionId}, troopId={TroopId}, currentPos=({PosX},{PosY}), pathCount={PathCount}",
                sessionId,
                troop?.Id,
                troop?.X,
                troop?.Y,
                troop?.Path.Count
            );

            if (troop.Path.Count == 0)
            {
                _logger.LogDebug("Troop {TroopId} has empty path, skipping movement", troop?.Id);
                return;
            }

            int prevX = troop.X;
            int prevY = troop.Y;

            _arenaService.RemoveEntity(arena, troop);

            Point point = troop.Path.Dequeue();
            troop.X = point.X;
            troop.Y = point.Y;
            troop.State = PositionedState.Moving;

            _arenaService.PlaceEntity(arena, troop);

            _logger.LogInformation(
                "Troop {TroopId} moved from ({PrevX},{PrevY}) to ({X},{Y})",
                troop?.Id,
                prevX,
                prevY,
                point.X,
                point.Y
            );

            await _notificationService.NotifyTroopMoved(
                sessionId,
                new TroopMovedNotification(
                    troop.Id,
                    troop.UserId,
                    troop.X,
                    troop.Y,
                    troop.State.ToString()
                )
            );
        }
    }
}
