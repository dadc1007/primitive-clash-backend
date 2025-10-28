using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    [JsonDerivedType(typeof(ArenaEntity), typeDiscriminator: "ArenaEntity")]
    [JsonDerivedType(typeof(Tower), typeDiscriminator: "Tower")]

    public class Positioned(Guid userId, int x, int y) : Point(x, y)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = userId;
        public int Health { get; set; }
        public PositionedState State { get; set; } = PositionedState.Idle;
        public Guid? CurrentTargetId { get; set; } = null;
        public Point? CurrentTargetPosition { get; set; } = null;
        public bool CurrentTargetIsTower { get; set; } = false;

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