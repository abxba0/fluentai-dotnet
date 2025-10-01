using FluentAI.Abstractions.Models;
using Xunit;

namespace FluentAI.NET.Tests.UnitTests.Models;

/// <summary>
/// Unit tests for ChatRole enum following the rigorous test plan template.
/// 
/// REQUIREMENT: Validate ChatRole enum correctly defines chat message participant roles
/// EXPECTED BEHAVIOR: Enum provides correct values for System, User, and Assistant roles
/// METRICS: Correctness (must have all expected values), type safety (enum behavior)
/// </summary>
public class ChatRoleTests
{
    // TEST #1: Normal case - enum values exist
    [Fact]
    public void ChatRole_HasAllExpectedValues()
    {
        // INPUT: Check for all expected enum values
        // EXPECTED: All three chat roles are defined

        Assert.True(Enum.IsDefined(typeof(ChatRole), ChatRole.System));
        Assert.True(Enum.IsDefined(typeof(ChatRole), ChatRole.User));
        Assert.True(Enum.IsDefined(typeof(ChatRole), ChatRole.Assistant));
    }

    // TEST #2: Normal case - enum value conversion to string
    [Theory]
    [InlineData(ChatRole.System, "System")]
    [InlineData(ChatRole.User, "User")]
    [InlineData(ChatRole.Assistant, "Assistant")]
    public void ChatRole_ToStringReturnsCorrectName(ChatRole role, string expectedName)
    {
        // INPUT: Each ChatRole enum value
        // EXPECTED: ToString returns the enum name
        
        Assert.Equal(expectedName, role.ToString());
    }

    // TEST #3: Normal case - enum integer values
    [Fact]
    public void ChatRole_HasCorrectIntegerValues()
    {
        // INPUT: Check underlying integer values of enum
        // EXPECTED: Values should be 0, 1, 2 (default enum ordering)
        
        Assert.Equal(0, (int)ChatRole.System);
        Assert.Equal(1, (int)ChatRole.User);
        Assert.Equal(2, (int)ChatRole.Assistant);
    }

    // TEST #4: Normal case - enum count
    [Fact]
    public void ChatRole_HasExactlyFourValues()
    {
        // INPUT: Count all enum values
        // EXPECTED: Exactly four values should exist (System, User, Assistant, Tool)
        
        var enumValues = Enum.GetValues(typeof(ChatRole));
        Assert.Equal(4, enumValues.Length);
    }

    // TEST #5: Normal case - enum parsing from string
    [Theory]
    [InlineData("System", ChatRole.System)]
    [InlineData("User", ChatRole.User)]
    [InlineData("Assistant", ChatRole.Assistant)]
    public void ChatRole_ParseFromStringReturnsCorrectValue(string roleName, ChatRole expectedRole)
    {
        // INPUT: String representation of enum names
        // EXPECTED: Parsing returns correct enum value
        
        var parsed = Enum.Parse<ChatRole>(roleName);
        Assert.Equal(expectedRole, parsed);
    }

    // TEST #6: Error case - invalid enum parsing
    [Theory]
    [InlineData("InvalidRole")]
    [InlineData("")]
    [InlineData("system")] // case sensitive
    public void ChatRole_ParseInvalidStringThrowsException(string invalidRole)
    {
        // INPUT: Invalid string values
        // EXPECTED: ArgumentException should be thrown
        
        Assert.Throws<ArgumentException>(() => Enum.Parse<ChatRole>(invalidRole));
    }

    // TEST #7: Normal case - enum comparison
    [Fact]
    public void ChatRole_EqualityComparisonWorks()
    {
        // INPUT: Compare enum values
        // EXPECTED: Equality works correctly
        
        var role1 = ChatRole.User;
        var role2 = ChatRole.User;
        var role3 = ChatRole.Assistant;

        Assert.Equal(role1, role2);
        Assert.True(role1 == role2);
        Assert.False(role1 == role3);
        Assert.NotEqual(role1, role3);
    }

    // TEST #8: Normal case - enum in switch statements
    [Theory]
    [InlineData(ChatRole.System, "System role")]
    [InlineData(ChatRole.User, "User role")]
    [InlineData(ChatRole.Assistant, "Assistant role")]
    public void ChatRole_WorksInSwitchStatements(ChatRole role, string expectedResult)
    {
        // INPUT: Use enum in switch expression
        // EXPECTED: Switch correctly matches enum values
        
        var result = role switch
        {
            ChatRole.System => "System role",
            ChatRole.User => "User role", 
            ChatRole.Assistant => "Assistant role",
            _ => "Unknown role"
        };

        Assert.Equal(expectedResult, result);
    }

    // TEST #9: Edge case - cast from integer
    [Theory]
    [InlineData(0, ChatRole.System)]
    [InlineData(1, ChatRole.User)]
    [InlineData(2, ChatRole.Assistant)]
    public void ChatRole_CastFromIntegerReturnsCorrectValue(int intValue, ChatRole expectedRole)
    {
        // INPUT: Integer values corresponding to enum
        // EXPECTED: Cast returns correct enum value
        
        var role = (ChatRole)intValue;
        Assert.Equal(expectedRole, role);
    }

    // TEST #10: Edge case - invalid integer cast doesn't throw but creates invalid enum
    [Fact]
    public void ChatRole_InvalidIntegerCastCreatesInvalidEnum()
    {
        // INPUT: Invalid integer value (outside defined range)
        // EXPECTED: Cast succeeds but creates undefined enum value
        
        var invalidRole = (ChatRole)999;
        
        // This should not throw, but the value is not defined
        Assert.False(Enum.IsDefined(typeof(ChatRole), invalidRole));
        Assert.Equal(999, (int)invalidRole);
    }
}