using FluentAssertions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class CardMapperTests
{
    [Fact]
    public void ToCardResponse_WithTroopCard_ReturnsMappedResponse()
    {
        // Arrange
        var troopCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Archer",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 50,
            Targets = new List<UnitClass> { UnitClass.Ground, UnitClass.Air },
            Hp = 200,
            Range = 5,
            UnitClass = UnitClass.Air,
            VisionRange = 6
        };

        // Act
        var result = troopCard.ToCardResponse();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(troopCard.Id);
        result.Name.Should().Be(troopCard.Name);
        result.ElixirCost.Should().Be(troopCard.ElixirCost);
        result.Rarity.Should().Be(troopCard.Rarity);
        result.Type.Should().Be(troopCard.Type);
        result.Damage.Should().Be(troopCard.Damage);
        result.Targets.Should().BeEquivalentTo(troopCard.Targets);
        result.AttackDetails.Should().NotBeNull();
        result.AttackDetails!.Hp.Should().Be(troopCard.Hp);
        result.AttackDetails.Range.Should().Be(troopCard.Range);
        result.AttackDetails.UnitClass.Should().Be(troopCard.UnitClass);
        result.TroopDetails.Should().NotBeNull();
        result.TroopDetails!.VisionRange.Should().Be(troopCard.VisionRange);
    }

    [Fact]
    public void ToCardResponse_WithTroopCardMeleeUnit_ReturnsMappedResponseWithMeleeClass()
    {
        // Arrange
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

        // Act
        var result = troopCard.ToCardResponse();

        // Assert
        result.AttackDetails!.UnitClass.Should().Be(UnitClass.Ground);
        result.AttackDetails.Range.Should().Be(1);
    }

    [Fact]
    public void ToCardResponse_WithBuildingCard_ThrowsInvalidOperationException()
    {
        // Arrange
        var buildingCard = new BuildingCard
        {
            Id = Guid.NewGuid(),
            Name = "Cannon",
            ElixirCost = 4,
            Rarity = CardRarity.Common,
            Type = CardType.Building,
            Damage = 80,
            Targets = new List<UnitClass> { UnitClass.Ground },
            Hp = 300,
            Range = 6
        };

        // Act
        var act = () => buildingCard.ToCardResponse();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToCardResponse_WithSpellCard_ThrowsInvalidOperationException()
    {
        // Arrange
        var spellCard = new SpellCard
        {
            Id = Guid.NewGuid(),
            Name = "Fireball",
            ElixirCost = 4,
            Rarity = CardRarity.Rare,
            Type = CardType.Spell,
            Damage = 200,
            Targets = new List<UnitClass> { UnitClass.Ground, UnitClass.Air },
            Radius = 3
        };

        // Act
        var act = () => spellCard.ToCardResponse();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ToCardResponse_WithTroopCardAirTarget_ReturnsMappedResponse()
    {
        // Arrange
        var troopCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Minion",
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Type = CardType.Troop,
            Damage = 40,
            Targets = new List<UnitClass> { UnitClass.Air },
            Hp = 100,
            Range = 2,
            UnitClass = UnitClass.Air,
            VisionRange = 5
        };

        // Act
        var result = troopCard.ToCardResponse();

        // Assert
        result.Targets.Should().ContainSingle().Which.Should().Be(UnitClass.Air);
    }
}
