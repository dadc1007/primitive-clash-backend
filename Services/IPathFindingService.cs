using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IPathfindingService
    {
        List<Point> FindPath(Arena arena, TroopEntity troop, Positioned target);
    }
}