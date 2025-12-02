using FluentAssertions;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services.Factories.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services.Factories;

public class ArenaEntityFactoryTests
{
    private readonly ArenaEntityFactory _factory;

    public ArenaEntityFactoryTests()
    {
        _factory = new ArenaEntityFactory();
    }

    private PlayerState CreateTestPlayerState()
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "TestUser",
            Email = "test@example.com"
        };

        return new PlayerState(userId, user.Username, new List<PlayerCard>());
    }

    [Fact]
    public void CreateEntity_WithTroopCard_CreatesTroopEntity()
    {
        // Arrange
        var player = CreateTestPlayerState();
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

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = player.Id,
            CardId = troopCard.Id,
            Card = troopCard,
            Level = 1
        };

        // Act
        var entity = _factory.CreateEntity(player, playerCard, 5, 10);

        // Assert
        entity.Should().BeOfType<TroopEntity>();
        entity.UserId.Should().Be(player.Id);
        entity.X.Should().Be(5);
        entity.Y.Should().Be(10);
    }

    [Fact]
    public void CreateEntity_WithBuildingCard_CreatesBuildingEntity()
    {
        // Arrange
        var player = CreateTestPlayerState();
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

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = player.Id,
            CardId = buildingCard.Id,
            Card = buildingCard,
            Level = 1
        };

        // Act
        var entity = _factory.CreateEntity(player, playerCard, 8, 15);

        // Assert
        entity.Should().BeOfType<BuildingEntity>();
        entity.UserId.Should().Be(player.Id);
        entity.X.Should().Be(8);
        entity.Y.Should().Be(15);
    }

    [Fact]
    public void CreateEntity_WithSpellCard_ThrowsCardTypeException()
    {
        // Arrange
        var player = CreateTestPlayerState();
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

        var playerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = player.Id,
            CardId = spellCard.Id,
            Card = spellCard,
            Level = 1
        };

        // Act
        var act = () => _factory.CreateEntity(player, playerCard, 5, 10);

        // Assert
        act.Should().Throw<CardTypeException>()
            .WithMessage("Unsupported card type: Spell");
    }
}
