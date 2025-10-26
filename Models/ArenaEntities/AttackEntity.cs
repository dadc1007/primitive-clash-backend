
using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    [JsonDerivedType(typeof(BuildingEntity), typeDiscriminator: "Building")]
    [JsonDerivedType(typeof(TroopEntity), typeDiscriminator: "Troop")]
    [JsonDerivedType(typeof(Tower), typeDiscriminator: "Tower")]

    public abstract class AttackEntity(Guid userId, int posX, int posY) : ArenaEntity(userId, posX, posY)
    {
        public int Health { get; set; }
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
