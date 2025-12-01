using FluentAssertions;
using Moq;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using StackExchange.Redis;
using System.Text.Json;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class GameServiceTests
{
    private readonly Mock<IPlayerStateService> _mockPlayerStateService;
    private readonly Mock<ITowerService> _mockTowerService;
    private readonly Mock<IArenaService> _mockArenaService;
    private readonly Mock<IGameLoopService> _mockGameLoopService;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<IDatabase> _mockRedis;
    private readonly GameService _gameService;

    public GameServiceTests()
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

    private Arena CreateTestArena()
    {
        var arenaTemplate = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };

        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        var leaderTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3,
            Size = 3
        };

        var player1Towers = new List<Tower>
        {
            new Tower(player1Id, leaderTemplate),
            new Tower(player1Id, guardianTemplate),
            new Tower(player1Id, guardianTemplate)
        };

        var player2Towers = new List<Tower>
        {
            new Tower(player2Id, leaderTemplate),
            new Tower(player2Id, guardianTemplate),
            new Tower(player2Id, guardianTemplate)
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            { player1Id, player1Towers },
            { player2Id, player2Towers }
        };

        return new Arena(arenaTemplate, towers);
    }

    #region CreateNewGame Tests

    [Fact]
    public async Task CreateNewGame_WithTwoPlayers_ShouldCreateGame()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var userIds = new List<Guid> { user1Id, user2Id };

        var playerState1 = new PlayerState(user1Id, new List<PlayerCard>());
        var playerState2 = new PlayerState(user2Id, new List<PlayerCard>());
        var arena = CreateTestArena();
        var towers = new Dictionary<Guid, List<Tower>>();

        _mockPlayerStateService
            .Setup(x => x.CreatePlayerState(user1Id))
            .ReturnsAsync(playerState1);

        _mockPlayerStateService
            .Setup(x => x.CreatePlayerState(user2Id))
            .ReturnsAsync(playerState2);

        _mockTowerService
            .Setup(x => x.CreateAllGameTowers(user1Id, user2Id))
            .ReturnsAsync(towers);

        _mockArenaService
            .Setup(x => x.CreateArena(towers))
            .ReturnsAsync(arena);

        _mockRedis
            .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis
            .Setup(x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _gameService.CreateNewGame(sessionId, userIds);

        // Assert
        _mockPlayerStateService.Verify(x => x.CreatePlayerState(user1Id), Times.Once);
        _mockPlayerStateService.Verify(x => x.CreatePlayerState(user2Id), Times.Once);
        _mockTowerService.Verify(x => x.CreateAllGameTowers(user1Id, user2Id), Times.Once);
        _mockArenaService.Verify(x => x.CreateArena(towers), Times.Once);
        _mockRedis.Verify(
            x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()),
            Times.Once
        );
        _mockRedis.Verify(
            x => x.SetAddAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()),
            Times.Exactly(2)
        );
    }

    [Fact]
    public async Task CreateNewGame_WithInvalidPlayerCount_ShouldThrowException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userIds = new List<Guid> { Guid.NewGuid() }; // Only 1 player

        // Act & Assert
        await Assert.ThrowsAsync<InvalidPlayersNumberException>(
            () => _gameService.CreateNewGame(sessionId, userIds)
        );
    }

    [Fact]
    public async Task CreateNewGame_WithThreePlayers_ShouldThrowException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidPlayersNumberException>(
            () => _gameService.CreateNewGame(sessionId, userIds)
        );
    }

    #endregion

    #region GetGame Tests

    [Fact]
    public async Task GetGame_WithExistingGame_ShouldReturnGame()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var playerStates = new List<PlayerState>
        {
            new PlayerState(user1Id, new List<PlayerCard>()),
            new PlayerState(user2Id, new List<PlayerCard>())
        };
        var game = new Game(sessionId, playerStates, CreateTestArena());
        var gameJson = JsonSerializer.Serialize(game);

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        // Act
        var result = await _gameService.GetGame(sessionId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(sessionId);
        result.PlayerStates.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGame_WithNonExistentGame_ShouldThrowException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(RedisValue.Null);

        // Act & Assert
        await Assert.ThrowsAsync<GameNotFoundException>(
            () => _gameService.GetGame(sessionId)
        );
    }

    #endregion

    #region SaveGame Tests

    [Fact]
    public async Task SaveGame_ShouldSerializeAndStoreInRedis()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var playerStates = new List<PlayerState>
        {
            new PlayerState(user1Id, new List<PlayerCard>()),
            new PlayerState(user2Id, new List<PlayerCard>())
        };
        var game = new Game(sessionId, playerStates, CreateTestArena());

        _mockRedis
            .Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        await _gameService.SaveGame(game);

        // Assert
        _mockRedis.Verify(
            x => x.StringSetAsync(
                It.Is<RedisKey>(k => k.ToString().Contains(sessionId.ToString())),
                It.IsAny<RedisValue>(),
                TimeSpan.FromMinutes(15),
                It.IsAny<bool>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()
            ),
            Times.Once
        );
    }

    #endregion

    #region EndGame Tests

    [Fact]
    public async Task EndGame_ShouldCleanupAndNotify()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var winnerId = Guid.NewGuid();
        var loserId = Guid.NewGuid();
        var playerStates = new List<PlayerState>
        {
            new PlayerState(winnerId, new List<PlayerCard>()),
            new PlayerState(loserId, new List<PlayerCard>())
        };
        var arena = CreateTestArena();
        var game = new Game(sessionId, playerStates, arena);
        var gameJson = JsonSerializer.Serialize(game);

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        _mockRedis
            .Setup(x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockRedis
            .Setup(x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        _mockArenaService
            .Setup(x => x.GetNumberTowers(arena, winnerId, loserId))
            .Returns((2, 1));

        _mockNotificationService
            .Setup(x => x.NotifyEndGame(It.IsAny<Guid>(), It.IsAny<EndGameNotification>()))
            .Returns(Task.CompletedTask);

        // Act
        await _gameService.EndGame(sessionId, arena, winnerId, loserId);

        // Assert
        _mockRedis.Verify(
            x => x.KeyDeleteAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()),
            Times.Once
        );
        _mockRedis.Verify(
            x => x.SetRemoveAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()),
            Times.Exactly(2)
        );
        _mockGameLoopService.Verify(x => x.StopGameLoop(sessionId), Times.Once);
        _mockNotificationService.Verify(
            x => x.NotifyEndGame(sessionId, It.Is<EndGameNotification>(n => n.WinnerId == winnerId && n.LosserId == loserId)),
            Times.Once
        );
    }

    #endregion

    #region IsUserInGame Tests

    [Fact]
    public async Task IsUserInGame_WithUserInGame_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis
            .Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(true);

        // Act
        var result = await _gameService.IsUserInGame(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsUserInGame_WithUserNotInGame_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRedis
            .Setup(x => x.SetContainsAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(false);

        // Act
        var result = await _gameService.IsUserInGame(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateElixir Tests

    [Fact]
    public async Task UpdateElixir_WithPlayersBelowMax_ShouldIncreaseElixir()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var playerState1 = new PlayerState(user1Id, new List<PlayerCard>())
        {
            CurrentElixir = 3.0m,
            ConnectionId = "conn1"
        };
        var playerState2 = new PlayerState(user2Id, new List<PlayerCard>())
        {
            CurrentElixir = 9.5m,
            ConnectionId = "conn2"
        };

        var game = new Game(
            Guid.NewGuid(),
            new List<PlayerState> { playerState1, playerState2 },
            CreateTestArena()
        );

        _mockNotificationService
            .Setup(x => x.NotifyNewElixir(It.IsAny<string>(), It.IsAny<decimal>()))
            .Returns(Task.CompletedTask);

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        playerState1.CurrentElixir.Should().Be(3.5m);
        playerState2.CurrentElixir.Should().Be(10.0m); // Capped at max
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir("conn1", 3.5m),
            Times.Once
        );
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir("conn2", 10.0m),
            Times.Once
        );
    }

    [Fact]
    public async Task UpdateElixir_WithPlayersAtMax_ShouldNotChange()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var playerState1 = new PlayerState(user1Id, new List<PlayerCard>())
        {
            CurrentElixir = 10.0m,
            ConnectionId = "conn1"
        };

        var game = new Game(
            Guid.NewGuid(),
            new List<PlayerState> { playerState1 },
            CreateTestArena()
        );

        // Act
        await _gameService.UpdateElixir(game);

        // Assert
        playerState1.CurrentElixir.Should().Be(10.0m);
        _mockNotificationService.Verify(
            x => x.NotifyNewElixir(It.IsAny<string>(), It.IsAny<decimal>()),
            Times.Never
        );
    }

    #endregion

    #region GetPlayerState Tests

    [Fact]
    public void GetPlayerState_WithValidUserId_ShouldReturnPlayerState()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerState = new PlayerState(userId, new List<PlayerCard>());
        var game = new Game(
            Guid.NewGuid(),
            new List<PlayerState> { playerState },
            CreateTestArena()
        );

        // Act
        var result = _gameService.GetPlayerState(game, userId);

        // Assert
        result.Should().Be(playerState);
        result.Id.Should().Be(userId);
    }

    [Fact]
    public void GetPlayerState_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var playerState = new PlayerState(userId, new List<PlayerCard>());
        var game = new Game(
            Guid.NewGuid(),
            new List<PlayerState> { playerState },
            CreateTestArena()
        );

        // Act & Assert
        Assert.Throws<PlayerNotInGameException>(
            () => _gameService.GetPlayerState(game, otherUserId)
        );
    }

    #endregion

    #region UpdatePlayerConnectionStatus Tests

    [Fact]
    public async Task UpdatePlayerConnectionStatus_ShouldUpdateStatus()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "test-connection";
        var playerState = new PlayerState(userId, new List<PlayerCard>())
        {
            IsConnected = false,
            ConnectionId = null
        };
        var game = new Game(sessionId, new List<PlayerState> { playerState }, CreateTestArena());
        var gameJson = JsonSerializer.Serialize(game);

        var mockTransaction = new Mock<ITransaction>();
        mockTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(true);
        mockTransaction.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        _mockRedis
            .Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(mockTransaction.Object);

        // Act
        var result = await _gameService.UpdatePlayerConnectionStatus(sessionId, userId, connectionId, true);

        // Assert
        result.Should().NotBeNull();
        var updatedPlayer = result.PlayerStates.First(p => p.Id == userId);
        updatedPlayer.IsConnected.Should().BeTrue();
        updatedPlayer.ConnectionId.Should().Be(connectionId);
    }

    [Fact]
    public async Task UpdatePlayerConnectionStatus_WithConcurrency_ShouldRetryAndSucceed()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "test-connection-retry";
        var playerState = new PlayerState(userId, new List<PlayerCard>())
        {
            IsConnected = false,
            ConnectionId = null
        };
        var game = new Game(sessionId, new List<PlayerState> { playerState }, CreateTestArena());
        var gameJson = JsonSerializer.Serialize(game);

        // First transaction fails, second succeeds
        var mockFailedTransaction = new Mock<ITransaction>();
        mockFailedTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(false);
        mockFailedTransaction.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        var mockSuccessTransaction = new Mock<ITransaction>();
        mockSuccessTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(true);
        mockSuccessTransaction.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        _mockRedis
            .SetupSequence(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(mockFailedTransaction.Object)
            .Returns(mockSuccessTransaction.Object);

        // Act
        var result = await _gameService.UpdatePlayerConnectionStatus(sessionId, userId, connectionId, true);

        // Assert
        result.Should().NotBeNull();
        var updatedPlayer = result.PlayerStates.First(p => p.Id == userId);
        updatedPlayer.IsConnected.Should().BeTrue();
        updatedPlayer.ConnectionId.Should().Be(connectionId);
        
        // Verify it created transaction twice (first failed, second succeeded)
        _mockRedis.Verify(x => x.CreateTransaction(It.IsAny<object>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdatePlayerConnectionStatus_WithMaxRetriesExceeded_ShouldThrowConcurrencyException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "test-connection-fail";
        var playerState = new PlayerState(userId, new List<PlayerCard>())
        {
            IsConnected = false,
            ConnectionId = null
        };
        var game = new Game(sessionId, new List<PlayerState> { playerState }, CreateTestArena());
        var gameJson = JsonSerializer.Serialize(game);

        var mockTransaction = new Mock<ITransaction>();
        mockTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(false); // Always fails
        mockTransaction.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        _mockRedis
            .Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(mockTransaction.Object);

        // Act
        Func<Task> act = async () => await _gameService.UpdatePlayerConnectionStatus(sessionId, userId, connectionId, true);

        // Assert
        await act.Should().ThrowAsync<ConcurrencyException>()
            .WithMessage("*after 3 retries*");
        
        _mockRedis.Verify(x => x.CreateTransaction(It.IsAny<object>()), Times.Exactly(3), "Should attempt exactly 3 times before throwing");
    }

    [Fact]
    public async Task UpdatePlayerConnectionStatus_WithNonExistentPlayer_ShouldReturnGameUnchanged()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingUserId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();
        var connectionId = "test-connection";
        var playerState = new PlayerState(existingUserId, new List<PlayerCard>())
        {
            IsConnected = false,
            ConnectionId = null
        };
        var game = new Game(sessionId, new List<PlayerState> { playerState }, CreateTestArena());
        var gameJson = JsonSerializer.Serialize(game);

        var mockTransaction = new Mock<ITransaction>();
        mockTransaction.Setup(x => x.ExecuteAsync(It.IsAny<CommandFlags>())).ReturnsAsync(true);
        mockTransaction.Setup(x => x.StringSetAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
            .Returns(Task.FromResult(true));

        _mockRedis
            .Setup(x => x.StringGetAsync(It.IsAny<RedisKey>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync((RedisValue)gameJson);

        _mockRedis
            .Setup(x => x.CreateTransaction(It.IsAny<object>()))
            .Returns(mockTransaction.Object);

        // Act
        var result = await _gameService.UpdatePlayerConnectionStatus(sessionId, nonExistentUserId, connectionId, true);

        // Assert
        result.Should().NotBeNull();
        var existingPlayer = result.PlayerStates.First(p => p.Id == existingUserId);
        existingPlayer.IsConnected.Should().BeFalse("Non-existent player should not affect existing players");
        existingPlayer.ConnectionId.Should().BeNull("Non-existent player should not affect existing players");
    }

    #endregion
}
