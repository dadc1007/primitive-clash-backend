using FluentAssertions;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models.Cards;

public class SpellCardTests
{
    [Fact]
    public void SpellCard_InheritsFromCard()
    {
        // Arrange & Act
        var spellCard = new SpellCard
        {
            Name = "Test",
            Targets = new List<UnitClass>()
        };

        // Assert
        spellCard.Should().BeAssignableTo<Card>();
    }

    [Fact]
    public void SpellCard_Duration_CanBeSetAndGet()
    {
        // Arrange
        var spellCard = new SpellCard
        {
            Name = "Test",
            Targets = new List<UnitClass>()
        };
        var duration = 5.5f;

        // Act
        spellCard.Duration = duration;

        // Assert
        spellCard.Duration.Should().Be(duration);
    }

    [Fact]
    public void SpellCard_Radius_CanBeSetAndGet()
    {
        // Arrange
        var spellCard = new SpellCard
        {
            Name = "Test",
            Targets = new List<UnitClass>()
        };
        var radius = 3;

        // Act
        spellCard.Radius = radius;

        // Assert
        spellCard.Radius.Should().Be(radius);
    }

    [Fact]
    public void SpellCard_WithAllProperties_InitializesCorrectly()
    {
        // Arrange & Act
        var spellCard = new SpellCard
        {
            Id = Guid.NewGuid(),
            Name = "Fireball",
            ElixirCost = 4,
            Rarity = CardRarity.Rare,
            Type = CardType.Spell,
            Targets = new List<UnitClass>(),
            Duration = 1.5f,
            Radius = 2
        };

        // Assert
        spellCard.Name.Should().Be("Fireball");
        spellCard.ElixirCost.Should().Be(4);
        spellCard.Type.Should().Be(CardType.Spell);
        spellCard.Rarity.Should().Be(CardRarity.Rare);
        spellCard.Duration.Should().Be(1.5f);
        spellCard.Radius.Should().Be(2);
    }
}
