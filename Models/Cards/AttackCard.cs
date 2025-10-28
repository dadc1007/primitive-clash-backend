using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.Cards
{
    public abstract class AttackCard : Card
    {
        public int Hp { get; set; }
        public int Range { get; set; }
        public int DamageArea { get; set; }
        public float HitSpeed { get; set; }
        public UnitClass UnitClass { get; set; }
    }
}