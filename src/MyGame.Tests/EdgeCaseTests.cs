using MyGame.Commands;
using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using System.IO;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Edge case hardening tests for issue #16:
/// null input, nonexistent references, blocked exits, inventory overflow protection.
/// </summary>
public class EdgeCaseTests
{
    #region GoCommand Edge Cases

    [Fact]
    public void Go_InvalidTargetRoom_PrintsError()
    {
        // Arrange
        var roomA = new Room { Id = "room_a", Name = "Room A", Description = "Test." };
        roomA.Exits["east"] = new Exit { Direction = "east", TargetRoomId = "nonexistent_room" };
        var state = new GameState
        {
            CurrentRoomId = "room_a",
            Rooms = new Dictionary<string, Room> { ["room_a"] = roomA }
        };
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "east"), state, io);

        // Assert
        Assert.True(
            io.OutputContains("nowhere") || io.OutputContains("error") || io.OutputContains("can't go"),
            $"Expected an error message about invalid exit but got: {io.AllOutput}");
        Assert.Equal("room_a", state.CurrentRoomId);
    }

    [Fact]
    public void Go_InvalidTargetRoom_DoesNotCrash()
    {
        // Arrange
        var roomA = new Room { Id = "room_a", Name = "Room A", Description = "Test." };
        roomA.Exits["east"] = new Exit { Direction = "east", TargetRoomId = "nonexistent_room" };
        var state = new GameState
        {
            CurrentRoomId = "room_a",
            Rooms = new Dictionary<string, Room> { ["room_a"] = roomA }
        };
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act & Assert
        var ex = Record.Exception(() => cmd.Execute(new ParsedCommand("go", "east"), state, io));
        Assert.Null(ex);
    }

    [Fact]
    public void Go_LockedExit_NoRequiredItem_PrintsLockedMessage()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState();
        state.Rooms["room_a"].Exits["east"].IsLocked = true;
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "east"), state, io);

        // Assert
        Assert.True(io.OutputContains("locked"), $"Expected 'locked' in output but got: {io.AllOutput}");
    }

    [Fact]
    public void Go_LockedExit_WithRequiredItem_PrintsItemNeeded()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState();
        state.Rooms["room_a"].Exits["east"].IsLocked = true;
        state.Rooms["room_a"].Exits["east"].RequiredItemId = "keycard";
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "east"), state, io);

        // Assert
        Assert.True(io.OutputContains("keycard"), $"Expected 'keycard' in output but got: {io.AllOutput}");
    }

    [Fact]
    public void Go_NoDirection_PrintsPrompt()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", null), state, io);

        // Assert
        Assert.False(string.IsNullOrEmpty(io.AllOutput), "Expected a prompt/error for missing direction.");
        Assert.True(
            io.OutputContains("where") || io.OutputContains("direction"),
            $"Expected directional prompt but got: {io.AllOutput}");
    }

    #endregion

    #region SaveCommand Edge Cases

    [Fact]
    public void Save_PathTraversal_StaysInBaseDirectory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new SaveCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("save", "../../../evil"), state, io);

            // Assert — saved path in output should not traverse up
            Assert.False(io.AllOutput.Contains(".."), $"Output should not contain '..': {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Save_PathTraversal_SlashInFilename()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new SaveCommand(tempDir);

            // Act — slash in noun should be stripped to just "file"
            cmd.Execute(new ParsedCommand("save", "subfolder/file"), state, io);

            // Assert
            Assert.True(io.OutputContains("file.json"), $"Expected 'file.json' in output but got: {io.AllOutput}");
            Assert.False(Directory.Exists(Path.Combine(tempDir, "subfolder")), "No subfolder should be created.");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Save_NullNoun_DefaultsToSavegame()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new SaveCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("save", null), state, io);

            // Assert
            Assert.True(io.OutputContains("savegame.json"), $"Expected 'savegame.json' in output but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Save_AddsJsonExtension()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new SaveCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("save", "mysave"), state, io);

            // Assert
            Assert.True(io.OutputContains("mysave.json"), $"Expected 'mysave.json' in output but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    #endregion

    #region LoadCommand Edge Cases

    [Fact]
    public void Load_FileNotFound_PrintsMessage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("load", "nonexistent_file"), state, io);

            // Assert
            Assert.True(
                io.OutputContains("not found") || io.OutputContains("no save") || io.OutputContains("not exist"),
                $"Expected 'not found' message but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_CorruptedJson_PrintsMessage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "corrupt.json"), "not valid json {{{");
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("load", "corrupt"), state, io);

            // Assert
            Assert.True(
                io.OutputContains("corrupt") || io.OutputContains("error") || io.OutputContains("invalid"),
                $"Expected error message for corrupted JSON but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_NullInventoryInSave_DoesNotCrash()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "nullsave.json"), """
                {
                  "currentRoomId": "test_room",
                  "inventory": null,
                  "flags": null
                }
                """);
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act & Assert
            var ex = Record.Exception(() => cmd.Execute(new ParsedCommand("load", "nullsave"), state, io));
            Assert.Null(ex);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_NullInventoryInSave_LoadsSuccessfully()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "nullsave.json"), """
                {
                  "currentRoomId": "test_room",
                  "inventory": null,
                  "flags": null
                }
                """);
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("load", "nullsave"), state, io);

            // Assert
            Assert.True(io.OutputContains("loaded"), $"Expected 'loaded' confirmation but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_PathTraversal_StaysInBaseDirectory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act — path traversal in noun should be stripped
            cmd.Execute(new ParsedCommand("load", "../../../evil"), state, io);

            // Assert — referenced path in output must not traverse up
            Assert.False(io.AllOutput.Contains(".."), $"Output should not contain '..': {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_UnknownRoomId_PrintsMessage()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "badroom.json"), """
                {
                  "currentRoomId": "does_not_exist",
                  "inventory": [],
                  "flags": []
                }
                """);
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act
            cmd.Execute(new ParsedCommand("load", "badroom"), state, io);

            // Assert — should report that the room no longer exists rather than crash
            Assert.False(string.IsNullOrEmpty(io.AllOutput), "Expected an error message for unknown room.");
            Assert.True(
                io.OutputContains("no longer") || io.OutputContains("not exist") || io.OutputContains("room"),
                $"Expected message about missing room but got: {io.AllOutput}");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void Load_UnknownItemInInventory_SkipsItem()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        try
        {
            File.WriteAllText(Path.Combine(tempDir, "ghostitem.json"), """
                {
                  "currentRoomId": "test_room",
                  "inventory": ["ghost_item"],
                  "flags": []
                }
                """);
            var state = WorldFactory.SingleRoomState();
            var io = new FakeInputOutput();
            var cmd = new LoadCommand(tempDir);

            // Act
            var ex = Record.Exception(() => cmd.Execute(new ParsedCommand("load", "ghostitem"), state, io));

            // Assert — should not crash, and the unknown item must not appear in inventory
            Assert.Null(ex);
            Assert.DoesNotContain(state.Inventory, i => i.Id == "ghost_item");
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    #endregion

    #region TakeCommand Edge Cases

    [Fact]
    public void Take_NullNoun_PrintsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", null), state, io);

        // Assert
        Assert.False(string.IsNullOrEmpty(io.AllOutput), "Expected an error prompt when noun is null.");
        Assert.True(io.OutputContains("Take what?"), $"Expected 'Take what?' but got: {io.AllOutput}");
    }

    [Fact]
    public void Take_NonexistentItem_PrintsMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "ghost_item"), state, io);

        // Assert
        Assert.False(string.IsNullOrEmpty(io.AllOutput), "Expected a message when item doesn't exist in room.");
        Assert.True(
            io.OutputContains("no") || io.OutputContains("here") || io.OutputContains("find"),
            $"Expected 'not here' message but got: {io.AllOutput}");
    }

    [Fact]
    public void Take_NotTakeable_PrintsMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var scenery = WorldFactory.SceneryItem("statue", "Marble Statue");
        state.Rooms["test_room"].Items.Add(scenery);
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "statue"), state, io);

        // Assert
        Assert.True(
            io.OutputContains("can't") || io.OutputContains("cannot") || io.OutputContains("taken"),
            $"Expected can't-be-taken message but got: {io.AllOutput}");
    }

    #endregion

    #region DropCommand Edge Cases

    [Fact]
    public void Drop_NullNoun_PrintsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", null), state, io);

        // Assert
        Assert.False(string.IsNullOrEmpty(io.AllOutput), "Expected an error prompt when noun is null.");
        Assert.True(io.OutputContains("Drop what?"), $"Expected 'Drop what?' but got: {io.AllOutput}");
    }

    [Fact]
    public void Drop_ItemNotInInventory_PrintsMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "ghost_item"), state, io);

        // Assert
        Assert.True(
            io.OutputContains("not carrying") || io.OutputContains("don't have") || io.OutputContains("not in"),
            $"Expected 'not carrying' message but got: {io.AllOutput}");
    }

    #endregion

    #region CommandRegistry Edge Cases

    [Fact]
    public void CommandRegistry_UnknownVerb_PrintsHelpMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var registry = new CommandRegistry();

        // Act
        registry.Execute(new ParsedCommand("xyzzy", null), state, io);

        // Assert
        Assert.True(
            io.OutputContains("Unknown command") || io.OutputContains("help"),
            $"Expected 'Unknown command' or 'help' but got: {io.AllOutput}");
    }

    #endregion
}
