
namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    using System.Text.Json.Serialization;

    public abstract class AttackEntity(Guid userId, int posX, int posY) : ArenaEntity(userId, posX, posY)
    {
        public int Health { get; set; }

        // Runtime-only reference to the current attack target. Must not be serialized.
        [JsonIgnore]
        public AttackEntity? CurrentTarget { get; set; }

        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
        }

        public bool IsAlive()
        {
            return Health > 0;
        }
    }
}
