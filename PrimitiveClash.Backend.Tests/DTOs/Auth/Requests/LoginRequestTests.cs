using FluentAssertions;
using PrimitiveClash.Backend.DTOs.Auth.Requests;
using Xunit;

namespace PrimitiveClash.Backend.Tests.DTOs.Auth.Requests;

public class LoginRequestTests
{
    [Fact]
    public void LoginRequest_Properties_AreCorrectlySet()
    {
        // Arrange & Act
        var email = "user@test.com";
        var password = "securePassword";
        var request = new LoginRequest(email, password);

        // Assert
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void LoginRequest_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var request = new LoginRequest(
            Email: "test@example.com",
            Password: "password123"
        );

        // Assert
        request.Should().NotBeNull();
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void LoginRequest_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var request1 = new LoginRequest("test@example.com", "password123");
        var request2 = new LoginRequest("test@example.com", "password123");
        var request3 = new LoginRequest("other@example.com", "password123");

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }
}
