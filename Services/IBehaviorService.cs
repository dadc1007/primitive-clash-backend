using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBehaviorService
    {
        void ExecuteTroopAction(Guid sessionId, Arena arena, TroopEntity troop);
    }
}