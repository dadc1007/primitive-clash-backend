using FluentAssertions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Utils.Mappers;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Utils.Mappers;

public class AuthMapperTests
{
    [Fact]
    public void ToAuthSuccessResponse_WithValidUser_ReturnsMappedResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com"
        };

        // Act
        var result = user.ToAuthSuccessResponse();

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Username.Should().Be(user.Username);
        result.Email.Should().Be(user.Email);
        result.Gold.Should().Be(user.Gold);
        result.Gems.Should().Be(user.Gems);
        result.Level.Should().Be(user.Level);
        result.Trophies.Should().Be(user.Trophies);
    }

    [Fact]
    public void ToAuthSuccessResponse_WithNullUser_ThrowsInvalidOperationException()
    {
        // Arrange
        User? user = null;

        // Act
        var act = () => user!.ToAuthSuccessResponse();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is null");
    }

    [Fact]
    public void ToAuthSuccessResponse_WithUserWithZeroValues_MapsCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "newuser",
            Email = "new@example.com"
        };

        // Act
        var result = user.ToAuthSuccessResponse();

        // Assert
        result.Gold.Should().Be(1000);
        result.Gems.Should().Be(100);
        result.Level.Should().Be(1);
        result.Trophies.Should().Be(0);
    }

    [Fact]
    public void ToAuthSuccessResponse_WithUserWithMaxValues_MapsCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "richuser",
            Email = "rich@example.com"
        };

        // Act
        var result = user.ToAuthSuccessResponse();

        // Assert
        result.Gold.Should().Be(1000);
        result.Gems.Should().Be(100);
        result.Level.Should().Be(1);
        result.Trophies.Should().Be(0);
    }
}
