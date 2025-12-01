using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class CellTests
{
    private static TroopEntity CreateTestGroundEntity()
    {
        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            CardId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Level = 1,
            Card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = "TestGroundCard",
                ElixirCost = 3,
                Rarity = CardRarity.Common,
                Type = CardType.Troop,
                Damage = 100,
                UnitClass = UnitClass.Ground,
                Targets = [UnitClass.Ground],
                Hp = 300,
                Range = 1,
                HitSpeed = 1.0f,
                MovementSpeed = MovementSpeed.Medium,
                ImageUrl = "test.png"
            }
        };

        return new TroopEntity(Guid.NewGuid(), playerCard, 0, 0);
    }

    private static TroopEntity CreateTestAirEntity()
    {
        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            CardId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Level = 1,
            Card = new TroopCard
            {
                Id = Guid.NewGuid(),
                Name = "TestAirCard",
                ElixirCost = 3,
                Rarity = CardRarity.Common,
                Type = CardType.Troop,
                Damage = 100,
                UnitClass = UnitClass.Air,
                Targets = [UnitClass.Ground, UnitClass.Air],
                Hp = 300,
                Range = 1,
                HitSpeed = 1.0f,
                MovementSpeed = MovementSpeed.Fast,
                ImageUrl = "test.png"
            }
        };

        return new TroopEntity(Guid.NewGuid(), playerCard, 0, 0);
    }

    #region IsWalkable Tests

    [Fact]
    public void IsWalkable_WithGroundCellAndNoTower_ShouldReturnTrue()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = false
        };
        var entity = CreateTestGroundEntity();

        var result = cell.IsWalkable(entity);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWalkable_WithBridgeCellAndNoTower_ShouldReturnTrue()
    {
        var cell = new Cell
        {
            Type = CellType.Bridge,
            Tower = false
        };
        var entity = CreateTestGroundEntity();

        var result = cell.IsWalkable(entity);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWalkable_WithRiverAndGroundEntity_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.River,
            Tower = false
        };
        var groundEntity = CreateTestGroundEntity();

        var result = cell.IsWalkable(groundEntity);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWalkable_WithRiverAndAirEntity_ShouldReturnTrue()
    {
        var cell = new Cell
        {
            Type = CellType.River,
            Tower = false
        };
        var airEntity = CreateTestAirEntity();

        var result = cell.IsWalkable(airEntity);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWalkable_WithTower_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = true
        };
        var entity = CreateTestGroundEntity();

        var result = cell.IsWalkable(entity);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWalkable_WithGroundEntityAlreadyPresent_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = false,
            GroundEntity = true
        };
        var entity = CreateTestGroundEntity();

        var result = cell.IsWalkable(entity);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWalkable_WithAirEntityAlreadyPresent_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = false,
            AirEntity = true
        };
        var entity = CreateTestAirEntity();

        var result = cell.IsWalkable(entity);

        result.Should().BeFalse();
    }

    #endregion

    #region PlaceEntity Tests

    [Fact]
    public void PlaceEntity_WithWalkableCell_ShouldReturnTrueAndUpdateEntity()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = false
        };
        var entity = CreateTestGroundEntity();

        var result = cell.PlaceEntity(entity);

        result.Should().BeTrue();
        cell.GroundEntity.Should().BeTrue();
    }

    [Fact]
    public void PlaceEntity_WithNonWalkableCell_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = true
        };
        var entity = CreateTestGroundEntity();

        var result = cell.PlaceEntity(entity);

        result.Should().BeFalse();
        cell.GroundEntity.Should().BeFalse();
    }

    #endregion
}
