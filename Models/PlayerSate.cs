namespace PrimitiveClash.Backend.Models
{
    public class PlayerState(Guid userId)
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; } = userId;
        public bool IsConnected { get; set; } = true;
        public string? ConnectionId { get; set; }
    }
}