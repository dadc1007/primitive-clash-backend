using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Models
{
    public class Tower : AttackEntity
    {
        public TowerTemplate TowerTemplate { get; set; }

        public Tower(Guid userId, TowerTemplate towerTemplate) : base(userId, 0, 0)
        {
            UserId = userId;
            TowerTemplate = towerTemplate;
            Health = towerTemplate.Hp;
        }
    }
}