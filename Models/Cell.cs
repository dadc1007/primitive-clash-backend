
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
            if (Tower)return false;

            if (Type == CellType.River && (newEntity.PlayerCard.Card as AttackCard)!.UnitClass != UnitClass.Air)
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

        private bool Collision(ArenaEntity newEntity)
        {
            AttackCard attackCard = (newEntity.PlayerCard.Card as AttackCard)!;

            switch (attackCard.UnitClass)
            {
                case UnitClass.Ground when GroundEntity:
                case UnitClass.Air when AirEntity:
                    return true;
                default:
                    return false;
            }
        }

        private void UpdateEntities(ArenaEntity entity, bool adding)
        {
            AttackCard attackCard = (entity.PlayerCard.Card as AttackCard)!;

            switch (attackCard.UnitClass)
            {
                case UnitClass.Ground:
                    GroundEntity = adding;
                    break;
                case UnitClass.Air:
                    AirEntity = adding;
                    break;
            }
        }
    }

}