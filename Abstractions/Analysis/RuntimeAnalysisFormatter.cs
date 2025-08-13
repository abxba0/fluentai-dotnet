using System.Text;

namespace FluentAI.Abstractions.Analysis
{
    /// <summary>
    /// Provides YAML formatting for runtime analysis results as specified in the requirements.
    /// </summary>
    public static class RuntimeAnalysisFormatter
    {
        /// <summary>
        /// Formats the runtime analysis result as a structured YAML report.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A formatted YAML string containing the analysis report.</returns>
        public static string FormatAsYaml(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var yaml = new StringBuilder();

            // Header
            yaml.AppendLine("# Runtime-Aware Code Analysis Report");
            yaml.AppendLine($"# Generated: {result.Metadata.AnalysisTimestamp:yyyy-MM-dd HH:mm:ss UTC}");
            yaml.AppendLine($"# Duration: {result.Metadata.Duration.TotalMilliseconds:F1}ms");
            yaml.AppendLine($"# Files Analyzed: {result.Metadata.AnalyzedFiles.Count}");
            yaml.AppendLine($"# Total Issues: {result.TotalIssueCount}");
            yaml.AppendLine();

            // 1. Runtime Issues
            yaml.AppendLine("# 1. Runtime Issues");
            if (result.RuntimeIssues.Any())
            {
                foreach (var issue in result.RuntimeIssues)
                {
                    yaml.AppendLine($"ISSUE #{issue.Id}:");
                    yaml.AppendLine($"  TYPE: {issue.Type}");
                    yaml.AppendLine($"  SEVERITY: {issue.Severity}");
                    yaml.AppendLine($"  DESCRIPTION: {EscapeYamlString(issue.Description)}");
                    
                    if (!string.IsNullOrEmpty(issue.FilePath))
                    {
                        yaml.AppendLine($"  FILE: {issue.FilePath}");
                    }
                    
                    if (issue.LineNumber.HasValue)
                    {
                        yaml.AppendLine($"  LINE: {issue.LineNumber}");
                    }

                    yaml.AppendLine("  PROOF:");
                    yaml.AppendLine($"    - Simulated Execution Step: {EscapeYamlString(issue.Proof.SimulatedExecutionStep)}");
                    yaml.AppendLine($"    - Trigger: {EscapeYamlString(issue.Proof.Trigger)}");
                    yaml.AppendLine($"    - Result: {EscapeYamlString(issue.Proof.Result)}");
                    
                    yaml.AppendLine("  SOLUTION:");
                    yaml.AppendLine($"    - Fix: {EscapeYamlString(issue.Solution.Fix)}");
                    yaml.AppendLine($"    - Verification: {EscapeYamlString(issue.Solution.Verification)}");
                    yaml.AppendLine();
                }
            }
            else
            {
                yaml.AppendLine("# No runtime issues detected");
                yaml.AppendLine();
            }

            // 2. Environment Risks
            yaml.AppendLine("# 2. Environment Risks");
            if (result.EnvironmentRisks.Any())
            {
                foreach (var risk in result.EnvironmentRisks)
                {
                    yaml.AppendLine($"RISK #{risk.Id}:");
                    yaml.AppendLine($"  COMPONENT: {EscapeYamlString(risk.Component)}");
                    yaml.AppendLine($"  DESCRIPTION: {EscapeYamlString(risk.Description)}");
                    yaml.AppendLine($"  LIKELIHOOD: {risk.Likelihood}");
                    yaml.AppendLine("  MITIGATION:");
                    
                    if (risk.Mitigation.RequiredChanges.Any())
                    {
                        yaml.AppendLine("    - Required Changes:");
                        foreach (var change in risk.Mitigation.RequiredChanges)
                        {
                            yaml.AppendLine($"      - {EscapeYamlString(change)}");
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(risk.Mitigation.Monitoring))
                    {
                        yaml.AppendLine($"    - Monitoring: {EscapeYamlString(risk.Mitigation.Monitoring)}");
                    }
                    yaml.AppendLine();
                }
            }
            else
            {
                yaml.AppendLine("# No environment risks detected");
                yaml.AppendLine();
            }

            // 3. Edge Case Failures
            yaml.AppendLine("# 3. Edge Case Failures");
            if (result.EdgeCaseFailures.Any())
            {
                foreach (var edgeCase in result.EdgeCaseFailures)
                {
                    yaml.AppendLine($"CASE #{edgeCase.Id}:");
                    yaml.AppendLine($"  INPUT: {EscapeYamlString(edgeCase.Input)}");
                    yaml.AppendLine($"  EXPECTED: {EscapeYamlString(edgeCase.Expected)}");
                    yaml.AppendLine($"  ACTUAL (Simulated): {EscapeYamlString(edgeCase.Actual)}");
                    yaml.AppendLine($"  FIX: {EscapeYamlString(edgeCase.Fix)}");
                    
                    if (!string.IsNullOrEmpty(edgeCase.FilePath))
                    {
                        yaml.AppendLine($"  FILE: {edgeCase.FilePath}");
                    }
                    yaml.AppendLine();
                }
            }
            else
            {
                yaml.AppendLine("# No edge case failures detected");
                yaml.AppendLine();
            }

            // Summary
            yaml.AppendLine("# Analysis Summary");
            yaml.AppendLine($"TOTAL_ISSUES: {result.TotalIssueCount}");
            yaml.AppendLine($"CRITICAL_ISSUES: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical)}");
            yaml.AppendLine($"HIGH_SEVERITY_ISSUES: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High)}");
            yaml.AppendLine($"HIGH_RISK_ENVIRONMENTS: {result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.High)}");
            yaml.AppendLine($"EDGE_CASE_FAILURES: {result.EdgeCaseFailures.Count}");
            yaml.AppendLine($"HAS_CRITICAL_ISSUES: {result.HasCriticalIssues}");

            return yaml.ToString();
        }

        /// <summary>
        /// Formats the runtime analysis result as a structured JSON report for programmatic consumption.
        /// </summary>
        /// <param name="result">The analysis result to format.</param>
        /// <returns>A formatted JSON string containing the analysis report.</returns>
        public static string FormatAsJson(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            return System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }

        /// <summary>
        /// Formats a summary of the analysis result for console output.
        /// </summary>
        /// <param name="result">The analysis result to summarize.</param>
        /// <returns>A formatted summary string.</returns>
        public static string FormatSummary(RuntimeAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var summary = new StringBuilder();
            
            summary.AppendLine("═══════════════════════════════════════════════════════════");
            summary.AppendLine("                RUNTIME ANALYSIS SUMMARY");
            summary.AppendLine("═══════════════════════════════════════════════════════════");
            summary.AppendLine();
            
            summary.AppendLine($"📊 Analysis completed in {result.Metadata.Duration.TotalMilliseconds:F1}ms");
            summary.AppendLine($"📁 Files analyzed: {result.Metadata.AnalyzedFiles.Count}");
            summary.AppendLine($"🔍 Total issues found: {result.TotalIssueCount}");
            
            if (result.HasCriticalIssues)
            {
                summary.AppendLine("⚠️  CRITICAL ISSUES DETECTED!");
            }
            else
            {
                summary.AppendLine("✅ No critical issues detected");
            }
            
            summary.AppendLine();
            summary.AppendLine("Issue Breakdown:");
            summary.AppendLine($"  • Runtime Issues: {result.RuntimeIssues.Count}");
            summary.AppendLine($"    - Critical: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Critical)}");
            summary.AppendLine($"    - High: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.High)}");
            summary.AppendLine($"    - Medium: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Medium)}");
            summary.AppendLine($"    - Low: {result.RuntimeIssues.Count(i => i.Severity == RuntimeIssueSeverity.Low)}");
            summary.AppendLine();
            summary.AppendLine($"  • Environment Risks: {result.EnvironmentRisks.Count}");
            summary.AppendLine($"    - High Likelihood: {result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.High)}");
            summary.AppendLine($"    - Medium Likelihood: {result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.Medium)}");
            summary.AppendLine($"    - Low Likelihood: {result.EnvironmentRisks.Count(r => r.Likelihood == RiskLikelihood.Low)}");
            summary.AppendLine();
            summary.AppendLine($"  • Edge Case Failures: {result.EdgeCaseFailures.Count}");
            summary.AppendLine();
            
            if (result.TotalIssueCount > 0)
            {
                summary.AppendLine("🔧 Recommendations:");
                summary.AppendLine("  1. Address critical and high-severity issues first");
                summary.AppendLine("  2. Review high-likelihood environment risks");
                summary.AppendLine("  3. Add tests for identified edge cases");
                summary.AppendLine("  4. Run full YAML report for detailed analysis");
            }
            else
            {
                summary.AppendLine("🎉 Great! No runtime issues detected in the analyzed code.");
                summary.AppendLine("   Consider this a clean baseline for future analysis.");
            }
            
            summary.AppendLine("═══════════════════════════════════════════════════════════");

            return summary.ToString();
        }

        private static string EscapeYamlString(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // Basic YAML string escaping
            var escaped = value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");

            // If the string contains special characters, wrap in quotes
            if (escaped.Contains(":") || escaped.Contains("-") || escaped.Contains("#") || 
                escaped.Contains("[") || escaped.Contains("]") || escaped.Contains("{") || 
                escaped.Contains("}") || escaped.StartsWith(" ") || escaped.EndsWith(" "))
            {
                return $"\"{escaped}\"";
            }

            return escaped;
        }
    }
}