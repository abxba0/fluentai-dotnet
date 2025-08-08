using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Models;

/// <summary>
/// Unit tests for ChatMessage record following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate ChatMessage record correctly stores role and content data
/// EXPECTED BEHAVIOR: Record creates, stores, and provides access to role and content properties
/// METRICS: Correctness (must be 100% accurate), immutability (record behavior)
/// </summary>
public class ChatMessageTests
{
    // TEST #1: Normal case - valid ChatMessage creation
    [Fact]
    public void Constructor_WithValidRoleAndContent_CreatesCorrectChatMessage()
    {
        // INPUT: Valid ChatRole and non-empty content string
        var role = ChatRole.User;
        var content = "Hello, how are you?";

        // EXPECTED: ChatMessage with correct role and content properties
        var message = new ChatMessage(role, content);

        Assert.Equal(role, message.Role);
        Assert.Equal(content, message.Content);
    }

    // TEST #2: Edge case - empty content string
    [Fact]
    public void Constructor_WithEmptyContent_CreatesValidChatMessage()
    {
        // INPUT: Valid role with empty content string
        var role = ChatRole.System;
        var content = "";

        // EXPECTED: ChatMessage created successfully with empty content
        var message = new ChatMessage(role, content);

        Assert.Equal(role, message.Role);
        Assert.Equal(content, message.Content);
    }

    // TEST #3: Edge case - null content (should handle gracefully)
    [Fact]
    public void Constructor_WithNullContent_CreatesValidChatMessage()
    {
        // INPUT: Valid role with null content
        var role = ChatRole.Assistant;
        string? content = null;

        // EXPECTED: ChatMessage created with null content (record allows null)
        var message = new ChatMessage(role, content!);

        Assert.Equal(role, message.Role);
        Assert.Null(message.Content);
    }

    // TEST #4: Normal case - all ChatRole enum values
    [Theory]
    [InlineData(ChatRole.System)]
    [InlineData(ChatRole.User)]
    [InlineData(ChatRole.Assistant)]
    public void Constructor_WithAllValidRoles_CreatesCorrectChatMessage(ChatRole role)
    {
        // INPUT: Each valid ChatRole enumeration value
        var content = "Test content";

        // EXPECTED: ChatMessage with correct role for each enum value
        var message = new ChatMessage(role, content);

        Assert.Equal(role, message.Role);
        Assert.Equal(content, message.Content);
    }

    // TEST #5: Edge case - very long content string
    [Fact]
    public void Constructor_WithVeryLongContent_CreatesValidChatMessage()
    {
        // INPUT: Valid role with very long content string (boundary test)
        var role = ChatRole.User;
        var content = new string('a', 10000); // 10k characters

        // EXPECTED: ChatMessage handles large content without issues
        var message = new ChatMessage(role, content);

        Assert.Equal(role, message.Role);
        Assert.Equal(content, message.Content);
        Assert.Equal(10000, message.Content.Length);
    }

    // TEST #6: Record equality behavior
    [Fact]
    public void Equality_WithSameRoleAndContent_ReturnsTrueForEquality()
    {
        // INPUT: Two ChatMessage instances with identical role and content
        var message1 = new ChatMessage(ChatRole.User, "Hello");
        var message2 = new ChatMessage(ChatRole.User, "Hello");

        // EXPECTED: Records should be equal (value equality)
        Assert.Equal(message1, message2);
        Assert.True(message1 == message2);
        Assert.False(message1 != message2);
    }

    // TEST #7: Record inequality behavior
    [Fact]
    public void Equality_WithDifferentContent_ReturnsFalseForEquality()
    {
        // INPUT: Two ChatMessage instances with same role but different content
        var message1 = new ChatMessage(ChatRole.User, "Hello");
        var message2 = new ChatMessage(ChatRole.User, "Hi");

        // EXPECTED: Records should not be equal
        Assert.NotEqual(message1, message2);
        Assert.False(message1 == message2);
        Assert.True(message1 != message2);
    }

    // TEST #8: Record immutability behavior (with expression)
    [Fact]
    public void WithExpression_ModifyingRole_CreatesNewInstance()
    {
        // INPUT: ChatMessage with modification using 'with' expression
        var original = new ChatMessage(ChatRole.User, "Hello");
        var modified = original with { Role = ChatRole.Assistant };

        // EXPECTED: New instance created, original unchanged
        Assert.Equal(ChatRole.User, original.Role);
        Assert.Equal(ChatRole.Assistant, modified.Role);
        Assert.Equal("Hello", original.Content);
        Assert.Equal("Hello", modified.Content);
        Assert.NotSame(original, modified);
    }

    // TEST #9: ToString() behavior
    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // INPUT: ChatMessage instance
        var message = new ChatMessage(ChatRole.User, "Test message");

        // EXPECTED: ToString returns formatted string representation
        var result = message.ToString();

        Assert.Contains("User", result);
        Assert.Contains("Test message", result);
    }

    // TEST #10: GetHashCode consistency
    [Fact]
    public void GetHashCode_WithEqualMessages_ReturnsSameHashCode()
    {
        // INPUT: Two equal ChatMessage instances
        var message1 = new ChatMessage(ChatRole.System, "Instructions");
        var message2 = new ChatMessage(ChatRole.System, "Instructions");

        // EXPECTED: Same hash code for equal objects
        Assert.Equal(message1.GetHashCode(), message2.GetHashCode());
    }
}