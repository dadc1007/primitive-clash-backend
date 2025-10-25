using System.Text.Json;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using StackExchange.Redis;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class GameService(IPlayerStateService playerStateService, ITowerService towerService, IArenaService arenaService, IDatabase redis, IGameLoopService gameLoopService) : IGameService
    {
        private const string GameKey = "game:";
        private const string PlayersInActiveGameSetKey = "game:active_players";
        private readonly IPlayerStateService _playerStateService = playerStateService;
        private readonly ITowerService _towerService = towerService;
        private readonly IArenaService _arenaService = arenaService;
        private readonly IDatabase _redis = redis;
        private readonly IGameLoopService _gameLoopService = gameLoopService;

        public async Task CreateNewGame(Guid sessionId, List<Guid> userIds)
        {
            if (userIds.Count != 2)
            {
                throw new InvalidPlayersNumberException();
            }

            List<PlayerState> playerStates = [];
            foreach (var userId in userIds)
            {
                PlayerState playerState = await _playerStateService.CreatePlayerState(userId);
                playerStates.Add(playerState);
            }

            Dictionary<Guid, List<Tower>> towers = await _towerService.CreateAllGameTowers(playerStates[0].Id, playerStates[1].Id);
            Arena arena = await _arenaService.CreateArena(towers);

            await SaveGame(new Game(sessionId, playerStates, arena));
            await Task.WhenAll(
                userIds.Select(u => _redis.SetAddAsync(PlayersInActiveGameSetKey, u.ToString()))
            );

            _gameLoopService.StartGameLoop(sessionId);
        }

        public async Task<Game> GetGame(Guid gameId)
        {
            string key = GetKey(gameId);

            RedisValue gameJson = await _redis.StringGetAsync(key);

            if (!gameJson.HasValue) throw new GameNotFoundException(gameId);

            Game? game = JsonSerializer.Deserialize<Game>(gameJson!)
                ?? throw new InvalidGameDataException(gameId);

            return game;
        }

        public async Task<Game> UpdatePlayerConnectionStatus(Guid sessionId, Guid userId, string? connectionId, bool isConnected)
        {
            string key = GetKey(sessionId);
            const int maxRetries = 3;
            const int baseDelayMs = 50;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                RedisValue gameJson = await _redis.StringGetAsync(key);

                if (!gameJson.HasValue)
                {
                    throw new GameNotFoundException(sessionId);
                }

                ITransaction transaction = _redis.CreateTransaction();
                transaction.AddCondition(Condition.StringEqual(key, gameJson));

                Game game = ApplyConnectionStatusChange(gameJson!, sessionId, userId, connectionId, isConnected);

                string updatedJson = JsonSerializer.Serialize(game);
                _ = transaction.StringSetAsync(key, updatedJson, TimeSpan.FromMinutes(15));

                bool committed = await transaction.ExecuteAsync();

                if (committed)
                {
                    return game;
                }

                if (attempt < maxRetries - 1)
                {
                    int delay = baseDelayMs * (int)Math.Pow(2, attempt);
                    await Task.Delay(delay);
                }
            }

            throw new ConcurrencyException(sessionId, maxRetries);
        }

        public async Task<bool> IsUserInGame(Guid userId)
        {
            return await _redis.SetContainsAsync(PlayersInActiveGameSetKey, userId.ToString());
        }

        public async Task SaveGame(Game game)
        {
            string gameJson = JsonSerializer.Serialize(game);
            string key = GetKey(game.Id);

            await _redis.StringSetAsync(key, gameJson, TimeSpan.FromMinutes(15));
        }

        private static string GetKey(Guid id)
        {
            return $"{GameKey}{id}";
        }

        private static Game ApplyConnectionStatusChange(RedisValue gameJson, Guid sessionId, Guid userId, string? connectionId, bool isConnected)
        {
            Game? game = JsonSerializer.Deserialize<Game>(gameJson!)
                ?? throw new InvalidGameDataException(sessionId);

            PlayerState? playerState = game.PlayerStates.FirstOrDefault(ps => ps.Id == userId);

            if (playerState == null) return game;

            playerState.IsConnected = isConnected;
            playerState.ConnectionId = connectionId;

            return game;
        }
    }
}