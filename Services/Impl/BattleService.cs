using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Entities;
using StackExchange.Redis;
using System.Text.Json;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class BattleService : IBattleService
    {
        private readonly IGameService _gameService;
        private readonly IDatabase _redis;

        public BattleService(IGameService gameService, IDatabase redis)
        {
            _gameService = gameService;
            _redis = redis;
        }

        public async Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            Game game = await _gameService.GetGame(sessionId);
            var player = game.PlayerStates.FirstOrDefault(p => p.UserId == userId)
                ?? throw new Exception("Player not found in game.");

            var card = player.Cards.FirstOrDefault(c => c.Id == cardId)
                ?? throw new InvalidCardException(cardId);

            game.GameArena.SpawnEntity(player, card, x, y);

            string key = $"game:{sessionId}";
            string updatedJson = JsonSerializer.Serialize(game);
            await _redis.StringSetAsync(key, updatedJson, TimeSpan.FromMinutes(15));
        }
    }
}
