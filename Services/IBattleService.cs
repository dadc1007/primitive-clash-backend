using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBattleService
    {
        Task<(ArenaEntity Entity, Cell cell)> SpawnCard(Guid sessionId, Guid userId, Guid cardId, int x, int y);
        void HandleAttack(AttackEntity attacker, AttackEntity target);
        void HandleMovement(TroopEntity troop, int nextX, int nextY, Arena arena, List<ArenaEntity> changedEntities, List<Cell> changedCells);
        bool CanExecuteMovement(Arena arena, TroopEntity troop, int x, int y);
    }
}