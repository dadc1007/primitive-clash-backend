using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Tests.Models;

public class CellBranchTests
{
    private static (TroopCard, PlayerCard, Guid) CreateGroundUnit()
    {
        var userId = Guid.NewGuid();
        var groundCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Knight",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground],
            ImageUrl = "",
            Hp = 1000,
            Damage = 100,
            HitSpeed = 1.5f,
            Range = 1,
            DamageArea = 0,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground,
            VisionRange = 5
        };

        var cardId = groundCard.Id;
        var playerCard = new PlayerCard { 
            Id = Guid.NewGuid(), 
            Card = groundCard, 
            CardId = cardId,
            UserId = userId,
            Level = 1 
        };
        
        return (groundCard, playerCard, userId);
    }

    private static (TroopCard, PlayerCard, Guid) CreateAirUnit()
    {
        var userId = Guid.NewGuid();
        var airCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Dragon",
            ElixirCost = 4,
            Rarity = CardRarity.Rare,
            Type = CardType.Troop,
            Targets = [UnitClass.Ground, UnitClass.Air],
            ImageUrl = "",
            Hp = 800,
            Damage = 150,
            HitSpeed = 1.8f,
            Range = 3,
            DamageArea = 1,
            MovementSpeed = MovementSpeed.Fast,
            UnitClass = UnitClass.Air,
            VisionRange = 6
        };

        var cardId = airCard.Id;
        var playerCard = new PlayerCard { 
            Id = Guid.NewGuid(), 
            Card = airCard, 
            CardId = cardId,
            UserId = userId,
            Level = 1 
        };
        
        return (airCard, playerCard, userId);
    }

    #region IsWalkable with River Tests

    [Fact]
    public void IsWalkable_RiverCell_WithGroundUnit_ShouldReturnFalse()
    {
        // Arrange
        var cell = new Cell { Type = CellType.River, Tower = false };
        var (_, playerCard, userId) = CreateGroundUnit();
        var groundEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        bool result = cell.IsWalkable(groundEntity);

        // Assert
        result.Should().BeFalse("Ground units cannot walk on River cells");
    }

    [Fact]
    public void IsWalkable_RiverCell_WithAirUnit_ShouldReturnTrue()
    {
        // Arrange
        var cell = new Cell { Type = CellType.River, Tower = false };
        var (_, playerCard, userId) = CreateAirUnit();
        var airEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        bool result = cell.IsWalkable(airEntity);

        // Assert
        result.Should().BeTrue("Air units can fly over River cells");
    }

    #endregion

    #region Collision Tests

    [Fact]
    public void PlaceEntity_GroundCell_WithExistingGroundEntity_ShouldReturnFalse()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground, GroundEntity = true };
        var (_, playerCard, userId) = CreateGroundUnit();
        var entity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        bool result = cell.PlaceEntity(entity);

        // Assert
        result.Should().BeFalse("Cannot place ground entity where another ground entity exists");
        cell.GroundEntity.Should().BeTrue("Cell should still have the original ground entity");
    }

    [Fact]
    public void PlaceEntity_AirCell_WithExistingAirEntity_ShouldReturnFalse()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground, AirEntity = true };
        var (_, playerCard, userId) = CreateAirUnit();
        var entity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        bool result = cell.PlaceEntity(entity);

        // Assert
        result.Should().BeFalse("Cannot place air entity where another air entity exists");
        cell.AirEntity.Should().BeTrue("Cell should still have the original air entity");
    }

    [Fact]
    public void PlaceEntity_GroundAndAirEntities_ShouldNotCollide()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground, GroundEntity = true };
        var (_, playerCard, userId) = CreateAirUnit();
        var airEntity = new TroopEntity(userId, playerCard, 0, 0);

        // Act
        bool result = cell.PlaceEntity(airEntity);

        // Assert
        result.Should().BeTrue("Air entity can be placed even if ground entity exists");
        cell.GroundEntity.Should().BeTrue("Ground entity should remain");
        cell.AirEntity.Should().BeTrue("Air entity should be added");
    }

    #endregion

    #region RemoveEntity Tests

    [Fact]
    public void RemoveEntity_GroundUnit_ShouldClearGroundEntityFlag()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground };
        var (_, playerCard, userId) = CreateGroundUnit();
        var entity = new TroopEntity(userId, playerCard, 0, 0);

        cell.PlaceEntity(entity);
        cell.GroundEntity.Should().BeTrue("Entity should be placed first");

        // Act
        cell.RemoveEntity(entity);

        // Assert
        cell.GroundEntity.Should().BeFalse("Ground entity flag should be cleared");
    }

    [Fact]
    public void RemoveEntity_AirUnit_ShouldClearAirEntityFlag()
    {
        // Arrange
        var cell = new Cell { Type = CellType.Ground };
        var (_, playerCard, userId) = CreateAirUnit();
        var entity = new TroopEntity(userId, playerCard, 0, 0);

        cell.PlaceEntity(entity);
        cell.AirEntity.Should().BeTrue("Entity should be placed first");

        // Act
        cell.RemoveEntity(entity);

        // Assert
        cell.AirEntity.Should().BeFalse("Air entity flag should be cleared");
    }

    #endregion
}
