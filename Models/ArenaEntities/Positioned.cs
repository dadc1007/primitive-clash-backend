using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    [JsonDerivedType(typeof(ArenaEntity), typeDiscriminator: "ArenaEntity")]
    [JsonDerivedType(typeof(Tower), typeDiscriminator: "Tower")]

    public class Positioned(Guid userId, int x, int y) : Point(x, y)
    {
        private readonly object _lock = new();
        
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = userId;
        public int Health { get; set; }
        public PositionedState State { get; set; } = PositionedState.Idle;
        public Guid? CurrentTargetId { get; set; } = null;
        public Point? CurrentTargetPosition { get; set; } = null;
        public bool CurrentTargetIsTower { get; set; } = false;

        public void TakeDamage(int damage)
        {
            lock (_lock)
            {
                Health -= damage;
            }
        }

        public bool IsAlive()
        {
            lock (_lock)
            {
                return Health > 0;
            }
        }
        
        public void UpdateState(PositionedState state, Guid? targetId, Point? targetPos)
        {
            lock(_lock)
            {
                State = state;
                CurrentTargetId = targetId;
                CurrentTargetPosition = targetPos;
            }
        }

        public object GetLock() => _lock;
    }
}