using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Models
{
    public class Game(Guid id, List<PlayerState> playerStates, Arena gameArena)
    {
        public Guid Id { get; set; } = id;
        public GameState State { get; set; } = GameState.InProgress;
        public Arena GameArena { get; set; } = gameArena;
        public List<PlayerState> PlayerStates { get; set; } = playerStates;
    }
}