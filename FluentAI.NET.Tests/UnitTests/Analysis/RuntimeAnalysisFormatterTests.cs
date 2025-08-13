using FluentAI.Abstractions.Analysis;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Analysis
{
    /// <summary>
    /// Unit tests for the RuntimeAnalysisFormatter.
    /// </summary>
    public class RuntimeAnalysisFormatterTests
    {
        [Fact]
        public void FormatAsYaml_WithNullResult_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                RuntimeAnalysisFormatter.FormatAsYaml(null!));
        }

        [Fact]
        public void FormatAsYaml_WithEmptyResult_ReturnsValidYaml()
        {
            // Arrange
            var result = new RuntimeAnalysisResult();

            // Act
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);

            // Assert
            Assert.NotNull(yaml);
            Assert.Contains("# Runtime-Aware Code Analysis Report", yaml);
            Assert.Contains("# No runtime issues detected", yaml);
            Assert.Contains("# No environment risks detected", yaml);
            Assert.Contains("# No edge case failures detected", yaml);
            Assert.Contains("TOTAL_ISSUES: 0", yaml);
        }

        [Fact]
        public void FormatAsYaml_WithRuntimeIssues_IncludesIssueDetails()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue
                    {
                        Id = 1,
                        Type = RuntimeIssueType.Performance,
                        Severity = RuntimeIssueSeverity.High,
                        Description = "Test issue",
                        FilePath = "test.cs",
                        LineNumber = 42,
                        Proof = new IssueProof
                        {
                            SimulatedExecutionStep = "Test step",
                            Trigger = "Test trigger",
                            Result = "Test result"
                        },
                        Solution = new IssueSolution
                        {
                            Fix = "Test fix",
                            Verification = "Test verification"
                        }
                    }
                }
            };

            // Act
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);

            // Assert
            Assert.Contains("ISSUE #1:", yaml);
            Assert.Contains("TYPE: Performance", yaml);
            Assert.Contains("SEVERITY: High", yaml);
            Assert.Contains("DESCRIPTION: Test issue", yaml);
            Assert.Contains("FILE: test.cs", yaml);
            Assert.Contains("LINE: 42", yaml);
            Assert.Contains("- Simulated Execution Step: Test step", yaml);
            Assert.Contains("- Trigger: Test trigger", yaml);
            Assert.Contains("- Result: Test result", yaml);
            Assert.Contains("- Fix: Test fix", yaml);
            Assert.Contains("- Verification: Test verification", yaml);
        }

        [Fact]
        public void FormatAsYaml_WithEnvironmentRisks_IncludesRiskDetails()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                EnvironmentRisks = new[]
                {
                    new EnvironmentRisk
                    {
                        Id = 1,
                        Component = "Database",
                        Description = "Connection failure risk",
                        Likelihood = RiskLikelihood.High,
                        Mitigation = new RiskMitigation
                        {
                            RequiredChanges = new[] { "Add retry logic", "Implement circuit breaker" },
                            Monitoring = "Monitor connection pool status"
                        }
                    }
                }
            };

            // Act
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);

            // Assert
            Assert.Contains("RISK #1:", yaml);
            Assert.Contains("COMPONENT: Database", yaml);
            Assert.Contains("DESCRIPTION: Connection failure risk", yaml);
            Assert.Contains("LIKELIHOOD: High", yaml);
            Assert.Contains("- Add retry logic", yaml);
            Assert.Contains("- Implement circuit breaker", yaml);
            Assert.Contains("- Monitoring: Monitor connection pool status", yaml);
        }

        [Fact]
        public void FormatAsYaml_WithEdgeCaseFailures_IncludesEdgeCaseDetails()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                EdgeCaseFailures = new[]
                {
                    new EdgeCaseFailure
                    {
                        Id = 1,
                        Input = "null input",
                        Expected = "Handle gracefully",
                        Actual = "Throws exception",
                        Fix = "Add null check",
                        FilePath = "service.cs"
                    }
                }
            };

            // Act
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);

            // Assert
            Assert.Contains("CASE #1:", yaml);
            Assert.Contains("INPUT: null input", yaml);
            Assert.Contains("EXPECTED: Handle gracefully", yaml);
            Assert.Contains("ACTUAL (Simulated): Throws exception", yaml);
            Assert.Contains("FIX: Add null check", yaml);
            Assert.Contains("FILE: service.cs", yaml);
        }

        [Fact]
        public void FormatAsYaml_WithAllIssueTypes_IncludesSummary()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Critical },
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.High },
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Medium }
                },
                EnvironmentRisks = new[]
                {
                    new EnvironmentRisk { Likelihood = RiskLikelihood.High },
                    new EnvironmentRisk { Likelihood = RiskLikelihood.Medium }
                },
                EdgeCaseFailures = new[]
                {
                    new EdgeCaseFailure()
                }
            };

            // Act
            var yaml = RuntimeAnalysisFormatter.FormatAsYaml(result);

            // Assert
            Assert.Contains("TOTAL_ISSUES: 6", yaml);
            Assert.Contains("CRITICAL_ISSUES: 1", yaml);
            Assert.Contains("HIGH_SEVERITY_ISSUES: 1", yaml);
            Assert.Contains("HIGH_RISK_ENVIRONMENTS: 1", yaml);
            Assert.Contains("EDGE_CASE_FAILURES: 1", yaml);
            Assert.Contains("HAS_CRITICAL_ISSUES: True", yaml);
        }

        [Fact]
        public void FormatAsJson_WithResult_ReturnsValidJson()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue { Id = 1, Description = "Test issue" }
                }
            };

            // Act
            var json = RuntimeAnalysisFormatter.FormatAsJson(result);

            // Assert
            Assert.NotNull(json);
            Assert.Contains("\"runtimeIssues\"", json);
            Assert.Contains("\"description\": \"Test issue\"", json);
        }

        [Fact]
        public void FormatSummary_WithNoIssues_ReturnsPositiveSummary()
        {
            // Arrange
            var result = new RuntimeAnalysisResult();

            // Act
            var summary = RuntimeAnalysisFormatter.FormatSummary(result);

            // Assert
            Assert.Contains("No runtime issues detected", summary);
            Assert.Contains("Great! No runtime issues detected", summary);
            Assert.Contains("✅", summary);
        }

        [Fact]
        public void FormatSummary_WithCriticalIssues_ReturnsWarningSummary()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Critical }
                }
            };

            // Act
            var summary = RuntimeAnalysisFormatter.FormatSummary(result);

            // Assert
            Assert.Contains("CRITICAL ISSUES DETECTED", summary);
            Assert.Contains("⚠️", summary);
            Assert.Contains("Critical: 1", summary);
            Assert.Contains("Recommendations:", summary);
        }

        [Fact]
        public void FormatSummary_WithMultipleIssueTypes_ShowsBreakdown()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                RuntimeIssues = new[]
                {
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.High },
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Medium },
                    new RuntimeIssue { Severity = RuntimeIssueSeverity.Low }
                },
                EnvironmentRisks = new[]
                {
                    new EnvironmentRisk { Likelihood = RiskLikelihood.High },
                    new EnvironmentRisk { Likelihood = RiskLikelihood.Medium }
                },
                EdgeCaseFailures = new[]
                {
                    new EdgeCaseFailure(),
                    new EdgeCaseFailure()
                }
            };

            // Act
            var summary = RuntimeAnalysisFormatter.FormatSummary(result);

            // Assert
            Assert.Contains("Runtime Issues: 3", summary);
            Assert.Contains("High: 1", summary);
            Assert.Contains("Medium: 1", summary);
            Assert.Contains("Low: 1", summary);
            Assert.Contains("Environment Risks: 2", summary);
            Assert.Contains("High Likelihood: 1", summary);
            Assert.Contains("Medium Likelihood: 1", summary);
            Assert.Contains("Edge Case Failures: 2", summary);
        }
    }
}