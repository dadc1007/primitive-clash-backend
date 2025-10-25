using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BattleService(IGameService gameService, IArenaService arenaService, ILogger<BattleService> logger) : IBattleService
    {
        private readonly IGameService _gameService = gameService;
        private readonly IArenaService _arenaService = arenaService;
        private readonly ILogger<BattleService> _logger = logger;

        public async Task<(ArenaEntity Entity, Cell cell)> SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            Game game = await _gameService.GetGame(sessionId);
            PlayerState player = game.PlayerStates.FirstOrDefault(p => p.Id == userId)
                ?? throw new PlayerNotInGameException(userId);

            PlayerCard card = player.Cards.FirstOrDefault(c => c.Id == cardId)
                ?? throw new InvalidCardException(cardId);

            _logger.LogDebug("User {UserId} has {Elixir} Elixir. Cost: {Cost}.", userId, player.CurrentElixir, card.Card.ElixirCost);

            if (player.CurrentElixir < card.Card.ElixirCost)
            {
                throw new NotEnoughElixirException(card.Card.ElixirCost, player.CurrentElixir);
            }

            _logger.LogDebug("Calling ArenaService.PlaceEntity at ({X},{Y}).", x, y);

            ArenaEntity entity = _arenaService.PlaceEntity(game.GameArena, player, card, x, y);
            Cell affectedCell = game.GameArena.Grid[y][x];

            _logger.LogDebug("Entity placed. GroundEntity at ({X},{Y}) is null: {IsNull}", x, y, affectedCell.GroundEntity is null);

            player.CurrentElixir -= card.Card.ElixirCost;

            _logger.LogDebug("Arena: PlayerEntities keys: {Keys}", string.Join(", ", game.GameArena.PlayerEntities.Keys));

            if (game.GameArena.PlayerEntities.TryGetValue(entity.UserId, out var list))
            {
                _logger.LogDebug("PlayerEntities count for user {UserId}: {Count}", entity.UserId, list.Count);
            }

            await _gameService.SaveGame(game);

            _logger.LogDebug("Game state saved to Redis.");

            return (entity, affectedCell);
        }

        public void HandleAttack(AttackEntity attacker, AttackEntity target)
        {
            // Logica de atacar
        }

        public void HandleMovement(TroopEntity troop, int nextX, int nextY, Arena arena, List<ArenaEntity> changedEntities, List<Cell> changedCells)
        {
            Cell oldCell = arena.Grid[troop.PosY][troop.PosX];

            arena.RemoveTroop(troop);
            troop.MoveTo(nextX, nextY);
            arena.PlaceEntity(troop);

            Cell newCell = arena.Grid[nextY][nextX];

            changedEntities.Add(troop);
            changedCells.Add(oldCell);
            changedCells.Add(newCell);
        }

        public bool CanExecuteMovement(Arena arena, TroopEntity troop, int x, int y)
        {
            return arena.IsInsideBounds(x, y) && arena.Grid[y][x].IsWalkable(troop);
        }
    }
}
