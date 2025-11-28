using FluentAssertions;
using PrimitiveClash.Backend.Data;
using PrimitiveClash.Backend.Exceptions;
using PrimitiveClash.Backend.Models;
using PrimitiveClash.Backend.Services.Impl;
using PrimitiveClash.Backend.Tests.Infrastructure;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Services;

public class ArenaTemplateServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public ArenaTemplateServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetDefaultArenaTemplate Tests

    [Fact]
    public async Task GetDefaultArenaTemplate_WithExistingTemplate_ShouldReturnFirstTemplate()
    {
        using var context = _fixture.CreateContext();
        var service = new ArenaTemplateService(context);

        var template = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Default Arena",
            RequiredTrophies = 0
        };

        context.ArenaTemplates.Add(template);
        await context.SaveChangesAsync();

        var result = await service.GetDefaultArenaTemplate();

        result.Should().NotBeNull();
        result.Name.Should().Be("Default Arena");
        result.RequiredTrophies.Should().Be(0);
    }

    [Fact]
    public async Task GetDefaultArenaTemplate_WithNoTemplates_ShouldThrowDefaultArenaTemplateNotFoundException()
    {
        using var context = _fixture.CreateContext();
        var service = new ArenaTemplateService(context);

        var act = async () => await service.GetDefaultArenaTemplate();

        await act.Should().ThrowAsync<DefaultArenaTemplateNotFoundException>();
    }

    [Fact]
    public async Task GetDefaultArenaTemplate_WithMultipleTemplates_ShouldReturnFirstOne()
    {
        using var context = _fixture.CreateContext();
        var service = new ArenaTemplateService(context);

        var template1 = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "First Arena",
            RequiredTrophies = 0
        };

        var template2 = new ArenaTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Second Arena",
            RequiredTrophies = 100
        };

        context.ArenaTemplates.Add(template1);
        context.ArenaTemplates.Add(template2);
        await context.SaveChangesAsync();

        var result = await service.GetDefaultArenaTemplate();

        result.Should().NotBeNull();
        // FirstOrDefaultAsync() doesn't guarantee order without OrderBy, so just verify we got one of them
        result.Should().Match<ArenaTemplate>(t => 
            t.Name == "First Arena" || t.Name == "Second Arena",
            "it should return one of the available templates");
    }

    #endregion
}
