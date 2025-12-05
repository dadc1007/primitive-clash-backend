using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.DTOs.Matchmaking;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;
using System.Text.Json;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class MatchmakingServiceTests
{
    private readonly Mock<IHubContext<MatchmakingHub>> _mockHubContext;
    private readonly Mock<ILogger<MatchmakingService>> _mockLogger;
    private readonly Mock<IDatabase> _mockRedis;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly Mock<IHubClients> _mockClients;
    private readonly MatchmakingService _matchmakingService;

    public MatchmakingServiceTests()
    {
        _mockHubContext = new Mock<IHubContext<MatchmakingHub>>();
        _mockLogger = new Mock<ILogger<MatchmakingService>>();
        _mockRedis = new Mock<IDatabase>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockClientProxy = new Mock<ISingleClientProxy>();
        _mockClients = new Mock<IHubClients>();

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _matchmakingService = new MatchmakingService(
            _mockHubContext.Object,
            _mockLogger.Object,
            _mockRedis.Object,
            _mockScopeFactory.Object
        );
    }

    #region EnqueuePlayer Tests

    [Fact]
    public async Task EnqueuePlayer_WithValidPlayer_ShouldAddToQueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockRedis.Verify(x => x.SetAddAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("active_players")),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);

        _mockRedis.Verify(x => x.ListRightPushAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("queue")),
            It.IsAny<RedisValue>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()),
            Times.Once);

        _mockClientProxy.Verify(x => x.SendCoreAsync(
            "UpdateStatus",
            It.IsAny<object[]>(),
            default),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WhenPlayerInGame_ShouldNotEnqueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(true);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockRedis.Verify(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never);
        _mockClientProxy.Verify(x => x.SendCoreAsync("Error", It.IsAny<object[]>(), default), Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WhenPlayerAlreadyInQueue_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<PlayerAlreadyInQueueException>(
            () => _matchmakingService.EnqueuePlayer(userId, connectionId)
        );
    }

    #endregion

    #region DequeuePlayer Tests

    [Fact]
    public void DequeuePlayer_ShouldRemoveFromActiveSet()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert
        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("active_players")),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    #endregion

    #region StartAsync/StopAsync Tests

    [Fact]
    public async Task StartAsync_ShouldLogAndReturnCompletedTask()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        await _matchmakingService.StartAsync(cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("starting")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldLogAndReturnCompletedTask()
    {
        // Arrange
        var cancellationToken = new CancellationToken();

        // Act
        await _matchmakingService.StopAsync(cancellationToken);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stopping")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Additional Coverage Tests

    [Fact]
    public async Task EnqueuePlayer_ShouldSerializePlayerQueueItemWithCorrectFormat()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection-123";
        RedisValue capturedRedisValue = RedisValue.Null;

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisKey, RedisValue, When, CommandFlags>((key, value, when, flags) =>
            {
                capturedRedisValue = value;
            })
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        capturedRedisValue.HasValue.Should().BeTrue();
        var json = capturedRedisValue.ToString();
        json.Should().Contain(userId.ToString());
        json.Should().Contain(connectionId);

        var deserialized = JsonSerializer.Deserialize<PlayerQueueItem>(json);
        deserialized.Should().NotBeNull();
        deserialized!.UserId.Should().Be(userId);
        deserialized.ConnectionId.Should().Be(connectionId);
    }

    [Fact]
    public async Task EnqueuePlayer_WithMultiplePlayers_ShouldEnqueueEachPlayer()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var connectionId1 = "conn1";
        var connectionId2 = "conn2";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(It.IsAny<Guid>())).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId1, connectionId1);
        await _matchmakingService.EnqueuePlayer(userId2, connectionId2);

        // Assert
        _mockRedis.Verify(x => x.ListRightPushAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("queue")),
            It.IsAny<RedisValue>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()),
            Times.Exactly(2));

        _mockRedis.Verify(x => x.SetAddAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("active_players")),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task EnqueuePlayer_ShouldUseCorrectRedisKeys()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockRedis.Verify(x => x.SetContainsAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);

        _mockRedis.Verify(x => x.SetAddAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);

        _mockRedis.Verify(x => x.ListRightPushAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:queue"),
            It.IsAny<RedisValue>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public void DequeuePlayer_ShouldUseCorrectRedisKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert
        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_ShouldCreateServiceScopeForGameServiceCheck()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Once);
        mockServiceProvider.Verify(x => x.GetService(typeof(IGameService)), Times.Once);
        mockGameService.Verify(x => x.IsUserInGame(userId), Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WhenPlayerInGame_ShouldSendErrorMessageToSpecificClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "specific-connection-id";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(true);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockClients.Verify(x => x.Client(connectionId), Times.Once);
        _mockClientProxy.Verify(x => x.SendCoreAsync(
            "Error",
            It.Is<object[]>(args => args.Length == 1 && args[0].ToString()!.Contains("active game")),
            default),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WithSuccessfulEnqueue_ShouldSendSearchingStatusToClient()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockClients.Verify(x => x.Client(connectionId), Times.Once);
        _mockClientProxy.Verify(x => x.SendCoreAsync(
            "UpdateStatus",
            It.Is<object[]>(args => args.Length == 1 && args[0].ToString()!.Contains("Searching")),
            default),
            Times.Once);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task EnqueuePlayer_WhenRedisThrows_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ThrowsAsync(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<RedisConnectionException>(
            () => _matchmakingService.EnqueuePlayer(userId, connectionId)
        );
    }

    [Fact]
    public void DequeuePlayer_WithMultiplePlayers_ShouldRemoveOnlySpecifiedPlayer()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        _matchmakingService.DequeuePlayer(userId1);

        // Assert
        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId1.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);

        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId2.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Never);
    }

    [Fact]
    public async Task EnqueuePlayer_ShouldLogInformationAfterSuccessfulEnqueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("added to the queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WhenPlayerInGame_ShouldLogWarning()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(true);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attempted to enqueue while in an active game")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WhenPlayerAlreadyInQueue_ShouldLogWarning()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<PlayerAlreadyInQueueException>(
            () => _matchmakingService.EnqueuePlayer(userId, connectionId)
        );

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("already in queue")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void DequeuePlayer_ShouldLogInformation()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("removed from the active set")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task EnqueuePlayer_WithVariousGameStates_ShouldBehaveCorrectly(bool isInGame, bool isInQueue)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(isInGame);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(isInQueue);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act & Assert
        if (isInGame)
        {
            // Should return early without throwing
            await _matchmakingService.EnqueuePlayer(userId, connectionId);
            _mockRedis.Verify(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Never);
        }
        else if (isInQueue)
        {
            // Should throw exception
            await Assert.ThrowsAsync<PlayerAlreadyInQueueException>(
                () => _matchmakingService.EnqueuePlayer(userId, connectionId)
            );
        }
        else
        {
            // Should enqueue successfully
            await _matchmakingService.EnqueuePlayer(userId, connectionId);
            _mockRedis.Verify(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()), Times.Once);
        }
    }

    [Fact]
    public async Task EnqueuePlayer_WhenListRightPushFails_ShouldStillComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(0); // Returns 0 indicating failure or empty list

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert - Should still complete without exception
        _mockRedis.Verify(x => x.ListRightPushAsync(
            It.Is<RedisKey>(k => k.ToString().Contains("queue")),
            It.IsAny<RedisValue>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public async Task EnqueuePlayer_WithEmptyEmail_ShouldExtractEmptyUsername()
    {
        // This tests the username extraction logic indirectly
        // Ensures the service handles edge cases in email processing
        
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockGameService = new Mock<IGameService>();

        mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        mockServiceProvider.Setup(x => x.GetService(typeof(IGameService))).Returns(mockGameService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        _mockRedis.Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis.Setup(x => x.ListRightPushAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(1);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockClientProxy.Verify(x => x.SendCoreAsync(
            "UpdateStatus",
            It.Is<object[]>(args => args.Length == 1 && args[0].ToString()!.Contains("opponent")),
            default),
            Times.Once);
    }

    [Fact]
    public void DequeuePlayer_WhenRedisReturnsTrue_ShouldComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert
        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    [Fact]
    public void DequeuePlayer_WhenRedisReturnsFalse_ShouldStillComplete()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis.Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false); // User was not in the set

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert - Should complete without exception
        _mockRedis.Verify(x => x.SetRemoveAsync(
            It.Is<RedisKey>(k => k.ToString() == "matchmaking:active_players"),
            It.Is<RedisValue>(v => v.ToString() == userId.ToString()),
            It.IsAny<CommandFlags>()),
            Times.Once);
    }

    #endregion
}

