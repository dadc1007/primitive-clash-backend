using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Services;

namespace PrimitiveClash.Backend.Hubs
{
    public class MatchmakingHub(IMatchmakingService matchmakingService) : Hub
    {
        private readonly IMatchmakingService _matchmakingService = matchmakingService;
        private const string UserIdKey = "UserId";

        public async Task SearchGame(Guid userId)
        {
            try
            {
                string connectionId = Context.ConnectionId;
                Context.Items[UserIdKey] = userId;
                await _matchmakingService.EnqueuePlayer(userId, connectionId);

                await Clients.Caller.SendAsync("UpdateStatus", "Searching for a game...");
            }
            catch (PlayerAlreadyInQueueException ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
            catch (Exception)
            {
                await Clients.Caller.SendAsync("Error", "Unexpected error when trying to join the queue. Please try again");
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.Items.TryGetValue(UserIdKey, out Object? userIdObj) && userIdObj is Guid userId)
            {
                _matchmakingService.DequeuePlayer(userId);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}