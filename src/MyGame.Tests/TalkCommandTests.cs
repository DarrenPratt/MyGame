using MyGame.Commands;
using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for TalkCommand — conversation system with NPCs.
/// </summary>
public class TalkCommandTests
{
    private static Npc CreateTestNpc(string id = "viktor", string name = "Viktor")
    {
        return new Npc
        {
            Id = id,
            Name = name,
            Description = "A test NPC.",
            Dialogue = new()
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "Hello, stranger.",
                    Responses = new()
                    {
                        new DialogueResponse
                        {
                            Text = "Who are you?",
                            NextNodeId = "who"
                        },
                        new DialogueResponse
                        {
                            Text = "Goodbye.",
                            NextNodeId = null
                        }
                    }
                },
                new DialogueNode
                {
                    Id = "who",
                    Text = "I'm Viktor, a data broker.",
                    Responses = new()
                }
            }
        };
    }

    [Fact]
    public void Execute_TalkToViktor_ViktorInRoom_ShowsDialogueStart()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var viktor = CreateTestNpc("viktor", "Viktor");
        state.CurrentRoom.Npcs.Add(viktor);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "to viktor"), state, io);

        // Assert
        Assert.True(io.OutputContains("Hello, stranger"));
    }

    [Fact]
    public void Execute_TalkViktor_NoToPrefix_ShowsDialogue()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var viktor = CreateTestNpc("viktor", "Viktor");
        state.CurrentRoom.Npcs.Add(viktor);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "viktor"), state, io);

        // Assert
        Assert.True(io.OutputContains("Hello, stranger"));
    }

    [Fact]
    public void Execute_TalkToNonexistentNpc_ShowsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "to ghost"), state, io);

        // Assert
        Assert.True(io.OutputContains("No one named"));
        Assert.True(io.OutputContains("ghost"));
    }

    [Fact]
    public void Execute_TalkWithNoNoun_ShowsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Talk to whom"));
    }

    [Fact]
    public void Execute_DialogueWithNoResponses_ShowsTextAndEnds()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var npc = new Npc
        {
            Id = "quiet",
            Name = "Quiet NPC",
            Description = "Says one thing.",
            Dialogue = new()
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "I have nothing more to say.",
                    Responses = new()
                }
            }
        };
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "quiet"), state, io);

        // Assert
        Assert.True(io.OutputContains("I have nothing more to say"));
        // Should not prompt for input when no responses available
    }

    [Fact]
    public void Execute_DialogueWithResponses_UserInputOne_AdvancesToNextNode()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var viktor = CreateTestNpc();
        state.CurrentRoom.Npcs.Add(viktor);
        var io = new FakeInputOutput("1"); // User chooses first option
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "viktor"), state, io);

        // Assert
        Assert.True(io.OutputContains("Hello, stranger")); // First node
        Assert.True(io.OutputContains("I'm Viktor, a data broker")); // Second node after choice
    }

    [Fact]
    public void Execute_NpcFoundById_ShowsDialogue()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var npc = CreateTestNpc("npc_001", "Data Broker");
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "npc_001"), state, io);

        // Assert
        Assert.True(io.OutputContains("Hello, stranger"));
    }

    [Fact]
    public void Execute_NpcFoundByName_ShowsDialogue()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var npc = CreateTestNpc("npc_001", "Data Broker");
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "data broker"), state, io);

        // Assert
        Assert.True(io.OutputContains("Hello, stranger"));
    }
}
