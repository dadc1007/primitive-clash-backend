namespace PrimitiveClash.Backend.Models
{
    public class ArenaTemplate
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public int RequiredTrophies { get; set; }
    }
}