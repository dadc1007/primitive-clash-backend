using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models.Cards;

namespace PrimitiveClashBackend.Models
{
    public class PlayerCard
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Level { get; set; } = 1;
        public int Quantity { get; private set; } = 0;
        public required Guid CardId { get; set; }
        public required Card Card { get; set; }
        public Guid? DeckId { get; set; }
        public required Guid UserId { get; set; }

        public void IncreaseQuantity(int amount)
        {
            Quantity += amount;
        }

        public void Upgrade(int amount)
        {
            DecreaseQuantity(amount);
            Level++;
        }

        private void DecreaseQuantity(int amount)
        {
            if (amount > Quantity)
            {
                throw new NotEnoughPlayerCardsException();
            }
            Quantity -= amount;
        }
    }
}