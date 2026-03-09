using MyGame.Commands;
using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using System;
using System.IO;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for SaveCommand and LoadCommand — game persistence.
/// </summary>
public class SaveLoadTests : IDisposable
{
    private readonly string _testDirectory;

    public SaveLoadTests()
    {
        // Create a unique temp directory for each test run
        _testDirectory = Path.Combine(Path.GetTempPath(), $"MyGameTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        // Clean up temp directory after tests
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public void Save_NoFilename_CreatesDefaultSavegameFile()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new SaveCommand(_testDirectory);

        // Act
        cmd.Execute(new ParsedCommand("save", null), state, io);

        // Assert
        var expectedPath = Path.Combine(_testDirectory, "savegame.json");
        Assert.True(File.Exists(expectedPath), "savegame.json should be created");
        Assert.True(io.OutputContains("saved"));
    }

    [Fact]
    public void Save_CustomFilename_UsesCustomFilename()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new SaveCommand(_testDirectory);

        // Act
        cmd.Execute(new ParsedCommand("save", "mysave"), state, io);

        // Assert
        var expectedPath = Path.Combine(_testDirectory, "mysave.json");
        Assert.True(File.Exists(expectedPath), "mysave.json should be created");
    }

    [Fact]
    public void SaveThenLoad_CurrentRoomId_IsRestored()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState(startId: "room_a");
        state.CurrentRoomId = "room_b";
        var saveIo = new FakeInputOutput();
        var saveCmd = new SaveCommand(_testDirectory);
        var loadCmd = new LoadCommand(_testDirectory);

        // Act - Save
        saveCmd.Execute(new ParsedCommand("save", "test"), state, saveIo);

        // Create fresh state and load
        var newState = WorldFactory.TwoRoomState(startId: "room_a");
        var loadIo = new FakeInputOutput();
        loadCmd.Execute(new ParsedCommand("load", "test"), newState, loadIo);

        // Assert
        Assert.Equal("room_b", newState.CurrentRoomId);
    }

    [Fact]
    public void SaveThenLoad_Flags_AreRestored()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("door_unlocked");
        state.Flags.Add("met_viktor");
        var saveIo = new FakeInputOutput();
        var saveCmd = new SaveCommand(_testDirectory);
        var loadCmd = new LoadCommand(_testDirectory);

        // Act - Save
        saveCmd.Execute(new ParsedCommand("save", "test"), state, saveIo);

        // Create fresh state and load
        var newState = WorldFactory.SingleRoomState();
        var loadIo = new FakeInputOutput();
        loadCmd.Execute(new ParsedCommand("load", "test"), newState, loadIo);

        // Assert
        Assert.Contains("door_unlocked", newState.Flags);
        Assert.Contains("met_viktor", newState.Flags);
    }

    [Fact]
    public void SaveThenLoad_InventoryItems_AreRestored()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var keycard = WorldFactory.TakeableItem("keycard", "Keycard");
        var flyer = WorldFactory.TakeableItem("flyer", "Flyer");
        state.Inventory.Add(keycard);
        state.Inventory.Add(flyer);
        // Add to catalog so they can be restored
        state.ItemCatalog["keycard"] = keycard;
        state.ItemCatalog["flyer"] = flyer;
        var saveIo = new FakeInputOutput();
        var saveCmd = new SaveCommand(_testDirectory);
        var loadCmd = new LoadCommand(_testDirectory);

        // Act - Save
        saveCmd.Execute(new ParsedCommand("save", "test"), state, saveIo);

        // Create fresh state with catalog and load
        var newState = WorldFactory.SingleRoomState();
        newState.ItemCatalog["keycard"] = keycard;
        newState.ItemCatalog["flyer"] = flyer;
        var loadIo = new FakeInputOutput();
        loadCmd.Execute(new ParsedCommand("load", "test"), newState, loadIo);

        // Assert
        Assert.Equal(2, newState.Inventory.Count);
        Assert.True(newState.Inventory.Any(i => i.Id == "keycard"), "Keycard should be in inventory");
        Assert.True(newState.Inventory.Any(i => i.Id == "flyer"), "Flyer should be in inventory");
    }

    [Fact]
    public void Load_NonexistentFile_ShowsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new LoadCommand(_testDirectory);

        // Act
        cmd.Execute(new ParsedCommand("load", "nonexistent"), state, io);

        // Assert
        Assert.True(io.OutputContains("not found") || io.OutputContains("error"));
    }

    [Fact]
    public void Load_CorruptedJson_ShowsError()
    {
        // Arrange
        var corruptFile = Path.Combine(_testDirectory, "corrupt.json");
        File.WriteAllText(corruptFile, "{this is not valid json}");
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new LoadCommand(_testDirectory);

        // Act
        cmd.Execute(new ParsedCommand("load", "corrupt"), state, io);

        // Assert
        Assert.True(io.OutputContains("error") || io.OutputContains("invalid") || io.OutputContains("corrupt"));
    }
}
