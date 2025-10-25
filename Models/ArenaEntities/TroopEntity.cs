
using PrimitiveClash.Backend.Models.Cards;
using System.Text.Json.Serialization;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    public class TroopEntity(Guid userId, int posX, int posY, PlayerCard card) : AttackEntity(userId, posX, posY)
    {
        public PlayerCard Card { get; set; } = card;
    public Queue<(int X, int Y)> Path { get; set; } = new Queue<(int X, int Y)>();

    // This is a runtime-only reference used for path recalculation. It must not be serialized
    // because it references abstract/concrete types that System.Text.Json can't round-trip.
    [JsonIgnore]
    public AttackEntity? PathTarget { get; set; }

        public void MoveTo(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public override void Act()
        {
        }
    }
}