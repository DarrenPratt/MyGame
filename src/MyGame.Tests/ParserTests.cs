using MyGame.Engine;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for Parser — specifically the new Target field in ParsedCommand.
/// Parser now splits "use X on Y" into Verb="use", Noun="X", Target="Y".
/// </summary>
public class ParserTests
{
    [Fact]
    public void Parse_UseKeycard_NoTarget()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("use keycard");

        // Assert
        Assert.Equal("use", result.Verb);
        Assert.Equal("keycard", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_UseKeycardOnDoor_HasTarget()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("use keycard on door");

        // Assert
        Assert.Equal("use", result.Verb);
        Assert.Equal("keycard", result.Noun);
        Assert.Equal("door", result.Target);
    }

    [Fact]
    public void Parse_UseItemOnTargetWithSpaces_TargetIncludesAllWordsAfterOn()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("use item on target with spaces");

        // Assert
        Assert.Equal("use", result.Verb);
        Assert.Equal("item", result.Noun);
        Assert.Equal("target with spaces", result.Target);
    }

    [Fact]
    public void Parse_TalkToViktor_DoesNotStripTo()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("talk to viktor");

        // Assert
        Assert.Equal("talk", result.Verb);
        Assert.Equal("to viktor", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_GoNorth_NoTarget()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("go north");

        // Assert
        Assert.Equal("go", result.Verb);
        Assert.Equal("north", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyVerbAndNulls()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse("");

        // Assert
        Assert.Equal("", result.Verb);
        Assert.Null(result.Noun);
        Assert.Null(result.Target);
    }
}
