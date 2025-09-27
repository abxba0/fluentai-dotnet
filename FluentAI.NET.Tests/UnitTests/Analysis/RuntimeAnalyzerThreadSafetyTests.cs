using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using FluentAI.Abstractions.Analysis;
using FluentAI.Services.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Analysis
{
    /// <summary>
    /// Tests for thread safety and regex accuracy fixes in DefaultRuntimeAnalyzer.
    /// </summary>
    public class RuntimeAnalyzerThreadSafetyTests
    {
        private readonly Mock<ILogger<DefaultRuntimeAnalyzer>> _mockLogger;

        public RuntimeAnalyzerThreadSafetyTests()
        {
            _mockLogger = new Mock<ILogger<DefaultRuntimeAnalyzer>>();
        }

        [Fact]
        public async Task ConcurrentAnalysis_GeneratesUniqueIssueIds()
        {
            // Arrange
            const int concurrentTasks = 10;
            const int issuesPerTask = 5;
            var analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
            var allIssueIds = new ConcurrentBag<int>();

            var problematicCode = @"
                public class ProblematicClass
                {
                    private static List<string> _cache = new List<string>();
                    
                    public async void ProcessData(string input)
                    {
                        var data = int.Parse(input);
                        var result = 10 / data;
                        
                        foreach (var item in _cache)
                        {
                            _cache.Add(item + result);
                        }
                    }
                }";

            // Act - Run multiple analysis tasks concurrently
            var tasks = Enumerable.Range(0, concurrentTasks)
                .Select(async i => 
                {
                    var result = await analyzer.AnalyzeSourceAsync(problematicCode, $"File{i}.cs");
                    foreach (var issue in result.RuntimeIssues)
                    {
                        allIssueIds.Add(issue.Id);
                    }
                    foreach (var edgeCase in result.EdgeCaseFailures)
                    {
                        allIssueIds.Add(edgeCase.Id);
                    }
                    foreach (var risk in result.EnvironmentRisks)
                    {
                        allIssueIds.Add(risk.Id);
                    }
                })
                .ToArray();

            await Task.WhenAll(tasks);

            // Assert - All IDs should be unique
            var uniqueIds = allIssueIds.Distinct().ToList();
            Assert.Equal(allIssueIds.Count, uniqueIds.Count);
            Assert.True(allIssueIds.Count >= concurrentTasks * 3); // At least 3 issues per task
        }

        [Fact]
        public async Task CollectionModificationRegex_DetectsProblematicPatterns()
        {
            // Arrange
            var analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
            var codeWithCollectionModification = @"
                public void ProcessItems()
                {
                    foreach (var item in items)
                    {
                        items.Add(ProcessItem(item));
                    }
                }";

            // Act
            var result = await analyzer.AnalyzeSourceAsync(codeWithCollectionModification, "Test.cs");

            // Assert
            Assert.True(result.RuntimeIssues.Any(i => i.Type == RuntimeIssueType.CollectionModification),
                "Should detect collection modification during iteration");
        }

        [Fact]
        public async Task StringConcatenationRegex_DetectsAllLoopTypes()
        {
            // Arrange
            var analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
            var codeWithDifferentLoops = @"
                public void TestLoops()
                {
                    string result = """";
                    
                    for (int i = 0; i < 10; i++)
                    {
                        result += ""value"";
                    }
                    
                    while (condition)
                    {
                        result += ""value"";
                    }
                    
                    foreach (var item in items)
                    {
                        result += item;
                    }
                }";

            // Act
            var result = await analyzer.AnalyzeSourceAsync(codeWithDifferentLoops, "Test.cs");

            // Assert
            var stringConcatIssues = result.RuntimeIssues.Where(i => i.Type == RuntimeIssueType.StringConcatenation).ToList();
            Assert.True(stringConcatIssues.Count >= 3, $"Should detect string concatenation in all loop types. Found: {stringConcatIssues.Count}");
        }
    }
}