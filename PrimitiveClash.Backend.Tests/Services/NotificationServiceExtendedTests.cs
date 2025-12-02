using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

public class NotificationServiceExtendedTests
{
    private readonly Mock<IHubContext<GameHub>> _mockGameHub;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly Mock<IClientProxy> _mockGroupProxy;
    private readonly NotificationService _notificationService;

    public NotificationServiceExtendedTests()
    {
        _mockGameHub = new Mock<IHubContext<GameHub>>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();
        _mockGroupProxy = new Mock<IClientProxy>();

        _mockGameHub.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(_mockGroupProxy.Object);
        _mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _notificationService = new NotificationService(
            _mockGameHub.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task NotifyTroopMoved_SendsNotification()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new TroopMovedNotification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            5,
            5,
            "Moving"
        );

        // Act
        await _notificationService.NotifyTroopMoved(sessionId, notification);

        // Assert
        _mockGroupProxy.Verify(
            x => x.SendCoreAsync("TroopMoved", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyUnitKilled_SendsNotification()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new UnitKilledNotificacion(Guid.NewGuid(), Guid.NewGuid());

        // Act
        await _notificationService.NotifyUnitKilled(sessionId, notification);

        // Assert
        _mockGroupProxy.Verify(
            x => x.SendCoreAsync("UnitKilled", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyEndGame_SendsNotification()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var notification = new EndGameNotification(Guid.NewGuid(), Guid.NewGuid(), 2, 1);

        // Act
        await _notificationService.NotifyEndGame(sessionId, notification);

        // Assert
        _mockGroupProxy.Verify(
            x => x.SendCoreAsync("EndGame", It.IsAny<object[]>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyNewElixir_WithValidConnectionId_SendsNotification()
    {
        // Arrange
        var connectionId = "conn123";
        var elixir = 7.5m;

        // Act
        await _notificationService.NotifyNewElixir(connectionId, elixir);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync("NewElixir", It.Is<object[]>(args => 
                args.Length == 1 && (decimal)args[0] == elixir
            ), default),
            Times.Once
        );
    }

    [Fact]
    public async Task NotifyNewElixir_WithEmptyConnectionId_LogsWarningAndDoesNotSend()
    {
        // Arrange
        var connectionId = "";
        var elixir = 7.5m;

        // Act
        await _notificationService.NotifyNewElixir(connectionId, elixir);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync("NewElixir", It.IsAny<object[]>(), default),
            Times.Never
        );
    }

    [Fact]
    public async Task NotifyNewElixir_WithNullConnectionId_LogsWarningAndDoesNotSend()
    {
        // Arrange
        string? connectionId = null;
        var elixir = 7.5m;

        // Act
        await _notificationService.NotifyNewElixir(connectionId!, elixir);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync("NewElixir", It.IsAny<object[]>(), default),
            Times.Never
        );
    }
}
