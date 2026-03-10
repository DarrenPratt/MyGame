using MyGame.Engine;
using MyGame.Models;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for the LoadedWorld record — verifies it holds all fields correctly
/// and that its forwarder properties delegate to the inner GameState.
/// </summary>
public class LoadedWorldTests
{
    private static GameState MinimalState(string roomId = "room1") => new()
    {
        CurrentRoomId = roomId,
        Rooms = new Dictionary<string, Room>
        {
            [roomId] = new Room { Id = roomId, Name = "Test Room", Description = "A room." }
        }
    };

    [Fact]
    public void LoadedWorld_HoldsAllConstructorValues()
    {
        var state = MinimalState();
        var world = new LoadedWorld(state, "win_room", "You win!", "Title", "Subtitle", "Intro", "You lose.");

        Assert.Same(state, world.State);
        Assert.Equal("win_room", world.WinRoomId);
        Assert.Equal("You win!", world.WinMessage);
        Assert.Equal("Title", world.Title);
        Assert.Equal("Subtitle", world.Subtitle);
        Assert.Equal("Intro", world.IntroText);
        Assert.Equal("You lose.", world.LoseMessage);
    }

    [Fact]
    public void LoadedWorld_CurrentRoomId_ForwardsFromState()
    {
        var state = MinimalState("alley");
        var world = new LoadedWorld(state, "server", "", "", "", "", "");

        Assert.Equal("alley", world.CurrentRoomId);
    }

    [Fact]
    public void LoadedWorld_Rooms_ForwardsFromState()
    {
        var state = MinimalState("room1");
        var world = new LoadedWorld(state, "room1", "", "", "", "", "");

        Assert.Same(state.Rooms, world.Rooms);
        Assert.True(world.Rooms.ContainsKey("room1"));
    }

    [Fact]
    public void LoadedWorld_ItemCatalog_ForwardsFromState()
    {
        var item = new Item { Id = "key", Name = "Key", Description = "A key." };
        var state = new GameState
        {
            CurrentRoomId = "room1",
            ItemCatalog = new Dictionary<string, Item> { ["key"] = item },
            Rooms = new Dictionary<string, Room>
            {
                ["room1"] = new Room { Id = "room1", Name = "Room", Description = "A room." }
            }
        };
        var world = new LoadedWorld(state, "room1", "", "", "", "", "");

        Assert.Same(state.ItemCatalog, world.ItemCatalog);
        Assert.True(world.ItemCatalog.ContainsKey("key"));
    }

    [Fact]
    public void LoadedWorld_NpcCatalog_ForwardsFromState()
    {
        var npc = new Npc { Id = "viktor", Name = "Viktor", Description = "A broker." };
        var state = new GameState
        {
            CurrentRoomId = "room1",
            NpcCatalog = new Dictionary<string, Npc> { ["viktor"] = npc },
            Rooms = new Dictionary<string, Room>
            {
                ["room1"] = new Room { Id = "room1", Name = "Room", Description = "A room." }
            }
        };
        var world = new LoadedWorld(state, "room1", "", "", "", "", "");

        Assert.Same(state.NpcCatalog, world.NpcCatalog);
        Assert.True(world.NpcCatalog.ContainsKey("viktor"));
    }

    [Fact]
    public void LoadedWorld_CurrentRoomId_ChangesWhenStateMutated()
    {
        // Forwarder is a live property — reflects current State value
        var state = MinimalState("room1");
        state.Rooms["room2"] = new Room { Id = "room2", Name = "Room 2", Description = "Second room." };
        var world = new LoadedWorld(state, "room2", "", "", "", "", "");

        state.CurrentRoomId = "room2";

        Assert.Equal("room2", world.CurrentRoomId);
    }

    [Fact]
    public void LoadedWorld_RecordEquality_SameValuesSameInstance()
    {
        var state = MinimalState();
        var world1 = new LoadedWorld(state, "win", "You win!", "Title", "Sub", "Intro", "Lose");
        var world2 = new LoadedWorld(state, "win", "You win!", "Title", "Sub", "Intro", "Lose");

        // Records use value equality — same State reference + same strings = equal
        Assert.Equal(world1, world2);
    }

    [Fact]
    public void LoadedWorld_WinRoomId_CanDifferFromStateWinRoomId()
    {
        // LoadedWorld.WinRoomId is an independent field; GameState.WinRoomId is set separately
        var state = MinimalState();
        state.WinRoomId = "server";
        var world = new LoadedWorld(state, "server_room", "", "", "", "", "");

        Assert.Equal("server_room", world.WinRoomId);
        Assert.Equal("server", state.WinRoomId);
    }
}
