
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class Cell
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required CellType Type { get; set; }
        public bool Tower { get; set; } = false;
        public bool GroundEntity { get; set; } = false;
        public bool AirEntity { get; set; } = false;

        public bool IsWalkable(AttackEntity newEntity)
        {
            if (Type == CellType.River || Tower)
                return false;

            var newType = GetMovementType(newEntity);

            return newType switch
            {
                MovementType.Ground => GroundEntity is false,
                MovementType.Air => AirEntity is false,
                _ => false
            };
        }

        public bool PlaceEntity(AttackEntity newEntity)
        {
            if (!IsWalkable(newEntity))
                return false;

            var type = GetMovementType(newEntity);

            if (type == MovementType.Ground)
                GroundEntity = true;
            else if (type == MovementType.Air)
                AirEntity = true;

            return true;
        }

        public void RemoveEntity(AttackEntity newEntity)
        {
            var type = GetMovementType(newEntity);

            if (type == MovementType.Ground)
                GroundEntity = false;
            else if (type == MovementType.Air)
                AirEntity = false;
        }

        public static MovementType GetMovementType(AttackEntity entity)
        {
            if (entity is TroopEntity troop && troop.Card.Card is TroopCard troopCard)
                return troopCard.MovementType;

            return MovementType.Ground;
        }
    }

}