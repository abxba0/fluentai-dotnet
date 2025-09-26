using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Default implementation of runtime-aware code analyzer that follows the 5-step methodology:
    /// 1. Static Review (baseline issues)
    /// 2. Runtime Simulation (execution path analysis)
    /// 3. Environment and Dependency Checks
    /// 4. Input and Edge Simulation
    /// 5. Error Propagation Analysis
    /// </summary>
    public class DefaultRuntimeAnalyzer : IRuntimeAnalyzer
    {
        private readonly ILogger<DefaultRuntimeAnalyzer> _logger;
        private readonly List<RuntimeIssue> _runtimeIssues = new();
        private readonly List<EnvironmentRisk> _environmentRisks = new();
        private readonly List<EdgeCaseFailure> _edgeCaseFailures = new();
        private int _runtimeIssueIdCounter = 1;
        private int _environmentRiskIdCounter = 1;
        private int _edgeCaseIdCounter = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRuntimeAnalyzer"/> class.
        /// </summary>
        /// <param name="logger">Logger for analysis activities.</param>
        public DefaultRuntimeAnalyzer(ILogger<DefaultRuntimeAnalyzer> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<RuntimeAnalysisResult> AnalyzeFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            _logger.LogInformation("Starting runtime analysis of file: {FilePath}", filePath);
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var sourceCode = await File.ReadAllTextAsync(filePath, cancellationToken);
                var result = await AnalyzeSourceInternalAsync(sourceCode, filePath, cancellationToken);
                
                stopwatch.Stop();
                _logger.LogInformation("Completed runtime analysis of {FilePath} in {Duration}ms", 
                    filePath, stopwatch.ElapsedMilliseconds);

                return result with 
                { 
                    Metadata = result.Metadata with 
                    { 
                        Duration = stopwatch.Elapsed,
                        AnalyzedFiles = new[] { filePath }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing file: {FilePath}", filePath);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<RuntimeAnalysisResult> AnalyzeFilesAsync(IEnumerable<string> filePaths, CancellationToken cancellationToken = default)
        {
            if (filePaths == null)
                throw new ArgumentNullException(nameof(filePaths));

            var filePathList = filePaths.ToList();
            if (!filePathList.Any())
                return new RuntimeAnalysisResult();

            _logger.LogInformation("Starting runtime analysis of {FileCount} files", filePathList.Count);
            var stopwatch = Stopwatch.StartNew();

            var allIssues = new List<RuntimeIssue>();
            var allRisks = new List<EnvironmentRisk>();
            var allEdgeCases = new List<EdgeCaseFailure>();
            var globalRuntimeIssueId = 1;
            var globalEnvironmentRiskId = 1;  
            var globalEdgeCaseId = 1;

            foreach (var filePath in filePathList)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    var result = await AnalyzeFileAsync(filePath, cancellationToken);
                    
                    // Re-assign IDs to ensure global uniqueness across files
                    foreach (var issue in result.RuntimeIssues)
                    {
                        allIssues.Add(issue with { Id = globalRuntimeIssueId++ });
                    }
                    foreach (var risk in result.EnvironmentRisks)
                    {
                        allRisks.Add(risk with { Id = globalEnvironmentRiskId++ });
                    }
                    foreach (var edgeCase in result.EdgeCaseFailures)
                    {
                        allEdgeCases.Add(edgeCase with { Id = globalEdgeCaseId++ });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping file due to error: {FilePath}", filePath);
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("Completed runtime analysis of {FileCount} files in {Duration}ms", 
                filePathList.Count, stopwatch.ElapsedMilliseconds);

            return new RuntimeAnalysisResult
            {
                RuntimeIssues = allIssues,
                EnvironmentRisks = allRisks,
                EdgeCaseFailures = allEdgeCases,
                Metadata = new AnalysisMetadata
                {
                    Duration = stopwatch.Elapsed,
                    AnalyzedFiles = filePathList
                }
            };
        }

        /// <inheritdoc />
        public async Task<RuntimeAnalysisResult> AnalyzeDirectoryAsync(
            string directoryPath, 
            string searchPattern = "*.cs", 
            bool includeSubdirectories = true, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var searchOption = includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);

            _logger.LogInformation("Found {FileCount} files matching pattern '{Pattern}' in directory: {DirectoryPath}", 
                files.Length, searchPattern, directoryPath);

            return await AnalyzeFilesAsync(files, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<RuntimeAnalysisResult> AnalyzeSourceAsync(string sourceCode, string? fileName = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
                throw new ArgumentException("Source code cannot be null or empty.", nameof(sourceCode));

            var stopwatch = Stopwatch.StartNew();
            var result = await AnalyzeSourceInternalAsync(sourceCode, fileName, cancellationToken);
            stopwatch.Stop();

            return result with 
            { 
                Metadata = result.Metadata with 
                { 
                    Duration = stopwatch.Elapsed,
                    AnalyzedFiles = fileName != null ? new[] { fileName } : Array.Empty<string>()
                }
            };
        }

        private async Task<RuntimeAnalysisResult> AnalyzeSourceInternalAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            // Clear previous analysis results
            _runtimeIssues.Clear();
            _environmentRisks.Clear();
            _edgeCaseFailures.Clear();
            _runtimeIssueIdCounter = 1;
            _environmentRiskIdCounter = 1;
            _edgeCaseIdCounter = 1;

            // Step 1: Static Review (Baseline)
            await PerformStaticReviewAsync(sourceCode, fileName, cancellationToken);

            // Step 2: Runtime Simulation
            await PerformRuntimeSimulationAsync(sourceCode, fileName, cancellationToken);

            // Step 3: Environment & Dependency Checks
            await PerformEnvironmentChecksAsync(sourceCode, fileName, cancellationToken);

            // Step 4: Input & Edge Simulation
            await PerformEdgeCaseSimulationAsync(sourceCode, fileName, cancellationToken);

            // Step 5: Error Propagation Analysis
            await PerformErrorPropagationAnalysisAsync(sourceCode, fileName, cancellationToken);

            return new RuntimeAnalysisResult
            {
                RuntimeIssues = _runtimeIssues.ToList(),
                EnvironmentRisks = _environmentRisks.ToList(),
                EdgeCaseFailures = _edgeCaseFailures.ToList(),
                Metadata = new AnalysisMetadata
                {
                    AnalysisTimestamp = DateTime.UtcNow
                }
            };
        }

        private async Task PerformStaticReviewAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing static review for {FileName}", fileName ?? "source code");

            // Check for obvious syntax issues and logic flaws
            await Task.Run(() =>
            {
                var lines = sourceCode.Split('\n');
                
                for (int i = 0; i < lines.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    
                    var line = lines[i];
                    var lineNumber = i + 1;

                    // Check for potentially risky constructs
                    CheckForRiskyPatterns(line, lineNumber, fileName);
                    
                    // Check for missing error handling
                    CheckForMissingErrorHandling(line, lineNumber, fileName);
                    
                    // Check for resource management issues
                    CheckForResourceManagementIssues(line, lineNumber, fileName);
                }
            }, cancellationToken);
        }

        private async Task PerformRuntimeSimulationAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing runtime simulation for {FileName}", fileName ?? "source code");

            await Task.Run(() =>
            {
                // Simulate execution paths and identify potential runtime issues
                SimulateAsyncOperations(sourceCode, fileName);
                SimulateMemoryUsage(sourceCode, fileName);
                SimulateConcurrencyIssues(sourceCode, fileName);
                SimulateExceptionHandling(sourceCode, fileName);
                SimulateResourceExhaustion(sourceCode, fileName);
            }, cancellationToken);
        }

        private async Task PerformEnvironmentChecksAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing environment checks for {FileName}", fileName ?? "source code");

            await Task.Run(() =>
            {
                // Check for external dependencies and environment requirements
                CheckExternalDependencies(sourceCode, fileName);
                CheckConfigurationDependencies(sourceCode, fileName);
                CheckPerformanceBottlenecks(sourceCode, fileName);
                CheckNetworkDependencies(sourceCode, fileName);
                CheckSystemResourceDependencies(sourceCode, fileName);
            }, cancellationToken);
        }

        private async Task PerformEdgeCaseSimulationAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing edge case simulation for {FileName}", fileName ?? "source code");

            await Task.Run(() =>
            {
                // Simulate boundary conditions and edge cases
                CheckBoundaryValues(sourceCode, fileName);
                CheckInputValidation(sourceCode, fileName);
                CheckDataTypeHandling(sourceCode, fileName);
                CheckScalingLimits(sourceCode, fileName);
                CheckPartialFailureScenarios(sourceCode, fileName);
            }, cancellationToken);
        }

        private async Task PerformErrorPropagationAnalysisAsync(string sourceCode, string? fileName, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Performing error propagation analysis for {FileName}", fileName ?? "source code");

            await Task.Run(() =>
            {
                // Analyze how errors propagate through the system
                CheckErrorPropagationPaths(sourceCode, fileName);
                CheckSilentFailures(sourceCode, fileName);
            }, cancellationToken);
        }

        #region Static Analysis Methods

        private void CheckForRiskyPatterns(string line, int lineNumber, string? fileName)
        {
            // Check for potential null reference issues
            if (Regex.IsMatch(line, @"\w+\.\w+\s*=\s*null") && !line.Contains("?"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Potential null reference assignment without null-safety",
                    $"Assignment of null value detected at line {lineNumber}",
                    "Variable could be dereferenced without null check",
                    "Null reference exception at runtime",
                    "Add null checks or use nullable reference types",
                    "Test with null inputs to verify exception handling",
                    fileName, lineNumber);
            }

            // Check for proper async method declarations (improved logic)
            if (Regex.IsMatch(line, @"public\s+async\s+\w+\s+\w+\s*\(") && !line.Contains("CancellationToken"))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "Async method without cancellation token",
                    $"Async method declaration detected at line {lineNumber}",
                    "Method marked async but lacks cancellation support",
                    "Long-running operations cannot be cancelled properly",
                    "Add CancellationToken parameter to async methods",
                    "Test cancellation behavior with long-running operations",
                    fileName, lineNumber);
            }
        }

        private void CheckForMissingErrorHandling(string line, int lineNumber, string? fileName)
        {
            // Check for network calls without error handling
            if (Regex.IsMatch(line, @"(HttpClient|WebRequest|WebClient)") && !line.Contains("try"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Network operation without error handling",
                    $"Network call detected at line {lineNumber}",
                    "Network operations without try-catch blocks",
                    "Unhandled network exceptions can crash the application",
                    "Wrap network calls in try-catch blocks with appropriate error handling",
                    "Test with network failures and timeouts",
                    fileName, lineNumber);
            }

            // Check for database operations without error handling
            if (Regex.IsMatch(line, @"(ExecuteQuery|ExecuteNonQuery|Open|Connect)") && !line.Contains("try"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Database operation without error handling",
                    $"Database operation detected at line {lineNumber}",
                    "Database operations without exception handling",
                    "Database connectivity issues can cause application crashes",
                    "Implement proper exception handling for database operations",
                    "Test with database connectivity failures",
                    fileName, lineNumber);
            }
        }

        private void CheckForResourceManagementIssues(string line, int lineNumber, string? fileName)
        {
            // Check for IDisposable usage without using statements
            if (Regex.IsMatch(line, @"new\s+\w*(Stream|Reader|Writer|Client|Connection)\w*") && !line.Contains("using"))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "Resource not properly disposed",
                    $"Resource allocation detected at line {lineNumber}",
                    "IDisposable resources created without using statements",
                    "Memory leaks and resource exhaustion",
                    "Use using statements or ensure Dispose is called in finally blocks",
                    "Monitor memory usage and resource handles during testing",
                    fileName, lineNumber);
            }
        }

        #endregion

        #region Runtime Simulation Methods

        private void SimulateAsyncOperations(string sourceCode, string? fileName)
        {
            var asyncMatches = Regex.Matches(sourceCode, @"async\s+\w+\s+\w+\([^)]*\)");
            foreach (Match match in asyncMatches)
            {
                var methodSignature = match.Value;
                if (!methodSignature.Contains("CancellationToken"))
                {
                    AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                        "Async method without cancellation support",
                        $"Async method '{methodSignature}' lacks cancellation token",
                        "Long-running async operations without cancellation",
                        "Operations cannot be cancelled, potential resource waste",
                        "Add CancellationToken parameter and check for cancellation",
                        "Test cancellation behavior under load",
                        fileName, null);
                }
            }
        }

        private void SimulateMemoryUsage(string sourceCode, string? fileName)
        {
            // Check for potential memory-intensive operations
            if (sourceCode.Contains("List<") && sourceCode.Contains("Add") && !sourceCode.Contains("Capacity"))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Low,
                    "List growth without capacity planning",
                    "List usage detected without initial capacity setting",
                    "Large lists without pre-allocated capacity",
                    "Frequent memory reallocations causing performance degradation",
                    "Set initial capacity for lists when size is known",
                    "Profile memory allocations under high load",
                    fileName, null);
            }

            // Check for string concatenation in loops
            if (Regex.IsMatch(sourceCode, @"(for|while|foreach)[^{]*\{[^}]*\w+\s*\+=\s*\w+", RegexOptions.Singleline))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "String concatenation in loops",
                    "String concatenation detected inside loop structures",
                    "Large numbers of iterations with string concatenation",
                    "Excessive memory allocations and GC pressure",
                    "Use StringBuilder for string concatenation in loops",
                    "Performance test with large iteration counts",
                    fileName, null);
            }

            // Check for large object allocations without disposal  
            var largeObjectMatches = Regex.Matches(sourceCode, @".*new\s+byte\s*\[\s*\d{4,}\s*\][^;]*;");
            foreach (Match match in largeObjectMatches)
            {  
                var line = match.Value.Trim();
                if (!line.Contains("using "))
                {
                    AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.High,
                        "Large object allocation without proper disposal",
                        "Large object allocation detected without using statement",
                        "Large objects (>85KB) created without proper lifecycle management",
                        "Memory pressure and frequent garbage collection of LOH",
                        "Use using statements for large object disposal or implement proper lifecycle management",
                        "Monitor LOH fragmentation and GC pressure under load",
                        fileName, null);
                    break; // Only report once per file
                }
            }

            // Check for potential memory leaks in event handlers
            if (Regex.IsMatch(sourceCode, @"\w+\.\w+\s*\+=\s*\w+") && sourceCode.Contains("event") && !sourceCode.Contains("-="))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "Event handler subscription without unsubscription",
                    "Event handler registration detected without corresponding unsubscription",
                    "Objects with event subscriptions that are not cleaned up",
                    "Memory leaks due to retained object references",
                    "Ensure event handlers are unsubscribed in disposal methods",
                    "Test object lifecycle and memory usage patterns",
                    fileName, null);
            }
        }

        private void SimulateConcurrencyIssues(string sourceCode, string? fileName)
        {
            // Check for static fields without thread safety
            if (Regex.IsMatch(sourceCode, @"static\s+\w+(?:<[^>]+>)?\s+\w+\s*=") && !sourceCode.Contains("readonly") && !sourceCode.Contains("lock"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Mutable static field without thread safety",
                    "Mutable static field detected without synchronization",
                    "Concurrent access to shared mutable state",
                    "Race conditions and data corruption",
                    "Make static fields readonly or add proper synchronization",
                    "Test with concurrent access scenarios",
                    fileName, null);
            }

            // Check for potential deadlock scenarios (nested locks)
            var lockMatches = Regex.Matches(sourceCode, @"lock\s*\([^)]+\)");
            if (lockMatches.Count > 1)
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Potential deadlock with multiple locks",
                    "Multiple lock statements detected in same method or class",
                    "Two threads acquiring locks in different orders",
                    "Deadlock causing application hang",
                    "Establish consistent lock ordering or use lock-free patterns",
                    "Test with concurrent threads under load to detect deadlocks",
                    fileName, null);
            }

            // Check for collection modifications during iteration
            if (Regex.IsMatch(sourceCode, @"foreach\s*\([^)]*\)[^{]*\{[^{}]*\.(?:Add|Remove|Clear)\s*\(", RegexOptions.Singleline))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Collection modification during iteration",
                    "Collection modification detected inside foreach loop",
                    "Collection modified while being enumerated",
                    "InvalidOperationException: collection was modified",
                    "Use for loop with index or create a copy of collection before iteration",
                    "Test concurrent modifications during enumeration",
                    fileName, null);
            }

            // Check for unsafe lazy initialization patterns
            if (Regex.IsMatch(sourceCode, @"if\s*\(\s*\w+\s*==\s*null\s*\)\s*\{[^}]*\w+\s*=\s*new"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.Medium,
                    "Unsafe lazy initialization pattern",
                    "Null-check initialization pattern detected without thread safety",
                    "Multiple threads executing lazy initialization simultaneously",
                    "Multiple instances created or race condition",
                    "Use Lazy<T> or double-checked locking pattern with volatile",
                    "Test lazy initialization under concurrent access",
                    fileName, null);
            }
        }

        private void SimulateExceptionHandling(string sourceCode, string? fileName)
        {
            // Check for generic catch blocks
            var genericCatchMatches = Regex.Matches(sourceCode, @"catch\s*\(\s*Exception\s+\w+\s*\)");
            foreach (Match match in genericCatchMatches)
            {
                AddRuntimeIssue(RuntimeIssueType.IncorrectOutput, RuntimeIssueSeverity.Medium,
                    "Generic exception handling",
                    "Generic Exception catch block detected",
                    "Catching all exceptions without specific handling",
                    "Important exceptions may be silently swallowed",
                    "Catch specific exception types and handle appropriately",
                    "Test with various exception scenarios",
                    fileName, null);
            }

            // Check for missing finally blocks with resource cleanup
            if (sourceCode.Contains("try") && !sourceCode.Contains("finally") && 
                Regex.IsMatch(sourceCode, @"new\s+\w*(Stream|Reader|Writer|Connection)"))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "Resource allocation in try block without finally cleanup",
                    "Resource allocation detected in try block without finally block",
                    "Exception thrown after resource allocation but before disposal",
                    "Resource leaks when exceptions occur",
                    "Use using statements or ensure cleanup in finally blocks",
                    "Test exception scenarios to verify resource disposal",
                    fileName, null);
            }

            // Check for async void methods (except event handlers)
            if (Regex.IsMatch(sourceCode, @"async\s+void\s+\w+") && !sourceCode.Contains("EventHandler"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Async void method detected",
                    "Async void method found outside of event handler context",
                    "Exception thrown in async void method",
                    "Unhandled exceptions crash the application",
                    "Use async Task instead of async void",
                    "Test exception handling in async methods",
                    fileName, null);
            }
        }

        private void SimulateResourceExhaustion(string sourceCode, string? fileName)
        {
            // Check for file handle exhaustion
            if (Regex.IsMatch(sourceCode, @"(File\.Open|new\s+FileStream)") && 
                !sourceCode.Contains("using") && !sourceCode.Contains("Dispose"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "File handle exhaustion risk",
                    "File operations without proper disposal pattern",
                    "High volume of file operations without handle cleanup",
                    "File handle exhaustion causing IOException",
                    "Use using statements for file operations",
                    "Test with large number of concurrent file operations",
                    fileName, null);
            }

            // Check for connection pool exhaustion
            var connectionMatches = Regex.Matches(sourceCode, @".*new\s+(HttpClient|SqlConnection)[^;]*;");
            foreach (Match match in connectionMatches)
            {
                var line = match.Value.Trim();
                if (!line.Contains("using ") && !line.Contains("static "))
                {
                    AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.High,
                        "Connection pool exhaustion risk",
                        "Connection objects created without proper lifecycle management",
                        "High concurrent requests creating new connections",
                        "Connection pool exhaustion and timeout exceptions",
                        "Use static HttpClient or proper connection pooling",
                        "Load test to verify connection pool behavior",
                        fileName, null);
                    break; // Only report once per file
                }
            }

            // Check for unbounded cache growth
            if (sourceCode.Contains("Dictionary") && sourceCode.Contains("Cache") && 
                !sourceCode.Contains("Remove") && !sourceCode.Contains("Evict"))
            {
                AddRuntimeIssue(RuntimeIssueType.Performance, RuntimeIssueSeverity.Medium,
                    "Unbounded cache growth",
                    "Cache implementation without eviction strategy",
                    "Continuous cache additions without removal",
                    "Memory exhaustion from unlimited cache growth",
                    "Implement cache eviction policy (LRU, TTL, size limits)",
                    "Monitor cache size and memory usage over time",
                    fileName, null);
            }
        }

        #endregion

        #region Environment Analysis Methods

        private void CheckExternalDependencies(string sourceCode, string? fileName)
        {
            // Check for hardcoded URLs or endpoints
            var urlMatches = Regex.Matches(sourceCode, @"https?://[^\s""']+");
            foreach (Match match in urlMatches)
            {
                AddEnvironmentRisk("External Service Dependency",
                    $"Hardcoded URL detected: {match.Value}",
                    RiskLikelihood.Medium,
                    new[] { "Move URLs to configuration", "Implement fallback mechanisms", "Add health checks" },
                    "Monitor endpoint availability and response times");
            }

            // Check for file system dependencies
            if (Regex.IsMatch(sourceCode, @"File\.(Read|Write|Open|Create)"))
            {
                AddEnvironmentRisk("File System Dependency",
                    "File system operations detected without path validation",
                    RiskLikelihood.High,
                    new[] { "Validate file paths", "Handle file not found exceptions", "Use relative paths where possible" },
                    "Monitor file system errors and disk space");
            }
        }

        private void CheckConfigurationDependencies(string sourceCode, string? fileName)
        {
            // Check for configuration without defaults
            if (sourceCode.Contains("Configuration[") && !sourceCode.Contains("??"))
            {
                AddEnvironmentRisk("Configuration Dependency",
                    "Configuration values accessed without default fallbacks",
                    RiskLikelihood.Medium,
                    new[] { "Provide default values for configuration", "Validate configuration at startup" },
                    "Monitor configuration loading and validation errors");
            }
        }

        private void CheckPerformanceBottlenecks(string sourceCode, string? fileName)
        {
            // Check for N+1 query patterns
            if (Regex.IsMatch(sourceCode, @"foreach.*ExecuteQuery", RegexOptions.Singleline))
            {
                AddEnvironmentRisk("Database Performance",
                    "Potential N+1 query pattern detected",
                    RiskLikelihood.High,
                    new[] { "Use batch queries", "Implement query optimization", "Add query performance monitoring" },
                    "Monitor database query performance and execution counts");
            }

            // Check for synchronous calls in async context
            if (Regex.IsMatch(sourceCode, @"async.*\{[^}]*\.Result") || 
                Regex.IsMatch(sourceCode, @"async.*\{[^}]*\.Wait\(\)"))
            {
                AddEnvironmentRisk("Async Performance",
                    "Synchronous calls detected in async methods",
                    RiskLikelihood.High,
                    new[] { "Use async/await throughout call chain", "Avoid .Result and .Wait()", "Configure async properly" },
                    "Monitor thread pool starvation and deadlock scenarios");
            }
        }

        private void CheckNetworkDependencies(string sourceCode, string? fileName)
        {
            // Check for network timeouts
            if (Regex.IsMatch(sourceCode, @"(HttpClient|WebRequest)") && !sourceCode.Contains("Timeout"))
            {
                AddEnvironmentRisk("Network Timeout",
                    "Network operations without explicit timeout configuration",
                    RiskLikelihood.Medium,
                    new[] { "Set appropriate timeout values", "Implement retry policies", "Handle timeout exceptions" },
                    "Monitor network request durations and timeout rates");
            }

            // Check for missing retry logic
            if (Regex.IsMatch(sourceCode, @"(HttpClient|RestClient)") && !sourceCode.Contains("Retry") && !sourceCode.Contains("Polly"))
            {
                AddEnvironmentRisk("Network Reliability",
                    "Network calls without retry mechanisms",
                    RiskLikelihood.High,
                    new[] { "Implement exponential backoff retry", "Use Polly for resilience", "Handle transient failures" },
                    "Monitor network failure rates and retry attempts");
            }

            // Check for circuit breaker patterns
            if (sourceCode.Contains("external") && sourceCode.Contains("api") && !sourceCode.Contains("CircuitBreaker"))
            {
                AddEnvironmentRisk("External Service Dependency",
                    "External API calls without circuit breaker protection",
                    RiskLikelihood.Medium,
                    new[] { "Implement circuit breaker pattern", "Add fallback mechanisms", "Monitor service health" },
                    "Track external service availability and response times");
            }
        }

        private void CheckSystemResourceDependencies(string sourceCode, string? fileName)
        {
            // Check for disk space dependencies
            if (Regex.IsMatch(sourceCode, @"File\.(Write|Create|Copy)") && !sourceCode.Contains("DriveInfo"))
            {
                AddEnvironmentRisk("Disk Space",
                    "File operations without disk space validation",
                    RiskLikelihood.Medium,
                    new[] { "Check available disk space before operations", "Handle disk full exceptions", "Implement cleanup policies" },
                    "Monitor disk usage and available space");
            }

            // Check for CPU-intensive operations
            if (Regex.IsMatch(sourceCode, @"(Parallel\.|Task\.Run|Thread\.Start)") && 
                !sourceCode.Contains("CancellationToken") && !sourceCode.Contains("MaxDegreeOfParallelism"))
            {
                AddEnvironmentRisk("CPU Resource",
                    "CPU-intensive operations without resource limits",
                    RiskLikelihood.Medium,
                    new[] { "Set MaxDegreeOfParallelism", "Use CancellationToken", "Monitor CPU usage" },
                    "Track CPU utilization and thread pool metrics");
            }

            // Check for environment variable dependencies
            if (sourceCode.Contains("Environment.GetEnvironmentVariable") && !sourceCode.Contains("??"))
            {
                AddEnvironmentRisk("Environment Configuration",
                    "Environment variables accessed without default values",
                    RiskLikelihood.High,
                    new[] { "Provide default values", "Validate environment setup", "Document required variables" },
                    "Monitor missing environment variable errors");
            }
        }

        #endregion

        #region Edge Case Analysis Methods

        private void CheckBoundaryValues(string sourceCode, string? fileName)
        {
            // Check for array/collection access without bounds checking
            if (Regex.IsMatch(sourceCode, @"\w+\[\w+\]") && !sourceCode.Contains("Length") && !sourceCode.Contains("Count"))
            {
                AddEdgeCaseFailure(
                    "Array index out of bounds",
                    "Successful array access within bounds", 
                    "IndexOutOfRangeException when index exceeds array length",
                    "Add bounds checking before array access",
                    fileName);
            }
        }

        private void CheckInputValidation(string sourceCode, string? fileName)
        {
            // Check for missing null parameter validation
            if (Regex.IsMatch(sourceCode, @"public\s+\w+\s+\w+\([^)]*string\s+\w+") && !sourceCode.Contains("ArgumentNullException"))
            {
                AddEdgeCaseFailure(
                    "null string parameter",
                    "ArgumentNullException thrown for null parameters",
                    "NullReferenceException or undefined behavior",
                    "Add null parameter validation with ArgumentNullException",
                    fileName);
            }
        }

        private void CheckDataTypeHandling(string sourceCode, string? fileName)
        {
            // Check for integer parsing without validation
            if (Regex.IsMatch(sourceCode, @"int\.Parse\(") && !sourceCode.Contains("TryParse"))
            {
                AddEdgeCaseFailure(
                    "Non-numeric string input",
                    "TryParse returns false for invalid input",
                    "FormatException thrown for non-numeric strings",
                    "Use int.TryParse instead of int.Parse for user input",
                    fileName);
            }

            // Check for DateTime parsing without culture
            if (Regex.IsMatch(sourceCode, @"DateTime\.(Parse|ParseExact)\(") && !sourceCode.Contains("CultureInfo"))
            {
                AddEdgeCaseFailure(
                    "Date string with different cultural format",
                    "Parse handles cultural differences correctly",
                    "FormatException due to cultural date format mismatch",
                    "Use DateTime.TryParseExact with explicit culture",
                    fileName);
            }

            // Check for division operations without zero check
            if (Regex.IsMatch(sourceCode, @"\w+\s*/\s*\w+") && !sourceCode.Contains("!= 0") && !sourceCode.Contains("== 0"))
            {
                AddEdgeCaseFailure(
                    "Zero divisor value",
                    "DivideByZeroException or error handling",
                    "Application crash with DivideByZeroException",
                    "Add zero-check before division operations",
                    fileName);
            }
        }

        private void CheckScalingLimits(string sourceCode, string? fileName)
        {
            // Check for algorithms that don't scale well
            if (Regex.IsMatch(sourceCode, @"for.*for.*for"))
            {
                AddEdgeCaseFailure(
                    "Large dataset (O(n³) complexity)",
                    "Reasonable performance with small datasets",
                    "Performance degradation or timeout with large datasets",
                    "Optimize algorithm or implement pagination for large datasets",
                    fileName);
            }

            // Check for memory-intensive operations without limits
            if (Regex.IsMatch(sourceCode, @"new\s+\w*\[\]\s*\{\}") && sourceCode.Contains("Add") && !sourceCode.Contains("Count"))
            {
                AddEdgeCaseFailure(
                    "Very large collection size",
                    "OutOfMemoryException or graceful handling",
                    "Application crash due to memory exhaustion",
                    "Implement size limits and validation for collections",
                    fileName);
            }

            // Check for string operations on large data
            if (Regex.IsMatch(sourceCode, @"string\.Join\(|string\.Concat\(") && !sourceCode.Contains("Length"))
            {
                AddEdgeCaseFailure(
                    "Very large string collections",
                    "Efficient string processing",
                    "OutOfMemoryException or poor performance",
                    "Add length validation and use StringBuilder for large operations",
                    fileName);
            }
        }

        private void CheckPartialFailureScenarios(string sourceCode, string? fileName)
        {
            // Check for batch operations without partial failure handling
            if (Regex.IsMatch(sourceCode, @"foreach.*\{[^}]*(Insert|Update|Delete|Send)"))
            {
                AddEdgeCaseFailure(
                    "Partial failure in batch operation",
                    "All operations succeed or proper rollback",
                    "Some operations succeed while others fail, leaving inconsistent state",
                    "Implement transaction handling or compensating actions",
                    fileName);
            }

            // Check for API response format assumptions
            if (sourceCode.Contains("JsonConvert") && !sourceCode.Contains("try") && !sourceCode.Contains("JsonException"))
            {
                AddEdgeCaseFailure(
                    "Unexpected JSON response format",
                    "Graceful handling of format changes",
                    "JsonException or NullReferenceException",
                    "Add JSON validation and handle deserialization errors",
                    fileName);
            }

            // Check for missing null checks after external calls
            if ((Regex.IsMatch(sourceCode, @"(await\s+\w+\.\w+\().*\)\s*\.\w+") || 
                 Regex.IsMatch(sourceCode, @"(JsonConvert\.DeserializeObject|JsonSerializer\.Deserialize).*\)\s*\.\w+")) && 
                !sourceCode.Contains("?"))
            {
                AddEdgeCaseFailure(
                    "null response from external service",
                    "null check before accessing properties",
                    "NullReferenceException when service returns null",
                    "Add null checks after external service calls",
                    fileName);
            }
        }

        #endregion

        #region Error Propagation Analysis Methods

        private void CheckErrorPropagationPaths(string sourceCode, string? fileName)
        {
            // Check for unhandled exceptions in async methods
            if (Regex.IsMatch(sourceCode, @"async\s+Task\s+\w+") && !sourceCode.Contains("try"))
            {
                AddRuntimeIssue(RuntimeIssueType.Crash, RuntimeIssueSeverity.High,
                    "Unhandled exceptions in async method",
                    "Async method without exception handling",
                    "Exceptions in async operations",
                    "Unobserved task exceptions can cause application instability",
                    "Add try-catch blocks in async methods or handle at caller level",
                    "Test exception propagation in async scenarios",
                    fileName, null);
            }
        }

        private void CheckSilentFailures(string sourceCode, string? fileName)
        {
            // Check for empty catch blocks
            if (Regex.IsMatch(sourceCode, @"catch[^{]*\{\s*\}"))
            {
                AddRuntimeIssue(RuntimeIssueType.IncorrectOutput, RuntimeIssueSeverity.High,
                    "Silent failure - empty catch block",
                    "Empty catch block detected",
                    "Exceptions are caught but not handled",
                    "Failures go unnoticed, leading to incorrect application state",
                    "Add proper error handling, logging, or re-throw exceptions",
                    "Verify all error scenarios are properly reported",
                    fileName, null);
            }
        }

        #endregion

        #region Helper Methods

        private void AddRuntimeIssue(
            RuntimeIssueType type,
            RuntimeIssueSeverity severity,
            string description,
            string executionStep,
            string trigger,
            string result,
            string fix,
            string verification,
            string? filePath,
            int? lineNumber)
        {
            _runtimeIssues.Add(new RuntimeIssue
            {
                Id = _runtimeIssueIdCounter++,
                Type = type,
                Severity = severity,
                Description = description,
                Proof = new IssueProof
                {
                    SimulatedExecutionStep = executionStep,
                    Trigger = trigger,
                    Result = result
                },
                Solution = new IssueSolution
                {
                    Fix = fix,
                    Verification = verification
                },
                FilePath = filePath,
                LineNumber = lineNumber
            });
        }

        private void AddEnvironmentRisk(
            string component,
            string description,
            RiskLikelihood likelihood,
            string[] requiredChanges,
            string monitoring)
        {
            _environmentRisks.Add(new EnvironmentRisk
            {
                Id = _environmentRiskIdCounter++,
                Component = component,
                Description = description,
                Likelihood = likelihood,
                Mitigation = new RiskMitigation
                {
                    RequiredChanges = requiredChanges,
                    Monitoring = monitoring
                }
            });
        }

        private void AddEdgeCaseFailure(
            string input,
            string expected,
            string actual,
            string fix,
            string? filePath)
        {
            _edgeCaseFailures.Add(new EdgeCaseFailure
            {
                Id = _edgeCaseIdCounter++,
                Input = input,
                Expected = expected,
                Actual = actual,
                Fix = fix,
                FilePath = filePath
            });
        }

        #endregion
    }
}