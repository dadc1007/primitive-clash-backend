using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PrimitiveClash.Backend.Configuration;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class GameLoopServiceTests
{
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<IGameService> _mockGameService;
    private readonly Mock<IBehaviorService> _mockBehaviorService;
    private readonly Mock<IArenaService> _mockArenaService;
    private readonly Mock<ILogger<GameLoopService>> _mockLogger;
    private readonly GameLoopService _service;

    public GameLoopServiceTests()
    {
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockGameService = new Mock<IGameService>();
        _mockBehaviorService = new Mock<IBehaviorService>();
        _mockArenaService = new Mock<IArenaService>();
        _mockLogger = new Mock<ILogger<GameLoopService>>();

        // Setup service scope chain
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IGameService)))
            .Returns(_mockGameService.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IBehaviorService)))
            .Returns(_mockBehaviorService.Object);
        _mockServiceProvider
            .Setup(x => x.GetService(typeof(IArenaService)))
            .Returns(_mockArenaService.Object);

        _service = new GameLoopService(_mockScopeFactory.Object, _mockLogger.Object);
    }

    #region StartGameLoop Tests

    [Fact]
    public async Task StartGameLoop_AddsSessionToActiveSessions()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        _service.StartGameLoop(sessionId);

        // Assert - Verify it was started by checking ProcessTick creates scope
        await _service.ProcessTick();
        
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task StartGameLoop_WithMultipleSessions_AddsAllSessions()
    {
        // Arrange
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();

        // Act
        _service.StartGameLoop(sessionId1);
        _service.StartGameLoop(sessionId2);

        // Assert
        await _service.ProcessTick();
        
        // Should create 2 scopes (one per session)
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Exactly(2));
    }

    #endregion

    #region StopGameLoop Tests

    [Fact]
    public async Task StopGameLoop_RemovesSessionFromActiveSessions()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _service.StartGameLoop(sessionId);

        // Act
        _service.StopGameLoop(sessionId);

        // Assert - ProcessTick should not create any scopes
        await _service.ProcessTick();
        
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
    }

    #endregion

    #region ProcessTick Tests

    [Fact]
    public async Task ProcessTick_WithNoActiveSessions_DoesNothing()
    {
        // Act
        await _service.ProcessTick();

        // Assert
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
    }

    [Fact]
    public async Task ProcessTick_WithActiveSession_ProcessesGameState()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var game = CreateTestGame(sessionId, playerId1, playerId2);
        var entities = new List<ArenaEntity>();
        var towers = new List<Tower>();

        _mockGameService.Setup(x => x.GetGame(sessionId)).ReturnsAsync(game);
        _mockArenaService.Setup(x => x.GetEntities(game.GameArena)).Returns(entities);
        _mockArenaService.Setup(x => x.GetTowers(game.GameArena)).Returns(towers);
        _mockGameService.Setup(x => x.UpdateElixir(game)).Returns(Task.CompletedTask);
        _mockGameService.Setup(x => x.SaveGame(game)).Returns(Task.CompletedTask);

        _service.StartGameLoop(sessionId);

        // Act
        await _service.ProcessTick();

        // Assert
        _mockGameService.Verify(x => x.GetGame(sessionId), Times.Once);
        _mockArenaService.Verify(x => x.GetEntities(game.GameArena), Times.Once);
        _mockArenaService.Verify(x => x.GetTowers(game.GameArena), Times.Once);
        _mockGameService.Verify(x => x.UpdateElixir(game), Times.Once);
        _mockGameService.Verify(x => x.SaveGame(game), Times.Once);
    }

    [Fact]
    public async Task ProcessTick_WithEntities_ExecutesActionForEachEntity()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var game = CreateTestGame(sessionId, playerId1, playerId2);
        
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
            UserId = playerId1,
            CardId = card.Id,
            Card = card,
            Level = 1
        };

        var entity1 = new TroopEntity(playerId1, playerCard, 5, 10);
        var entity2 = new TroopEntity(playerId1, playerCard, 8, 12);
        
        var entities = new List<ArenaEntity> { entity1, entity2 };
        var towers = new List<Tower>();

        _mockGameService.Setup(x => x.GetGame(sessionId)).ReturnsAsync(game);
        _mockArenaService.Setup(x => x.GetEntities(game.GameArena)).Returns(entities);
        _mockArenaService.Setup(x => x.GetTowers(game.GameArena)).Returns(towers);
        _mockGameService.Setup(x => x.UpdateElixir(game)).Returns(Task.CompletedTask);
        _mockGameService.Setup(x => x.SaveGame(game)).Returns(Task.CompletedTask);

        _service.StartGameLoop(sessionId);

        // Act
        await _service.ProcessTick();

        // Assert
        _mockBehaviorService.Verify(
            x => x.ExecuteAction(sessionId, game.GameArena, It.IsAny<ArenaEntity>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessTick_WithTowers_ExecutesActionForEachTower()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var game = CreateTestGame(sessionId, playerId1, playerId2);
        
        var entities = new List<ArenaEntity>();
        var towers = game.GameArena.Towers.Values.SelectMany(t => t).ToList();

        _mockGameService.Setup(x => x.GetGame(sessionId)).ReturnsAsync(game);
        _mockArenaService.Setup(x => x.GetEntities(game.GameArena)).Returns(entities);
        _mockArenaService.Setup(x => x.GetTowers(game.GameArena)).Returns(towers);
        _mockGameService.Setup(x => x.UpdateElixir(game)).Returns(Task.CompletedTask);
        _mockGameService.Setup(x => x.SaveGame(game)).Returns(Task.CompletedTask);

        _service.StartGameLoop(sessionId);

        // Act
        await _service.ProcessTick();

        // Assert - Should execute for all 6 towers (3 per player)
        _mockBehaviorService.Verify(
            x => x.ExecuteAction(sessionId, game.GameArena, It.IsAny<Tower>()),
            Times.Exactly(6));
    }

    [Fact]
    public async Task ProcessTick_WhenExceptionOccurs_StopsGameLoop()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        _mockGameService
            .Setup(x => x.GetGame(sessionId))
            .ThrowsAsync(new Exception("Game not found"));

        _service.StartGameLoop(sessionId);

        // Act
        await _service.ProcessTick();

        // Assert - Session should be removed after exception
        await _service.ProcessTick(); // Second call should do nothing
        
        // Only one scope created (from first ProcessTick that failed)
        _mockScopeFactory.Verify(x => x.CreateScope(), Times.Once);
    }

    [Fact]
    public async Task ProcessTick_WithEntityException_ContinuesProcessing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var game = CreateTestGame(sessionId, playerId1, playerId2);
        
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
            UserId = playerId1,
            CardId = card.Id,
            Card = card,
            Level = 1
        };

        var entity = new TroopEntity(playerId1, playerCard, 5, 10);
        var entities = new List<ArenaEntity> { entity };
        var towers = new List<Tower>();

        _mockGameService.Setup(x => x.GetGame(sessionId)).ReturnsAsync(game);
        _mockArenaService.Setup(x => x.GetEntities(game.GameArena)).Returns(entities);
        _mockArenaService.Setup(x => x.GetTowers(game.GameArena)).Returns(towers);
        _mockBehaviorService
            .Setup(x => x.ExecuteAction(sessionId, game.GameArena, entity))
            .Throws(new Exception("Entity error"));
        _mockGameService.Setup(x => x.UpdateElixir(game)).Returns(Task.CompletedTask);
        _mockGameService.Setup(x => x.SaveGame(game)).Returns(Task.CompletedTask);

        _service.StartGameLoop(sessionId);

        // Act
        await _service.ProcessTick();

        // Assert - Should still save game despite entity exception
        _mockGameService.Verify(x => x.SaveGame(game), Times.Once);
    }

    [Fact]
    public async Task ProcessTick_WithMultipleSessions_ProcessesInParallel()
    {
        // Arrange
        var sessionId1 = Guid.NewGuid();
        var sessionId2 = Guid.NewGuid();
        var playerId1 = Guid.NewGuid();
        var playerId2 = Guid.NewGuid();

        var game1 = CreateTestGame(sessionId1, playerId1, playerId2);
        var game2 = CreateTestGame(sessionId2, playerId1, playerId2);

        _mockGameService.Setup(x => x.GetGame(sessionId1)).ReturnsAsync(game1);
        _mockGameService.Setup(x => x.GetGame(sessionId2)).ReturnsAsync(game2);
        _mockArenaService.Setup(x => x.GetEntities(It.IsAny<Arena>())).Returns(new List<ArenaEntity>());
        _mockArenaService.Setup(x => x.GetTowers(It.IsAny<Arena>())).Returns(new List<Tower>());
        _mockGameService.Setup(x => x.UpdateElixir(It.IsAny<Game>())).Returns(Task.CompletedTask);
        _mockGameService.Setup(x => x.SaveGame(It.IsAny<Game>())).Returns(Task.CompletedTask);

        _service.StartGameLoop(sessionId1);
        _service.StartGameLoop(sessionId2);

        // Act
        await _service.ProcessTick();

        // Assert - Both games should be processed
        _mockGameService.Verify(x => x.GetGame(sessionId1), Times.Once);
        _mockGameService.Verify(x => x.GetGame(sessionId2), Times.Once);
        _mockGameService.Verify(x => x.SaveGame(It.IsAny<Game>()), Times.Exactly(2));
    }

    #endregion

    #region Helper Methods

    private Game CreateTestGame(Guid sessionId, Guid playerId1, Guid playerId2)
    {
        var arenaTemplate = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Test Arena",
            RequiredTrophies = 0
        };

        var leaderTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 2000,
            Damage = 50,
            Range = 5,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 1500,
            Damage = 40,
            Range = 6,
            Size = 3
        };

        var towersDict = new Dictionary<Guid, List<Tower>>
        {
            {
                playerId1,
                new List<Tower>
                {
                    new(playerId1, leaderTemplate),
                    new(playerId1, guardianTemplate),
                    new(playerId1, guardianTemplate)
                }
            },
            {
                playerId2,
                new List<Tower>
                {
                    new(playerId2, leaderTemplate),
                    new(playerId2, guardianTemplate),
                    new(playerId2, guardianTemplate)
                }
            }
        };

        var arena = new Arena(arenaTemplate, towersDict);

        var card1 = new TroopCard
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

        var card2 = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Archer",
            ElixirCost = 2,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 50,
            UnitClass = UnitClass.Ground,
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 200,
            Range = 5,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Fast,
            ImageUrl = "archer.png"
        };

        var playerCard1 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId1,
            CardId = card1.Id,
            Card = card1,
            Level = 1
        };

        var playerCard2 = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = playerId2,
            CardId = card2.Id,
            Card = card2,
            Level = 1
        };

        var playerState1 = new PlayerState(playerId1, "Player1", [playerCard1])
        {
            CurrentElixir = 5m
        };

        var playerState2 = new PlayerState(playerId2, "Player2", [playerCard2])
        {
            CurrentElixir = 5m
        };

        var game = new Game(sessionId, [playerState1, playerState2], arena)
        {
            State = GameState.InProgress
        };

        return game;
    }

    #endregion
}
