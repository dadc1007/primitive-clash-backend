using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

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

            AttackEntity entity = _arenaService.CreateEntity(game.GameArena, player, card, x, y);
            player.CurrentElixir -= card.Card.ElixirCost;

            _logger.LogInformation("Spawned entity of card {CardId} for player {PlayerId} at ({X},{Y})", cardId, player.Id, x, y);

            await _gameService.SaveGame(game);
            _logger.LogDebug("Game saved after spawning card for session {SessionId}", sessionId);

            await NotifyPlayers(sessionId, entity);
            _logger.LogDebug("NotifyPlayers called for session {SessionId} with entity type {Type}", sessionId, entity?.GetType().Name);
        }

        public void HandleAttack(AttackEntity attacker, AttackEntity target)
        {
            _logger.LogInformation("HandleAttack called: attacker={AttackerId}, target={TargetId}", attacker?.Id, target?.Id);
            // Logica de atacar
            // TODO: agregar logs detallados durante el proceso de ataque (daño, estado, resultados)
        }

        public async Task HandleMovement(Guid sessionId, TroopEntity troop, Arena arena)
        {
            _logger.LogDebug("HandleMovement called: session={SessionId}, troopId={TroopId}, currentPos=({PosX},{PosY}), pathCount={PathCount}", sessionId, troop?.Id, troop?.PosX, troop?.PosY, troop?.Path.Count);

            if (troop.Path.Count == 0)
            {
                _logger.LogDebug("Troop {TroopId} has empty path, skipping movement", troop?.Id);
                return;
            }

            // Record previous position for trace
            var prevX = troop.PosX;
            var prevY = troop.PosY;

            _logger.LogTrace("Removing troop {TroopId} from arena at ({PrevX},{PrevY})", troop?.Id, prevX, prevY);
            _arenaService.RemoveEntity(arena, troop);

            Point point = troop.Path.Dequeue();
            troop.PosX = point.X;
            troop.PosY = point.Y;
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
