using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Models;

/// <summary>
/// Simple unit tests for ChatMessage record to verify test setup.
/// </summary>
public class ChatMessageTests
{
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
}