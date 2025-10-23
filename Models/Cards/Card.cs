using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.Cards
{
    [JsonDerivedType(typeof(SpellCard), typeDiscriminator: "Spell")]
    [JsonDerivedType(typeof(TroopCard), typeDiscriminator: "Troop")]
    [JsonDerivedType(typeof(BuildingCard), typeDiscriminator: "Building")]

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