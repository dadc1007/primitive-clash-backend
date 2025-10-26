
using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    public class TroopEntity : AttackEntity
    {
        public PlayerCard Card { get; set; }
        public TroopState State { get; set; } = TroopState.Idle;
        // Solo para serialización
        public List<Point> PathSteps { get; set; } = new();
        [JsonIgnore]
        public Queue<Point> Path { get; set; } = new();
        public Point TargetPosition { get; set; } = new();

        public TroopEntity(Guid userId, int posX, int posY, PlayerCard card) : base(userId, posX, posY)
        {
            Card = card;
            Health = (card.Card as AttackCard)!.Hp;
        }

        public void MoveTo(int x, int y)
        {
            PosX = x;
            PosY = y;
        }

        public void SyncPathFromSteps()
        {
            Path = new Queue<Point>(PathSteps);
        }

        public void SyncStepsFromPath()
        {
            PathSteps = [.. Path];
        }

        public override void Act()
        {
        }
    }
}