using FluentAI.Abstractions.Analysis;
using FluentAI.Services.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Analysis;

/// <summary>
/// Unit tests for edge case detection in runtime analyzer.
/// </summary>
public class EdgeCaseDetectionTests
{
    private readonly Mock<ILogger<DefaultRuntimeAnalyzer>> _mockLogger;
    private readonly DefaultRuntimeAnalyzer _analyzer;

    public EdgeCaseDetectionTests()
    {
        _mockLogger = new Mock<ILogger<DefaultRuntimeAnalyzer>>();
        _analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeSource_WithDivisionOperation_DetectsDivisionByZeroEdgeCase()
    {
        // Arrange
        var sourceCode = @"
            public class Calculator
            {
                public int Divide(int a, int b)
                {
                    return a / b;
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "Calculator.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EdgeCaseFailures);
        Assert.Contains(result.EdgeCaseFailures, e => e.Input.Contains("Zero divisor"));
        Assert.Contains(result.EdgeCaseFailures, e => e.ExpectedFailure.Contains("DivideByZeroException"));
    }

    [Fact]
    public async Task AnalyzeSource_WithIntParse_DetectsFormatExceptionEdgeCase()
    {
        // Arrange
        var sourceCode = @"
            public class Parser
            {
                public int ParseValue(string input)
                {
                    return int.Parse(input);
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "Parser.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EdgeCaseFailures);
        Assert.Contains(result.EdgeCaseFailures, e => e.Input.Contains("Non-numeric"));
        Assert.Contains(result.EdgeCaseFailures, e => e.ExpectedFailure.Contains("FormatException"));
    }

    [Fact]
    public async Task AnalyzeSource_WithMultipleDivisions_DetectsAllEdgeCases()
    {
        // Arrange
        var sourceCode = @"
            public class MathOperations
            {
                public double Calculate(int x, int y, int z)
                {
                    var result1 = x / y;
                    var result2 = result1 / z;
                    return result2;
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "MathOperations.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EdgeCaseFailures);
        // Should detect at least 2 division operations
        Assert.True(result.EdgeCaseFailures.Count(e => e.Input.Contains("Zero divisor")) >= 2,
            "Should detect multiple division operations");
    }

    [Fact]
    public async Task AnalyzeSource_WithNoEdgeCases_ReturnsEmptyEdgeCaseList()
    {
        // Arrange
        var sourceCode = @"
            public class SimpleClass
            {
                public string GetMessage()
                {
                    return ""Hello World"";
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "SimpleClass.cs");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.EdgeCaseFailures);
    }

    [Fact]
    public async Task AnalyzeSource_WithBoundaryValueCalculations_DetectsEdgeCases()
    {
        // Arrange
        var sourceCode = @"
            public class BoundaryTest
            {
                public int ProcessValue(string maxValue)
                {
                    var parsed = int.Parse(maxValue);
                    var divided = 100 / parsed;
                    return divided;
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "BoundaryTest.cs");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.EdgeCaseFailures.Count() >= 2, 
            "Should detect both int.Parse and division edge cases");
    }

    [Fact]
    public async Task AnalyzeSource_WithComplexExpression_DetectsHiddenEdgeCases()
    {
        // Arrange
        var sourceCode = @"
            public class ComplexCalc
            {
                public double Calculate(int a, int b, string c)
                {
                    var parsed = int.Parse(c);
                    return (a / b) + (parsed / 10);
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ComplexCalc.cs");

        // Assert
        Assert.NotNull(result);
        // Should detect int.Parse and multiple divisions
        Assert.True(result.EdgeCaseFailures.Count() >= 3,
            "Should detect int.Parse and division edge cases in complex expression");
    }

    [Theory]
    [InlineData("int.Parse(value)", "int.Parse")]
    [InlineData("Int32.Parse(input)", "int.Parse")]
    public async Task AnalyzeSource_WithParseVariations_DetectsAllFormats(string parseExpression, string expectedPattern)
    {
        // Arrange
        var sourceCode = $@"
            public class ParseTest
            {{
                public int Test(string value)
                {{
                    return {parseExpression};
                }}
            }}";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ParseTest.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EdgeCaseFailures);
    }

    [Fact]
    public async Task AnalyzeSource_ValidatesMetadata_IncludesEdgeCaseCount()
    {
        // Arrange
        var sourceCode = @"
            public class TestClass
            {
                public int Process(string input)
                {
                    return int.Parse(input) / 2;
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "TestClass.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Metadata);
        Assert.True(result.EdgeCaseFailures.Count() > 0);
        Assert.Equal(result.EdgeCaseFailures.Count(), 
            result.TotalIssueCount - result.RuntimeIssues.Count() - result.EnvironmentRisks.Count());
    }

    [Fact]
    public async Task AnalyzeSource_EdgeCaseSeverity_IsAppropriate()
    {
        // Arrange
        var sourceCode = @"
            public class SeverityTest
            {
                public int Divide(int x, int y)
                {
                    return x / y;
                }
                
                public int Parse(string s)
                {
                    return int.Parse(s);
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "SeverityTest.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EdgeCaseFailures);
        
        // Division by zero should be high severity
        var divEdgeCase = result.EdgeCaseFailures.FirstOrDefault(e => e.Input.Contains("Zero divisor"));
        Assert.NotNull(divEdgeCase);
        Assert.Equal(EdgeCaseSeverity.High, divEdgeCase.Severity);
        
        // Parse failures should be medium severity
        var parseEdgeCase = result.EdgeCaseFailures.FirstOrDefault(e => e.Input.Contains("Non-numeric"));
        Assert.NotNull(parseEdgeCase);
        Assert.Equal(EdgeCaseSeverity.Medium, parseEdgeCase.Severity);
    }
}
