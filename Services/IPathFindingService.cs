using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IPathfindingService
    {
        List<(int X, int Y)> FindPath(Arena arena, TroopEntity troop, int startX, int startY, int targetX, int targetY);
    }
}