using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using FluentAI.Abstractions.Analysis;

namespace FluentAI.Services.Analysis
{
    /// <summary>
    /// Provides formatting methods for runtime analysis results.
    /// </summary>
    public static class RuntimeAnalysisFormatter
    {
        /// <summary>
        /// Formats the analysis result as a human-readable summary.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A formatted summary string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
        public static string FormatSummary(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var sb = new StringBuilder();
            
            if (result.HasCriticalIssues)
            {
                sb.AppendLine("⚠️  CRITICAL ISSUES DETECTED");
                sb.AppendLine("═══════════════════════════");
            }
            else
            {
                sb.AppendLine("✅ RUNTIME ANALYSIS COMPLETE");
                sb.AppendLine("═══════════════════════════");
            }

            sb.AppendLine();
            sb.AppendLine($"📊 Analysis Summary:");
            sb.AppendLine($"   • Total Issues: {result.TotalIssueCount}");
            
            var criticalCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical);
            var highCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High);
            var mediumCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Medium);
            var lowCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Low);

            sb.AppendLine($"   • Critical: {criticalCount}");
            sb.AppendLine($"   • High: {highCount}");
            sb.AppendLine($"   • Medium: {mediumCount}");
            sb.AppendLine($"   • Low: {lowCount}");
            
            sb.AppendLine($"   • Environment Risks: {result.EnvironmentRisks.Count()}");
            sb.AppendLine($"   • Edge Case Failures: {result.EdgeCaseFailures.Count()}");

            if (result.HasCriticalIssues)
            {
                sb.AppendLine();
                sb.AppendLine("🚨 Recommendations:");
                sb.AppendLine("   • Review critical issues immediately");
                sb.AppendLine("   • Fix high-severity issues before deployment");
                sb.AppendLine("   • Consider performance impact of medium issues");
            }

            if (result.Metadata?.AnalyzedFiles?.Any() == true)
            {
                sb.AppendLine();
                sb.AppendLine("📁 Analyzed Files:");
                foreach (var file in result.Metadata.AnalyzedFiles)
                {
                    sb.AppendLine($"   • {file}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats the analysis result as YAML.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A YAML-formatted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
        public static string FormatAsYaml(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var sb = new StringBuilder();
            sb.AppendLine("# Runtime-Aware Code Analysis Report");
            sb.AppendLine($"# Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            
            sb.AppendLine($"TOTAL_ISSUES: {result.TotalIssueCount}");
            sb.AppendLine($"HAS_CRITICAL_ISSUES: {result.HasCriticalIssues.ToString().ToLower()}");
            sb.AppendLine();

            // Runtime Issues
            if (result.RuntimeIssues.Any())
            {
                sb.AppendLine("RUNTIME_ISSUES:");
                foreach (var issue in result.RuntimeIssues)
                {
                    sb.AppendLine($"  - id: {issue.Id}");
                    sb.AppendLine($"    type: {issue.Type}");
                    sb.AppendLine($"    severity: {issue.Severity}");
                    sb.AppendLine($"    description: \"{issue.Description}\"");
                    sb.AppendLine($"    location: \"{issue.Location}\"");
                    sb.AppendLine($"    suggested_fix: \"{issue.SuggestedFix}\"");
                }
            }
            else
            {
                sb.AppendLine("# No runtime issues detected");
            }

            sb.AppendLine();

            // Environment Risks
            if (result.EnvironmentRisks.Any())
            {
                sb.AppendLine("ENVIRONMENT_RISKS:");
                foreach (var risk in result.EnvironmentRisks)
                {
                    sb.AppendLine($"  - id: {risk.Id}");
                    sb.AppendLine($"    type: {risk.Type}");
                    sb.AppendLine($"    likelihood: {risk.Likelihood}");
                    sb.AppendLine($"    description: \"{risk.Description}\"");
                    sb.AppendLine($"    impact: \"{risk.Impact}\"");
                    
                    if (risk.Mitigation != null)
                    {
                        sb.AppendLine($"    mitigation:");
                        sb.AppendLine($"      required_changes:");
                        foreach (var change in risk.Mitigation.RequiredChanges)
                        {
                            sb.AppendLine($"        - \"{change}\"");
                        }
                        sb.AppendLine($"      monitoring: \"{risk.Mitigation.Monitoring}\"");
                    }
                    else
                    {
                        sb.AppendLine($"    mitigation: \"\"");
                    }
                }
            }
            else
            {
                sb.AppendLine("# No environment risks detected");
            }

            sb.AppendLine();

            // Edge Case Failures
            if (result.EdgeCaseFailures.Any())
            {
                sb.AppendLine("EDGE_CASE_FAILURES:");
                foreach (var failure in result.EdgeCaseFailures)
                {
                    sb.AppendLine($"  - id: {failure.Id}");
                    sb.AppendLine($"    input: \"{failure.Input}\"");
                    sb.AppendLine($"    scenario: \"{failure.Scenario}\"");
                    sb.AppendLine($"    expected_failure: \"{failure.ExpectedFailure}\"");
                    sb.AppendLine($"    severity: {failure.Severity}");
                    sb.AppendLine($"    location: \"{failure.Location}\"");
                }
            }
            else
            {
                sb.AppendLine("# No edge case failures detected");
            }

            // Metadata
            if (result.Metadata != null)
            {
                sb.AppendLine();
                sb.AppendLine("METADATA:");
                sb.AppendLine($"  analysis_timestamp: \"{result.Metadata.AnalysisTimestamp:yyyy-MM-dd HH:mm:ss} UTC\"");
                sb.AppendLine($"  analyzer_version: \"{result.Metadata.AnalyzerVersion}\"");
                sb.AppendLine($"  analysis_duration: \"{result.Metadata.AnalysisDuration}\"");
                
                if (result.Metadata.AnalyzedFiles?.Any() == true)
                {
                    sb.AppendLine("  analyzed_files:");
                    foreach (var file in result.Metadata.AnalyzedFiles)
                    {
                        sb.AppendLine($"    - \"{file}\"");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Formats the analysis result as JSON.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A JSON-formatted string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
        public static string FormatAsJson(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(result, jsonOptions);
        }

        /// <summary>
        /// Formats the analysis result as a structured report.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A structured report string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
        public static string FormatAsStructuredReport(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var sb = new StringBuilder();
            
            sb.AppendLine("╔════════════════════════════════════════════════════════════════╗");
            sb.AppendLine("║                 RUNTIME ANALYSIS REPORT                        ║");
            sb.AppendLine("╚════════════════════════════════════════════════════════════════╝");
            sb.AppendLine();

            // Executive Summary
            sb.AppendLine("EXECUTIVE SUMMARY");
            sb.AppendLine("═════════════════");
            sb.AppendLine($"Status: {(result.HasCriticalIssues ? "⚠️  ATTENTION REQUIRED" : "✅ ACCEPTABLE")}");
            sb.AppendLine($"Total Issues: {result.TotalIssueCount}");
            sb.AppendLine($"Analysis Date: {result.Metadata?.AnalysisTimestamp:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();

            // Runtime Issues Detail
            if (result.RuntimeIssues.Any())
            {
                sb.AppendLine("RUNTIME ISSUES");
                sb.AppendLine("══════════════");
                
                var groupedIssues = result.RuntimeIssues.GroupBy(i => i.Severity).OrderByDescending(g => g.Key);
                
                foreach (var group in groupedIssues)
                {
                    sb.AppendLine($"\n{group.Key} Severity ({group.Count()} issues):");
                    sb.AppendLine(new string('─', 40));
                    
                    foreach (var issue in group)
                    {
                        sb.AppendLine($"• [{issue.Type}] {issue.Description}");
                        sb.AppendLine($"  Location: {issue.Location}");
                        sb.AppendLine($"  Fix: {issue.SuggestedFix}");
                        sb.AppendLine();
                    }
                }
            }

            // Environment Risks
            if (result.EnvironmentRisks.Any())
            {
                sb.AppendLine("ENVIRONMENT RISKS");
                sb.AppendLine("═════════════════");
                
                foreach (var risk in result.EnvironmentRisks.OrderByDescending(r => r.Likelihood))
                {
                    sb.AppendLine($"• [{risk.Likelihood} Risk] {risk.Description}");
                    sb.AppendLine($"  Impact: {risk.Impact}");
                    if (risk.Mitigation != null)
                    {
                        sb.AppendLine($"  Mitigation: {string.Join(", ", risk.Mitigation.RequiredChanges)}");
                        if (!string.IsNullOrEmpty(risk.Mitigation.Monitoring))
                        {
                            sb.AppendLine($"  Monitoring: {risk.Mitigation.Monitoring}");
                        }
                    }
                    sb.AppendLine();
                }
            }

            // Edge Cases
            if (result.EdgeCaseFailures.Any())
            {
                sb.AppendLine("EDGE CASE FAILURES");
                sb.AppendLine("══════════════════");
                
                foreach (var failure in result.EdgeCaseFailures.OrderByDescending(f => f.Severity))
                {
                    sb.AppendLine($"• [{failure.Severity}] {failure.Scenario}");
                    sb.AppendLine($"  Input: {failure.Input}");
                    sb.AppendLine($"  Expected Failure: {failure.ExpectedFailure}");
                    sb.AppendLine($"  Location: {failure.Location}");
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}