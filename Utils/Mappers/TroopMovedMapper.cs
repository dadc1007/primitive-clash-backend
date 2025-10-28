using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class TroopMovedMapper
{
    public static TroopMovedNotification ToTroopMovedNotification(
        Guid troopId,
        Guid playerId,
        int x,
        int y,
        string state
    )
    {
        return new TroopMovedNotification(troopId, playerId, x, y, state);
    }
}
