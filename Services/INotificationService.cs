using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services;

public interface INotificationService
{
    Task NotifyCardSpawned(Guid sessionId, CardSpawnedNotification notification);
    Task NotifyTroopMoved(Guid sessionId, TroopMovedNotification notification);
    Task NotifyUnitDamaged(Guid sessionId, UnitDamagedNotification notification);
    Task NotifyUnitKilled(Guid sessionId, UnitKilledNotificacion unitDamagedNotification);
}
