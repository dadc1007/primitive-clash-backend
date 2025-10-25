using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IArenaService
    {
        Task<Arena> CreateArena(Dictionary<Guid, List<Tower>> towers);
        ArenaEntity PlaceEntity(Arena arena, PlayerState player, PlayerCard card, int x, int y);
        double CalculateDistance(ArenaEntity entity1, ArenaEntity entity2);
        (AttackEntity? Target, double Distance) FindClosestTarget(Arena arena, TroopEntity troop, double visionRange);
    }
}