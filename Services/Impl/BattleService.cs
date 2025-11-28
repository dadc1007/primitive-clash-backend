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
            PlayerState player = _gameService.GetPlayerState(game, userId);
            PlayerCard card =
                player.Cards.FirstOrDefault(c => c.Id == cardId)
                ?? throw new InvalidCardException(cardId);
            ArenaEntity entity;
            Cell cell = game.GameArena.Grid[y][x];
            PlayerCard cardToPut;

            lock (player.GetLock())
            {
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

                lock (cell)
                { 
                    entity = _arenaService.CreateEntity(game.GameArena, player, card, x, y);
                }
                
                player.CurrentElixir -= card.Card.ElixirCost;
                cardToPut = player.GetNextCard();
                player.PlayCard(cardId);
            }

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
                sessionId, player, entity, cardToPut);
        }

        public async Task HandleAttack(
            Guid sessionId,
            Arena arena,
            Positioned attacker,
            Positioned target
        )
        {
            Task? notifyDamage;
            Task? notifyKill = null;
            Task? endGame = null;

            lock (target.GetLock())
            {
                if (!target.IsAlive())
                    return;

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
                    attacker.UpdateState(PositionedState.Idle, null, null);
                    _arenaService.KillPositioned(arena, target);
                    _logger.LogWarning(
                        "[{SessionId}] {TargetType} {TargetId} was killed by {AttackerId}",
                        sessionId,
                        target.Id,
                        target.GetType().Name,
                        attacker.Id
                    );
                }

                notifyDamage = _notificationService.NotifyUnitDamaged(
                sessionId,
                UnitDamagedNotificationMapper.ToUnitDamagedNotification(attacker, target, damage)
            );

                if (died)
                {
                    notifyKill = _notificationService.NotifyUnitKilled(
                        sessionId,
                        new UnitKilledNotificacion(attacker.Id, target.Id)
                    );

                    if (target is Tower)
                        endGame = _gameService.EndGame(
                            sessionId,
                            arena,
                            attacker.UserId,
                            target.UserId
                        );
                }
            }

            List<Task> tasks = [notifyDamage];
            if (notifyKill != null)
                tasks.Add(notifyKill);
            if (endGame != null)
                tasks.Add(endGame);

            await Task.WhenAll(tasks);
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

            lock (troop.GetLock())
            {
                int prevX = troop.X;
                int prevY = troop.Y;
                Point next = troop.Path.Peek();
                Cell currentCell = arena.Grid[prevY][prevX];
                Cell nextCell = arena.Grid[next.Y][next.X];

                object firstLock = currentCell.GetHashCode() < nextCell.GetHashCode() ? currentCell : nextCell;
                object secondLock = currentCell.GetHashCode() < nextCell.GetHashCode() ? nextCell : currentCell;

                lock (firstLock)
                {
                    lock (secondLock)
                    {
                        arena.RemoveEntity(troop);

                        troop.Path.Dequeue();
                        troop.X = next.X;
                        troop.Y = next.Y;
                        troop.State = PositionedState.Moving;

                        arena.PlaceEntity(troop);
                    }
                }

                _logger.LogInformation(
                    "Troop {TroopId} moved from ({PrevX},{PrevY}) to ({X},{Y})",
                    troop?.Id,
                    prevX,
                    prevY,
                    troop.X,
                    troop.Y
                );
            }

            await _notificationService.NotifyTroopMoved(
                sessionId,
                new TroopMovedNotification(
                    troop.Id,
                    troop.UserId,
                    troop.PlayerCard.Card.Id,
                    troop.X,
                    troop.Y,
                    troop.State.ToString()
                )
            );
        }
    }
}
