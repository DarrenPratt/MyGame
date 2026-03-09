using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for GameState — the mutable state container.
/// GameState itself has no logic; tests verify its data-holding contract.
/// </summary>
public class GameStateTests
{
    // ──────────────────────────────────────────────
    // Starting conditions
    // ──────────────────────────────────────────────

    [Fact]
    public void PlayerStartsInCorrectStartingRoom()
    {
        // Arrange & Act
        var state = WorldFactory.TwoRoomState(startId: "room_a");

        // Assert
        Assert.Equal("room_a", state.CurrentRoomId);
    }

    [Fact]
    public void CurrentRoom_ReturnsRoomMatchingCurrentRoomId()
    {
        var state = WorldFactory.TwoRoomState(startId: "room_a");

        Assert.Equal("room_a", state.CurrentRoom.Id);
        Assert.Equal("Room A", state.CurrentRoom.Name);
    }

    [Fact]
    public void InventoryIsEmptyAtStart()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.Empty(state.Inventory);
    }

    [Fact]
    public void IsRunning_IsTrueAtStart()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.True(state.IsRunning);
    }

    [Fact]
    public void HasWon_IsFalseAtStart()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.False(state.HasWon);
    }

    // ──────────────────────────────────────────────
    // Inventory manipulation
    // ──────────────────────────────────────────────

    [Fact]
    public void AddItemToInventory_ItemAppearsInInventory()
    {
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem();

        state.Inventory.Add(item);

        Assert.Contains(item, state.Inventory);
    }

    [Fact]
    public void RemoveItemFromInventory_ItemNoLongerInInventory()
    {
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem();
        state.Inventory.Add(item);

        state.Inventory.Remove(item);

        Assert.DoesNotContain(item, state.Inventory);
    }

    [Fact]
    public void InventoryCanHoldMultipleItems()
    {
        var state = WorldFactory.SingleRoomState();
        var item1 = WorldFactory.TakeableItem("a", "Item A");
        var item2 = WorldFactory.TakeableItem("b", "Item B");

        state.Inventory.Add(item1);
        state.Inventory.Add(item2);

        Assert.Equal(2, state.Inventory.Count);
        Assert.Contains(item1, state.Inventory);
        Assert.Contains(item2, state.Inventory);
    }

    // ──────────────────────────────────────────────
    // Room navigation (state mutation)
    // ──────────────────────────────────────────────

    [Fact]
    public void SettingCurrentRoomId_UpdatesCurrentRoom()
    {
        var state = WorldFactory.TwoRoomState(startId: "room_a");

        state.CurrentRoomId = "room_b";

        Assert.Equal("room_b", state.CurrentRoomId);
        Assert.Equal("Room B", state.CurrentRoom.Name);
    }

    [Fact]
    public void CurrentRoom_ReflectsNewRoomAfterChange()
    {
        var state = WorldFactory.TwoRoomState(startId: "room_a");

        state.CurrentRoomId = "room_b";

        Assert.Equal("room_b", state.CurrentRoom.Id);
    }

    // ──────────────────────────────────────────────
    // Game flags
    // ──────────────────────────────────────────────

    [Fact]
    public void Flags_AreEmptyAtStart()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.Empty(state.Flags);
    }

    [Fact]
    public void SetFlag_FlagCanBeRead()
    {
        var state = WorldFactory.SingleRoomState();

        state.Flags.Add("door_unlocked");

        Assert.Contains("door_unlocked", state.Flags);
    }

    [Fact]
    public void SetMultipleFlags_AllCanBeRead()
    {
        var state = WorldFactory.SingleRoomState();

        state.Flags.Add("flag_one");
        state.Flags.Add("flag_two");

        Assert.Contains("flag_one", state.Flags);
        Assert.Contains("flag_two", state.Flags);
        Assert.Equal(2, state.Flags.Count);
    }

    [Fact]
    public void FlagNotSet_IsNotPresent()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.DoesNotContain("nonexistent_flag", state.Flags);
    }

    [Fact]
    public void FlagSet_ThenRemoved_IsNotPresent()
    {
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("temp_flag");

        state.Flags.Remove("temp_flag");

        Assert.DoesNotContain("temp_flag", state.Flags);
    }

    // ──────────────────────────────────────────────
    // Terminal state flags
    // ──────────────────────────────────────────────

    [Fact]
    public void SettingIsRunningFalse_StopsGame()
    {
        var state = WorldFactory.SingleRoomState();

        state.IsRunning = false;

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void SettingHasWon_MarksGameAsWon()
    {
        var state = WorldFactory.SingleRoomState();

        state.HasWon = true;

        Assert.True(state.HasWon);
    }

    // ──────────────────────────────────────────────
    // New catalog fields
    // ──────────────────────────────────────────────

    [Fact]
    public void ItemCatalog_DefaultsToEmptyDictionary()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.NotNull(state.ItemCatalog);
        Assert.Empty(state.ItemCatalog);
    }

    [Fact]
    public void NpcCatalog_DefaultsToEmptyDictionary()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.NotNull(state.NpcCatalog);
        Assert.Empty(state.NpcCatalog);
    }

    [Fact]
    public void WinRoomId_DefaultsToServer()
    {
        var state = WorldFactory.SingleRoomState();

        Assert.Equal("server", state.WinRoomId);
    }
}
