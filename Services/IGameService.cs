using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IGameService
    {
        Task CreateNewGame(Guid sessionId, List<Guid> userIds);
        Task<Game> GetGame(Guid gameId);
        Task<Game> UpdatePlayerConnectionStatus(Guid sessionId, Guid userId, string? connectionId, bool isConnected);
        Task<bool> IsUserInGame(Guid userId);
    }
}