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

    [Fact]
    public void Execute_DialogueNodeWithSetsFlag_SetsFlagOnGameState()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var npc = new Npc
        {
            Id = "informant",
            Name = "Informant",
            Description = "A contact with useful intel.",
            Dialogue = new()
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "Meet me at the checkpoint at midnight.",
                    SetsFlag = "informant_tipped_off",
                    Responses = new()
                }
            }
        };
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "informant"), state, io);

        // Assert
        Assert.Contains("informant_tipped_off", state.Flags);
    }

    [Fact]
    public void Execute_DialogueNodeWithoutSetsFlag_DoesNotAddExtraFlag()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var npc = CreateTestNpc("viktor", "Viktor");
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput();
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "viktor"), state, io);

        // Assert: only the _met flag should be set, no extra flags
        Assert.Contains("viktor_met", state.Flags);
        Assert.Single(state.Flags);
    }

    [Fact]
    public void Execute_MultiNodeDialogue_SetsFlag_OnlyWhenNodeReached()
    {
        // Arrange: first node has no flag, second node sets a flag
        var state = WorldFactory.SingleRoomState();
        var npc = new Npc
        {
            Id = "contact",
            Name = "Contact",
            Description = "A street contact.",
            Dialogue = new()
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "Who are you?",
                    Responses = new()
                    {
                        new DialogueResponse { Text = "A runner.", NextNodeId = "reveal" }
                    }
                },
                new DialogueNode
                {
                    Id = "reveal",
                    Text = "Then I'll tell you about the door.",
                    SetsFlag = "contact_revealed_door",
                    Responses = new()
                }
            }
        };
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput("1"); // choose first option to advance
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "contact"), state, io);

        // Assert: flag only set because we reached the second node
        Assert.Contains("contact_revealed_door", state.Flags);
    }

    [Fact]
    public void Execute_MultiNodeDialogue_FlagNotSet_WhenNodeNotReached()
    {
        // Arrange: two responses; second exits before flag node
        var state = WorldFactory.SingleRoomState();
        var npc = new Npc
        {
            Id = "contact",
            Name = "Contact",
            Description = "A street contact.",
            Dialogue = new()
            {
                new DialogueNode
                {
                    Id = "start",
                    Text = "Who are you?",
                    Responses = new()
                    {
                        new DialogueResponse { Text = "A runner.", NextNodeId = "reveal" },
                        new DialogueResponse { Text = "Nobody.", NextNodeId = null }
                    }
                },
                new DialogueNode
                {
                    Id = "reveal",
                    Text = "Then I'll tell you about the door.",
                    SetsFlag = "contact_revealed_door",
                    Responses = new()
                }
            }
        };
        state.CurrentRoom.Npcs.Add(npc);
        var io = new FakeInputOutput("2"); // choose "Nobody." — exits without reaching reveal
        var cmd = new TalkCommand();

        // Act
        cmd.Execute(new ParsedCommand("talk", "contact"), state, io);

        // Assert: flag NOT set because we never reached the second node
        Assert.DoesNotContain("contact_revealed_door", state.Flags);
    }
}
