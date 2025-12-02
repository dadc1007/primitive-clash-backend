using FluentAssertions;
using Moq;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;
using System.Text.Json;

namespace PrimitiveClash.Backend.Tests.Services;

public class GameServiceExtendedTests
{
    private readonly Mock<IPlayerStateService> _mockPlayerStateService;
    private readonly Mock<ITowerService> _mockTowerService;
    private readonly Mock<IArenaService> _mockArenaService;
    private readonly Mock<IGameLoopService> _mockGameLoopService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IDatabase> _mockRedis;
    private readonly GameService _gameService;

    public GameServiceExtendedTests()
    {
        _mockPlayerStateService = new Mock<IPlayerStateService>();
        _mockTowerService = new Mock<ITowerService>();
        _mockArenaService = new Mock<IArenaService>();
        _mockGameLoopService = new Mock<IGameLoopService>();
        _mockNotificationService = new Mock<INotificationService>();
        _mockRedis = new Mock<IDatabase>();

        _gameService = new GameService(
            _mockPlayerStateService.Object,
            _mockTowerService.Object,
            _mockArenaService.Object,
            _mockGameLoopService.Object,
            _mockNotificationService.Object,
            _mockRedis.Object
        );
    }

    [Fact]
    public async Task CreateNewGame_WithInvalidPlayerCount_ThrowsException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userIds = new List<Guid> { Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidPlayersNumberException>(
            async () => await _gameService.CreateNewGame(sessionId, userIds)
        );
    }

    [Fact]
    public async Task GetGame_WhenGameDoesNotExist_ThrowsGameNotFoundException()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        _mockRedis.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act & Assert
        await Assert.ThrowsAsync<GameNotFoundException>(
            async () => await _gameService.GetGame(gameId)
        );
    }

    [Fact]
    public async Task UpdatePlayerConnectionStatus_WhenGameNotFound_ThrowsException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _mockRedis.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act & Assert
        await Assert.ThrowsAsync<GameNotFoundException>(
            async () => await _gameService.UpdatePlayerConnectionStatus(
                sessionId, userId, "conn1", true
            )
        );
    }

    [Fact]
    public async Task UpdatePlayerConnectionStatus_WithConcurrencyFailure_ThrowsConcurrencyException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var player1 = new PlayerState(userId, "user1", []);
        var player2 = new PlayerState(Guid.NewGuid(), "user2", []);
        var arena = TestHelpers.CreateTestArena();
        var game = new Game(sessionId, [player1, player2], arena);

        var gameJson = JsonSerializer.Serialize(game);
        _mockRedis.Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        var mockTransaction = new Mock<ITransaction>();
        mockTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        _mockRedis.Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(mockTransaction.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConcurrencyException>(
            async () => await _gameService.UpdatePlayerConnectionStatus(
                sessionId, userId, "conn1", true
            )
        );
    }

    [Fact]
    public async Task UpdateElixir_WithPlayersAtMaxElixir_DoesNotNotify()
    {
        // Arrange
        var player1 = new PlayerState(Guid.NewGuid(), "user1", []);
        player1.CurrentElixir = Game.MaxElixir;
        player1.IsConnected = true;
        player1.ConnectionId = "conn1";

        var arena = TestHelpers.CreateTestArena();
        var game = new Game(Guid.NewGuid(), [player1], arena);

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir(It.IsAny<string>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateElixir_WithDisconnectedPlayer_DoesNotNotify()
    {
        // Arrange
        var player = new PlayerState(Guid.NewGuid(), "user1", []);
        player.CurrentElixir = 5m;
        player.IsConnected = false;
        player.ConnectionId = "conn1";

        var arena = TestHelpers.CreateTestArena();
        var game = new Game(Guid.NewGuid(), [player], arena);

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir(It.IsAny<string>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateElixir_WithNullConnectionId_DoesNotNotify()
    {
        // Arrange
        var player = new PlayerState(Guid.NewGuid(), "user1", []);
        player.CurrentElixir = 5m;
        player.IsConnected = true;
        player.ConnectionId = null;

        var arena = TestHelpers.CreateTestArena();
        var game = new Game(Guid.NewGuid(), [player], arena);

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir(It.IsAny<string>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    [Fact]
    public async Task UpdateElixir_WithConnectedPlayerBelowMax_UpdatesAndNotifies()
    {
        // Arrange
        var player = new PlayerState(Guid.NewGuid(), "user1", []);
        player.CurrentElixir = 5m;
        player.IsConnected = true;
        player.ConnectionId = "conn1";

        var arena = TestHelpers.CreateTestArena();
        var game = new Game(Guid.NewGuid(), [player], arena);

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        player.CurrentElixir.Should().Be(6m);
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir("conn1", 6m),
            Times.Once
        );
    }

    [Fact]
    public void GetPlayerState_WithNonExistentPlayer_ThrowsException()
    {
        // Arrange
        var player1 = new PlayerState(Guid.NewGuid(), "user1", []);
        var arena = TestHelpers.CreateTestArena();
        var game = new Game(Guid.NewGuid(), [player1], arena);
        var nonExistentUserId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<PlayerNotInGameException>(
            () => _gameService.GetPlayerState(game, nonExistentUserId)
        );
    }

    [Fact]
    public async Task IsUserInGame_WhenUserIsInGame_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRedis.Setup(x => x.SetContainsAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()
        )).ReturnsAsync(true);

        // Act
        var result = await _gameService.IsUserInGame(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserInGame_WhenUserIsNotInGame_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRedis.Setup(x => x.SetContainsAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            It.IsAny<CommandFlags>()
        )).ReturnsAsync(false);

        // Act
        var result = await _gameService.IsUserInGame(userId);

        // Assert
        result.Should().BeFalse();
    }
}

public static class TestHelpers
{
    public static Arena CreateTestArena()
    {
        var arenaTemplate = new ArenaTemplate 
        { 
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };
        
        var towers = new Dictionary<Guid, List<Tower>>();
        var entities = new Dictionary<Guid, List<ArenaEntity>>();
        var grid = new Cell[30][];
        for (int i = 0; i < 30; i++)
        {
            grid[i] = new Cell[18];
            for (int j = 0; j < 18; j++)
            {
                grid[i][j] = new Cell { Type = CellType.Ground };
            }
        }
        
        // Usar el constructor JSON que no inicializa el layout
        return new Arena(Guid.NewGuid(), arenaTemplate, grid, towers, entities);
    }
}
