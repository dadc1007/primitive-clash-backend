using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Impl;

namespace PrimitiveClash.Backend.Tests.Services;

public class NotificationServiceBranchTests
{
    private readonly Mock<IHubContext<GameHub>> _mockHubContext;
    private readonly Mock<ILogger<NotificationService>> _mockLogger;
    private readonly NotificationService _notificationService;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<IClientProxy> _mockClientProxy;
    private readonly Mock<ISingleClientProxy> _mockSingleClientProxy;

    public NotificationServiceBranchTests()
    {
        _mockHubContext = new Mock<IHubContext<GameHub>>();
        _mockLogger = new Mock<ILogger<NotificationService>>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<IClientProxy>();
        _mockSingleClientProxy = new Mock<ISingleClientProxy>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        _mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(_mockSingleClientProxy.Object);

        _notificationService = new NotificationService(_mockHubContext.Object, _mockLogger.Object);
    }

    #region NotifyNewElixir Tests

    [Fact]
    public async Task NotifyNewElixir_WithNullConnectionId_ShouldLogWarningAndNotSendNotification()
    {
        // Arrange
        string? nullConnectionId = null;
        decimal elixir = 5.5m;

        // Act
        await _notificationService.NotifyNewElixir(nullConnectionId!, elixir);

        // Assert
        _mockSingleClientProxy.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never,
            "Should not send notification with null connectionId"
        );

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("playerConnectionId es null o vacío")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log warning when connectionId is null"
        );
    }

    [Fact]
    public async Task NotifyNewElixir_WithEmptyConnectionId_ShouldLogWarningAndNotSendNotification()
    {
        // Arrange
        string emptyConnectionId = string.Empty;
        decimal elixir = 7.0m;

        // Act
        await _notificationService.NotifyNewElixir(emptyConnectionId, elixir);

        // Assert
        _mockSingleClientProxy.Verify(
            x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]>(), default),
            Times.Never,
            "Should not send notification with empty connectionId"
        );

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("playerConnectionId es null o vacío")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "Should log warning when connectionId is empty"
        );
    }

    [Fact]
    public async Task NotifyNewElixir_WithValidConnectionId_ShouldSendNotification()
    {
        // Arrange
        string validConnectionId = "valid-connection-123";
        decimal elixir = 8.5m;

        // Act
        await _notificationService.NotifyNewElixir(validConnectionId, elixir);

        // Assert
        _mockClients.Verify(
            x => x.Client(validConnectionId),
            Times.Once,
            "Should send notification to specific client"
        );

        _mockSingleClientProxy.Verify(
            x => x.SendCoreAsync(
                "NewElixir",
                It.Is<object[]>(args => args.Length == 1 && (decimal)args[0] == elixir),
                default),
            Times.Once,
            "Should send NewElixir notification with correct elixir value"
        );
    }

    #endregion
}
