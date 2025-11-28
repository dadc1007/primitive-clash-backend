using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Models
{
    public class Tower : Positioned
    {
        public TowerTemplate TowerTemplate { get; set; }

        public Tower(Guid userId, TowerTemplate towerTemplate) : base(userId, 0, 0)
        {
            TowerTemplate = towerTemplate;
            Health = towerTemplate.Hp;
        }

        public IEnumerable<(int X, int Y)> GetOccupiedCells()
        {
            for (int x = X; x < X + TowerTemplate.Size; x++)
            {
                for (int y = Y; y < Y + TowerTemplate.Size; y++)
                {
                    yield return (x, y);
                }
            }
        }
    }
}