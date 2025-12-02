using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class JoinedToGameNotificationMapperTests
{
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
            Hp = 2000,
            Damage = 100,
            Range = 7,
            Size = 4
        };

        var guardianTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 1500,
            Damage = 80,
            Range = 6,
            Size = 3
        };

        var towers = new Dictionary<Guid, List<Tower>>
        {
            {
                player1Id,
                new List<Tower>
                {
                    new Tower(player1Id, leaderTemplate),
                    new Tower(player1Id, guardianTemplate),
                    new Tower(player1Id, guardianTemplate)
                }
            },
            {
                player2Id,
                new List<Tower>
                {
                    new Tower(player2Id, leaderTemplate),
                    new Tower(player2Id, guardianTemplate),
                    new Tower(player2Id, guardianTemplate)
                }
            }
        };

        return new Arena(arenaTemplate, towers);
    }

    private List<PlayerState> CreateTestPlayerStates()
    {
        var user1 = new User { Id = Guid.NewGuid(), Username = "Player1", Email = "p1@test.com" };
        var user2 = new User { Id = Guid.NewGuid(), Username = "Player2", Email = "p2@test.com" };

        return new List<PlayerState>
        {
            new PlayerState(user1.Id, user1.Username, new List<PlayerCard>()),
            new PlayerState(user2.Id, user2.Username, new List<PlayerCard>())
        };
    }

    [Fact]
    public void ToJoinedToGameNotification_WithValidGame_ReturnsCorrectNotification()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.Should().NotBeNull();
        result.GameId.Should().Be(gameId);
        result.State.Should().Be(GameState.InProgress);
        result.Players.Should().HaveCount(2);
        result.Arena.Should().NotBeNull();
    }

    [Fact]
    public void ToJoinedToGameNotification_PlayerStates_HaveCorrectProperties()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        foreach (var playerState in result.Players)
        {
            playerState.Id.Should().NotBeEmpty();
            playerState.CurrentElixir.Should().Be(Game.InitialElixir);
        }
    }

    [Fact]
    public void ToJoinedToGameNotification_Arena_HasCorrectStructure()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.Arena.Should().NotBeNull();
        result.Arena.Id.Should().Be(arena.Id);
        result.Arena.ArenaTemplate.Should().NotBeNull();
        result.Arena.Towers.Should().NotBeNull();
        result.Arena.Entities.Should().NotBeNull();
    }

    [Fact]
    public void ToJoinedToGameNotification_WithFinishedGameState_MapsCorrectly()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena)
        {
            State = GameState.Finished
        };

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.State.Should().Be(GameState.Finished);
    }

    [Fact]
    public void ToJoinedToGameNotification_WithConnectedPlayers_MapsConnectionStatus()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        playerStates[0].IsConnected = true;
        playerStates[0].ConnectionId = "conn-123";
        playerStates[1].IsConnected = false;
        playerStates[1].ConnectionId = null;
        
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.Players[0].IsConnected.Should().BeTrue();
        result.Players[0].ConnectionId.Should().Be("conn-123");
        result.Players[1].IsConnected.Should().BeFalse();
        result.Players[1].ConnectionId.Should().BeNull();
    }

    [Fact]
    public void ToJoinedToGameNotification_Arena_MapsAllTowers()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.Arena.Towers.Should().HaveCount(6); // 2 players × 3 towers each
        result.Arena.Towers.Should().Contain(t => t.Type == TowerType.Leader);
        result.Arena.Towers.Should().Contain(t => t.Type == TowerType.Guardian);
    }

    [Fact]
    public void ToJoinedToGameNotification_WithEntities_MapsAllEntities()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        
        // Add entities to arena
        var userId = playerStates[0].Id;
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            Targets = [UnitClass.Ground, UnitClass.Air],
            Hp = 100,
            Damage = 50,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground
        };
        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = card.Id,
            Card = card,
            Level = 1
        };
        
        var entity = new PrimitiveClash.Backend.Models.ArenaEntities.TroopEntity(userId, playerCard, 5, 5)
        {
            Health = 100
        };
        
        arena.Entities.Add(userId, new List<PrimitiveClash.Backend.Models.ArenaEntities.ArenaEntity> { entity });
        
        var game = new Game(gameId, playerStates, arena);

        // Act
        var result = JoinedToGameNotificationMapper.ToJoinedToGameNotification(game);

        // Assert
        result.Arena.Entities.Should().HaveCount(1);
        result.Arena.Entities[0].UnitId.Should().Be(entity.Id);
        result.Arena.Entities[0].UserId.Should().Be(userId);
        result.Arena.Entities[0].Level.Should().Be(1);
        result.Arena.Entities[0].X.Should().Be(5);
        result.Arena.Entities[0].Y.Should().Be(5);
        result.Arena.Entities[0].Health.Should().Be(100);
        result.Arena.Entities[0].MaxHealth.Should().Be(100);
    }
}
