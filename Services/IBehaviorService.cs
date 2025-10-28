using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services
{
    public interface IBehaviorService
    {
        void ExecuteAction(Guid sessionId, Arena arena, Positioned unit);
    }
}