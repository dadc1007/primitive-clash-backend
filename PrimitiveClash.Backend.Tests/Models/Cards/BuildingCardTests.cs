using FluentAssertions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models.Cards;

public class BuildingCardTests
{
    [Fact]
    public void BuildingCard_InheritsFromAttackCard()
    {
        // Arrange & Act
        var buildingCard = new BuildingCard
        {
            Name = "Test",
            Targets = new List<UnitClass>()
        };

        // Assert
        buildingCard.Should().BeAssignableTo<AttackCard>();
    }

    [Fact]
    public void BuildingCard_Duration_CanBeSetAndGet()
    {
        // Arrange
        var buildingCard = new BuildingCard
        {
            Name = "Test",
            Targets = new List<UnitClass>()
        };
        var duration = 30.5f;

        // Act
        buildingCard.Duration = duration;

        // Assert
        buildingCard.Duration.Should().Be(duration);
    }

    [Fact]
    public void BuildingCard_WithAllProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var buildingCard = new BuildingCard
        {
            Id = Guid.NewGuid(),
            Name = "Cannon",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Building,
            Damage = 100,
            Targets = new List<UnitClass> { UnitClass.Ground },
            Hp = 500,
            Range = 6,
            Duration = 40f
        };

        // Assert
        buildingCard.Name.Should().Be("Cannon");
        buildingCard.ElixirCost.Should().Be(3);
        buildingCard.Type.Should().Be(CardType.Building);
        buildingCard.Damage.Should().Be(100);
        buildingCard.Hp.Should().Be(500);
        buildingCard.Duration.Should().Be(40f);
    }
}
