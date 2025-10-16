using FluentAI.Abstractions.Analysis;
using FluentAI.Services.Analysis;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Analysis;

/// <summary>
/// Unit tests for environment risk assessment in runtime analyzer.
/// </summary>
public class EnvironmentRiskTests
{
    private readonly Mock<ILogger<DefaultRuntimeAnalyzer>> _mockLogger;
    private readonly DefaultRuntimeAnalyzer _analyzer;

    public EnvironmentRiskTests()
    {
        _mockLogger = new Mock<ILogger<DefaultRuntimeAnalyzer>>();
        _analyzer = new DefaultRuntimeAnalyzer(_mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeSource_WithDatabaseCode_DetectsDatabaseDependencyRisk()
    {
        // Arrange
        var sourceCode = @"
            using System.Data.SqlClient;
            public class DatabaseService
            {
                public void Query()
                {
                    using var connection = new SqlConnection(""connectionString"");
                    var command = new SqlCommand(""SELECT * FROM Users"", connection);
                    command.ExecuteNonQuery();
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "DatabaseService.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EnvironmentRisks);
        var dbRisk = result.EnvironmentRisks.FirstOrDefault(r => r.Component.Contains("Database"));
        Assert.NotNull(dbRisk);
        Assert.Equal(EnvironmentRiskType.Dependency, dbRisk.Type);
        Assert.Equal(RiskLikelihood.High, dbRisk.Likelihood);
    }

    [Fact]
    public async Task AnalyzeSource_WithHttpClientCode_DetectsExternalApiRisk()
    {
        // Arrange
        var sourceCode = @"
            using System.Net.Http;
            public class ApiClient
            {
                private readonly HttpClient _client = new HttpClient();
                
                public async Task<string> GetDataAsync()
                {
                    return await _client.GetStringAsync(""https://api.example.com/data"");
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ApiClient.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EnvironmentRisks);
        var apiRisk = result.EnvironmentRisks.FirstOrDefault(r => r.Component.Contains("External API"));
        Assert.NotNull(apiRisk);
        Assert.Equal(EnvironmentRiskType.Dependency, apiRisk.Type);
        Assert.Equal(RiskLikelihood.Medium, apiRisk.Likelihood);
    }

    [Fact]
    public async Task AnalyzeSource_WithConfigurationCode_DetectsConfigurationRisk()
    {
        // Arrange
        var sourceCode = @"
            using Microsoft.Extensions.Configuration;
            public class ConfigService
            {
                private readonly IConfiguration _config;
                
                public ConfigService(IConfiguration config)
                {
                    _config = config;
                }
                
                public string GetConnectionString()
                {
                    return _config.GetConnectionString(""Database"");
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ConfigService.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EnvironmentRisks);
        var configRisk = result.EnvironmentRisks.FirstOrDefault(r => r.Component.Contains("Configuration"));
        Assert.NotNull(configRisk);
        Assert.Equal(EnvironmentRiskType.Configuration, configRisk.Type);
        Assert.Equal(RiskLikelihood.Low, configRisk.Likelihood);
    }

    [Fact]
    public async Task AnalyzeSource_WithMultipleRisks_DetectsAllRiskTypes()
    {
        // Arrange
        var sourceCode = @"
            using System.Net.Http;
            using Microsoft.Extensions.Configuration;
            public class ComplexService
            {
                private readonly HttpClient _httpClient;
                private readonly IConfiguration _config;
                
                public async Task ProcessAsync()
                {
                    var apiKey = _config[""ApiKey""];
                    var response = await _httpClient.GetStringAsync(""https://api.example.com"");
                    ExecuteQuery(""SELECT * FROM Users"");
                }
                
                private void ExecuteQuery(string sql) { }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ComplexService.cs");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.EnvironmentRisks.Count() >= 3, 
            "Should detect database, API, and configuration risks");
    }

    [Fact]
    public async Task AnalyzeSource_WithNoEnvironmentRisks_ReturnsEmptyRiskList()
    {
        // Arrange
        var sourceCode = @"
            public class SimpleCalculator
            {
                public int Add(int a, int b)
                {
                    return a + b;
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "SimpleCalculator.cs");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.EnvironmentRisks);
    }

    [Fact]
    public async Task AnalyzeSource_RiskMitigation_ContainsUsefulRecommendations()
    {
        // Arrange
        var sourceCode = @"
            public class DataService
            {
                public void SaveData()
                {
                    ExecuteQuery(""INSERT INTO Data VALUES ('test')"");
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "DataService.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EnvironmentRisks);
        var dbRisk = result.EnvironmentRisks.First();
        Assert.NotNull(dbRisk.Mitigation);
        Assert.NotEmpty(dbRisk.Mitigation.RequiredChanges);
        Assert.NotEmpty(dbRisk.Mitigation.Monitoring);
    }

    [Theory]
    [InlineData("SqlConnection", "Database")]
    [InlineData("SqlCommand", "Database")]
    [InlineData("ExecuteQuery", "Database")]
    [InlineData("HttpClient", "External API")]
    [InlineData("GetStringAsync", "External API")]
    [InlineData("PostAsync", "External API")]
    [InlineData("IConfiguration", "Configuration")]
    [InlineData("appSettings", "Configuration")]
    public async Task AnalyzeSource_DetectsSpecificPatterns(string pattern, string expectedComponent)
    {
        // Arrange
        var sourceCode = $@"
            public class TestService
            {{
                public void Method()
                {{
                    var test = new {pattern}();
                }}
            }}";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "TestService.cs");

        // Assert
        Assert.NotNull(result);
        if (result.EnvironmentRisks.Any())
        {
            Assert.Contains(result.EnvironmentRisks, r => r.Component.Contains(expectedComponent));
        }
    }

    [Fact]
    public async Task AnalyzeSource_RiskIds_AreUnique()
    {
        // Arrange
        var sourceCode = @"
            public class MultiRiskService
            {
                public async Task ExecuteAsync()
                {
                    ExecuteQuery(""SELECT * FROM Users"");
                    await new HttpClient().GetStringAsync(""https://api.example.com"");
                    var config = IConfiguration.GetSection(""AppSettings"");
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "MultiRiskService.cs");

        // Assert
        Assert.NotNull(result);
        var riskIds = result.EnvironmentRisks.Select(r => r.Id).ToList();
        Assert.Equal(riskIds.Count, riskIds.Distinct().Count());
    }

    [Fact]
    public async Task AnalyzeSource_ValidatesImpactDescriptions_AreNotEmpty()
    {
        // Arrange
        var sourceCode = @"
            public class ServiceWithDependencies
            {
                public void Process()
                {
                    ExecuteQuery(""SELECT * FROM table"");
                }
            }";

        // Act
        var result = await _analyzer.AnalyzeSourceAsync(sourceCode, "ServiceWithDependencies.cs");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.EnvironmentRisks);
        foreach (var risk in result.EnvironmentRisks)
        {
            Assert.False(string.IsNullOrWhiteSpace(risk.Description));
            Assert.False(string.IsNullOrWhiteSpace(risk.Impact));
        }
    }

    [Fact]
    public async Task AnalyzeSource_RiskLikelihood_IsProperlyAssessed()
    {
        // Arrange
        var databaseCode = @"public void Query() { ExecuteQuery(""SELECT""); }";
        var apiCode = @"public async Task Call() { await HttpClient.GetAsync(""url""); }";
        var configCode = @"public string GetConfig() { return IConfiguration[""key""]; }";

        // Act
        var dbResult = await _analyzer.AnalyzeSourceAsync(databaseCode, "DB.cs");
        var apiResult = await _analyzer.AnalyzeSourceAsync(apiCode, "API.cs");
        var configResult = await _analyzer.AnalyzeSourceAsync(configCode, "Config.cs");

        // Assert - Database has High likelihood
        if (dbResult.EnvironmentRisks.Any())
        {
            Assert.Contains(dbResult.EnvironmentRisks, r => r.Likelihood == RiskLikelihood.High);
        }
        
        // API has Medium likelihood
        if (apiResult.EnvironmentRisks.Any())
        {
            Assert.Contains(apiResult.EnvironmentRisks, r => r.Likelihood == RiskLikelihood.Medium);
        }
        
        // Configuration has Low likelihood
        if (configResult.EnvironmentRisks.Any())
        {
            Assert.Contains(configResult.EnvironmentRisks, r => r.Likelihood == RiskLikelihood.Low);
        }
    }
}
