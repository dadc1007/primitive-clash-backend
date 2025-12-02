using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Hubs;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;

namespace PrimitiveClash.Backend.Tests.Services;

public class MatchmakingServiceExtendedTests
{
    private readonly Mock<IHubContext<MatchmakingHub>> _mockHubContext;
    private readonly Mock<ILogger<MatchmakingService>> _mockLogger;
    private readonly Mock<IDatabase> _mockRedis;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IGameService> _mockGameService;
    private readonly Mock<IHubClients> _mockClients;
    private readonly Mock<ISingleClientProxy> _mockClientProxy;
    private readonly MatchmakingService _matchmakingService;

    public MatchmakingServiceExtendedTests()
    {
        _mockHubContext = new Mock<IHubContext<MatchmakingHub>>();
        _mockLogger = new Mock<ILogger<MatchmakingService>>();
        _mockRedis = new Mock<IDatabase>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockGameService = new Mock<IGameService>();
        _mockClients = new Mock<IHubClients>();
        _mockClientProxy = new Mock<ISingleClientProxy>();

        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IGameService)))
            .Returns(_mockGameService.Object);

        _mockHubContext.Setup(x => x.Clients).Returns(_mockClients.Object);
        _mockClients.Setup(x => x.Client(It.IsAny<string>())).Returns(_mockClientProxy.Object);

        _matchmakingService = new MatchmakingService(
            _mockHubContext.Object,
            _mockLogger.Object,
            _mockRedis.Object,
            _mockScopeFactory.Object
        );
    }

    [Fact]
    public async Task EnqueuePlayer_WhenUserIsInActiveGame_SendsErrorAndDoesNotEnqueue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "conn123";
        _mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(true);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "Error",
                It.IsAny<object[]>(),
                default
            ),
            Times.Once
        );

        _mockRedis.Verify(
            x => x.ListRightPushAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ),
            Times.Never
        );
    }

    [Fact]
    public async Task EnqueuePlayer_WhenUserAlreadyInQueue_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "conn123";
        _mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        _mockRedis.Setup(x => x.SetContainsAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()
        )).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<PlayerAlreadyInQueueException>(
            async () => await _matchmakingService.EnqueuePlayer(userId, connectionId)
        );
    }

    [Fact]
    public async Task EnqueuePlayer_WithValidUser_AddsToQueueAndSendsUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var connectionId = "conn123";
        _mockGameService.Setup(x => x.IsUserInGame(userId)).ReturnsAsync(false);
        _mockRedis.Setup(x => x.SetContainsAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()
        )).ReturnsAsync(false);

        // Act
        await _matchmakingService.EnqueuePlayer(userId, connectionId);

        // Assert
        _mockRedis.Verify(
            x => x.SetAddAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()
            ),
            Times.Once
        );

        _mockRedis.Verify(
            x => x.ListRightPushAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ),
            Times.Once
        );

        _mockClientProxy.Verify(
            x => x.SendCoreAsync(
                "UpdateStatus",
                It.IsAny<object[]>(),
                default
            ),
            Times.Once
        );
    }

    [Fact]
    public void DequeuePlayer_RemovesPlayerFromActiveSet()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        _matchmakingService.DequeuePlayer(userId);

        // Assert
        _mockRedis.Verify(
            x => x.SetRemoveAsync(
                It.IsAny<RedisKey>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()
            ),
            Times.Once
        );
    }
}
