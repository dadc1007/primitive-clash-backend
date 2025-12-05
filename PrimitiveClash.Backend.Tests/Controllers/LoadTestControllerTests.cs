using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.Controllers;
using PrimitiveClash.Backend.DTOs.LoadTest.Responses;
using Xunit;

namespace PrimitiveClash.Backend.Tests.Controllers;

public class LoadTestControllerTests
{
    private readonly LoadTestController _controller;

    public LoadTestControllerTests()
    {
        _controller = new LoadTestController();
    }

    [Fact]
    public void CpuIntensive_ShouldReturnOkWithCpuResponse()
    {
        // Act
        var result = _controller.CpuIntensive();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<CpuResponse>();
        
        var response = okResult.Value as CpuResponse;
        response!.Status.Should().Be("ok");
        response.Result.Should().NotBe(0);
        response.Hash.Should().NotBeNullOrEmpty();
        response.Server.Should().NotBeNullOrEmpty();
        response.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CpuIntensive_ShouldPerformMathematicalCalculations()
    {
        // Act
        var result = _controller.CpuIntensive();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as CpuResponse;
        
        // The result should be a non-zero number due to the calculations
        // Note: Due to logarithm operations with sin/cos, result can be negative
        response!.Result.Should().NotBe(0);
    }

    [Fact]
    public void CpuIntensive_ShouldGenerateValidHash()
    {
        // Act
        var result = _controller.CpuIntensive();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as CpuResponse;
        
        // Base64 string should be valid and non-empty
        response!.Hash.Should().NotBeNullOrEmpty();
        response.Hash.Length.Should().BeGreaterThan(0);
        
        // Should be valid Base64
        var hashBytes = Convert.FromBase64String(response.Hash);
        hashBytes.Should().NotBeNull();
        hashBytes.Length.Should().Be(32); // SHA256 produces 32 bytes
    }

    [Fact]
    public void CpuIntensive_MultipleCalls_ShouldProduceDifferentResults()
    {
        // Act
        var result1 = _controller.CpuIntensive();
        var result2 = _controller.CpuIntensive();

        // Assert
        var okResult1 = result1 as OkObjectResult;
        var okResult2 = result2 as OkObjectResult;
        
        var response1 = okResult1!.Value as CpuResponse;
        var response2 = okResult2!.Value as CpuResponse;

        // Hash should be different due to random data
        response1!.Hash.Should().NotBe(response2!.Hash);
    }

    [Fact]
    public void ProcessData_ShouldReturnOkWithProcessResponse()
    {
        // Act
        var result = _controller.ProcessData();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeOfType<ProcessResponse>();
        
        var response = okResult.Value as ProcessResponse;
        response!.Processed.Should().BeTrue();
        response.Batches.Should().Be(20);
        response.Server.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProcessData_ShouldProcessMultipleBatches()
    {
        // Act
        var result = _controller.ProcessData();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ProcessResponse;
        
        response!.Batches.Should().BeGreaterThan(0);
        response.Processed.Should().BeTrue();
    }

    [Fact]
    public void ProcessData_ShouldIncludeServerName()
    {
        // Act
        var result = _controller.ProcessData();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ProcessResponse;
        
        response!.Server.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void CpuIntensive_ShouldIncludeServerName()
    {
        // Act
        var result = _controller.CpuIntensive();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as CpuResponse;
        
        response!.Server.Should().Be(Environment.MachineName);
    }

    [Fact]
    public void ProcessData_MultipleCalls_ShouldBeConsistent()
    {
        // Act
        var result1 = _controller.ProcessData();
        var result2 = _controller.ProcessData();

        // Assert
        var okResult1 = result1 as OkObjectResult;
        var okResult2 = result2 as OkObjectResult;
        
        var response1 = okResult1!.Value as ProcessResponse;
        var response2 = okResult2!.Value as ProcessResponse;

        // Should consistently process the same number of batches
        response1!.Batches.Should().Be(response2!.Batches);
        response1.Processed.Should().Be(response2.Processed);
    }
}
