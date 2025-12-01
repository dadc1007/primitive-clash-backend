using FluentAssertions;
using PrimitiveClash.Backend.DTOs.Auth.Requests;
using Xunit;

namespace PrimitiveClash.Backend.Tests.DTOs.Auth.Requests;

public class SignupRequestTests
{
    [Fact]
    public void SignupRequest_Properties_AreCorrectlySet()
    {
        // Arrange & Act
        var username = "johndoe";
        var email = "john@example.com";
        var password = "securePassword123";
        var request = new SignupRequest(username, email, password);

        // Assert
        request.Username.Should().Be(username);
        request.Email.Should().Be(email);
        request.Password.Should().Be(password);
    }

    [Fact]
    public void SignupRequest_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var request = new SignupRequest(
            Username: "testuser",
            Email: "test@example.com",
            Password: "password123"
        );

        // Assert
        request.Should().NotBeNull();
        request.Username.Should().Be("testuser");
        request.Email.Should().Be("test@example.com");
        request.Password.Should().Be("password123");
    }

    [Fact]
    public void SignupRequest_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var request1 = new SignupRequest("testuser", "test@example.com", "password123");
        var request2 = new SignupRequest("testuser", "test@example.com", "password123");
        var request3 = new SignupRequest("otheruser", "test@example.com", "password123");

        // Assert
        request1.Should().Be(request2);
        request1.Should().NotBe(request3);
    }
}
