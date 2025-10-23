
namespace PrimitiveClash.Backend.Models.Entities
{
    public class TroopEntity : AttackEntity
    {
        public TroopEntity(Guid userId, PlayerCard card, int posX, int posY) : base(userId, card, posX, posY)
        {
        }

        public void MoveTo(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public override void Act()
        {

        }
    }
}