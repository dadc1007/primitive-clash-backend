using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.Cards
{
    public class TroopCard : AttackCard
    {
        public MovementSpeed MovementSpeed { get; set; }
        public int VisionRange { get; set; }
    }
}