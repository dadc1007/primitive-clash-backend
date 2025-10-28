using PrimitiveClash.Backend.DTOs.Notifications;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class CardSpawnedMapper
{
    public static CardSpawnedNotification ToCardSpawnedNotification(
        Guid unitId,
        Guid userId,
        Guid cardId,
        int level,
        int x,
        int y
    )
    {
        return new CardSpawnedNotification(unitId, userId, cardId, level, x, y);
    }
}