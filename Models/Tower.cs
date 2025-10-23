namespace PrimitiveClash.Backend.Models
{
    public class Tower(TowerTemplate towerTemplate, Guid playerStateId)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Health { get; set; } = towerTemplate.Hp;
        public TowerTemplate TowerTemplate { get; set; } = towerTemplate;
        public Guid PlayerStateId { get; set; } = playerStateId;
    }
}