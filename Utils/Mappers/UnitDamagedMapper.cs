using PrimitiveClash.Backend.DTOs.Notifications;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class UnitDamagedMapper
{
    public static UnitDamagedNotification ToUnitDamagedNotification(
        Guid attackerId,
        Guid targetId,
        int damage,
        int health
    )
    {
        return new UnitDamagedNotification(attackerId, targetId, damage, health);
    }
}
