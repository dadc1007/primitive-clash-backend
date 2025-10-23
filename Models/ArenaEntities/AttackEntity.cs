
namespace PrimitiveClash.Backend.Models.Entities
{
    public abstract class AttackEntity : ArenaEntity
    {
        protected AttackEntity(Guid userId, PlayerCard card, int posX, int posY) : base(userId, card, posX, posY)
        {
        }

        public int Health { get; set; }

        public virtual void Attack() { }
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
