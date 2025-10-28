using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;

namespace PrimitiveClash.Backend.Services.Impl;

public class NotificationService(IHubContext<GameHub> gameHub, ILogger<NotificationService> logger)
    : INotificationService
{
    private readonly IHubContext<GameHub> _gameHub = gameHub;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task NotifyCardSpawned(Guid sessionId, CardSpawnedNotification obj)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("CardSpawned", obj);

        _logger.LogDebug(
            "Sent CardSpawned notification to session {SessionId}: {@Obj}",
            sessionId,
            obj
        );
    }

    public async Task NotifyTroopMoved(Guid sessionId, TroopMovedNotification obj)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("TroopMoved", obj);

        _logger.LogDebug(
            "Sent TroopMoved notification to session {SessionId}: {@Obj}",
            sessionId,
            obj
        );
    }

    public async Task NotifyUnitDamaged(Guid sessionId, UnitDamagedNotification obj)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("UnitDamaged", obj);

        _logger.LogDebug(
            "Sent UnitDamaged notification to session {SessionId}: {@Obj}",
            sessionId,
            obj
        );
    }

    public async Task NotifyUnitKilled(Guid sessionId, UnitKilledNotificacion obj)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("UnitKilled", obj);

        _logger.LogDebug(
            "Sent UnitKilled notification to session {SessionId}: {@Obj}",
            sessionId,
            obj
        );
    }

    public async Task NotifyEndGame(Guid sessionId, EndGameNotification obj)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("EndGame", obj);
        
        _logger.LogDebug(
            "Sent EndGame notification to session {SessionId}: {@Obj}",
            sessionId,
            obj
        );
    }
}
