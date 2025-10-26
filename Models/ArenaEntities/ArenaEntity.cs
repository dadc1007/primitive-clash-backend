using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models.ArenaEntities

{
    [JsonDerivedType(typeof(BuildingEntity), typeDiscriminator: "Building")]
    [JsonDerivedType(typeof(TroopEntity), typeDiscriminator: "Troop")]

    public abstract class ArenaEntity(Guid userId, PlayerCard playerCard, int x, int y) : Positioned(userId, x, y)
    {
        public PlayerCard PlayerCard { get; set; } = playerCard;
    }
}
