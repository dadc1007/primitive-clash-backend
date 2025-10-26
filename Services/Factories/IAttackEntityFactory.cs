using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services.Factories
{
    public interface IAttackEntityFactory
    {
        AttackEntity CreateEntity(PlayerState player, PlayerCard card, int x, int y);
    }
}