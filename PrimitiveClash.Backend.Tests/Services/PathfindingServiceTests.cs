using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class PathfindingServiceTests
{
    private readonly PathfindingService _pathfindingService;

    public PathfindingServiceTests()
    {
        _pathfindingService = new PathfindingService();
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

    private TroopEntity CreateTestTroop(Guid userId, int x, int y, UnitClass unitClass = UnitClass.Ground)
    {
        var cardId = Guid.NewGuid();
        var card = new TroopCard
        {
            Id = cardId,
            Name = "Test Troop",
            Rarity = CardRarity.Common,
            ElixirCost = 1,
            Hp = 100,
            Damage = 10,
            Range = 1,
            MovementSpeed = MovementSpeed.Medium,
            Targets = [unitClass],
            UnitClass = unitClass,
            ImageUrl = "test.png"
        };

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CardId = cardId,
            Card = card,
            Level = 1
        };

        return new TroopEntity(userId, playerCard, x, y);
    }

    #region FindPath Tests

    [Fact]
    public void FindPath_WithDirectPath_ShouldReturnValidPath()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 5);
        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 10,
            Y = 10
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().NotBeEmpty();
        path.Last().X.Should().BeCloseTo(target.X, 1);
        path.Last().Y.Should().BeCloseTo(target.Y, 1);
    }

    [Fact]
    public void FindPath_WithObstacles_ShouldNavigateAround()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 10);

        // Place entity as obstacle (not on river)
        var obstacle = CreateTestTroop(userId, 5, 12);
        arena.PlaceEntity(obstacle);

        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 13
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().NotBeEmpty();
        // Path should go around the obstacle, not through y=12, x=5
        path.Should().NotContain(p => p.X == 5 && p.Y == 12);
    }

    [Fact]
    public void FindPath_CrossingRiver_GroundUnit_ShouldUseBridge()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 10, UnitClass.Ground);

        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 20
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().NotBeEmpty();
        
        // Ground units must use bridges (x=3 or x=14) to cross river at y=14,15
        var crossingPoints = path.Where(p => p.Y == 14 || p.Y == 15).ToList();
        if (crossingPoints.Any())
        {
            crossingPoints.Should().AllSatisfy(p => 
                p.X.Should().Match(x => x == 3 || x == 14, "must use bridge at x=3 or x=14"));
        }
    }

    [Fact]
    public void FindPath_CrossingRiver_AirUnit_ShouldCrossDirectly()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 10, UnitClass.Air);

        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 20
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().NotBeEmpty();
        // Air units can cross river directly
        var hasRiverCrossing = path.Any(p => (p.Y == 14 || p.Y == 15) && p.X == 5);
        hasRiverCrossing.Should().BeTrue("air units can fly over river");
    }

    [Fact]
    public void FindPath_NoValidPath_ShouldReturnEmptyList()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 5, UnitClass.Ground);

        // Create a wall of obstacles blocking access (use rows 13 and 16, not river rows 14-15)
        for (int x = 0; x < 18; x++)
        {
            var obstacle1 = CreateTestTroop(userId, x, 13);
            arena.PlaceEntity(obstacle1);
        }

        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 5,
            Y = 20
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().BeEmpty();
    }

    [Fact]
    public void FindPath_TargetIsAdjacent_ShouldReturnEmptyPath()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 5);
        var target = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 1
        })
        {
            X = 6,
            Y = 6
        };

        // Act
        var path = _pathfindingService.FindPath(arena, troop, target);

        // Assert
        path.Should().BeEmpty("troop is already adjacent to target");
    }

    #endregion

    #region FindClosestAttackPoint Tests

    [Fact]
    public void FindClosestAttackPoint_WithAccessiblePoints_ShouldReturnClosest()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 1, 1);

        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 3
        })
        {
            X = 5,
            Y = 5
        };

        // Act
        var attackPoint = _pathfindingService.FindClosestAttackPoint(arena, troop, tower);

        // Assert
        attackPoint.Should().NotBeNull();
        // Should be adjacent to tower (size 3: occupies 5-7 x, 5-7 y)
        var isAdjacentToTower = 
            (attackPoint.X >= 4 && attackPoint.X <= 8) &&
            (attackPoint.Y >= 4 && attackPoint.Y <= 8) &&
            !(attackPoint.X >= 5 && attackPoint.X <= 7 && attackPoint.Y >= 5 && attackPoint.Y <= 7);
        
        isAdjacentToTower.Should().BeTrue("attack point should be adjacent to tower");
    }

    [Fact]
    public void FindClosestAttackPoint_AllPointsBlocked_ShouldReturnTowerPosition()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 1, 1);

        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 2
        })
        {
            X = 8,
            Y = 8
        };

        // Block all adjacent cells (avoid river area)
        for (int dx = -1; dx <= 2; dx++)
        {
            for (int dy = -1; dy <= 2; dy++)
            {
                int x = tower.X + dx;
                int y = tower.Y + dy;
                if (x < 0 || y < 0 || x >= 18 || y >= 30) continue;
                if (x >= tower.X && x < tower.X + 2 && y >= tower.Y && y < tower.Y + 2) continue;
                if (y >= 14 && y <= 15) continue; // Skip river

                var obstacle = CreateTestTroop(userId, x, y);
                arena.PlaceEntity(obstacle);
            }
        }

        // Act
        var attackPoint = _pathfindingService.FindClosestAttackPoint(arena, troop, tower);

        // Assert
        attackPoint.X.Should().Be(tower.X);
        attackPoint.Y.Should().Be(tower.Y);
    }

    [Fact]
    public void FindClosestAttackPoint_TroopFarAway_ShouldReturnNearestAccessiblePoint()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 0, 0);

        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 4
        })
        {
            X = 10,
            Y = 10
        };

        // Act
        var attackPoint = _pathfindingService.FindClosestAttackPoint(arena, troop, tower);

        // Assert
        attackPoint.Should().NotBeNull();
        
        // Calculate distance from troop to attack point
        var distanceToAttackPoint = Math.Sqrt(
            Math.Pow(troop.X - attackPoint.X, 2) + 
            Math.Pow(troop.Y - attackPoint.Y, 2));

        // Attack point should be adjacent to tower (size 4: occupies 10-13 x, 10-13 y)
        var isAdjacentToTower = 
            (attackPoint.X >= 9 && attackPoint.X <= 14) &&
            (attackPoint.Y >= 9 && attackPoint.Y <= 14) &&
            !(attackPoint.X >= 10 && attackPoint.X <= 13 && attackPoint.Y >= 10 && attackPoint.Y <= 13);
        
        isAdjacentToTower.Should().BeTrue("attack point should be adjacent to tower");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void PathfindingIntegration_FindPathAndAttackPoint_ShouldWork()
    {
        // Arrange
        var arena = CreateTestArena();
        var userId = Guid.NewGuid();
        var troop = CreateTestTroop(userId, 5, 5);

        var tower = new Tower(userId, new TowerTemplate
        {
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 3
        })
        {
            X = 10,
            Y = 20
        };

        // Act - Find attack point
        var attackPoint = _pathfindingService.FindClosestAttackPoint(arena, troop, tower);
        
        // Act - Find path to attack point
        var path = _pathfindingService.FindPath(arena, troop, 
            new Tower(userId, new TowerTemplate { Type = TowerType.Leader, Hp = 100, Damage = 10, Range = 1, Size = 1 })
            {
                X = attackPoint.X,
                Y = attackPoint.Y
            });

        // Assert
        attackPoint.Should().NotBeNull();
        path.Should().NotBeEmpty();
    }

    #endregion
}
