using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;

namespace PrimitiveClash.Backend.Services;

public interface INotificationService
{
    Task NotifyCardSpawned(Guid sessionId, PlayerState player, ArenaEntity entity, PlayerCard cardToPut);
    Task NotifyTroopMoved(Guid sessionId, TroopMovedNotification obj);
    Task NotifyUnitDamaged(Guid sessionId, UnitDamagedNotification obj);
    Task NotifyUnitKilled(Guid sessionId, UnitKilledNotificacion obj);
    Task NotifyEndGame(Guid sessionId, EndGameNotification obj);
    Task NotifyNewElixir(string playerConnectionId, decimal playerCurrentElixir);
}
