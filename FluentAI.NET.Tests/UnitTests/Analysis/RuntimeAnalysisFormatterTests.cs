using FluentAI.Abstractions.Analysis;
using FluentAI.Services.Analysis;
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
            Assert.Contains("ENVIRONMENT_RISKS:", yaml);
            Assert.Contains("id: 1", yaml);
            Assert.Contains("likelihood: High", yaml);
            Assert.Contains("description: \"Connection failure risk\"", yaml);
            Assert.Contains("- \"Add retry logic\"", yaml);
            Assert.Contains("- \"Implement circuit breaker\"", yaml);
            Assert.Contains("monitoring: \"Monitor connection pool status\"", yaml);
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
            Assert.Contains("EDGE_CASE_FAILURES:", yaml);
            Assert.Contains("id: 1", yaml);
            Assert.Contains("input: \"null input\"", yaml);
            Assert.Contains("TOTAL_ISSUES: 1", yaml);
            // YAML format uses 'location' field, not 'FILE'
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
            Assert.Contains("HAS_CRITICAL_ISSUES: true", yaml); // lowercase 'true'
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
            Assert.Contains("✅", summary);
            Assert.Contains("RUNTIME ANALYSIS COMPLETE", summary);
            Assert.Contains("Total Issues: 0", summary);
            Assert.Contains("Runtime Issues: 0", summary);
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

        [Fact]
        public void FormatAsStructuredReport_WithNullResult_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                RuntimeAnalysisFormatter.FormatAsStructuredReport(null!));
        }

        [Fact]
        public void FormatAsStructuredReport_WithRuntimeIssues_MatchesRequiredFormat()
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
                        Description = "Performance degradation issue",
                        Proof = new IssueProof
                        {
                            Trigger = "Large dataset processing",
                            SimulatedExecutionStep = "Optimal performance without resource exhaustion",
                            Result = "Memory exhaustion and slow response times"
                        },
                        Solution = new IssueSolution
                        {
                            Fix = "Implement pagination and lazy loading",
                            Verification = "Load test with large datasets"
                        }
                    }
                }
            };

            // Act
            var report = RuntimeAnalysisFormatter.FormatAsStructuredReport(result);

            // Assert - Verify exact format matching the issue requirements
            Assert.Contains("ISSUE #1:", report);
            Assert.Contains("TYPE: Performance", report);
            Assert.Contains("SEVERITY: High", report);
            Assert.Contains("DESCRIPTION: Performance degradation issue", report);
            Assert.Contains("TRIGGER: Large dataset processing", report);
            Assert.Contains("EXPECTED: Optimal performance without resource exhaustion", report);
            Assert.Contains("ACTUAL (Simulated): Memory exhaustion and slow response times", report);
            Assert.Contains("SOLUTION: Implement pagination and lazy loading", report);
        }

        [Fact] 
        public void FormatAsStructuredReport_WithEnvironmentRisks_ConvertsToIssueFormat()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                EnvironmentRisks = new[]
                {
                    new EnvironmentRisk
                    {
                        Id = 2,
                        Component = "Database Connection",
                        Description = "Database connection timeout",
                        Likelihood = RiskLikelihood.High,
                        Mitigation = new RiskMitigation
                        {
                            RequiredChanges = new[] { "Add connection pooling", "Implement retry logic" },
                            Monitoring = "Monitor connection timeouts and pool exhaustion"
                        }
                    }
                }
            };

            // Act
            var report = RuntimeAnalysisFormatter.FormatAsStructuredReport(result);

            // Assert
            Assert.Contains("ISSUE #2:", report);
            Assert.Contains("TYPE: Environment", report);
            Assert.Contains("SEVERITY: High", report);
            Assert.Contains("DESCRIPTION: Database connection timeout", report);
            Assert.Contains("TRIGGER: Database Connection dependency failure or misconfiguration", report);
            Assert.Contains("EXPECTED: Graceful handling of database connection unavailability", report);
            Assert.Contains("ACTUAL (Simulated): Service failure, timeout, or exception", report);
            Assert.Contains("SOLUTION: Add connection pooling; Implement retry logic", report);
            // Note: VERIFICATION field is not included in structured report for environment risks
        }

        [Fact]
        public void FormatAsStructuredReport_WithEdgeCaseFailures_ConvertsToIssueFormat()
        {
            // Arrange
            var result = new RuntimeAnalysisResult
            {
                EdgeCaseFailures = new[]
                {
                    new EdgeCaseFailure
                    {
                        Id = 3,
                        Input = "null string parameter",
                        Expected = "ArgumentNullException with clear message",
                        Actual = "NullReferenceException during string manipulation",
                        Fix = "Add null parameter validation at method entry"
                    }
                }
            };

            // Act
            var report = RuntimeAnalysisFormatter.FormatAsStructuredReport(result);

            // Assert
            Assert.Contains("ISSUE #3:", report);
            Assert.Contains("TYPE: Logic", report);
            Assert.Contains("SEVERITY: Medium", report);
            Assert.Contains("DESCRIPTION: Edge case handling failure for null string parameter", report);
            Assert.Contains("TRIGGER: null string parameter", report);
            Assert.Contains("EXPECTED: ArgumentNullException with clear message", report);
            Assert.Contains("ACTUAL (Simulated): NullReferenceException during string manipulation", report);
            Assert.Contains("SOLUTION: Add null parameter validation at method entry", report);
            Assert.Contains("VERIFICATION: Test with edge case inputs including null string parameter", report);
        }
    }
}