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

    // -------------------------------------------------------------------------
    // Issue #35 — Save/Load State Corruption Tests
    // These tests prove DroneThreatLevel and exit locked states survive a
    // save/load round-trip. Tests marked TODO will pass after #35 is fixed.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a two-room state where room_a has a north exit that starts LOCKED.
    /// Used to test exit-state persistence.
    /// </summary>
    private GameState TwoRoomStateWithLockedExit()
    {
        var roomA = new Room { Id = "room_a", Name = "Room A", Description = "A plain room." };
        var roomB = new Room { Id = "room_b", Name = "Room B", Description = "Another room." };
        roomA.Exits["north"] = new Exit { Direction = "north", TargetRoomId = "room_b", IsLocked = true };
        roomB.Exits["south"] = new Exit { Direction = "south", TargetRoomId = "room_a" };
        return new GameState
        {
            CurrentRoomId = "room_a",
            Rooms = new Dictionary<string, Room> { ["room_a"] = roomA, ["room_b"] = roomB }
        };
    }

    [Fact]
    public void SaveLoad_PreservesDroneThreatLevel()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.DroneThreatLevel = 3;
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "threat-level"), state, new FakeInputOutput());

        // Act — load into a fresh state (default DroneThreatLevel = 0)
        var newState = WorldFactory.SingleRoomState();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "threat-level"), newState, new FakeInputOutput());

        // TODO: will pass after #35 is fixed (SaveCommand must serialize DroneThreatLevel)
        Assert.Equal(3, newState.DroneThreatLevel);
    }

    [Fact]
    public void SaveLoad_PreservesDroneThreatThreshold()
    {
        // DroneThreatThreshold is init-only; construct the source state with the custom value directly.
        // NOTE: Fixing this test end-to-end also requires DroneThreatThreshold to become mutable
        // (change `init` → `set` in GameState) so LoadCommand can restore it.
        var room = new Room { Id = "test_room", Name = "Test Room", Description = "A test room." };
        var state = new GameState
        {
            CurrentRoomId = "test_room",
            Rooms = new Dictionary<string, Room> { ["test_room"] = room },
            DroneThreatThreshold = 6   // non-default (default is 4)
        };
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "threat-thresh"), state, new FakeInputOutput());

        // Act — load into a fresh state that has the default threshold (4)
        var newState = WorldFactory.SingleRoomState();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "threat-thresh"), newState, new FakeInputOutput());

        // TODO: will pass after #35 is fixed (requires DroneThreatThreshold to become settable)
        Assert.Equal(6, newState.DroneThreatThreshold);
    }

    [Fact]
    public void SaveLoad_PreservesUnlockedExit()
    {
        // Arrange — start with a locked exit, then unlock it before saving
        var state = TwoRoomStateWithLockedExit();
        state.Rooms["room_a"].Exits["north"].IsLocked = false;  // player unlocked the door
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "unlock-exit"), state, new FakeInputOutput());

        // Act — load into a fresh state where that exit is locked again by default
        var newState = TwoRoomStateWithLockedExit();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "unlock-exit"), newState, new FakeInputOutput());

        // TODO: will pass after #35 is fixed (LoadCommand must restore exit IsLocked states)
        Assert.False(newState.Rooms["room_a"].Exits["north"].IsLocked, "Exit should remain unlocked after reload");
    }

    [Fact]
    public void SaveLoad_DroneThreatZeroByDefault_NotCorrupted()
    {
        // Arrange — default DroneThreatLevel starts at 0
        var state = WorldFactory.SingleRoomState();
        Assert.Equal(0, state.DroneThreatLevel); // sanity-check the starting condition
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "zero-threat"), state, new FakeInputOutput());

        // Act
        var newState = WorldFactory.SingleRoomState();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "zero-threat"), newState, new FakeInputOutput());

        // A 0 value must survive the round-trip — it must not be silently dropped or treated as null
        Assert.Equal(0, newState.DroneThreatLevel);
    }

    [Fact]
    public void SaveLoad_NonZeroThreatSurvivesRoundtrip()
    {
        // Player must not be able to exploit save/load to reset their drone threat level
        var state = WorldFactory.SingleRoomState();
        state.DroneThreatLevel = 5;  // high-risk accumulated threat
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "non-zero-threat"), state, new FakeInputOutput());

        // Act — load into a fresh state (default DroneThreatLevel = 0)
        var newState = WorldFactory.SingleRoomState();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "non-zero-threat"), newState, new FakeInputOutput());

        // TODO: will pass after #35 is fixed (SaveCommand must serialize DroneThreatLevel)
        Assert.Equal(5, newState.DroneThreatLevel);
    }

    [Fact]
    public void SaveLoad_LockedExitRemainsLockedAfterReload()
    {
        // Baseline regression guard: a locked exit must stay locked across a save/load cycle
        var state = TwoRoomStateWithLockedExit();
        // Do NOT unlock the exit before saving
        new SaveCommand(_testDirectory).Execute(new ParsedCommand("save", "locked-exit"), state, new FakeInputOutput());

        // Act
        var newState = TwoRoomStateWithLockedExit();
        new LoadCommand(_testDirectory).Execute(new ParsedCommand("load", "locked-exit"), newState, new FakeInputOutput());

        Assert.True(newState.Rooms["room_a"].Exits["north"].IsLocked, "Locked exit must remain locked after reload");
    }
}
