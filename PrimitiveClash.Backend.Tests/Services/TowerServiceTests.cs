using FluentAssertions;
using Moq;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Models.Enums;
using PrimitiveClash.Backend.Services;
using PrimitiveClash.Backend.Services.Impl;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class TowerServiceTests
{
    private readonly Mock<ITowerTemplateService> _mockTowerTemplateService;
    private readonly TowerService _towerService;

    public TowerServiceTests()
    {
        _mockTowerTemplateService = new Mock<ITowerTemplateService>();
        _towerService = new TowerService(_mockTowerTemplateService.Object);
    }

    private TowerTemplate CreateLeaderTemplate()
    {
        return new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Leader,
            Hp = 1000,
            Damage = 100,
            Range = 5,
            Size = 4
        };
    }

    private TowerTemplate CreateGuardianTemplate()
    {
        return new TowerTemplate
        {
            Id = Guid.NewGuid(),
            Type = TowerType.Guardian,
            Hp = 500,
            Damage = 50,
            Range = 3,
            Size = 3
        };
    }

    [Fact]
    public async Task CreateLeaderTower_ShouldReturnLeaderTower()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var leaderTemplate = CreateLeaderTemplate();

        _mockTowerTemplateService
            .Setup(x => x.GetLeaderTowerTemplate())
            .ReturnsAsync(leaderTemplate);

        // Act
        var result = await _towerService.CreateLeaderTower(playerId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(playerId);
        result.TowerTemplate.Type.Should().Be(TowerType.Leader);
        result.Health.Should().Be(leaderTemplate.Hp);
    }

    [Fact]
    public async Task CreateGuardianTower_ShouldReturnGuardianTower()
    {
        // Arrange
        var playerId = Guid.NewGuid();
        var guardianTemplate = CreateGuardianTemplate();

        _mockTowerTemplateService
            .Setup(x => x.GetGuardianTowerTemplate())
            .ReturnsAsync(guardianTemplate);

        // Act
        var result = await _towerService.CreateGuardianTower(playerId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(playerId);
        result.TowerTemplate.Type.Should().Be(TowerType.Guardian);
        result.Health.Should().Be(guardianTemplate.Hp);
    }

    [Fact]
    public async Task CreateAllGameTowers_ShouldCreateTowersForBothPlayers()
    {
        // Arrange
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var leaderTemplate = CreateLeaderTemplate();
        var guardianTemplate = CreateGuardianTemplate();

        _mockTowerTemplateService
            .Setup(x => x.GetLeaderTowerTemplate())
            .ReturnsAsync(leaderTemplate);

        _mockTowerTemplateService
            .Setup(x => x.GetGuardianTowerTemplate())
            .ReturnsAsync(guardianTemplate);

        // Act
        var result = await _towerService.CreateAllGameTowers(player1Id, player2Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().ContainKey(player1Id);
        result.Should().ContainKey(player2Id);

        result[player1Id].Should().HaveCount(3);
        result[player1Id].Should().Contain(t => t.TowerTemplate.Type == TowerType.Leader);
        result[player1Id].Where(t => t.TowerTemplate.Type == TowerType.Guardian).Should().HaveCount(2);

        result[player2Id].Should().HaveCount(3);
        result[player2Id].Should().Contain(t => t.TowerTemplate.Type == TowerType.Leader);
        result[player2Id].Where(t => t.TowerTemplate.Type == TowerType.Guardian).Should().HaveCount(2);
    }
}
