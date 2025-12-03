using FluentAssertions;
using PrimitiveClash.Backend.DTOs.Notifications;
using PrimitiveClash.Backend.Models.Enums;

namespace PrimitiveClash.Backend.Tests.DTOs.Notifications;

public class NotificationDtosTests
{
    #region CardSpawnedNotification Tests

    [Fact]
    public void CardSpawnedNotification_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardPlayedId = Guid.NewGuid();
        var level = 5;
        var x = 10;
        var y = 20;
        var health = 100;
        var maxHealth = 150;

        // Act
        var notification = new CardSpawnedNotification(
            unitId, userId, cardPlayedId, level, x, y, health, maxHealth
        );

        // Assert
        notification.UnitId.Should().Be(unitId);
        notification.UserId.Should().Be(userId);
        notification.CardPlayedId.Should().Be(cardPlayedId);
        notification.Level.Should().Be(level);
        notification.X.Should().Be(x);
        notification.Y.Should().Be(y);
        notification.Health.Should().Be(health);
        notification.MaxHealth.Should().Be(maxHealth);
    }

    [Fact]
    public void CardSpawnedNotification_WithDifferentInstances_ShouldBeEqual()
    {
        // Arrange
        var unitId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cardPlayedId = Guid.NewGuid();

        var notification1 = new CardSpawnedNotification(
            unitId, userId, cardPlayedId, 5, 10, 20, 100, 150
        );
        var notification2 = new CardSpawnedNotification(
            unitId, userId, cardPlayedId, 5, 10, 20, 100, 150
        );

        // Act & Assert
        notification1.Should().Be(notification2);
        notification1.GetHashCode().Should().Be(notification2.GetHashCode());
    }

    #endregion

    #region TowerNotification Tests

    [Fact]
    public void TowerNotification_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var towerTemplateId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var type = TowerType.Leader;
        var health = 500;
        var maxHealth = 1000;
        var x = 5;
        var y = 10;

        // Act
        var notification = new TowerNotification(
            id, towerTemplateId, userId, type, health, maxHealth, x, y
        );

        // Assert
        notification.Id.Should().Be(id);
        notification.TowerTemplateId.Should().Be(towerTemplateId);
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be(type);
        notification.Health.Should().Be(health);
        notification.MaxHealth.Should().Be(maxHealth);
        notification.X.Should().Be(x);
        notification.Y.Should().Be(y);
    }

    [Fact]
    public void TowerNotification_WithGuardianType_ShouldCreateInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var towerTemplateId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var notification = new TowerNotification(
            id, towerTemplateId, userId, TowerType.Guardian, 300, 600, 3, 7
        );

        // Assert
        notification.Type.Should().Be(TowerType.Guardian);
        notification.Health.Should().Be(300);
        notification.MaxHealth.Should().Be(600);
    }

    [Fact]
    public void TowerNotification_WithDifferentInstances_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var towerTemplateId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var notification1 = new TowerNotification(
            id, towerTemplateId, userId, TowerType.Leader, 500, 1000, 5, 10
        );
        var notification2 = new TowerNotification(
            id, towerTemplateId, userId, TowerType.Leader, 500, 1000, 5, 10
        );

        // Act & Assert
        notification1.Should().Be(notification2);
    }

    #endregion

    #region TroopMovedNotification Tests

    [Fact]
    public void TroopMovedNotification_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var troopId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var x = 15;
        var y = 25;
        var state = "Moving";

        // Act
        var notification = new TroopMovedNotification(
            troopId, playerId, cardId, x, y, state
        );

        // Assert
        notification.TroopId.Should().Be(troopId);
        notification.PlayerId.Should().Be(playerId);
        notification.CardId.Should().Be(cardId);
        notification.X.Should().Be(x);
        notification.Y.Should().Be(y);
        notification.State.Should().Be(state);
    }

    [Fact]
    public void TroopMovedNotification_WithAttackingState_ShouldCreateInstance()
    {
        // Arrange
        var troopId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        // Act
        var notification = new TroopMovedNotification(
            troopId, playerId, cardId, 10, 20, "Attacking"
        );

        // Assert
        notification.State.Should().Be("Attacking");
    }

    [Fact]
    public void TroopMovedNotification_WithDifferentInstances_ShouldBeEqual()
    {
        // Arrange
        var troopId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var cardId = Guid.NewGuid();

        var notification1 = new TroopMovedNotification(
            troopId, playerId, cardId, 15, 25, "Moving"
        );
        var notification2 = new TroopMovedNotification(
            troopId, playerId, cardId, 15, 25, "Moving"
        );

        // Act & Assert
        notification1.Should().Be(notification2);
    }

    #endregion

    #region UnitDamagedNotification Tests

    [Fact]
    public void UnitDamagedNotification_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var damage = 50;
        var health = 150;
        var maxHealth = 200;

        // Act
        var notification = new UnitDamagedNotification(
            attackerId, targetId, damage, health, maxHealth
        );

        // Assert
        notification.AttackerId.Should().Be(attackerId);
        notification.TargetId.Should().Be(targetId);
        notification.Damage.Should().Be(damage);
        notification.Health.Should().Be(health);
        notification.MaxHealth.Should().Be(maxHealth);
    }

    [Fact]
    public void UnitDamagedNotification_WithZeroDamage_ShouldCreateInstance()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        // Act
        var notification = new UnitDamagedNotification(
            attackerId, targetId, 0, 100, 100
        );

        // Assert
        notification.Damage.Should().Be(0);
        notification.Health.Should().Be(100);
    }

    [Fact]
    public void UnitDamagedNotification_WithDifferentInstances_ShouldBeEqual()
    {
        // Arrange
        var attackerId = Guid.NewGuid();
        var targetId = Guid.NewGuid();

        var notification1 = new UnitDamagedNotification(
            attackerId, targetId, 50, 150, 200
        );
        var notification2 = new UnitDamagedNotification(
            attackerId, targetId, 50, 150, 200
        );

        // Act & Assert
        notification1.Should().Be(notification2);
    }

    #endregion

    #region PlayerStateNotification Tests

    [Fact]
    public void PlayerStateNotification_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var id = Guid.NewGuid();
        var isConnected = true;
        var connectionId = "conn-123";
        var currentElixir = 7.5m;

        // Act
        var notification = new PlayerStateNotification(
            id, isConnected, connectionId, currentElixir
        );

        // Assert
        notification.Id.Should().Be(id);
        notification.IsConnected.Should().BeTrue();
        notification.ConnectionId.Should().Be(connectionId);
        notification.CurrentElixir.Should().Be(currentElixir);
    }

    [Fact]
    public void PlayerStateNotification_WithDisconnectedPlayer_ShouldCreateInstance()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var notification = new PlayerStateNotification(
            id, false, null, 5.0m
        );

        // Assert
        notification.IsConnected.Should().BeFalse();
        notification.ConnectionId.Should().BeNull();
        notification.CurrentElixir.Should().Be(5.0m);
    }

    [Fact]
    public void PlayerStateNotification_WithDifferentInstances_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();

        var notification1 = new PlayerStateNotification(
            id, true, "conn-123", 7.5m
        );
        var notification2 = new PlayerStateNotification(
            id, true, "conn-123", 7.5m
        );

        // Act & Assert
        notification1.Should().Be(notification2);
    }

    #endregion
}
