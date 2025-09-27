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
            sb.AppendLine($"   • Runtime Issues: {result.RuntimeIssues.Count()}");
            
            var criticalCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical);
            var highCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High);
            var mediumCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Medium);
            var lowCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Low);

            sb.AppendLine($"   • Critical: {criticalCount}");
            sb.AppendLine($"   • High: {highCount}");
            sb.AppendLine($"   • Medium: {mediumCount}");
            sb.AppendLine($"   • Low: {lowCount}");
            
            sb.AppendLine($"   • Environment Risks: {result.EnvironmentRisks.Count()}");
            
            // Add risk likelihood breakdown
            var highLikelihoodRisks = result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.High);
            var mediumLikelihoodRisks = result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.Medium);
            var lowLikelihoodRisks = result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.Low);
            
            if (highLikelihoodRisks > 0)
                sb.AppendLine($"   • High Likelihood: {highLikelihoodRisks}");
            if (mediumLikelihoodRisks > 0)
                sb.AppendLine($"   • Medium Likelihood: {mediumLikelihoodRisks}");
            if (lowLikelihoodRisks > 0)
                sb.AppendLine($"   • Low Likelihood: {lowLikelihoodRisks}");
                
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
            
            // Add summary statistics
            var criticalCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical);
            var highCount = result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High);
            var highRiskEnvironments = result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.High);
            var edgeCaseCount = result.EdgeCaseFailures.Count();
            
            if (criticalCount > 0)
                sb.AppendLine($"CRITICAL_ISSUES: {criticalCount}");
            if (highCount > 0)
                sb.AppendLine($"HIGH_SEVERITY_ISSUES: {highCount}");
            if (highRiskEnvironments > 0)
                sb.AppendLine($"HIGH_RISK_ENVIRONMENTS: {highRiskEnvironments}");
            if (edgeCaseCount > 0)
                sb.AppendLine($"EDGE_CASE_FAILURES: {edgeCaseCount}");
                
            sb.AppendLine();

            // Runtime Issues
            if (result.RuntimeIssues.Any())
            {
                foreach (var issue in result.RuntimeIssues)
                {
                    sb.AppendLine($"ISSUE #{issue.Id}:");
                    sb.AppendLine($"  TYPE: {issue.Type}");
                    sb.AppendLine($"  SEVERITY: {issue.Severity}");
                    sb.AppendLine($"  DESCRIPTION: {issue.Description}");
                    sb.AppendLine($"  FILE: {issue.FilePath}");
                    sb.AppendLine($"  LINE: {issue.LineNumber}");
                    
                    if (issue.Proof != null)
                    {
                        sb.AppendLine($"  PROOF:");
                        sb.AppendLine($"    - Simulated Execution Step: {issue.Proof.SimulatedExecutionStep}");
                        sb.AppendLine($"    - Trigger: {issue.Proof.Trigger}");
                        sb.AppendLine($"    - Result: {issue.Proof.Result}");
                    }
                    
                    if (issue.Solution != null)
                    {
                        sb.AppendLine($"  SOLUTION:");
                        sb.AppendLine($"    - Fix: {issue.Solution.Fix}");
                        sb.AppendLine($"    - Verification: {issue.Solution.Verification}");
                    }
                    
                    sb.AppendLine();
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
                sb.AppendLine();
                
                foreach (var issue in result.RuntimeIssues.OrderByDescending(i => i.Severity))
                {
                    sb.AppendLine($"ISSUE #{issue.Id}:");
                    sb.AppendLine($"TYPE: {issue.Type}");
                    sb.AppendLine($"SEVERITY: {issue.Severity}");
                    sb.AppendLine($"DESCRIPTION: {issue.Description}");
                    
                    if (issue.Proof != null)
                    {
                        sb.AppendLine($"TRIGGER: {issue.Proof.Trigger}");
                        sb.AppendLine($"EXPECTED: {issue.Proof.SimulatedExecutionStep}");
                        sb.AppendLine($"ACTUAL (Simulated): {issue.Proof.Result}");
                    }
                    
                    if (issue.Solution != null)
                    {
                        sb.AppendLine($"SOLUTION: {issue.Solution.Fix}");
                    }
                    
                    sb.AppendLine();
                }
            }

            // Environment Risks as Issues
            if (result.EnvironmentRisks.Any())
            {
                if (!result.RuntimeIssues.Any())
                {
                    sb.AppendLine("ENVIRONMENT RISKS");
                    sb.AppendLine("═════════════════");
                    sb.AppendLine();
                }
                
                foreach (var risk in result.EnvironmentRisks.OrderByDescending(r => r.Likelihood))
                {
                    sb.AppendLine($"ISSUE #{risk.Id}:");
                    sb.AppendLine($"TYPE: Environment");
                    sb.AppendLine($"SEVERITY: {risk.Likelihood}");
                    sb.AppendLine($"DESCRIPTION: {risk.Description}");
                    sb.AppendLine($"TRIGGER: {risk.Component} dependency failure or misconfiguration");
                    sb.AppendLine($"EXPECTED: Graceful handling of {risk.Component.ToLower()} unavailability");
                    sb.AppendLine($"ACTUAL (Simulated): Service failure, timeout, or exception");
                    
                    if (risk.Mitigation != null && risk.Mitigation.RequiredChanges.Any())
                    {
                        sb.AppendLine($"SOLUTION: {string.Join("; ", risk.Mitigation.RequiredChanges)}");
                    }
                    
                    sb.AppendLine();
                }
            }

            // Edge Cases as Issues
            if (result.EdgeCaseFailures.Any())
            {
                if (!result.RuntimeIssues.Any() && !result.EnvironmentRisks.Any())
                {
                    sb.AppendLine("EDGE CASE FAILURES");
                    sb.AppendLine("══════════════════");
                    sb.AppendLine();
                }
                
                foreach (var failure in result.EdgeCaseFailures.OrderByDescending(f => f.Severity))
                {
                    sb.AppendLine($"ISSUE #{failure.Id}:");
                    sb.AppendLine($"TYPE: Logic");
                    sb.AppendLine($"SEVERITY: Medium");
                    sb.AppendLine($"DESCRIPTION: Edge case handling failure for {failure.Input}");
                    sb.AppendLine($"TRIGGER: {failure.Input}");
                    sb.AppendLine($"EXPECTED: {failure.Expected}");
                    sb.AppendLine($"ACTUAL (Simulated): {failure.Actual}");
                    
                    if (!string.IsNullOrEmpty(failure.Fix))
                    {
                        sb.AppendLine($"SOLUTION: {failure.Fix}");
                        sb.AppendLine($"VERIFICATION: Test with edge case inputs including {failure.Input}");
                    }
                    
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}