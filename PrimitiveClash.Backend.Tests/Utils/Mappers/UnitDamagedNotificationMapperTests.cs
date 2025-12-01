using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.ArenaEntities;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Utils.Mappers;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class UnitDamagedNotificationMapperTests
{
    [Fact]
    public void ToUnitDamagedNotification_WithArenaEntityAsTarget_ShouldMapCorrectly()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        var attackerCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Attacker",
            Targets = [UnitClass.Ground],
            Hp = 150,
            Damage = 50,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground
        };
        
        var targetCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Target",
            Targets = [UnitClass.Ground],
            Hp = 200,
            Damage = 30,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Slow,
            UnitClass = UnitClass.Ground
        };
        
        var attackerPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = attackerId,
            CardId = attackerCard.Id,
            Card = attackerCard,
            Level = 1
        };
        
        var targetPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            CardId = targetCard.Id,
            Card = targetCard,
            Level = 1
        };
        
        var attacker = new TroopEntity(attackerId, attackerPlayerCard, 5, 5)
        {
            Health = 150
        };
        
        var target = new TroopEntity(targetUserId, targetPlayerCard, 6, 6)
        {
            Health = 150
        };
        
        var damage = 50;

        // Act
        var result = UnitDamagedNotificationMapper.ToUnitDamagedNotification(attacker, target, damage);

        // Assert
        result.Should().NotBeNull();
        result.AttackerId.Should().Be(attacker.Id);
        result.TargetId.Should().Be(target.Id);
        result.Damage.Should().Be(damage);
        result.Health.Should().Be(150);
        result.MaxHealth.Should().Be(200);
    }

    [Fact]
    public void ToUnitDamagedNotification_WithTowerAsTarget_ShouldMapCorrectly()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var towerUserId = Guid.NewGuid();
        
        var attackerCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Attacker",
            Targets = [UnitClass.Ground, UnitClass.Buildings],
            Hp = 100,
            Damage = 80,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Fast,
            UnitClass = UnitClass.Ground
        };
        
        var attackerPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = attackerId,
            CardId = attackerCard.Id,
            Card = attackerCard,
            Level = 1
        };
        
        var attacker = new TroopEntity(attackerId, attackerPlayerCard, 5, 5)
        {
            Health = 100
        };
        
        var towerTemplate = new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 3000,
            Damage = 100,
            Range = 7,
            Size = 4
        };
        
        var tower = new Tower(towerUserId, towerTemplate)
        {
            Health = 2500
        };
        
        var damage = 80;

        // Act
        var result = UnitDamagedNotificationMapper.ToUnitDamagedNotification(attacker, tower, damage);

        // Assert
        result.Should().NotBeNull();
        result.AttackerId.Should().Be(attacker.Id);
        result.TargetId.Should().Be(tower.Id);
        result.Damage.Should().Be(damage);
        result.Health.Should().Be(2500);
        result.MaxHealth.Should().Be(3000);
    }

    [Fact]
    public void ToUnitDamagedNotification_WithBuildingEntity_ShouldMapCorrectly()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var buildingUserId = Guid.NewGuid();
        
        var attackerCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Attacker",
            Targets = [UnitClass.Buildings],
            Hp = 120,
            Damage = 60,
            Range = 1,
            HitSpeed = 1.5f,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground
        };
        
        var buildingCard = new BuildingCard
        {
            Id = Guid.NewGuid(),
            Name = "Cannon",
            Targets = [UnitClass.Ground],
            Hp = 500,
            Damage = 100,
            Range = 5,
            HitSpeed = 0.8f,
            Duration = 30
        };
        
        var attackerPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = attackerId,
            CardId = attackerCard.Id,
            Card = attackerCard,
            Level = 1
        };
        
        var buildingPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = buildingUserId,
            CardId = buildingCard.Id,
            Card = buildingCard,
            Level = 1
        };
        
        var attacker = new TroopEntity(attackerId, attackerPlayerCard, 5, 5)
        {
            Health = 120
        };
        
        var building = new BuildingEntity(buildingUserId, buildingPlayerCard, 10, 10)
        {
            Health = 400
        };
        
        var damage = 60;

        // Act
        var result = UnitDamagedNotificationMapper.ToUnitDamagedNotification(attacker, building, damage);

        // Assert
        result.Should().NotBeNull();
        result.AttackerId.Should().Be(attacker.Id);
        result.TargetId.Should().Be(building.Id);
        result.Damage.Should().Be(damage);
        result.Health.Should().Be(400);
        result.MaxHealth.Should().Be(500);
    }

    [Fact]
    public void ToUnitDamagedNotification_WithZeroDamage_ShouldMapCorrectly()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        
        var attackerCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Attacker",
            Targets = [UnitClass.Ground],
            Hp = 100,
            Damage = 50,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Medium,
            UnitClass = UnitClass.Ground
        };
        
        var targetCard = new TroopCard
        {
            Id = Guid.NewGuid(),
            Name = "Target",
            Targets = [UnitClass.Ground],
            Hp = 200,
            Damage = 30,
            Range = 1,
            HitSpeed = 1.0f,
            MovementSpeed = MovementSpeed.Slow,
            UnitClass = UnitClass.Ground
        };
        
        var attackerPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = attackerId,
            CardId = attackerCard.Id,
            Card = attackerCard,
            Level = 1
        };
        
        var targetPlayerCard = new PlayerCard
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId,
            CardId = targetCard.Id,
            Card = targetCard,
            Level = 1
        };
        
        var attacker = new TroopEntity(attackerId, attackerPlayerCard, 5, 5)
        {
            Health = 100
        };
        
        var target = new TroopEntity(targetUserId, targetPlayerCard, 6, 6)
        {
            Health = 200
        };

        // Act
        var result = UnitDamagedNotificationMapper.ToUnitDamagedNotification(attacker, target, 0);

        // Assert
        result.Damage.Should().Be(0);
        result.Health.Should().Be(200);
    }
}
