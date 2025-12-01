using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using PrimitiveClash.Backend.Utils;

namespace PrimitiveClash.Backend.Tests.Utils;

public class HubExtensionsTests
{
    private class TestHub : Hub
    {
        public void SetContext(HubCallerContext context)
        {
            Context = context;
        }
    }

    private TestHub CreateHubWithClaims(params Claim[] claims)
    {
        var mockContext = new Mock<HubCallerContext>();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        
        mockContext.Setup(c => c.User).Returns(claimsPrincipal);
        
        var hub = new TestHub();
        hub.SetContext(mockContext.Object);
        
        return hub;
    }

    private TestHub CreateHubWithNullUser()
    {
        var mockContext = new Mock<HubCallerContext>();
        mockContext.Setup(c => c.User).Returns((ClaimsPrincipal?)null);
        
        var hub = new TestHub();
        hub.SetContext(mockContext.Object);
        
        return hub;
    }

    #region GetAuthenticatedUserId Tests

    [Fact]
    public void GetAuthenticatedUserId_WithValidOidClaim_ShouldReturnGuid()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var hub = CreateHubWithClaims(new Claim("oid", expectedUserId.ToString()));

        // Act
        var result = hub.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithNullUser_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithNullUser();

        // Act
        Action act = () => hub.GetAuthenticatedUserId();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*no autenticado*");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithMissingOidClaim_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("other_claim", "value"));

        // Act
        Action act = () => hub.GetAuthenticatedUserId();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*no autenticado*");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithEmptyOidClaim_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("oid", ""));

        // Act
        Action act = () => hub.GetAuthenticatedUserId();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*no autenticado*");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithWhitespaceOidClaim_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("oid", "   "));

        // Act
        Action act = () => hub.GetAuthenticatedUserId();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*no autenticado*");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithInvalidGuidFormat_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("oid", "not-a-valid-guid"));

        // Act
        Action act = () => hub.GetAuthenticatedUserId();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*token inválido*");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithMultipleClaims_ShouldReturnOidValue()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var hub = CreateHubWithClaims(
            new Claim("name", "John Doe"),
            new Claim("oid", expectedUserId.ToString()),
            new Claim("email", "john@example.com")
        );

        // Act
        var result = hub.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    #endregion

    #region GetAuthenticatedUserEmail Tests

    [Fact]
    public void GetAuthenticatedUserEmail_WithValidPreferredUsernameClaim_ShouldReturnEmail()
    {
        // Arrange
        var expectedEmail = "user@example.com";
        var hub = CreateHubWithClaims(new Claim("preferred_username", expectedEmail));

        // Act
        var result = hub.GetAuthenticatedUserEmail();

        // Assert
        result.Should().Be(expectedEmail);
    }

    [Fact]
    public void GetAuthenticatedUserEmail_WithNullUser_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithNullUser();

        // Act
        Action act = () => hub.GetAuthenticatedUserEmail();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*Email no encontrado*");
    }

    [Fact]
    public void GetAuthenticatedUserEmail_WithMissingPreferredUsernameClaim_ShouldThrowHubException()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("other_claim", "value"));

        // Act
        Action act = () => hub.GetAuthenticatedUserEmail();

        // Assert
        act.Should().Throw<HubException>()
            .WithMessage("*Email no encontrado*");
    }

    [Fact]
    public void GetAuthenticatedUserEmail_WithMultipleClaims_ShouldReturnPreferredUsername()
    {
        // Arrange
        var expectedEmail = "user@example.com";
        var hub = CreateHubWithClaims(
            new Claim("name", "John Doe"),
            new Claim("preferred_username", expectedEmail),
            new Claim("oid", Guid.NewGuid().ToString())
        );

        // Act
        var result = hub.GetAuthenticatedUserEmail();

        // Assert
        result.Should().Be(expectedEmail);
    }

    [Fact]
    public void GetAuthenticatedUserEmail_WithEmptyPreferredUsername_ShouldReturnEmptyString()
    {
        // Arrange
        var hub = CreateHubWithClaims(new Claim("preferred_username", ""));

        // Act
        var result = hub.GetAuthenticatedUserEmail();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Integration Tests (Both Methods)

    [Fact]
    public void HubExtensions_WithValidAuthenticatedUser_ShouldReturnBothUserIdAndEmail()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var expectedEmail = "user@example.com";
        var hub = CreateHubWithClaims(
            new Claim("oid", expectedUserId.ToString()),
            new Claim("preferred_username", expectedEmail)
        );

        // Act
        var userId = hub.GetAuthenticatedUserId();
        var email = hub.GetAuthenticatedUserEmail();

        // Assert
        userId.Should().Be(expectedUserId);
        email.Should().Be(expectedEmail);
    }

    [Fact]
    public void HubExtensions_WithRealWorldAzureAdClaims_ShouldWorkCorrectly()
    {
        // Arrange - Simulating real Azure AD token claims
        var expectedUserId = Guid.NewGuid();
        var expectedEmail = "john.doe@company.com";
        var hub = CreateHubWithClaims(
            new Claim("aud", "api://primitive-clash"),
            new Claim("iss", "https://login.microsoftonline.com/tenant-id/v2.0"),
            new Claim("iat", "1638360000"),
            new Claim("nbf", "1638360000"),
            new Claim("exp", "1638363600"),
            new Claim("name", "John Doe"),
            new Claim("oid", expectedUserId.ToString()),
            new Claim("preferred_username", expectedEmail),
            new Claim("rh", "0.AX..."),
            new Claim("sub", "sub-value"),
            new Claim("tid", "tenant-id"),
            new Claim("uti", "uti-value"),
            new Claim("ver", "2.0")
        );

        // Act
        var userId = hub.GetAuthenticatedUserId();
        var email = hub.GetAuthenticatedUserEmail();

        // Assert
        userId.Should().Be(expectedUserId);
        email.Should().Be(expectedEmail);
    }

    #endregion
}
