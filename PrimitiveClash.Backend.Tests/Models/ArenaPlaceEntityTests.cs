using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Tests.Models;

public class ArenaPlaceEntityTests
{
    [Fact]
    public void PlaceEntity_WhenEntityAlreadyExists_DoesNotAddDuplicate()
    {
        // Arrange
        var arenaTemplate = new ArenaTemplate 
        { 
            Id = Guid.NewGuid(), 
            Name = "Test Arena",
            RequiredTrophies = 0
        };
        
        var userId = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
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
            Hp = 800, 
            Damage = 80, 
            Range = 5,
            Size = 3
        };
        
        var towers = new Dictionary<Guid, List<Tower>>
        {
            { userId, new List<Tower> 
                { 
                    new Tower(userId, leaderTemplate),
                    new Tower(userId, guardianTemplate),
                    new Tower(userId, guardianTemplate)
                } 
            },
            { userId2, new List<Tower> 
                { 
                    new Tower(userId2, leaderTemplate),
                    new Tower(userId2, guardianTemplate),
                    new Tower(userId2, guardianTemplate)
                } 
            }
        };
        
        var arena = new Arena(arenaTemplate, towers);
        
        var card = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Test Troop",
            Type = CardType.Troop,
            ElixirCost = 3,
            Rarity = CardRarity.Common,
            Damage = 50,
            Targets = new List<UnitClass> { UnitClass.Ground },
            ImageUrl = "",
            Hp = 100,
            Range = 1,
            DamageArea = 0,
            HitSpeed = 1.0f,
            UnitClass = UnitClass.Ground,
            VisionRange = 5,
            MovementSpeed = MovementSpeed.Fast
        };
        
        var playerCard = new PlayerCard 
        { 
            CardId = card.Id, 
            UserId = userId, 
            Card = card 
        };
        
        var entity = new TroopEntity(userId, playerCard, 5, 5);
        
        // Act - Place entity first time
        arena.PlaceEntity(entity);
        var countAfterFirstPlace = arena.Entities[userId].Count;
        
        // Act - Try to place same entity again (same Id)
        // First remove it from the cell to allow re-placement
        arena.Grid[5][5].RemoveEntity(entity);
        arena.PlaceEntity(entity);
        var countAfterSecondPlace = arena.Entities[userId].Count;
        
        // Assert - Entity should not be duplicated in the list
        countAfterFirstPlace.Should().Be(1);
        countAfterSecondPlace.Should().Be(1, "entity with same Id should not be added twice");
        arena.Entities[userId].Should().ContainSingle(e => e.Id == entity.Id);
    }
}
