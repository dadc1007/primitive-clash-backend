namespace PrimitiveClash.Backend.Models
{
    public class PlayerState(Guid id, List<PlayerCard> cards)
    {
        public Guid Id { get; set; } = id;
        public bool IsConnected { get; set; } = true;
        public string? ConnectionId { get; set; }
        public List<PlayerCard> Cards { get; set; } = cards;
        public float CurrentElixir { get; set; } = 100.0f;
        public float ElixirPerSecond { get; set; } = 0.5f;

        public List<PlayerCard> GetHand()
        {
            return Cards.GetRange(0, 4);
        }

        public PlayerCard GetNextCard()
        {
            return Cards[4];
        }
    }
}