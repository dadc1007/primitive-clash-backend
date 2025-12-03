using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ISingleClientProxy> _mockSingleClientProxy;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockSingleClientProxy = new Mock<ISingleClientProxy>();
        _mockLogger = new Mock<ILogger<NotificationService>>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        
        // Setup for Group calls
        _mockClients.Setup(x => x.Group(It.IsAny<string>()))
            .Returns(_mockClientProxy.Object);
        
        // Setup for Client calls (single user)
        _mockClients.Setup(x => x.Client(It.IsAny<string>()))
            .Returns(_mockSingleClientProxy.Object);

        _service = new NotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    #region NotifyCardSpawned Tests

    [Fact]
    public async Task NotifyCardSpawned_SendsNotificationToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var connectionId = "test-connection-id";

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId,
            CardId = card.Id,
            Card = card,
            Level = 1
        };

        var nextCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId,
            CardId = Guid.NewGuid(),
            Card = card,
            Level = 1
        };

        var playerState = new PlayerState(playerId, [playerCard, nextCard])
        {
            ConnectionId = connectionId,
            CurrentElixir = 7.5m
        };

        var entity = new TroopEntity(playerId, playerCard, 5, 10);

        // Act
        await _service.NotifyCardSpawned(sessionId, playerState, entity, playerCard);

        // Assert - CardSpawned sent to group
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "CardSpawned",
                It.Is<object[]>(args => args.Length == 1 && args[0] is CardSpawnedNotification),
                default),
            Times.Once);
        
        // RefreshHand is called via Client (verified by checking Client was called)
        _mockClients.Verify(x => x.Client(connectionId), Times.Once);
    }

    #endregion

    #region NotifyTroopMoved Tests

    [Fact]
    public async Task NotifyTroopMoved_SendsNotificationToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new TroopMovedNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            10,
            "Moving"
        );

        // Act
        await _service.NotifyTroopMoved(sessionId, notification);

        // Assert
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "TroopMoved",
                It.Is<object[]>(args => args.Length == 1 && args[0] is TroopMovedNotification),
                default),
            Times.Once);
    }

    #endregion

    #region NotifyUnitDamaged Tests

    [Fact]
    public async Task NotifyUnitDamaged_SendsNotificationToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new UnitDamagedNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100,
            500,
            600
        );

        // Act
        await _service.NotifyUnitDamaged(sessionId, notification);

        // Assert
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "UnitDamaged",
                It.Is<object[]>(args => args.Length == 1 && args[0] is UnitDamagedNotification),
                default),
            Times.Once);
    }

    #endregion

    #region NotifyUnitKilled Tests

    [Fact]
    public async Task NotifyUnitKilled_SendsNotificationToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new UnitKilledNotificacion(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _service.NotifyUnitKilled(sessionId, notification);

        // Assert
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "UnitKilled",
                It.Is<object[]>(args => args.Length == 1 && args[0] is UnitKilledNotificacion),
                default),
            Times.Once);
    }

    #endregion

    #region NotifyEndGame Tests

    [Fact]
    public async Task NotifyEndGame_SendsNotificationToGroup()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new EndGameNotification(Guid.NewGuid(), Guid.NewGuid(), 3, 1);

        // Act
        await _service.NotifyEndGame(sessionId, notification);

        // Assert
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "EndGame",
                It.Is<object[]>(args => args.Length == 1 && args[0] is EndGameNotification),
                default),
            Times.Once);
    }

    #endregion

    #region NotifyNewElixir Tests

    [Fact]
    public async Task NotifyNewElixir_WithValidConnectionId_SendsNotificationToClient()
    {
        // Arrange
        var connectionId = "player-connection-123";
        var elixir = 8.5m;

        // Act
        await _service.NotifyNewElixir(connectionId, elixir);

        // Assert
        _mockClients.Verify(x => x.Client(connectionId), Times.Once);
    }

    [Fact]
    public async Task NotifyNewElixir_WithNullConnectionId_DoesNotSendNotification()
    {
        // Arrange
        string? connectionId = null;
        var elixir = 8.5m;

        // Act
        await _service.NotifyNewElixir(connectionId!, elixir);

        // Assert
        _mockClients.Verify(x => x.Client(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NotifyNewElixir_WithEmptyConnectionId_DoesNotSendNotification()
    {
        // Arrange
        var connectionId = string.Empty;
        var elixir = 8.5m;

        // Act
        await _service.NotifyNewElixir(connectionId, elixir);

        // Assert
        _mockClients.Verify(x => x.Client(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region NotifyRefreshHand Tests (via NotifyCardSpawned)

    [Fact]
    public async Task NotifyCardSpawned_WithNullConnectionId_SkipsRefreshHandNotification()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId,
            CardId = card.Id,
            Card = card,
            Level = 1
        };

        var playerState = new PlayerState(playerId, [playerCard])
        {
            ConnectionId = null, // No connection
            CurrentElixir = 7.5m
        };

        var entity = new TroopEntity(playerId, playerCard, 5, 10);

        // Act
        await _service.NotifyCardSpawned(sessionId, playerState, entity, playerCard);

        // Assert - CardSpawned should still be sent to group
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "CardSpawned",
                It.IsAny<object[]>(),
                default),
            Times.Once);
        
        // But RefreshHand should NOT be sent because ConnectionId is null
        _mockClients.Verify(x => x.Client(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task NotifyCardSpawned_WithException_LogsErrorButDoesNotThrow()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var connectionId = "test-connection-id";

        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 600,
            Range = 1,
            HitSpeed = 1.2f,
            MovementSpeed = MovementSpeed.Medium,
            ImageUrl = "knight.png"
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId,
            CardId = card.Id,
            Card = card,
            Level = 1
        };

        var playerState = new PlayerState(playerId, [playerCard])
        {
            ConnectionId = connectionId,
            CurrentElixir = 7.5m
        };

        var entity = new TroopEntity(playerId, playerCard, 5, 10);

        // Setup Client to throw exception when sending RefreshHand
        _mockSingleClientProxy
            .Setup(x => x.SendCoreAsync(
                "RefreshHand",
                It.IsAny<object[]>(),
                default))
            .ThrowsAsync(new Exception("Connection lost"));

        // Act
        var act = async () => await _service.NotifyCardSpawned(sessionId, playerState, entity, playerCard);

        // Assert - Should not throw (exception is caught and logged)
        await act.Should().NotThrowAsync();
        
        // CardSpawned should still have been sent
        _mockClients.Verify(x => x.Group(sessionId.ToString()), Times.Once);
    }

    #endregion
}
