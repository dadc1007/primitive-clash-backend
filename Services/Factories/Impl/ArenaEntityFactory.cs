using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Services.Factories.Impl
{
    public class ArenaEntityFactory : IArenaEntityFactory
    {
        public ArenaEntity CreateEntity(PlayerState player, PlayerCard playerCard, int x, int y)
        {
            return playerCard.Card.Type switch
            {
                CardType.Troop => new TroopEntity(player.Id, playerCard, x, y),
                CardType.Building => new BuildingEntity(player.Id, playerCard, x, y),
                _ => throw new CardTypeException(playerCard.Card.Type),
            };
        }
    }
}
