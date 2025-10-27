using FluentAssertions;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Cards;
using PrimitiveClash.Backend.Models.Enums;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Models;

public class UserTests
{
    #region Gold Tests

    [Fact]
    public void AddGold_WithPositiveAmount_ShouldIncreaseGold()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        var initialGold = user.Gold;

        // Act
        user.AddGold(500);

        // Assert
        user.Gold.Should().Be(initialGold + 500);
    }

    [Fact]
    public void SpendGold_WithSufficientGold_ShouldDecreaseGold()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        var initialGold = user.Gold;

        user.SpendGold(300);

        user.Gold.Should().Be(initialGold - 300);
    }

    [Fact]
    public void SpendGold_WithInsufficientGold_ShouldThrowNotEnoughGoldException()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        var act = () => user.SpendGold(2000);

        act.Should().Throw<NotEnoughGoldException>();
    }

    #endregion

    #region Gems Tests

    [Fact]
    public void AddGems_WithPositiveAmount_ShouldIncreaseGems()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        var initialGems = user.Gems;

        user.AddGems(50);

        user.Gems.Should().Be(initialGems + 50);
    }

    [Fact]
    public void SpendGems_WithSufficientGems_ShouldDecreaseGems()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        var initialGems = user.Gems;

        user.SpendGems(30);

        user.Gems.Should().Be(initialGems - 30);
    }

    [Fact]
    public void SpendGems_WithInsufficientGems_ShouldThrowNotEnoughGemsException()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        var act = () => user.SpendGems(200);

        act.Should().Throw<NotEnoughGemsException>();
    }

    #endregion

    #region Trophies Tests

    [Fact]
    public void AddTrophies_WithPositiveAmount_ShouldIncreaseTrophies()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        user.AddTrophies(100);

        user.Trophies.Should().Be(100);
    }

    [Fact]
    public void RemoveTrophies_WithSufficientTrophies_ShouldDecreaseTrophies()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        user.AddTrophies(200);

        user.RemoveTrophies(50);

        user.Trophies.Should().Be(150);
    }

    [Fact]
    public void RemoveTrophies_WithInsufficientTrophies_ShouldThrowNotEnoughTrophiesException()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        var act = () => user.RemoveTrophies(100);

        act.Should().Throw<NotEnoughTrophiesException>();
    }

    #endregion

    #region Level Tests

    [Fact]
    public void LevelUp_ShouldIncreaseLevel()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };
        var initialLevel = user.Level;

        user.LevelUp();

        user.Level.Should().Be(initialLevel + 1);
    }

    #endregion
}
