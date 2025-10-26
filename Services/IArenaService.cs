using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IArenaService
    {
        Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers);
        ArenaEntity CreateEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y);
        void PlaceEntity(Arena arena, ArenaEntity entity);
        void RemoveEntity(Arena arena, ArenaEntity entity);
        double CalculateDistance(Positioned sourceEntity, Positioned targetEntity);
        IEnumerable<ArenaEntity> GetEnemiesInVision(Arena arena, TroopEntity troop);
        Tower GetNearestEnemyTower(Arena arena, TroopEntity troop);
    }
}