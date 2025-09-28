using FluentAI.Abstractions.Analysis;
using Microsoft.Extensions.Logging;

namespace FluentAI.Examples.ConsoleApp.Services
{
    /// <summary>
    /// Demonstrates the Runtime-Aware Code Analyzer features.
    /// </summary>
    public class RuntimeAnalysisDemoService
    {
        private readonly IRuntimeAnalyzer _runtimeAnalyzer;
        private readonly ILogger<RuntimeAnalysisDemoService> _logger;

        public RuntimeAnalysisDemoService(IRuntimeAnalyzer runtimeAnalyzer, ILogger<RuntimeAnalysisDemoService> logger)
        {
            _runtimeAnalyzer = runtimeAnalyzer ?? throw new ArgumentNullException(nameof(runtimeAnalyzer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task RunRuntimeAnalysisDemo()
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                 🔍 RUNTIME-AWARE CODE ANALYZER               ║");
            Console.WriteLine("║     Comprehensive analysis for runtime behavior prediction    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            await ShowAnalysisMethodology();
            await DemoCleanCode();
            await DemoProblematicCode();
            await DemoRealWorldScenarios();
            await DemoOutputFormats();
        }

        private async Task ShowAnalysisMethodology()
        {
            Console.WriteLine("📋 Analysis Methodology (5-Step Process):");
            Console.WriteLine("   ═══════════════════════════════════════");
            Console.WriteLine();
            Console.WriteLine("1. 🔍 Static Review - Baseline syntax and logic analysis");
            Console.WriteLine("2. ⚡ Runtime Simulation - Mental execution of code paths");
            Console.WriteLine("3. 🌐 Environment Checks - External dependencies and performance");
            Console.WriteLine("4. 🎯 Edge Case Simulation - Boundary values and type handling");
            Console.WriteLine("5. 🔗 Error Propagation - How errors flow through the system");
            Console.WriteLine();
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        private async Task DemoCleanCode()
        {
            Console.WriteLine("✅ Clean Code Analysis:");
            Console.WriteLine("   ══════════════════════");
            Console.WriteLine();

            var cleanCode = @"
                public class WeatherService
                {
                    private readonly ILogger<WeatherService> _logger;
                    private readonly HttpClient _httpClient;

                    public WeatherService(ILogger<WeatherService> logger, HttpClient httpClient)
                    {
                        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
                    }

                    public async Task<WeatherData?> GetWeatherAsync(string city, CancellationToken cancellationToken = default)
                    {
                        if (string.IsNullOrWhiteSpace(city))
                            throw new ArgumentException(""City cannot be null or empty"", nameof(city));

                        try
                        {
                            var response = await _httpClient.GetStringAsync($""api/weather/{city}"", cancellationToken);
                            return JsonSerializer.Deserialize<WeatherData>(response);
                        }
                        catch (HttpRequestException ex)
                        {
                            _logger.LogError(ex, ""Failed to fetch weather for {City}"", city);
                            return null;
                        }
                    }
                }";

            Console.WriteLine("📄 Code Sample:");
            Console.WriteLine(cleanCode);
            Console.WriteLine();

            Console.WriteLine("🔍 Analyzing clean code...");
            var result = await _runtimeAnalyzer.AnalyzeSourceAsync(cleanCode, "WeatherService.cs");

            // DEMO FIX: Create simple formatter inline to prevent build errors
            FormatAndDisplayResult(result);
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        // DEMO FIX: Simple inline formatter to replace missing RuntimeAnalysisFormatter
        private static void FormatAndDisplayResult(RuntimeAnalysisResult result)
        {
            Console.WriteLine($"Analysis Summary: {result.Issues.Count} issues found, {result.Risks.Count} risks identified");
            if (result.Issues.Any())
            {
                Console.WriteLine("Issues found:");
                foreach (var issue in result.Issues.Take(3))
                {
                    Console.WriteLine($"  - {issue.Severity}: {issue.Description}");
                }
            }
        }

        private async Task DemoProblematicCode()
        {
            Console.WriteLine("⚠️  Problematic Code Analysis:");
            Console.WriteLine("   ══════════════════════════════");
            Console.WriteLine();

            var problematicCode = @"
                public class ProblematicService
                {
                    public static List<string> Cache = new List<string>();
                    
                    public async Task ProcessData()
                    {
                        var data = GetDataFromDatabase();
                        foreach(var item in data)
                        {
                            var details = ExecuteQuery(""SELECT * FROM details WHERE id = "" + item.Id);
                            var result = ProcessItem(item);
                            Cache.Add(result); // Potential memory leak
                        }
                    }

                    public int ParseUserInput(string input)
                    {
                        return int.Parse(input); // No error handling
                    }

                    public void ProcessFile()
                    {
                        var file = new FileStream(""data.txt"", FileMode.Open);
                        // No using statement - resource leak
                        var content = file.ReadToEnd();
                    }

                    public async void FireAndForgetOperation()
                    {
                        await Task.Delay(1000); // async void - dangerous
                    }
                }";

            Console.WriteLine("📄 Code Sample (with issues):");
            Console.WriteLine(problematicCode);
            Console.WriteLine();

            Console.WriteLine("🔍 Analyzing problematic code...");
            var result = await _runtimeAnalyzer.AnalyzeSourceAsync(problematicCode, "ProblematicService.cs");

            FormatAndDisplayResult(result);
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        private async Task DemoRealWorldScenarios()
        {
            Console.WriteLine("🌍 Real-World Scenario Analysis:");
            Console.WriteLine("   ═══════════════════════════════");
            Console.WriteLine();

            var scenarios = new[]
            {
                ("Database Connection", @"
                    public class OrderService
                    {
                        public List<Order> GetOrdersByUser(int userId)
                        {
                            var connection = new SqlConnection(""Server=localhost;Database=Orders;"");
                            connection.Open();
                            var command = new SqlCommand($""SELECT * FROM Orders WHERE UserId = {userId}"", connection);
                            // SQL injection vulnerability + no connection disposal
                            var reader = command.ExecuteReader();
                            var orders = new List<Order>();
                            while(reader.Read())
                            {
                                orders.Add(MapToOrder(reader));
                            }
                            return orders;
                        }
                    }"),

                ("API Service", @"
                    public class ApiService
                    {
                        public async Task<ApiResponse> CallExternalApi(string endpoint)
                        {
                            var client = new HttpClient();
                            // Hardcoded URL + no timeout + no retry
                            var response = await client.GetAsync(""https://api.external.com/"" + endpoint);
                            var content = await response.Content.ReadAsStringAsync();
                            return JsonSerializer.Deserialize<ApiResponse>(content);
                        }
                    }"),

                ("Data Processing", @"
                    public class DataProcessor
                    {
                        public string ProcessLargeDataset(IEnumerable<DataRecord> records)
                        {
                            var result = """";
                            foreach(var record in records)
                            {
                                result += ProcessSingleRecord(record) + ""\n""; // String concatenation in loop
                            }
                            
                            var processedRecords = records.Select(r => r.Process()).ToList();
                            // Potential multiple enumeration
                            var validRecords = records.Where(r => r.IsValid).ToList();
                            
                            return result;
                        }
                    }")
            };

            foreach (var (name, code) in scenarios)
            {
                Console.WriteLine($"📊 Analyzing: {name}");
                Console.WriteLine("   " + new string('─', 50));

                var result = await _runtimeAnalyzer.AnalyzeSourceAsync(code, $"{name.Replace(" ", "")}.cs");
                
                Console.WriteLine($"   Issues Found: {result.TotalIssueCount}");
                Console.WriteLine($"   Critical: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical)}");
                Console.WriteLine($"   High: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High)}");
                Console.WriteLine($"   Environment Risks: {result.EnvironmentRisks.Count}");
                Console.WriteLine($"   Edge Cases: {result.EdgeCaseFailures.Count}");
                Console.WriteLine();
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }

        private async Task DemoOutputFormats()
        {
            Console.WriteLine("📄 Output Format Examples:");
            Console.WriteLine("   ════════════════════════");
            Console.WriteLine();

            var sampleCode = @"
                public class SampleCode
                {
                    public string ProcessInput(string input)
                    {
                        return input.ToUpper(); // Potential null reference
                    }
                    
                    public void DatabaseOperation()
                    {
                        foreach(var item in items)
                        {
                            ExecuteQuery(""SELECT * FROM table WHERE id = "" + item.Id);
                        }
                    }
                }";

            var result = await _runtimeAnalyzer.AnalyzeSourceAsync(sampleCode, "SampleCode.cs");

            Console.WriteLine("1. 📊 Summary Format:");
            Console.WriteLine("   ──────────────────");
            FormatAndDisplayResult(result);
            
            Console.WriteLine("\n2. 📋 Simple Format:");
            Console.WriteLine("   ──────────────────────────────────");
            // DEMO FIX: Replace YAML formatting with simple text output
            Console.WriteLine($"Total Issues: {result.Issues.Count}");
            Console.WriteLine($"Total Risks: {result.Risks.Count}");
            Console.WriteLine($"Analysis completed at: {result.AnalysisTimestamp}");
                Console.WriteLine($"   {line}");
            }
            Console.WriteLine("   ... (truncated for display)");
            
            Console.WriteLine("\n3. 🔧 JSON Format Available:");
            Console.WriteLine("   ──────────────────────────");
            Console.WriteLine("   • Programmatic consumption");
            Console.WriteLine("   • CI/CD pipeline integration");
            Console.WriteLine("   • Automated reporting systems");
            
            Console.WriteLine("\n✨ Analysis Complete!");
            Console.WriteLine("   ═══════════════════");
            Console.WriteLine("   • Use YAML for human-readable reports");
            Console.WriteLine("   • Use JSON for automated processing");
            Console.WriteLine("   • Use Summary for quick status checks");
            Console.WriteLine();
        }
    }
}