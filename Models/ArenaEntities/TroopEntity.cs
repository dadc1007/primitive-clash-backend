
using System.Text.Json.Serialization;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models.ArenaEntities
{
    public class TroopEntity : ArenaEntity
    {
        private readonly object _lock = new();
        
        // Solo para serialización
        public List<Point> PathSteps { get; set; } = [];
        [JsonIgnore]
        public Queue<Point> Path { get; set; } = new();
        public Point TargetPosition { get; set; } = new(0, 0);


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
        
        public object GetLock() => _lock;
    }
}