using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IArenaService
    {
        Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers);
        AttackEntity CreateEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y);
        void PlaceEntity(Arena arena, AttackEntity entity);
        void RemoveEntity(Arena arena, AttackEntity entity);
        double CalculateDistance(AttackEntity sourceEntity, AttackEntity targetEntity);
        IEnumerable<AttackEntity> GetEnemiesInVision(Arena arena, TroopEntity troop);
        Tower GetNearestEnemyTower(Arena arena, TroopEntity troop);
    }
}