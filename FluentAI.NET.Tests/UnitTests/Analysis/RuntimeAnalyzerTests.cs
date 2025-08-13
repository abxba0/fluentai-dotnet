using FluentAI.Abstractions.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Analysis
{
    /// <summary>
    /// Unit tests for the RuntimeAnalyzer components.
    /// </summary>
    public class RuntimeAnalyzerTests
    {
        private readonly Mock<ILogger<DefaultRuntimeAnalyzer>> _mockLogger;
        private readonly DefaultRuntimeAnalyzer _analyzer;

        public RuntimeAnalyzerTests()
        {
            _mockLogger = new Mock<ILogger<DefaultRuntimeAnalyzer>>();
            _analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithValidCode_ReturnsResult()
        {
            // Arrange
            var sourceCode = @"
                public class TestClass
                {
                    public void TestMethod()
                    {
                        Console.WriteLine(""Hello World"");
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "TestClass.cs");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Metadata);
            Assert.Equal("TestClass.cs", result.Metadata.AnalyzedFiles.Single());
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithRiskyCode_DetectsIssues()
        {
            // Arrange
            var sourceCode = @"
                public class RiskyClass
                {
                    public void RiskyMethod()
                    {
                        string value = null;
                        var client = new HttpClient();
                        var result = client.GetStringAsync(""http://example.com"");
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "RiskyClass.cs");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalIssueCount > 0, "Should detect runtime issues in risky code");
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithNullCode_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _analyzer.AnalyzeSourceAsync(null!, "test.cs"));
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithEmptyCode_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _analyzer.AnalyzeSourceAsync("", "test.cs"));
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithAsyncCode_DetectsAsyncIssues()
        {
            // Arrange
            var sourceCode = @"
                public class AsyncClass
                {
                    public async Task AsyncMethod()
                    {
                        // No cancellation token
                        await Task.Delay(1000);
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "AsyncClass.cs");

            // Assert
            Assert.NotNull(result);
            // Should detect async methods without cancellation tokens
            Assert.True(result.RuntimeIssues.Any(i => i.Description.Contains("cancellation")), 
                "Should detect async methods without cancellation support");
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithDatabaseCode_DetectsEnvironmentRisks()
        {
            // Arrange
            var sourceCode = @"
                public class DatabaseClass
                {
                    public void ExecuteQuery()
                    {
                        foreach(var item in items)
                        {
                            connection.ExecuteQuery(""SELECT * FROM table WHERE id = "" + item.Id);
                        }
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "DatabaseClass.cs");

            // Assert
            Assert.NotNull(result);
            // Should detect N+1 query pattern
            Assert.True(result.EnvironmentRisks.Any(r => r.Description.Contains("N+1")), 
                "Should detect N+1 query patterns");
        }

        [Fact]
        public async Task AnalyzeSourceAsync_WithParsingCode_DetectsEdgeCases()
        {
            // Arrange
            var sourceCode = @"
                public class ParsingClass
                {
                    public int ParseValue(string input)
                    {
                        return int.Parse(input);
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ParsingClass.cs");

            // Assert
            Assert.NotNull(result);
            // Should detect int.Parse without TryParse
            Assert.True(result.EdgeCaseFailures.Any(e => e.Input.Contains("Non-numeric")), 
                "Should detect int.Parse edge cases");
        }

        [Fact]
        public async Task AnalyzeFilesAsync_WithMultipleFiles_CombinesResults()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var file1 = Path.Combine(tempDir, "test1.cs");
            var file2 = Path.Combine(tempDir, "test2.cs");

            try
            {
                await File.WriteAllTextAsync(file1, "public class Test1 { }");
                await File.WriteAllTextAsync(file2, "public class Test2 { }");

                // Act
                var result = await _analyzer.AnalyzeFilesAsync(new[] { file1, file2 });

                // Assert
                Assert.NotNull(result);
                Assert.Equal(2, result.Metadata.AnalyzedFiles.Count);
            }
            finally
            {
                // Cleanup
                if (File.Exists(file1)) File.Delete(file1);
                if (File.Exists(file2)) File.Delete(file2);
            }
        }

        [Fact]
        public void RuntimeAnalysisResult_HasCriticalIssues_ReturnsTrueForCriticalSeverity()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Critical }
                }
            };

            // Act & Assert
            Assert.True(result.HasCriticalIssues);
        }

        [Fact]
        public void RuntimeAnalysisResult_HasCriticalIssues_ReturnsTrueForHighRiskEnvironment()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                EnvironmentRisks = new[]
                {
                    new EnvironmentRisk { Likelihood = RiskLikelihood.High }
                }
            };

            // Act & Assert
            Assert.True(result.HasCriticalIssues);
        }

        [Fact]
        public void RuntimeAnalysisResult_TotalIssueCount_SumsAllIssueTypes()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[] { new RuntimeIssue(), new RuntimeIssue() },
                EnvironmentRisks = new[] { new EnvironmentRisk() },
                EdgeCaseFailures = new[] { new EdgeCaseFailure(), new EdgeCaseFailure(), new EdgeCaseFailure() }
            };

            // Act & Assert
            Assert.Equal(6, result.TotalIssueCount);
        }
    }
}