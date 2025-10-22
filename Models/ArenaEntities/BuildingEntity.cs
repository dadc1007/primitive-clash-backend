
namespace PrimitiveClash.Backend.Models.Entities
{
    public class BuildingEntity : AttackEntity
    {
        public BuildingEntity(Guid userId, PlayerCard card, int posX, int posY) : base(userId, card, posX, posY)
        {
        }

        public int LeftTime { get; set; }

        public override void Act()
        {
            LeftTime--;
        }
    }
}