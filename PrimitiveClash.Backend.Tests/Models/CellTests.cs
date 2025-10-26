using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class CellTests
{
    #region IsWalkable Tests

    [Fact]
    public void IsWalkable_WithGroundAndNoTower_ShouldReturnTrue()
    {
        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = null
        };

        var result = cell.IsWalkable();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWalkable_WithBridgeAndNoTower_ShouldReturnTrue()
    {
        var cell = new Cell
        {
            Type = CellType.Bridge,
            Tower = null
        };

        var result = cell.IsWalkable();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsWalkable_WithRiver_ShouldReturnFalse()
    {
        var cell = new Cell
        {
            Type = CellType.River,
            Tower = null
        };

        var result = cell.IsWalkable();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWalkable_WithTower_ShouldReturnFalse()
    {
        var towerTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3
        };

        var cell = new Cell
        {
            Type = CellType.Ground,
            Tower = new Tower(towerTemplate, Guid.NewGuid())
        };

        var result = cell.IsWalkable();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsWalkable_WithRiverAndTower_ShouldReturnFalse()
    {
        var towerTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3
        };

        var cell = new Cell
        {
            Type = CellType.River,
            Tower = new Tower(towerTemplate, Guid.NewGuid())
        };

        var result = cell.IsWalkable();

        result.Should().BeFalse();
    }

    #endregion
}
