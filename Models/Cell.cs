
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Entities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class Cell
    {
        public required CellType Type { get; set; }
        public Tower? Tower { get; set; }

        public ArenaEntity? Entity { get; set; }
        public bool IsWalkable()
        {
            return Type != CellType.River && Tower is null;

        }

        public bool IsSummable(ArenaEntity newEntity)
        {
            if (Entity is null)
                return true;

            var existingType = GetMovementType(Entity);
            var newType = GetMovementType(newEntity);

            return existingType != newType;
        }

        private static MovementType GetMovementType(ArenaEntity entity)
        {
            if (entity is TroopEntity troop && troop.Card.Card is TroopCard troopCard)
                return troopCard.MovementType;

            return MovementType.Ground;
        }


    }
}