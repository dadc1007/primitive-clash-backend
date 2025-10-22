using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClash.Backend.Models.Entities

{
    public abstract class ArenaEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }

        public PlayerCard Card { get; set; }

        public ArenaEntity(Guid userId, PlayerCard card, int posX, int posY)
        {
            UserId = userId;
            Card = card;
            PosX = posX;
            PosY = posY;
        }

        public virtual void Act() { }
    }
}
