namespace PrimitiveClash.Backend.DTOs.LoadTest.Responses
{
    public record CpuResponse(
        string Status,
        double Result,
        string Hash,
        string Server,
        DateTime Timestamp
    );
}
