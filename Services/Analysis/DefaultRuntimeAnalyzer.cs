using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAI.Abstractions.Analysis;
using Microsoft.Extensions.Logging;

namespace FluentAI.Services.Analysis
{
    /// <summary>
    /// Default implementation of runtime-aware code analyzer.
    /// </summary>
    public class DefaultRuntimeAnalyzer : IRuntimeAnalyzer
    {
        private readonly ILogger<DefaultRuntimeAnalyzer> _logger;
        private static int _issueIdCounter = 1;

        public DefaultRuntimeAnalyzer(ILogger<DefaultRuntimeAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RuntimeAnalysisResult> AnalyzeSourceAsync(string sourceCode, string fileName)
        {
            if (string.IsNullOrEmpty(sourceCode))
                throw new ArgumentException("Source code cannot be null or empty", nameof(sourceCode));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            _logger.LogDebug("Analyzing source code for file: {FileName}", fileName);

            var startTime = DateTime.UtcNow;
            var issues = new List<RuntimeIssue>();
            var risks = new List<EnvironmentRisk>();
            var edgeCases = new List<EdgeCaseFailure>();

            // Analyze for various runtime issues
            await AnalyzeAsyncVoidMethods(sourceCode, issues);
            await AnalyzeMutableStaticFields(sourceCode, issues);
            await AnalyzeStringConcatenationInLoops(sourceCode, issues);
            await AnalyzeResourceManagement(sourceCode, issues);
            await AnalyzeCollectionModification(sourceCode, issues);
            await AnalyzeLargeObjectAllocation(sourceCode, issues);
            await AnalyzeConnectionPoolIssues(sourceCode, issues);
            await AnalyzeNullReferenceRisks(sourceCode, issues);
            await AnalyzeDivisionByZero(sourceCode, edgeCases);
            await AnalyzeIntParseEdgeCases(sourceCode, edgeCases);
            await AnalyzeAsyncCancellation(sourceCode, issues);
            
            // Analyze for environment risks
            await AnalyzeEnvironmentRisks(sourceCode, risks);

            var endTime = DateTime.UtcNow;

            return new RuntimeAnalysisResult
            {
                RuntimeIssues = issues,
                EnvironmentRisks = risks,
                EdgeCaseFailures = edgeCases,
                Metadata = new AnalysisMetadata
                {
                    AnalysisTimestamp = startTime,
                    AnalyzedFiles = new[] { fileName },
                    AnalysisDuration = endTime - startTime,
                    AnalyzerVersion = "1.0.0"
                }
            };
        }

        public async Task<RuntimeAnalysisResult> AnalyzeFilesAsync(string[] filePaths)
        {
            if (filePaths == null || filePaths.Length == 0)
                throw new ArgumentException("File paths cannot be null or empty", nameof(filePaths));

            var allIssues = new List<RuntimeIssue>();
            var allRisks = new List<EnvironmentRisk>();
            var allEdgeCases = new List<EdgeCaseFailure>();
            var analyzedFiles = new List<string>();

            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    var content = await File.ReadAllTextAsync(filePath);
                    var result = await AnalyzeSourceAsync(content, Path.GetFileName(filePath));
                    
                    allIssues.AddRange(result.RuntimeIssues);
                    allRisks.AddRange(result.EnvironmentRisks);
                    allEdgeCases.AddRange(result.EdgeCaseFailures);
                    analyzedFiles.Add(Path.GetFileName(filePath));
                }
            }

            return new RuntimeAnalysisResult
            {
                RuntimeIssues = allIssues,
                EnvironmentRisks = allRisks,
                EdgeCaseFailures = allEdgeCases,
                Metadata = new AnalysisMetadata
                {
                    AnalyzedFiles = analyzedFiles,
                    AnalyzerVersion = "1.0.0"
                }
            };
        }

