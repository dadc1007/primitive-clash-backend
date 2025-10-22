using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class Cell
    {
        public required CellType Type { get; set; }
        public Tower? Tower { get; set; }

        public bool IsWalkable()
        {
            return Type != CellType.River && Tower is null;
        }
    }
}