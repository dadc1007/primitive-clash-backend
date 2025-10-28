using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;

namespace PrimitiveClash.Backend.Hubs
{
    public class GameHub(
        IServiceScopeFactory scopeFactory,
        IGameService gameService,
        IBattleService battleService,
        ILogger<GameHub> logger
    ) : Hub
    {
        private const string SessionIdKey = "SessionId";
        private const string UserIdKey = "UserId";
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IGameService _gameService = gameService;
        private readonly IBattleService _battleService = battleService;
        private readonly ILogger<GameHub> _logger = logger;

        public async Task JoinGame(Guid sessionId, Guid userId)
        {
            try
            {
                Game game = await _gameService.GetGame(sessionId);

                if (game.PlayerStates.All(p => p.Id != userId))
                {
                    await Clients.Caller.SendAsync(
                        "Error",
                        "You are not authorized to join this game session."
                    );
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

                Context.Items[SessionIdKey] = sessionId;
                Context.Items[UserIdKey] = userId;

                Game updatedGame = await _gameService.UpdatePlayerConnectionStatus(
                    sessionId,
                    userId,
                    Context.ConnectionId,
                    isConnected: true
                );

                await Clients.Caller.SendAsync("JoinedToGame", updatedGame);

                StartLoop(updatedGame, sessionId);
            }
            catch (GameNotFoundException)
            {
                await Clients.Caller.SendAsync("Error", "Game session not found or has expired.");
            }
            catch (ConcurrencyException)
            {
                await Clients.Caller.SendAsync(
                    "Error",
                    "Failed to join game due to concurrent connection attempt. Please try again."
                );
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync(
                    "Error",
                    "An unexpected error occurred while joining the game."
                );
            }
        }

        public async Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            _logger.LogInformation(
                "Attempting spawn: Session={SessionId}, User={UserId}, Card={CardId}, Pos=({X},{Y})",
                sessionId,
                userId,
                cardId,
                x,
                y
            );

            try
            {
                await _battleService.SpawnCard(sessionId, userId, cardId, x, y);

                _logger.LogInformation("Spawn successful");
            }
            catch (NotEnoughElixirException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (InvalidSpawnPositionException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (InvalidCardException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "SpawnCard failed for Session={SessionId}. Error: {Message}",
                    sessionId,
                    ex.Message
                );
                await Clients.Caller.SendAsync("Error", $"Unexpected error: {ex.Message}");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (
                Context.Items.TryGetValue(SessionIdKey, out object? sessionIdObj)
                && sessionIdObj is Guid sessionId
                && Context.Items.TryGetValue(UserIdKey, out object? userIdObj)
                && userIdObj is Guid disconnectedUserId
            )
            {
                await _gameService.UpdatePlayerConnectionStatus(
                    sessionId,
                    disconnectedUserId,
                    null,
                    isConnected: false
                );
            }

            await base.OnDisconnectedAsync(exception);
        }

        private void StartLoop(Game game, Guid sessionId)
        {
            if (!game.PlayerStates.All(p => p.IsConnected))
                return;
            using IServiceScope scope = _scopeFactory.CreateScope();
            IGameLoopService gameLoopService =
                scope.ServiceProvider.GetRequiredService<IGameLoopService>();
            gameLoopService.StartGameLoop(sessionId);
        }
    }
}
