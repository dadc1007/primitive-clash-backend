
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    public class BuildingEntity : ArenaEntity
    {
        public int LeftTime { get; set; }

        public BuildingEntity(Guid userId, PlayerCard playerCard, int x, int y) : base(userId, playerCard, x, y)
        {
            Health = (playerCard.Card as BuildingCard)!.Hp;
        }
    }
}