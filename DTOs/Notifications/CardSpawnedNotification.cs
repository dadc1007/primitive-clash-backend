namespace PrimitiveClash.Backend.DTOs.Notifications;

public record CardSpawnedNotification(
    Guid UnitId,
    Guid UserId,
    Guid CardPlayedId,
    int Level,
    int X,
    int Y,
    Guid NextCardId
);
