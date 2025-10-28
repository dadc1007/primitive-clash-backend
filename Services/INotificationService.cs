using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services;

public interface INotificationService
{
    Task NotifyCardSpawned(Guid sessionId, CardSpawnedNotification obj);
    Task NotifyTroopMoved(Guid sessionId, TroopMovedNotification obj);
    Task NotifyUnitDamaged(Guid sessionId, UnitDamagedNotification obj);
    Task NotifyUnitKilled(Guid sessionId, UnitKilledNotificacion obj);
    Task NotifyEndGame(Guid sessionId, EndGameNotification obj);
}
