using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models.ArenaEntities;

public class TroopEntityTests
{
    private PlayerCard CreateTestPlayerCard()
    {
        var troopCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 100,
            Targets = new List<UnitClass> { UnitClass.Ground },
            Hp = 500,
            Range = 1,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        return new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            CardId = troopCard.Id,
            Card = troopCard,
            Level = 1
        };
    }

    [Fact]
    public void TroopEntity_Constructor_InitializesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var x = 5;
        var y = 10;

        // Act
        var troopEntity = new TroopEntity(userId, playerCard, x, y);

        // Assert
        troopEntity.UserId.Should().Be(userId);
        troopEntity.X.Should().Be(x);
        troopEntity.Y.Should().Be(y);
        troopEntity.Health.Should().Be(500); // Hp from AttackCard
        troopEntity.Path.Should().BeEmpty();
        troopEntity.PathSteps.Should().BeEmpty();
    }

    [Fact]
    public void MoveTo_UpdatesXAndYCoordinates()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        troopEntity.MoveTo(8, 12);

        // Assert
        troopEntity.X.Should().Be(8);
        troopEntity.Y.Should().Be(12);
    }

    [Fact]
    public void SyncPathFromSteps_CreatesQueueFromPathSteps()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);
        
        troopEntity.PathSteps = new List<Point>
        {
            new Point(1, 1),
            new Point(2, 2),
            new Point(3, 3)
        };

        // Act
        troopEntity.SyncPathFromSteps();

        // Assert
        troopEntity.Path.Should().HaveCount(3);
        troopEntity.Path.Dequeue().Should().BeEquivalentTo(new Point(1, 1));
        troopEntity.Path.Dequeue().Should().BeEquivalentTo(new Point(2, 2));
        troopEntity.Path.Dequeue().Should().BeEquivalentTo(new Point(3, 3));
    }

    [Fact]
    public void SyncStepsFromPath_CreatesListFromPath()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);
        
        troopEntity.Path.Enqueue(new Point(4, 4));
        troopEntity.Path.Enqueue(new Point(5, 5));
        troopEntity.Path.Enqueue(new Point(6, 6));

        // Act
        troopEntity.SyncStepsFromPath();

        // Assert
        troopEntity.PathSteps.Should().HaveCount(3);
        troopEntity.PathSteps[0].Should().BeEquivalentTo(new Point(4, 4));
        troopEntity.PathSteps[1].Should().BeEquivalentTo(new Point(5, 5));
        troopEntity.PathSteps[2].Should().BeEquivalentTo(new Point(6, 6));
    }

    [Fact]
    public void GetLock_ReturnsLockObject()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        var lockObject = troopEntity.GetLock();

        // Assert
        lockObject.Should().NotBeNull();
        lockObject.Should().BeOfType<object>();
    }

    [Fact]
    public void GetLock_ReturnsSameObjectOnMultipleCalls()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        var lock1 = troopEntity.GetLock();
        var lock2 = troopEntity.GetLock();

        // Assert
        lock1.Should().BeSameAs(lock2);
    }

    [Fact]
    public void TargetPosition_CanBeSetAndGet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);
        var targetPosition = new Point(10, 15);

        // Act
        troopEntity.TargetPosition = targetPosition;

        // Assert
        troopEntity.TargetPosition.Should().BeEquivalentTo(targetPosition);
    }

    [Fact]
    public void TroopEntity_InheritsFromArenaEntity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var playerCard = CreateTestPlayerCard();

        // Act
        var troopEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Assert
        troopEntity.Should().BeAssignableTo<ArenaEntity>();
    }
}