        private async Task AnalyzeAsyncVoidMethods(string sourceCode, List<RuntimeIssue> issues)
        {
            var asyncVoidPattern = @"public\s+async\s+void\s+\w+\s*\(";
            var matches = Regex.Matches(sourceCode, asyncVoidPattern);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.AsyncVoid,
                    Severity = RuntimeIssueSeverity.High,
                    Description = "Async void methods can cause unhandled exceptions and should return Task instead",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Change 'async void' to 'async Task'",
                    FilePath = "analyzed file",
                    LineNumber = GetLineNumber(sourceCode, match.Index),
                    Proof = new IssueProof
                    {
                        SimulatedExecutionStep = "Exception handling in async void context",
                        Trigger = "Unhandled exception in async void method",
                        Result = "Application crash or silent failure"
                    },
                    Solution = new IssueSolution
                    {
                        Fix = "Change method signature from 'async void' to 'async Task'",
                        Verification = "Ensure all callers await the method properly"
                    }
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeMutableStaticFields(string sourceCode, List<RuntimeIssue> issues)
        {
            var staticFieldPattern = @"private\s+static\s+(?!readonly).*List<.*>.*=.*new.*List";
            var matches = Regex.Matches(sourceCode, staticFieldPattern);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.MutableStaticField,
                    Severity = RuntimeIssueSeverity.Medium,
                    Description = "Mutable static field can cause thread safety issues and memory leaks",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Consider making the field readonly or using thread-safe collections",
                    FilePath = "analyzed file",
                    LineNumber = GetLineNumber(sourceCode, match.Index),
                    Proof = new IssueProof
                    {
                        SimulatedExecutionStep = "Concurrent access to mutable static field",
                        Trigger = "Multiple threads accessing static collection",
                        Result = "Race conditions, data corruption, or memory leaks"
                    },
                    Solution = new IssueSolution
                    {
                        Fix = "Use ConcurrentBag<T> or make field readonly with proper initialization",
                        Verification = "Test with concurrent access scenarios"
                    }
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeStringConcatenationInLoops(string sourceCode, List<RuntimeIssue> issues)
        {
            var loopStringConcatPattern = @"for\s*\([^)]*\)\s*\{[^}]*\w+\s*\+=\s*[^}]*\}";
            var matches = Regex.Matches(sourceCode, loopStringConcatPattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.StringConcatenation,
                    Severity = RuntimeIssueSeverity.Medium,
                    Description = "String concatenation in loops can cause performance issues",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Use StringBuilder for efficient string concatenation in loops"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeResourceManagement(string sourceCode, List<RuntimeIssue> issues)
        {
            var resourcePattern = @"new\s+(FileStream|HttpClient|StreamReader|StreamWriter|SqlConnection)\s*\([^)]*\)";
            var usingPattern = @"using\s*\([^)]*\)|using\s+var\s+\w+\s*=";
            
            var resourceMatches = Regex.Matches(sourceCode, resourcePattern);
            var usingMatches = Regex.Matches(sourceCode, usingPattern);

            if (resourceMatches.Count > usingMatches.Count)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.ResourceManagement,
                    Severity = RuntimeIssueSeverity.High,
                    Description = "Resource objects should be wrapped in using statements to ensure proper disposal",
                    Location = "Multiple locations",
                    SuggestedFix = "Wrap IDisposable objects in using statements"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeCollectionModification(string sourceCode, List<RuntimeIssue> issues)
        {
            var modifyDuringIterationPattern = @"foreach\s*\([^)]*\)\s*\{[^}]*\.Add\([^}]*\}";
            var matches = Regex.Matches(sourceCode, modifyDuringIterationPattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.CollectionModification,
                    Severity = RuntimeIssueSeverity.High,
                    Description = "Collection modification during iteration can cause InvalidOperationException",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Create a separate collection for new items or use for loop with index"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeLargeObjectAllocation(string sourceCode, List<RuntimeIssue> issues)
        {
            var largeArrayPattern = @"new\s+\w+\[\s*(\d+)\s*\]";
            var matches = Regex.Matches(sourceCode, largeArrayPattern);

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out int size) && size > 85000)
                {
                    issues.Add(new RuntimeIssue
                    {
                        Id = _issueIdCounter++,
                        Type = RuntimeIssueType.LargeObjectAllocation,
                        Severity = RuntimeIssueSeverity.Medium,
                        Description = "Large object allocation without disposal can impact garbage collection",
                        Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                        SuggestedFix = "Consider streaming or chunking data, or implement IDisposable"
                    });
                }
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeConnectionPoolIssues(string sourceCode, List<RuntimeIssue> issues)
        {
            var httpClientPattern = @"new\s+HttpClient\s*\(\s*\)";
            var matches = Regex.Matches(sourceCode, httpClientPattern);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.ConnectionPoolExhaustion,
                    Severity = RuntimeIssueSeverity.Medium,
                    Description = "Connection pool exhaustion risk from multiple HttpClient instances",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Use IHttpClientFactory or static HttpClient instance"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeNullReferenceRisks(string sourceCode, List<RuntimeIssue> issues)
        {
            var nullAssignmentPattern = @"string\s+\w+\s*=\s*null;";
            var matches = Regex.Matches(sourceCode, nullAssignmentPattern);

            foreach (Match match in matches)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.NullReference,
                    Severity = RuntimeIssueSeverity.Medium,
                    Description = "Null assignment without null checks can cause NullReferenceException",
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    SuggestedFix = "Add null checks before using the variable"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeDivisionByZero(string sourceCode, List<EdgeCaseFailure> edgeCases)
        {
            var divisionPattern = @"\w+\s*/\s*\w+";
            var matches = Regex.Matches(sourceCode, divisionPattern);

            foreach (Match match in matches)
            {
                edgeCases.Add(new EdgeCaseFailure
                {
                    Id = _issueIdCounter++,
                    Input = "Zero divisor",
                    Scenario = "Division operation without zero check",
                    ExpectedFailure = "DivideByZeroException",
                    Severity = EdgeCaseSeverity.High,
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeIntParseEdgeCases(string sourceCode, List<EdgeCaseFailure> edgeCases)
        {
            var parsePattern = @"int\.Parse\s*\(\s*\w+\s*\)";
            var matches = Regex.Matches(sourceCode, parsePattern);

            foreach (Match match in matches)
            {
                edgeCases.Add(new EdgeCaseFailure
                {
                    Id = _issueIdCounter++,
                    Input = "Non-numeric string",
                    Scenario = "int.Parse with invalid input",
                    ExpectedFailure = "FormatException",
                    Severity = EdgeCaseSeverity.Medium,
                    Location = $"Line {GetLineNumber(sourceCode, match.Index)}",
                    Expected = "Graceful error handling with TryParse",
                    Actual = "FormatException thrown",
                    Fix = "Use int.TryParse instead of int.Parse",
                    FilePath = "analyzed file"
                });
            }

            await Task.CompletedTask;
        }

        private async Task AnalyzeAsyncCancellation(string sourceCode, List<RuntimeIssue> issues)
        {
            var asyncMethodPattern = @"async\s+Task\s+\w+\s*\([^)]*\)";
            var cancellationTokenPattern = @"CancellationToken";
            
            var asyncMethods = Regex.Matches(sourceCode, asyncMethodPattern);
            var hasCancellationToken = Regex.IsMatch(sourceCode, cancellationTokenPattern);

            if (asyncMethods.Count > 0 && !hasCancellationToken)
            {
                issues.Add(new RuntimeIssue
                {
                    Id = _issueIdCounter++,
                    Type = RuntimeIssueType.Threading,
                    Severity = RuntimeIssueSeverity.Low,
                    Description = "Async methods without cancellation support may not be responsive to cancellation requests",
                    Location = "Async methods",
                    SuggestedFix = "Add CancellationToken parameter to async methods"
                });
            }

            await Task.CompletedTask;
        }

        private static int GetLineNumber(string text, int position)
        {
            if (position < 0 || position >= text.Length)
                return 1;

            return text.Take(position).Count(c => c == '\n') + 1;
        }

        private async Task AnalyzeEnvironmentRisks(string sourceCode, List<EnvironmentRisk> risks)
        {
            // Database dependency risks
            var databasePattern = @"ExecuteQuery\s*\(\s*[""'][^""']*[""']\s*\)|SELECT\s+.*\s+FROM|SqlConnection|SqlCommand";
            if (Regex.IsMatch(sourceCode, databasePattern, RegexOptions.IgnoreCase))
            {
                risks.Add(new EnvironmentRisk
                {
                    Id = _issueIdCounter++,
                    Type = EnvironmentRiskType.Dependency,
                    Likelihood = RiskLikelihood.High,
                    Component = "Database",
                    Description = "Database dependency failure can cause service outages",
                    Impact = "Service downtime, data inconsistency, transaction failures",
                    Mitigation = new RiskMitigation
                    {
                        RequiredChanges = new[] { "Implement circuit breaker pattern", "Add database health checks", "Implement retry logic with exponential backoff" },
                        Monitoring = "Monitor database connection pool, query response times, and error rates"
                    }
                });
            }

            // External API risks
            var apiPattern = @"HttpClient|GetStringAsync|PostAsync|PutAsync|DeleteAsync|RestClient";
            if (Regex.IsMatch(sourceCode, apiPattern, RegexOptions.IgnoreCase))
            {
                risks.Add(new EnvironmentRisk
                {
                    Id = _issueIdCounter++,
                    Type = EnvironmentRiskType.Dependency,
                    Likelihood = RiskLikelihood.Medium,
                    Component = "External API",
                    Description = "External API unavailability can impact service functionality",
                    Impact = "Feature degradation, timeout exceptions, cascade failures",
                    Mitigation = new RiskMitigation
                    {
                        RequiredChanges = new[] { "Implement timeout policies", "Add fallback mechanisms", "Cache responses when appropriate" },
                        Monitoring = "Track API response times, success rates, and availability"
                    }
                });
            }

            // Configuration risks
            var configPattern = @"ConfigurationManager|IConfiguration|appSettings|connectionString";
            if (Regex.IsMatch(sourceCode, configPattern, RegexOptions.IgnoreCase))
            {
                risks.Add(new EnvironmentRisk
                {
                    Id = _issueIdCounter++,
                    Type = EnvironmentRiskType.Configuration,
                    Likelihood = RiskLikelihood.Low,
                    Component = "Configuration",
                    Description = "Missing or invalid configuration can cause runtime failures",
                    Impact = "Service startup failures, incorrect behavior, security vulnerabilities",
                    Mitigation = new RiskMitigation
                    {
                        RequiredChanges = new[] { "Validate configuration on startup", "Provide default values", "Implement configuration monitoring" },
                        Monitoring = "Log configuration validation results and changes"
                    }
                });
            }

            await Task.CompletedTask;
        }
    }
}