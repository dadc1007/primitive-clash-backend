using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    [JsonDerivedType(typeof(ArenaEntity), typeDiscriminator: "ArenaEntity")]
    [JsonDerivedType(typeof(Tower), typeDiscriminator: "Tower")]

    public class Positioned(Guid userId, int x, int y) : Point(x, y)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = userId;
        public int Health { get; set; }
        public Point TargetPosition { get; set; } = new(0, 0);

        public void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public bool IsAlive()
        {
            return Health > 0;
        }
    }
}