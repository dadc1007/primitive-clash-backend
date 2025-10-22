
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Entities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class Cell
    {
        public required CellType Type { get; set; }
        public Tower? Tower { get; set; }

        public ArenaEntity? GroundEntity { get; set; }
        public ArenaEntity? AirEntity { get; set; }

        public bool IsWalkable(ArenaEntity newEntity)
        {
            if (Type == CellType.River || Tower is not null)
                return false;

            var newType = GetMovementType(newEntity);

            return newType switch
            {
                MovementType.Ground => GroundEntity is null,
                MovementType.Air => AirEntity is null,
                _ => false
            };
        }

        public bool TryPlaceEntity(ArenaEntity newEntity)
        {
            if (!IsWalkable(newEntity))
                return false;

            var type = GetMovementType(newEntity);

            if (type == MovementType.Ground)
                GroundEntity = newEntity;
            else if (type == MovementType.Air)
                AirEntity = newEntity;

            return true;
        }

        private static MovementType GetMovementType(ArenaEntity entity)
        {
            if (entity is TroopEntity troop && troop.Card.Card is TroopCard troopCard)
                return troopCard.MovementType;

            return MovementType.Ground;
        }
    }

}