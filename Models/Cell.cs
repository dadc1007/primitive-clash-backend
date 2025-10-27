
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

        public bool IsWalkable(ArenaEntity newEntity)
        {
            if (Type == CellType.River || Tower)
                return false;

            return !Collision(newEntity);
        }

        public bool PlaceEntity(ArenaEntity newEntity)
        {
            if (!IsWalkable(newEntity))
                return false;

            UpdateEntities(newEntity, true);

            return true;
        }

        public void RemoveEntity(ArenaEntity newEntity)
        {
            UpdateEntities(newEntity, false);
        }

        public void RemoveTower()
        {
            Tower = false;
        }

        public bool Collision(ArenaEntity newEntity)
        {
            AttackCard attackCard = (newEntity.PlayerCard.Card as AttackCard)!;

            if (attackCard.UnitClass == UnitClass.Ground && GroundEntity)
            {
                return true;
            }

            if (attackCard.UnitClass == UnitClass.Air && AirEntity)
            {
                return true;
            }

            return false;
        }

        public void UpdateEntities(ArenaEntity entity, bool adding)
        {
            AttackCard attackCard = (entity.PlayerCard.Card as AttackCard)!;

            if (attackCard.UnitClass == UnitClass.Ground)
                GroundEntity = adding;
            if (attackCard.UnitClass == UnitClass.Air)
                AirEntity = adding;
        }
    }

}