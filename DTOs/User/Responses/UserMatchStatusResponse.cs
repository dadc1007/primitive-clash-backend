namespace PrimitiveClash.Backend.DTOs.User.Responses
{
    public record UserMatchStatusResponse(
        Guid UserId,
        bool IsInMatch,
        Guid? MatchId
    );
}
