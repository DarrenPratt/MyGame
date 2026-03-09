using MyGame.Engine;
using MyGame.Models;

namespace MyGame.Tests.Helpers;

/// <summary>
/// Factory helpers for building minimal GameState objects in tests.
/// Tests should use these rather than touching WorldBuilder directly when
/// they only need a controlled subset of the world.
/// </summary>
public static class WorldFactory
{
    /// <summary>
    /// Two connected rooms: "room_a" (east→room_b) and "room_b" (west→room_a).
    /// Player starts in room_a.
    /// </summary>
    public static GameState TwoRoomState(
        string startId = "room_a",
        string? startDescription = null,
        string? destDescription = null)
    {
        var roomA = new Room
        {
            Id = "room_a",
            Name = "Room A",
            Description = startDescription ?? "A plain room.",
        };
        var roomB = new Room
        {
            Id = "room_b",
            Name = "Room B",
            Description = destDescription ?? "Another room.",
        };

        roomA.Exits["east"] = new Exit { Direction = "east", TargetRoomId = "room_b" };
        roomB.Exits["west"] = new Exit { Direction = "west", TargetRoomId = "room_a" };

        return new GameState
        {
            CurrentRoomId = startId,
            Rooms = new Dictionary<string, Room>
            {
                ["room_a"] = roomA,
                ["room_b"] = roomB,
            }
        };
    }

    /// <summary>
    /// Single room state — useful for commands that don't involve navigation.
    /// </summary>
    public static GameState SingleRoomState(string description = "A test room.")
    {
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = description,
        };

        return new GameState
        {
            CurrentRoomId = "test_room",
            Rooms = new Dictionary<string, Room> { ["test_room"] = room }
        };
    }

    /// <summary>Creates a takeable item for use in tests.</summary>
    public static Item TakeableItem(string id = "widget", string name = "Widget") =>
        new Item { Id = id, Name = name, Description = "A test item.", CanPickUp = true };

    /// <summary>Creates a non-takeable scenery item for use in tests.</summary>
    public static Item SceneryItem(string id = "mural", string name = "Mural") =>
        new Item { Id = id, Name = name, Description = "Painted on the wall.", CanPickUp = false };
}
