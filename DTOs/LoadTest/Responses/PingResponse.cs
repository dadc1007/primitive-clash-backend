namespace PrimitiveClash.Backend.DTOs.LoadTest.Responses
{
    public record PingResponse(
        string Status,
        string Server,
        DateTime Timestamp,
        int Delay
    );
}
