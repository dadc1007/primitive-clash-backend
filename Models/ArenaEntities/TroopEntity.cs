
using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    public class TroopEntity : ArenaEntity
    {
        public TroopState State { get; set; } = TroopState.Idle;
        // Solo para serialización
        public List<Point> PathSteps { get; set; } = new();
        [JsonIgnore]
        public Queue<Point> Path { get; set; } = new();


        public TroopEntity(Guid userId, PlayerCard playerCard, int x, int y) : base(userId, playerCard, x, y)
        {
            Health = (playerCard.Card as AttackCard)!.Hp;
        }

        public void MoveTo(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void SyncPathFromSteps()
        {
            Path = new Queue<Point>(PathSteps);
        }

        public void SyncStepsFromPath()
        {
            PathSteps = [.. Path];
        }
    }
}