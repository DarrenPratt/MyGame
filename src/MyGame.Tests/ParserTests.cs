using MyGame.Engine;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for CommandParser — specifically the Target field in ParsedCommand.
/// CommandParser.Parse splits "use X on Y" into Verb="use", Noun="X", Target="Y".
/// </summary>
public class ParserTests
{
    [Fact]
    public void Parse_UseKeycard_NoTarget()
    {
        var result = CommandParser.Parse("use keycard");

        Assert.Equal("use", result.Verb);
        Assert.Equal("keycard", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_UseKeycardOnDoor_HasTarget()
    {
        var result = CommandParser.Parse("use keycard on door");

        Assert.Equal("use", result.Verb);
        Assert.Equal("keycard", result.Noun);
        Assert.Equal("door", result.Target);
    }

    [Fact]
    public void Parse_UseItemOnTargetWithSpaces_TargetIncludesAllWordsAfterOn()
    {
        var result = CommandParser.Parse("use item on target with spaces");

        Assert.Equal("use", result.Verb);
        Assert.Equal("item", result.Noun);
        Assert.Equal("target with spaces", result.Target);
    }

    [Fact]
    public void Parse_TalkToViktor_DoesNotStripTo()
    {
        var result = CommandParser.Parse("talk to viktor");

        Assert.Equal("talk", result.Verb);
        Assert.Equal("to viktor", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_GoNorth_NoTarget()
    {
        var result = CommandParser.Parse("go north");

        Assert.Equal("go", result.Verb);
        Assert.Equal("north", result.Noun);
        Assert.Null(result.Target);
    }

    [Fact]
    public void Parse_EmptyString_ReturnsEmptyVerbAndNulls()
    {
        var result = CommandParser.Parse("");

        Assert.Equal("", result.Verb);
        Assert.Null(result.Noun);
        Assert.Null(result.Target);
    }
}
