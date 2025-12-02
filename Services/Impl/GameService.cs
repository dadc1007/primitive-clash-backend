using System.Text.Json;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using StackExchange.Redis;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class GameService(
        IPlayerStateService playerStateService,
        ITowerService towerService,
        IArenaService arenaService,
        IGameLoopService gameLoopService,
        INotificationService notificationService,
        IDatabase redis
    ) : IGameService
    {
        private const string GameKey = "game:";
        private const string PlayersInActiveGameSetKey = "game:active_players";
        private readonly IPlayerStateService _playerStateService = playerStateService;
        private readonly ITowerService _towerService = towerService;
        private readonly IArenaService _arenaService = arenaService;
        private readonly IGameLoopService _gameLoopService = gameLoopService;
        private readonly INotificationService _notificationService = notificationService;
        private readonly IDatabase _redis = redis;

        public async Task CreateNewGame(Guid sessionId, List<Guid> userIds)
        {
            if (userIds.Count != 2)
            {
                throw new InvalidPlayersNumberException();
            }

            List<PlayerState> playerStates = [];
            foreach (Guid userId in userIds)
            {
                PlayerState playerState = await _playerStateService.CreatePlayerState(userId);
                playerStates.Add(playerState);
            }

            playerStates[0].ArenaPosition = Models.Enums.ArenaPosition.Top;
            playerStates[1].ArenaPosition = Models.Enums.ArenaPosition.Bottom;

            Dictionary<Guid, List<Tower>> towers = await _towerService.CreateAllGameTowers(
                playerStates[0].Id,
                playerStates[1].Id
            );
            Arena arena = await _arenaService.CreateArena(towers);

            await SaveGame(new Game(sessionId, playerStates, arena));
            await Task.WhenAll(
                userIds.Select(u => _redis.SetAddAsync(PlayersInActiveGameSetKey, u.ToString()))
            );
        }

        public async Task EndGame(Guid sessionId, Arena arena, Guid winnerId, Guid losserId)
        {
            string key = GetKey(sessionId);
            Game game = await GetGame(sessionId);

            await _redis.KeyDeleteAsync(key);

            foreach (PlayerState player in game.PlayerStates)
            {
                await _redis.SetRemoveAsync(PlayersInActiveGameSetKey, player.Id.ToString());
            }

            _gameLoopService.StopGameLoop(sessionId);

            (int towersWinner, int towersLosser) = _arenaService.GetNumberTowers(
                arena,
                winnerId,
                losserId
            );

            await _notificationService.NotifyEndGame(
                sessionId,
                new EndGameNotification(winnerId, losserId, towersWinner, towersLosser)
            );
        }

        public async Task SaveGame(Game game)
        {
            foreach (TroopEntity troop in game.GameArena.GetAllTroops())
            {
                troop.SyncStepsFromPath();
            }

            string gameJson = JsonSerializer.Serialize(game);
            string key = GetKey(game.Id);

            await _redis.StringSetAsync(key, gameJson, TimeSpan.FromMinutes(15));
        }

        public async Task<Game> GetGame(Guid gameId)
        {
            string key = GetKey(gameId);

            RedisValue gameJson = await _redis.StringGetAsync(key);

            if (!gameJson.HasValue)
                throw new GameNotFoundException(gameId);

            Game? game =
                JsonSerializer.Deserialize<Game>(gameJson!)
                ?? throw new InvalidGameDataException(gameId);

            foreach (TroopEntity troop in game.GameArena.GetAllTroops())
            {
                troop.SyncPathFromSteps();
            }

            return game;
        }

        public async Task<Game> UpdatePlayerConnectionStatus(
            Guid sessionId,
            Guid userId,
            string? connectionId,
            bool isConnected
        )
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

                Game game = ApplyConnectionStatusChange(
                    gameJson!,
                    sessionId,
                    userId,
                    connectionId,
                    isConnected
                );

                string updatedJson = JsonSerializer.Serialize(game);
                _ = transaction.StringSetAsync(key, updatedJson, TimeSpan.FromMinutes(15));

                bool committed = await transaction.ExecuteAsync();

                if (committed)
                {
                    return game;
                }

                if (attempt >= maxRetries - 1)
                    continue;

                int delay = baseDelayMs * (int)Math.Pow(2, attempt);
                await Task.Delay(delay);
            }

            throw new ConcurrencyException(sessionId, maxRetries);
        }

        public async Task<bool> IsUserInGame(Guid userId)
        {
            return await _redis.SetContainsAsync(PlayersInActiveGameSetKey, userId.ToString());
        }

        public async Task UpdateElixir(Game game)
        {
            foreach (
                PlayerState player in game.PlayerStates.Where(p => p.CurrentElixir < Game.MaxElixir)
            )
            {
                decimal before = player.CurrentElixir;
                player.CurrentElixir = Math.Min(before + Game.ElixirPerSecond, Game.MaxElixir);

                if (player.CurrentElixir != before && player.IsConnected && player.ConnectionId != null)
                {
                    await _notificationService.NotifyNewElixir(
                        player.ConnectionId,
                        player.CurrentElixir
                    );
                }
            }
        }

        public PlayerState GetPlayerState(Game game, Guid userId)
        {
            return game.PlayerStates.FirstOrDefault(ps => ps.Id == userId)
                ?? throw new PlayerNotInGameException(userId);
        }

        private static string GetKey(Guid id)
        {
            return $"{GameKey}{id}";
        }

        private static Game ApplyConnectionStatusChange(
            RedisValue gameJson,
            Guid sessionId,
            Guid userId,
            string? connectionId,
            bool isConnected
        )
        {
            Game? game =
                JsonSerializer.Deserialize<Game>(gameJson!)
                ?? throw new InvalidGameDataException(sessionId);

            PlayerState? playerState = game.PlayerStates.FirstOrDefault(ps => ps.Id == userId);

            if (playerState == null)
                return game;

            playerState.IsConnected = isConnected;
            playerState.ConnectionId = connectionId;

            return game;
        }
    }
}
