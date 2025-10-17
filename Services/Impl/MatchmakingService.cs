using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.DTOs.Matchmaking;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Hubs;
using StackExchange.Redis;

namespace PrimitiveClash.Backend.Services.Impl
{
    public class MatchmakingService(IHubContext<MatchmakingHub> hubContext, ILogger<MatchmakingService> logger, IDatabase redis, IServiceScopeFactory scopeFactory) : IMatchmakingService, IHostedService
    {
        private const string MatchmakingQueueKey = "matchmaking:queue";
        private const string PlayersInQueueSetKey = "matchmaking:active_players";
        private readonly IHubContext<MatchmakingHub> _hubContext = hubContext;
        private readonly ILogger<MatchmakingService> _logger = logger;
        private readonly IDatabase _redis = redis;
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Matchmaking Service starting background loop.");
            Task.Run(() => ProcessMatchesLoop(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Matchmaking Service stopping.");
            return Task.CompletedTask;
        }

        public async Task EnqueuePlayer(Guid userId, string connectionId)
        {
            using var scope = _scopeFactory.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            if (await gameService.IsUserInGame(userId))
            {
                _logger.LogWarning("Player {UserId} attempted to enqueue while in an active game.", userId);
                await _hubContext.Clients.Client(connectionId).SendAsync("Error", "You are still considered in an active game session. Cannot search for a new match.");
                return;
            }

            if (await _redis.SetContainsAsync(PlayersInQueueSetKey, userId.ToString()))
            {
                _logger.LogWarning("Attempted to enqueue player {UserId} who is already in queue.", userId);
                throw new PlayerAlreadyInQueueException(userId);
            }

            await _redis.SetAddAsync(PlayersInQueueSetKey, userId.ToString());

            PlayerQueueItem playerItem = new(userId, connectionId);
            string playerJson = JsonSerializer.Serialize(playerItem);

            await _redis.ListRightPushAsync(MatchmakingQueueKey, playerJson);

            _logger.LogInformation("Player {UserId} added to the queue.", userId);
            await _hubContext.Clients.Client(connectionId).SendAsync("UpdateStatus", "Searching for a opponent...");
        }

        public void DequeuePlayer(Guid userId)
        {
            _redis.SetRemoveAsync(PlayersInQueueSetKey, userId.ToString());
            _logger.LogInformation("Player {UserId} removed from the active set (disconnected).", userId);
        }

        private async Task ProcessMatchesLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TimeSpan delay = TimeSpan.FromSeconds(2);

                try
                {
                    long queueSize = await _redis.ListLengthAsync(MatchmakingQueueKey);

                    if (queueSize >= 2)
                    {
                        await TryMatchPlayers();
                        delay = TimeSpan.FromMilliseconds(500);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CRITICAL: Redis connection error in matchmaking loop. Applying long delay before retry.");
                    delay = TimeSpan.FromSeconds(10);
                }

                await Task.Delay(delay, cancellationToken);
            }
        }

        private async Task TryMatchPlayers()
        {
            var (player1, player1Json) = await PopAndDeserializeAsync();
            if (player1 is null) return;

            var (player2, player2Json) = await PopAndDeserializeAsync();
            if (player2 is null)
            {
                await _redis.ListLeftPushAsync(MatchmakingQueueKey, player1Json);
                _logger.LogInformation("Second player not available. Re-queued player {UserId}.", player1.UserId);
                return;
            }

            _logger.LogInformation("Attempting to match players {Player1Id} and {Player2Id}.", player1.UserId, player2.UserId);
            var activeStatus = await CheckPlayerActivityAsync(player1, player2);

            if (!activeStatus.player1Active || !activeStatus.player2Active)
            {
                await HandleStalePlayersAsync(player1, player1Json, player2, player2Json, activeStatus);
                return;
            }

            await FinalizeMatchAsync(player1, player2);
        }

        private async Task<(PlayerQueueItem? player, RedisValue json)> PopAndDeserializeAsync()
        {
            RedisValue json = await _redis.ListLeftPopAsync(MatchmakingQueueKey);

            if (!json.HasValue) return (null, RedisValue.Null);

            PlayerQueueItem? player = JsonSerializer.Deserialize<PlayerQueueItem>(json.ToString());

            if (player is null)
            {
                _logger.LogWarning("Corrupt or invalid data found in the matchmaking queue. Json discarded: {Json}", json.ToString());
                return (null, RedisValue.Null);
            }

            return (player, json);
        }

        private async Task<(bool player1Active, bool player2Active)> CheckPlayerActivityAsync(
            PlayerQueueItem player1, PlayerQueueItem player2)
        {
            bool p1Active = await _redis.SetContainsAsync(PlayersInQueueSetKey, player1.UserId.ToString());
            bool p2Active = await _redis.SetContainsAsync(PlayersInQueueSetKey, player2.UserId.ToString());
            return (p1Active, p2Active);
        }

        private async Task HandleStalePlayersAsync(
            PlayerQueueItem player1, RedisValue player1Json,
            PlayerQueueItem player2, RedisValue player2Json,
            (bool player1Active, bool player2Active) activeStatus)
        {
            _logger.LogWarning("Match attempt failed due to inactive players. P1 Active: {P1Active}, P2 Active: {P2Active}. Starting cleanup.",
                activeStatus.player1Active, activeStatus.player2Active);

            if (activeStatus.player1Active) await _redis.ListLeftPushAsync(MatchmakingQueueKey, player1Json);
            if (activeStatus.player2Active) await _redis.ListLeftPushAsync(MatchmakingQueueKey, player2Json);

            if (!activeStatus.player1Active) await _redis.SetRemoveAsync(PlayersInQueueSetKey, player1.UserId.ToString());
            if (!activeStatus.player2Active) await _redis.SetRemoveAsync(PlayersInQueueSetKey, player2.UserId.ToString());
        }

        private async Task FinalizeMatchAsync(PlayerQueueItem player1, PlayerQueueItem player2)
        {
            Guid sessionId = Guid.NewGuid();
            List<Guid> userIds = [player1.UserId, player2.UserId];

            _logger.LogInformation("Match finalized. Session ID: {SessionId} between {Player1Id} and {Player2Id}.",
                sessionId, player1.UserId, player2.UserId);

            using var scope = _scopeFactory.CreateScope();
            var gameService = scope.ServiceProvider.GetRequiredService<IGameService>();

            try
            {
                await gameService.CreateNewGame(sessionId, userIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create game for session {SessionId}. Players: {P1}, {P2}",
                    sessionId, player1.UserId, player2.UserId);

                await _hubContext.Clients.Client(player1.ConnectionId).SendAsync("Error", "Game creation failed. Try again.");
                await _hubContext.Clients.Client(player2.ConnectionId).SendAsync("Error", "Game creation failed. Try again.");

                return;
            }

            await _hubContext.Clients.Client(player1.ConnectionId).SendAsync("MatchFound", new { sessionId, opponentId = player2.UserId, userId = player1.UserId });
            await _hubContext.Clients.Client(player2.ConnectionId).SendAsync("MatchFound", new { sessionId, opponentId = player1.UserId, userId = player2.UserId });
        }
    }
}
