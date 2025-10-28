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

    public async Task NotifyCardSpawned(Guid sessionId, CardSpawnedNotification notification)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("CardSpawned", notification);

        _logger.LogDebug(
            "Sent CardSpawned notification to session {SessionId}: {@Notification}",
            sessionId,
            notification
        );
    }

    public async Task NotifyTroopMoved(Guid sessionId, TroopMovedNotification notification)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("TroopMoved", notification);

        _logger.LogDebug(
            "Sent TroopMoved notification to session {SessionId}: {@Notification}",
            sessionId,
            notification
        );
    }

    public async Task NotifyUnitDamaged(Guid sessionId, UnitDamagedNotification notification)
    {
        await _gameHub.Clients.Group(sessionId.ToString()).SendAsync("UnitDamaged", notification);

        _logger.LogDebug(
            "Sent UnitDamaged notification to session {SessionId}: {@Notification}",
            sessionId,
            notification
        );
    }

    public async Task NotifyUnitKilled(
        Guid sessionId,
        UnitKilledNotificacion unitDamagedNotification
    )
    {
        await _gameHub
            .Clients.Group(sessionId.ToString())
            .SendAsync("UnitKilled", unitDamagedNotification);

        _logger.LogDebug(
            "Sent UnitKilled notification to session {SessionId}: {@Notification}",
            sessionId,
            unitDamagedNotification
        );
    }
}
