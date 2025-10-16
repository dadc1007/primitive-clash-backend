using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services;

namespace PrimitiveClash.Backend.Hubs
{
    public class GameHub(IGameService gameService) : Hub
    {
        private const string SessionIdKey = "SessionId";
        private const string UserIdKey = "UserId";
        private readonly IGameService _gameService = gameService;

        public async Task JoinGame(Guid sessionId, Guid userId)
        {
            try
            {
                Game game = await _gameService.GetGame(sessionId);

                // Ensure that the user is part of the game
                if (!game.PlayerStates.Any(p => p.UserId == userId))
                {
                    await Clients.Caller.SendAsync("Error", "You are not authorized to join this game session.");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId.ToString());

                Context.Items[SessionIdKey] = sessionId;
                Context.Items[UserIdKey] = userId;

                Game gameUpdated = await _gameService.UpdatePlayerConnectionStatus(sessionId, userId, Context.ConnectionId, isConnected: true);

                await Clients.Caller.SendAsync("GameSync", gameUpdated);
                await Clients.Group(sessionId.ToString()).SendAsync("PlayerJoined", userId);
            }
            catch (GameNotFoundException)
            {
                await Clients.Caller.SendAsync("Error", "Game session not found or has expired.");
            }
            catch (ConcurrencyException)
            {
                await Clients.Caller.SendAsync("Error", "Failed to join game due to concurrent connection attempt. Please try again.");
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("Error", "An unexpected error occurred while joining the game.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue(SessionIdKey, out object? sessionIdObj) && sessionIdObj is Guid sessionId &&
                Context.Items.TryGetValue(UserIdKey, out object? userIdObj) && userIdObj is Guid disconnectedUserId)
            {
                await _gameService.UpdatePlayerConnectionStatus(
                    sessionId, disconnectedUserId, null, isConnected: false);
                await Clients.Group(sessionId.ToString()).SendAsync("OpponentDisconnected", disconnectedUserId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}