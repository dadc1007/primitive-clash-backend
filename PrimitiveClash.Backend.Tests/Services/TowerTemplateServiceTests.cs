// using FluentAssertions;
// using PrimitiveClash.Backend.Data;
// using PrimitiveClash.Backend.Exceptions;
// using PrimitiveClash.Backend.Models;
// using PrimitiveClash.Backend.Models.Enums;
// using PrimitiveClash.Backend.Services.Impl;
// using PrimitiveClash.Backend.Tests.Infrastructure;
// using Xunit;

// namespace PrimitiveClash.Backend.Tests.Services;

// public class TowerTemplateServiceTests : IClassFixture<DatabaseFixture>
// {
//     private readonly DatabaseFixture _fixture;

//     public TowerTemplateServiceTests(DatabaseFixture fixture)
//     {
//         _fixture = fixture;
//     }

//     #region GetLeaderTowerTemplate Tests

//     [Fact]
//     public async Task GetLeaderTowerTemplate_WithExistingLeader_ShouldReturnLeaderTemplate()
//     {
//         using var context = _fixture.CreateContext();
//         var service = new TowerTemplateService(context);

//         var leaderTemplate = new TowerTemplate
//         {
//             Id = Guid.NewGuid(),
//             Type = TowerType.Leader,
//             Hp = 1000,
//             Damage = 100,
//             Range = 5
//         };

//         context.TowerTemplates.Add(leaderTemplate);
//         await context.SaveChangesAsync();

//         var result = await service.GetLeaderTowerTemplate();

//         result.Should().NotBeNull();
//         result.Type.Should().Be(TowerType.Leader);
//         result.Hp.Should().Be(1000);
//     }

//     [Fact]
//     public async Task GetLeaderTowerTemplate_WithNoLeader_ShouldThrowTowerTemplateNotFoundException()
//     {
//         using var context = _fixture.CreateContext();
//         var service = new TowerTemplateService(context);

//         var act = async () => await service.GetLeaderTowerTemplate();

//         await act.Should().ThrowAsync<TowerTemplateNotFoundException>();
//     }

//     #endregion

//     #region GetGuardianTowerTemplate Tests

//     [Fact]
//     public async Task GetGuardianTowerTemplate_WithExistingGuardian_ShouldReturnGuardianTemplate()
//     {
//         using var context = _fixture.CreateContext();
//         var service = new TowerTemplateService(context);

//         var guardianTemplate = new TowerTemplate
//         {
//             Id = Guid.NewGuid(),
//             Type = TowerType.Guardian,
//             Hp = 500,
//             Damage = 50,
//             Range = 3
//         };

//         context.TowerTemplates.Add(guardianTemplate);
//         await context.SaveChangesAsync();

//         var result = await service.GetGuardianTowerTemplate();

//         result.Should().NotBeNull();
//         result.Type.Should().Be(TowerType.Guardian);
//         result.Hp.Should().Be(500);
//     }

//     [Fact]
//     public async Task GetGuardianTowerTemplate_WithNoGuardian_ShouldThrowTowerTemplateNotFoundException()
//     {
//         using var context = _fixture.CreateContext();
//         var service = new TowerTemplateService(context);

//         var act = async () => await service.GetGuardianTowerTemplate();

//         await act.Should().ThrowAsync<TowerTemplateNotFoundException>();
//     }

//     [Fact]
//     public async Task GetGuardianTowerTemplate_WithMultipleGuardians_ShouldReturnFirst()
//     {
//         using var context = _fixture.CreateContext();
//         var service = new TowerTemplateService(context);

//         // Note: TowerTemplate has a unique index on Type, so we can only have one Guardian template
//         // This test verifies that the service correctly returns the Guardian template
//         var guardianTemplate = new TowerTemplate
//         {
//             Id = Guid.NewGuid(),
//             Type = TowerType.Guardian,
//             Hp = 500,
//             Damage = 50,
//             Range = 3
//         };

//         context.TowerTemplates.Add(guardianTemplate);
//         await context.SaveChangesAsync();

//         var result = await service.GetGuardianTowerTemplate();

//         result.Should().NotBeNull();
//         result.Type.Should().Be(TowerType.Guardian);
//         result.Id.Should().Be(guardianTemplate.Id);
//     }

//     #endregion
// }
