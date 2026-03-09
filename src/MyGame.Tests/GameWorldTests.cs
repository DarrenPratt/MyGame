using MyGame.Content;
using MyGame.Engine;
using MyGame.Models;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests that verify the game world built by WorldBuilder is internally consistent.
/// No game logic — just data integrity checks.
/// </summary>
public class GameWorldTests
{
    // Cache the world once per test class instance; WorldBuilder is deterministic.
    private readonly GameState _world = WorldBuilder.Build();

    // ──────────────────────────────────────────────
    // Expected world constants (from ARCHITECTURE.md V1 World Map)
    // ──────────────────────────────────────────────

    private static readonly string[] ExpectedRoomIds =
    [
        "alley",    // Neon Alley — start
        "bar",      // The Byte Bar
        "rooftop",  // Rooftop
        "lobby",    // Corp Lobby
        "server",   // Server Room — win
    ];

    // ──────────────────────────────────────────────
    // Room existence
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("alley")]
    [InlineData("bar")]
    [InlineData("rooftop")]
    [InlineData("lobby")]
    [InlineData("server")]
    public void Room_Exists(string roomId)
    {
        Assert.True(_world.Rooms.ContainsKey(roomId),
            $"Expected room '{roomId}' to exist in the world.");
    }

    [Fact]
    public void AllExpectedRoomsExist()
    {
        foreach (var id in ExpectedRoomIds)
            Assert.True(_world.Rooms.ContainsKey(id), $"Missing room: {id}");
    }

    [Fact]
    public void AllRooms_HaveNonEmptyName()
    {
        foreach (var (id, room) in _world.Rooms)
            Assert.False(string.IsNullOrWhiteSpace(room.Name),
                $"Room '{id}' has an empty name.");
    }

    [Fact]
    public void AllRooms_HaveNonEmptyDescription()
    {
        foreach (var (id, room) in _world.Rooms)
            Assert.False(string.IsNullOrWhiteSpace(room.Description),
                $"Room '{id}' has an empty description.");
    }

    [Fact]
    public void AllRooms_IdMatchesKey()
    {
        foreach (var (key, room) in _world.Rooms)
            Assert.Equal(key, room.Id);
    }

    // ──────────────────────────────────────────────
    // Exit sanity — no orphaned destinations
    // ──────────────────────────────────────────────

    [Fact]
    public void AllExits_PointToExistingRooms()
    {
        foreach (var (roomId, room) in _world.Rooms)
        {
            foreach (var (direction, exit) in room.Exits)
            {
                Assert.True(_world.Rooms.ContainsKey(exit.TargetRoomId),
                    $"Room '{roomId}' has exit '{direction}' → '{exit.TargetRoomId}' which does not exist.");
            }
        }
    }

    [Fact]
    public void AllExits_DirectionMatchesKey()
    {
        foreach (var (roomId, room) in _world.Rooms)
        {
            foreach (var (direction, exit) in room.Exits)
            {
                Assert.True(direction == exit.Direction,
                    $"Room '{roomId}' exit key '{direction}' does not match Exit.Direction '{exit.Direction}'.");
            }
        }
    }

    // ──────────────────────────────────────────────
    // Bidirectional exits
    // ──────────────────────────────────────────────

    private static readonly Dictionary<string, string> Opposites = new()
    {
        ["north"] = "south",
        ["south"] = "north",
        ["east"]  = "west",
        ["west"]  = "east",
        ["up"]    = "down",
        ["down"]  = "up",
    };

    [Fact]
    public void Exits_AreBidirectional_WherePossible()
    {
        // For every traversable (unlocked) exit A→direction→B,
        // B should have the opposite exit back to A.
        var violations = new List<string>();

        foreach (var (roomId, room) in _world.Rooms)
        {
            foreach (var (direction, exit) in room.Exits)
            {
                // Only check cardinal directions we know how to reverse
                if (!Opposites.TryGetValue(direction, out var opposite))
                    continue;

                var dest = _world.Rooms[exit.TargetRoomId];
                if (!dest.Exits.TryGetValue(opposite, out var reverseExit) ||
                    reverseExit.TargetRoomId != roomId)
                {
                    violations.Add($"{roomId} →{direction}→ {exit.TargetRoomId} has no reverse {opposite} exit.");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "One-way exits found (expected bidirectional):\n" + string.Join("\n", violations));
    }

    // ──────────────────────────────────────────────
    // Specific expected connections
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("alley", "east", "bar")]
    [InlineData("bar", "west", "alley")]
    [InlineData("bar", "east", "lobby")]
    [InlineData("lobby", "west", "bar")]
    [InlineData("bar", "up", "rooftop")]
    [InlineData("rooftop", "down", "bar")]
    [InlineData("lobby", "north", "server")]
    [InlineData("server", "south", "lobby")]
    public void Room_HasExpectedExit(string fromRoom, string direction, string toRoom)
    {
        Assert.True(_world.Rooms.ContainsKey(fromRoom), $"Room '{fromRoom}' not found.");
        var room = _world.Rooms[fromRoom];
        Assert.True(room.Exits.ContainsKey(direction),
            $"Room '{fromRoom}' has no '{direction}' exit. Available: {string.Join(", ", room.Exits.Keys)}");
        Assert.Equal(toRoom, room.Exits[direction].TargetRoomId);
    }

    // ──────────────────────────────────────────────
    // Starting room
    // ──────────────────────────────────────────────

    [Fact]
    public void StartingRoom_IsAlley()
    {
        Assert.Equal("alley", _world.CurrentRoomId);
    }

    [Fact]
    public void StartingRoom_ExistsInRooms()
    {
        Assert.True(_world.Rooms.ContainsKey(_world.CurrentRoomId),
            $"Starting room '{_world.CurrentRoomId}' not found in Rooms dictionary.");
    }

    // ──────────────────────────────────────────────
    // Item placement
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("rooftop", "keycard")]
    [InlineData("alley", "flyer")]
    [InlineData("bar", "terminal")]
    [InlineData("server", "drive")]
    public void Item_IsInCorrectStartingRoom(string roomId, string itemId)
    {
        Assert.True(_world.Rooms.ContainsKey(roomId), $"Room '{roomId}' not found.");
        var room = _world.Rooms[roomId];
        Assert.True(room.Items.Any(i => i.Id == itemId),
            $"Expected item '{itemId}' in room '{roomId}'. Found: [{string.Join(", ", room.Items.Select(i => i.Id))}]");
    }

    [Fact]
    public void Keycard_IsTakeable()
    {
        var rooftop = _world.Rooms["rooftop"];
        var keycard = rooftop.Items.FirstOrDefault(i => i.Id == "keycard");

        Assert.NotNull(keycard);
        Assert.True(keycard.CanPickUp);
    }

    [Fact]
    public void Terminal_IsNotTakeable()
    {
        var bar = _world.Rooms["bar"];
        var terminal = bar.Items.FirstOrDefault(i => i.Id == "terminal");

        Assert.NotNull(terminal);
        Assert.False(terminal.CanPickUp);
    }

    [Fact]
    public void Flyer_IsTakeable()
    {
        var alley = _world.Rooms["alley"];
        var flyer = alley.Items.FirstOrDefault(i => i.Id == "flyer");

        Assert.NotNull(flyer);
        Assert.True(flyer.CanPickUp);
    }

    [Fact]
    public void LobbyNorthExit_IsLockedByDefault()
    {
        var lobby = _world.Rooms["lobby"];
        Assert.True(lobby.Exits.ContainsKey("north"), "Lobby should have a north exit.");
        Assert.True(lobby.Exits["north"].IsLocked, "North exit from lobby should be locked by default.");
    }

    [Fact]
    public void LobbyNorthExit_RequiresKeycard()
    {
        var lobby = _world.Rooms["lobby"];
        Assert.Equal("keycard", lobby.Exits["north"].RequiredItemId);
    }

    [Fact]
    public void AllItems_HaveNonEmptyNames()
    {
        foreach (var (roomId, room) in _world.Rooms)
        {
            foreach (var item in room.Items)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Name),
                    $"Item '{item.Id}' in room '{roomId}' has an empty name.");
            }
        }
    }

    [Fact]
    public void AllItems_HaveNonEmptyDescriptions()
    {
        foreach (var (roomId, room) in _world.Rooms)
        {
            foreach (var item in room.Items)
            {
                Assert.False(string.IsNullOrWhiteSpace(item.Description),
                    $"Item '{item.Id}' in room '{roomId}' has an empty description.");
            }
        }
    }

    // ──────────────────────────────────────────────
    // Initial state
    // ──────────────────────────────────────────────

    [Fact]
    public void InitialInventory_IsEmpty()
    {
        Assert.Empty(_world.Inventory);
    }

    [Fact]
    public void InitialFlags_AreEmpty()
    {
        Assert.Empty(_world.Flags);
    }

    [Fact]
    public void InitialIsRunning_IsTrue()
    {
        Assert.True(_world.IsRunning);
    }

    [Fact]
    public void InitialHasWon_IsFalse()
    {
        Assert.False(_world.HasWon);
    }
}
