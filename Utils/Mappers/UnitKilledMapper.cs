using PrimitiveClash.Backend.DTOs.Notifications;

namespace PrimitiveClash.Backend.Utils.Mappers;

public static class UnitKilledMapper
{
    public static UnitKilledNotificacion ToUnitKilledNotificacion(
        Guid attackerId,
        Guid targetId
    )
    {
        return new UnitKilledNotificacion(attackerId, targetId);
    }
}
