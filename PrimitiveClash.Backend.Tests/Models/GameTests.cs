using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class GameTests
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
            new PlayerState(user1.Id, new List<PlayerCard>()),
            new PlayerState(user2.Id, new List<PlayerCard>())
        };
    }

    [Fact]
    public void Game_Constructor_InitializesCorrectly()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();

        // Act
        var game = new Game(gameId, playerStates, arena);

        // Assert
        game.Id.Should().Be(gameId);
        game.State.Should().Be(GameState.InProgress);
        game.GameArena.Should().Be(arena);
        game.PlayerStates.Should().BeEquivalentTo(playerStates);
        game.PlayerStates.Should().HaveCount(2);
    }

    [Fact]
    public void Game_State_CanBeChanged()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);

        // Act
        game.State = GameState.Finished;

        // Assert
        game.State.Should().Be(GameState.Finished);
    }

    [Fact]
    public void Game_Constants_HaveCorrectValues()
    {
        // Assert
        Game.InitialElixir.Should().Be(5m);
        Game.ElixirPerSecond.Should().Be(1m);
        Game.MaxElixir.Should().Be(10m);
    }

    [Fact]
    public void Game_Id_CanBeModified()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates, arena);
        var newId = Guid.NewGuid();

        // Act
        game.Id = newId;

        // Assert
        game.Id.Should().Be(newId);
    }

    [Fact]
    public void Game_GameArena_CanBeModified()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates = CreateTestPlayerStates();
        var arena1 = CreateTestArena();
        var game = new Game(gameId, playerStates, arena1);
        var arena2 = CreateTestArena();

        // Act
        game.GameArena = arena2;

        // Assert
        game.GameArena.Should().Be(arena2);
    }

    [Fact]
    public void Game_PlayerStates_CanBeModified()
    {
        // Arrange
        var gameId = Guid.NewGuid();
        var playerStates1 = CreateTestPlayerStates();
        var arena = CreateTestArena();
        var game = new Game(gameId, playerStates1, arena);
        var playerStates2 = CreateTestPlayerStates();

        // Act
        game.PlayerStates = playerStates2;

        // Assert
        game.PlayerStates.Should().BeEquivalentTo(playerStates2);
        game.PlayerStates.Should().NotBeSameAs(playerStates1);
    }
}
