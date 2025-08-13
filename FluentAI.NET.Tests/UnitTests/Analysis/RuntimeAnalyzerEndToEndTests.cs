using FluentAI.Abstractions.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace FluentAI.NET.Tests.UnitTests.Analysis
{
    /// <summary>
    /// Integration test demonstrating the end-to-end runtime analyzer functionality.
    /// </summary>
    public class RuntimeAnalyzerEndToEndTests
    {
        private readonly ITestOutputHelper _output;
        private readonly Mock<ILogger<DefaultRuntimeAnalyzer>> _mockLogger;
        private readonly DefaultRuntimeAnalyzer _analyzer;

        public RuntimeAnalyzerEndToEndTests(ITestOutputHelper output)
        {
            _output = output;
            _mockLogger = new Mock<ILogger<DefaultRuntimeAnalyzer>>();
            _analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
        }

        [Fact]
        public async Task EndToEnd_AnalyzeRealWorldProblematicCode_DetectsMultipleIssues()
        {
            // Arrange - Real-world problematic code sample
            var problematicCode = @"
                public class ProblematicService
                {
                    public static List<string> Cache = new List<string>(); // Mutable static field
                    
                    public async Task ProcessData(IEnumerable<DataRecord> records)
                    {
                        foreach(var record in records) // N+1 query pattern
                        {
                            var details = ExecuteQuery(""SELECT * FROM details WHERE id = "" + record.Id);
                            Cache.Add(ProcessRecord(record)); // Potential memory leak
                        }
                    }
                    
                    public int ParseUserInput(string input)
                    {
                        return int.Parse(input); // No error handling for parsing
                    }
                    
                    public void ProcessFile()
                    {
                        var file = new FileStream(""data.txt"", FileMode.Open); // Resource not disposed
                        var content = file.ReadToEnd();
                    }
                    
                    public async void FireAndForgetOperation() // async void is dangerous
                    {
                        await Task.Delay(1000); // No cancellation token
                    }
                    
                    public string BuildMessage(IEnumerable<string> items)
                    {
                        var result = """";
                        foreach(var item in items)
                        {
                            result += item + "",""; // String concatenation in loop
                        }
                        return result;
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(problematicCode, "ProblematicService.cs");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.TotalIssueCount > 0, "Should detect multiple issues in problematic code");

            // Verify specific issue types are detected
            Assert.True(result.RuntimeIssues.Any(), "Should detect runtime issues");
            Assert.True(result.EnvironmentRisks.Any(), "Should detect environment risks");
            Assert.True(result.EdgeCaseFailures.Any(), "Should detect edge case failures");

            // Output the analysis results for verification
            _output.WriteLine("=== RUNTIME ANALYSIS RESULTS ===");
            _output.WriteLine($"Total Issues: {result.TotalIssueCount}");
            _output.WriteLine($"Runtime Issues: {result.RuntimeIssues.Count}");
            _output.WriteLine($"Environment Risks: {result.EnvironmentRisks.Count}");
            _output.WriteLine($"Edge Case Failures: {result.EdgeCaseFailures.Count}");
            _output.WriteLine($"Has Critical Issues: {result.HasCriticalIssues}");
            _output.WriteLine("");

            // Output summary format
            var summary = RuntimeAnalysisFormatter.FormatSummary(result);
            _output.WriteLine("=== SUMMARY FORMAT ===");
            _output.WriteLine(summary);
            _output.WriteLine("");

            // Output YAML format (first 30 lines)
            var yamlReport = RuntimeAnalysisFormatter.FormatAsYaml(result);
            var yamlLines = yamlReport.Split('\n').Take(30);
            _output.WriteLine("=== YAML FORMAT (first 30 lines) ===");
            foreach (var line in yamlLines)
            {
                _output.WriteLine(line);
            }
            _output.WriteLine("");

            // Output new structured format (as specified in issue requirements)
            var structuredReport = RuntimeAnalysisFormatter.FormatAsStructuredReport(result);
            var structuredLines = structuredReport.Split('\n').Take(50);
            _output.WriteLine("=== STRUCTURED FORMAT (first 50 lines) ===");
            foreach (var line in structuredLines)
            {
                _output.WriteLine(line);
            }
        }

        [Fact]
        public async Task EndToEnd_FormattingOutputs_ProduceValidResults()
        {
            // Arrange - Use code that will actually trigger some issues
            var sampleCode = @"
                public class SampleService
                {
                    public string ProcessInput(string input)
                    {
                        return input.ToUpper(); // Potential null reference
                    }
                    
                    public int ParseNumber(string value)
                    {
                        return int.Parse(value); // Should trigger edge case detection
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(sampleCode, "SampleService.cs");

            // Assert & Test all output formats
            Assert.NotNull(result);

            // Test Summary format
            var summary = RuntimeAnalysisFormatter.FormatSummary(result);
            Assert.NotNull(summary);
            Assert.Contains("RUNTIME ANALYSIS SUMMARY", summary);

            // Test YAML format
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);
            Assert.NotNull(yaml);
            Assert.Contains("# Runtime-Aware Code Analysis Report", yaml);
            Assert.Contains("TOTAL_ISSUES:", yaml);

            // Test JSON format
            var json = RuntimeAnalysisFormatter.FormatAsJson(result);
            Assert.NotNull(json);
            Assert.Contains("\"runtimeIssues\"", json);
            Assert.Contains("\"environmentRisks\"", json);
            Assert.Contains("\"edgeCaseFailures\"", json);

            // Output for verification
            _output.WriteLine("=== ALL FORMAT VALIDATION PASSED ===");
            _output.WriteLine($"Summary length: {summary.Length} characters");
            _output.WriteLine($"YAML length: {yaml.Length} characters");
            _output.WriteLine($"JSON length: {json.Length} characters");
            _output.WriteLine($"Issues detected: {result.TotalIssueCount}");
        }

        [Fact]
        public async Task EndToEnd_StructuredFormat_MatchesRequiredFormat()
        {
            // Arrange - Code with specific issues to validate format
            var testCode = @"
                public class TestService
                {
                    public static List<string> GlobalCache = new List<string>(); // Mutable static field
                    
                    public int ParseInput(string input)
                    {
                        return int.Parse(input); // No error handling
                    }
                    
                    public async Task ProcessAsync()
                    {
                        // No try-catch in async method
                        await Task.Delay(1000);
                    }
                }";

            // Act
            var result = await _analyzer.AnalyzeSourceAsync(testCode, "TestService.cs");

            // Assert
            Assert.True(result.TotalIssueCount > 0, "Should detect issues to validate format");

            // Get structured report
            var structuredReport = RuntimeAnalysisFormatter.FormatAsStructuredReport(result);
            
            // Verify format structure
            Assert.Contains("ISSUE #", structuredReport);
            Assert.Contains("TYPE:", structuredReport);
            Assert.Contains("SEVERITY:", structuredReport);
            Assert.Contains("DESCRIPTION:", structuredReport);
            Assert.Contains("TRIGGER:", structuredReport);
            Assert.Contains("EXPECTED:", structuredReport);
            Assert.Contains("ACTUAL (Simulated):", structuredReport);
            Assert.Contains("SOLUTION:", structuredReport);
            Assert.Contains("VERIFICATION:", structuredReport);

            // Output for verification
            _output.WriteLine("=== STRUCTURED FORMAT VALIDATION ===");
            _output.WriteLine(structuredReport);
        }
    }
}