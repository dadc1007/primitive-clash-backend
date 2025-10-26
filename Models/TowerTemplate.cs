using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class TowerTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Level { get; set; } = 1;
        public int Hp { get; set; }
        public int Damage { get; set; }
        public int Range { get; set; }
        public required TowerType Type { get; set; }
        public int Size { get; set; }
    }
}