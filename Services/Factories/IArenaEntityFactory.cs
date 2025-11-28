using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Factories
{
    public interface IArenaEntityFactory
    {
        ArenaEntity CreateEntity(PlayerState player, PlayerCard playerCard, int x, int y);
    }
}