using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services
{
    public interface IBattleService
    {
        Task<bool> SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y);
    }
}