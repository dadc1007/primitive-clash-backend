namespace PrimitiveClash.Backend.Configuration
{
    public record GameSettings
    {
        public required List<string> StarterCardNames { get; init; }
        public required int MaxDeckSize { get; init; }
    }
}