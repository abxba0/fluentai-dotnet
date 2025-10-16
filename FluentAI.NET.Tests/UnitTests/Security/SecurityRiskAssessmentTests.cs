using FluentAI.Abstractions.Security;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Security;

public class SecurityRiskAssessmentTests
{
    [Fact]
    public void SecurityRiskAssessment_WithNoneRiskLevel_ShouldNotBlock()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.None
        };

        // Assert
        Assert.Equal(SecurityRiskLevel.None, assessment.RiskLevel);
        Assert.False(assessment.ShouldBlock);
    }

    [Fact]
    public void SecurityRiskAssessment_WithLowRiskLevel_ShouldNotBlock()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Low
        };

        // Assert
        Assert.Equal(SecurityRiskLevel.Low, assessment.RiskLevel);
        Assert.False(assessment.ShouldBlock);
    }

    [Fact]
    public void SecurityRiskAssessment_WithMediumRiskLevel_ShouldNotBlock()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Medium
        };

        // Assert
        Assert.Equal(SecurityRiskLevel.Medium, assessment.RiskLevel);
        Assert.False(assessment.ShouldBlock);
    }

    [Fact]
    public void SecurityRiskAssessment_WithHighRiskLevel_ShouldBlock()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.High
        };

        // Assert
        Assert.Equal(SecurityRiskLevel.High, assessment.RiskLevel);
        Assert.True(assessment.ShouldBlock);
    }

    [Fact]
    public void SecurityRiskAssessment_WithCriticalRiskLevel_ShouldBlock()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Critical
        };

        // Assert
        Assert.Equal(SecurityRiskLevel.Critical, assessment.RiskLevel);
        Assert.True(assessment.ShouldBlock);
    }

    [Fact]
    public void SecurityRiskAssessment_WithDetectedConcerns_ShouldStoreThemCorrectly()
    {
        // Arrange
        var concerns = new[] { "SQL Injection attempt", "XSS detected" };

        // Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.High,
            DetectedConcerns = concerns
        };

        // Assert
        Assert.Equal(2, assessment.DetectedConcerns.Count);
        Assert.Contains("SQL Injection attempt", assessment.DetectedConcerns);
        Assert.Contains("XSS detected", assessment.DetectedConcerns);
    }

    [Fact]
    public void SecurityRiskAssessment_WithAdditionalInfo_ShouldStoreItCorrectly()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Medium,
            AdditionalInfo = "Contains suspicious patterns but not confirmed malicious"
        };

        // Assert
        Assert.Equal("Contains suspicious patterns but not confirmed malicious", assessment.AdditionalInfo);
    }

    [Fact]
    public void SecurityRiskAssessment_WithoutDetectedConcerns_ShouldHaveEmptyList()
    {
        // Arrange & Act
        var assessment = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.None
        };

        // Assert
        Assert.NotNull(assessment.DetectedConcerns);
        Assert.Empty(assessment.DetectedConcerns);
    }

    [Fact]
    public void SecurityRiskAssessment_RecordEquality_SameValuesAreEqual()
    {
        // Arrange
        var concerns = new[] { "Concern1" };
        var assessment1 = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Medium,
            DetectedConcerns = concerns,
            AdditionalInfo = "Test"
        };

        var assessment2 = new SecurityRiskAssessment
        {
            RiskLevel = SecurityRiskLevel.Medium,
            DetectedConcerns = concerns, // Same reference for collection equality
            AdditionalInfo = "Test"
        };

        // Act & Assert - Records with same property values including collection reference are equal
        Assert.Equal(assessment1, assessment2);
    }

    [Theory]
    [InlineData(SecurityRiskLevel.None, 0)]
    [InlineData(SecurityRiskLevel.Low, 1)]
    [InlineData(SecurityRiskLevel.Medium, 2)]
    [InlineData(SecurityRiskLevel.High, 3)]
    [InlineData(SecurityRiskLevel.Critical, 4)]
    public void SecurityRiskLevel_ShouldHaveCorrectNumericValues(SecurityRiskLevel level, int expectedValue)
    {
        // Assert
        Assert.Equal(expectedValue, (int)level);
    }

    [Theory]
    [InlineData(SecurityRiskLevel.None, false)]
    [InlineData(SecurityRiskLevel.Low, false)]
    [InlineData(SecurityRiskLevel.Medium, false)]
    [InlineData(SecurityRiskLevel.High, true)]
    [InlineData(SecurityRiskLevel.Critical, true)]
    public void ShouldBlock_BasedOnRiskLevel_ReturnsCorrectValue(SecurityRiskLevel level, bool expectedShouldBlock)
    {
        // Arrange
        var assessment = new SecurityRiskAssessment { RiskLevel = level };

        // Act & Assert
        Assert.Equal(expectedShouldBlock, assessment.ShouldBlock);
    }
}
