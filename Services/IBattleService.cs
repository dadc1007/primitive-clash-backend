using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBattleService
    {
        Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y);
        Task HandleAttack(Guid sessionId, Arena arena, Positioned attacker, Positioned target);
        Task HandleMovement(Guid sessionId, TroopEntity troop, Arena arena);
    }
}