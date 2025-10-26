using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Services.Factories.Impl
{
    public class AttackEntityFactory : IAttackEntityFactory
    {
        public AttackEntity CreateEntity(PlayerState player, PlayerCard card, int x, int y)
        {
            return card.Card.Type switch
            {
                CardType.Troop => new TroopEntity(player.Id, x, y, card),
                CardType.Building => new BuildingEntity(player.Id, x, y, card),
                _ => throw new CardTypeException(card.Card.Type),
            };
        }
    }
}
