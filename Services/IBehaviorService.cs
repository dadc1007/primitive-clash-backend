using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBehaviorService
    {
        void ExecuteTroopAction(Arena arena, TroopEntity troop, List<ArenaEntity> changedEntities, List<Cell> changedCells);
    }
}