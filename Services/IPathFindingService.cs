using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IPathfindingService
    {
        List<Point> FindPath(Arena arena, TroopEntity troop, int startX, int startY, int targetX, int targetY);
        Point FindClosestAttackPoint(Arena arena, TroopEntity troop, int sourceX, int sourceY, Tower tower);
    }
}