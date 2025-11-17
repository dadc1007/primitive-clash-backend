using Microsoft.AspNetCore.SignalR;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Services.Impl;

public class NotificationService(IHubContext<GameHub> gameHub, ILogger<NotificationService> logger)
    : INotificationService
{
    private readonly IHubContext<GameHub> _gameHub = gameHub;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task NotifyCardSpawned(
        Guid sessionId,
        PlayerState player,
        ArenaEntity entity,
        PlayerCard cardToPut
    )
    {
        CardSpawnedNotification cardSpawnedNotification = new CardSpawnedNotification(
            entity.Id,
            entity.UserId,
            entity.PlayerCard.Card.Id,
            entity.PlayerCard.Level,
            entity.X,
            entity.Y,
            entity.Health,
            (entity.PlayerCard.Card as AttackCard)!.Hp
        );

        await _gameHub
            .Clients.Group(sessionId.ToString())
            .SendAsync("CardSpawned", cardSpawnedNotification);

        _logger.LogDebug(
            "Sent CardSpawned notification to session {SessionId}: {@Obj}",
            sessionId,
            cardSpawnedNotification
        );

        await NotifyRefreshHand(player, cardToPut);
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

    public async Task NotifyNewElixir(string playerConnectionId, decimal playerCurrentElixir)
    {
        if (string.IsNullOrEmpty(playerConnectionId))
        {
            _logger.LogWarning("NotifyNewElixir: playerConnectionId es null o vacío, no se enviará notificación.");
            return;
        }

        await _gameHub
            .Clients.Client(playerConnectionId)
            .SendAsync("NewElixir", playerCurrentElixir);

        _logger.LogDebug(
            "Sent NewElixir notification to player {PlayerConnectionId}: {PlayerCurrentElixir}",
            playerConnectionId,
            playerCurrentElixir
        );
    }

    private async Task NotifyRefreshHand(PlayerState player, PlayerCard cardToPut)
    {
        if (string.IsNullOrEmpty(player.ConnectionId))
        {
            _logger.LogWarning(
                "No se envió RefreshHand: player {PlayerId} sin ConnectionId",
                player.Id
            );
            return;
        }

        try
        {
            await _gameHub
                .Clients.Client(player.ConnectionId)
                .SendAsync(
                    "RefreshHand",
                    RefreshHandNotificationMapper.ToRefreshHandNotification(
                        cardToPut,
                        player.GetNextCard(),
                        player.CurrentElixir
                    )
                );

            _logger.LogDebug(
                "Sent RefreshHand notification to player {PlayerConnectionId}: Elixir={PlayerCurrentElixir}, PlayedCard={PlayedCardId}, NextCard={NextCardId}",
                player.ConnectionId,
                player.CurrentElixir,
                cardToPut?.Id,
                player.GetNextCard()?.Id
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error enviando RefreshHand a {ConnectionId}",
                player.ConnectionId
            );
        }
    }
}
