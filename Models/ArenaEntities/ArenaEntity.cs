using PrimitiveClash.Backend.Models.Cards;
using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models.ArenaEntities

{
    [JsonDerivedType(typeof(BuildingEntity), typeDiscriminator: "Building")]
    [JsonDerivedType(typeof(TroopEntity), typeDiscriminator: "Troop")]
    [JsonDerivedType(typeof(SpellEntity), typeDiscriminator: "Spell")]
    [JsonDerivedType(typeof(Tower), typeDiscriminator: "Tower")]

    public abstract class ArenaEntity(Guid userId, int posX, int posY)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = userId;
        public int PosX { get; set; } = posX;
        public int PosY { get; set; } = posY;

        public virtual void Act() { }
    }
}
