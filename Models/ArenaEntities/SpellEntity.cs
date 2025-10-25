
namespace PrimitiveClash.Backend.Models.ArenaEntities

{
    public class SpellEntity(Guid userId, int posX, int posY, PlayerCard card) : ArenaEntity(userId, posX, posY)
    {
        public int LeftTime { get; set; }
        public PlayerCard Card { get; set; } = card;

        public override void Act()
        {
            LeftTime--;
        }
    }
}