using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Hubs
{
    public class BattleHub(IBattleService battleService, IGameService gameService) : Hub
    {
        private readonly IBattleService _battleService = battleService;
        private readonly IGameService _gameService = gameService;

        public async Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y)
        {
            try
            {
                bool success = await _battleService.SpawnCard(sessionId, userId, cardId, x, y);

                if (!success)
                {
                    await Clients.Caller.SendAsync("Error", "Failed to spawn card. Check elixir or position.");
                    return;
                }

                await Clients.Group(sessionId.ToString())
                    .SendAsync("CardSpawned", new
                    {
                        userId,
                        cardId,
                        x,
                        y
                    });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", $"Unexpected error: {ex.Message}");
            }
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
