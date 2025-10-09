using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.Cards
{
    public abstract class Card
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public int ElixirCost { get; set; }
        public CardRarity Rarity { get; set; }
        public CardType Type { get; set; }
        public int Damage { get; set; }
        public required List<CardTarget> Targets { get; set; }
    }
}