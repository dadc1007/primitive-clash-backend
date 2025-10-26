using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBattleService
    {
        Task SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y);
        void HandleAttack(AttackEntity attacker, AttackEntity target);
        Task HandleMovement(Guid sessionId, TroopEntity troop, Arena arena);
        bool CanExecuteMovement(Arena arena, TroopEntity troop, int x, int y);
    }
}