using MyGame.Commands;
using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for individual command implementations.
/// Each test builds only the minimal GameState it needs — no WorldBuilder.
/// </summary>
public class CommandTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Creates a registry with all standard commands registered.
    /// </summary>
    private static CommandRegistry BuildRegistry()
    {
        var registry = new CommandRegistry();
        registry.Register(new LookCommand());
        registry.Register(new GoCommand());
        registry.Register(new TakeCommand());
        registry.Register(new DropCommand());
        registry.Register(new InventoryCommand());
        registry.Register(new UseCommand());
        registry.Register(new HelpCommand(registry));
        registry.Register(new QuitCommand());
        return registry;
    }

    // ──────────────────────────────────────────────
    // LookCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Look_NoNoun_PrintsRoomDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState("Grimy walls covered in graffiti.");
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Grimy walls covered in graffiti."));
    }

    [Fact]
    public void Look_NoNoun_ListsExits()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState();
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", null), state, io);

        // Assert — "east" exit should appear somewhere in output
        Assert.True(io.OutputContains("east"));
    }

    [Fact]
    public void Look_NoNoun_ListsItemsInRoom()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.TakeableItem("flyer", "Crumpled Flyer"));
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Crumpled Flyer"));
    }

    [Fact]
    public void Look_WithItemNoun_PrintsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "terminal",
            Name = "Broken Terminal",
            Description = "A fried machine from decades past.",
            CanPickUp = false
        };
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", "terminal"), state, io);

        // Assert
        Assert.True(io.OutputContains("A fried machine from decades past."));
    }

    [Fact]
    public void Look_WithInventoryItem_PrintsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("keycard", "Corp Keycard");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", "keycard"), state, io);

        // Assert
        Assert.True(io.OutputContains("A test item."));
    }

    // ──────────────────────────────────────────────
    // GoCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Go_ValidDirection_MovesPlayer()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState(startId: "room_a");
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "east"), state, io);

        // Assert
        Assert.Equal("room_b", state.CurrentRoomId);
    }

    [Fact]
    public void Go_ValidDirection_AutoLooksAtNewRoom()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState(destDescription: "You arrived in Room B.");
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "east"), state, io);

        // Assert — auto-look should print destination description
        Assert.True(io.OutputContains("Room B"));
    }

    [Fact]
    public void Go_InvalidDirection_DoesNotMovePlayer()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState(startId: "room_a");
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "north"), state, io);

        // Assert — stays in original room
        Assert.Equal("room_a", state.CurrentRoomId);
    }

    [Fact]
    public void Go_InvalidDirection_PrintsErrorMessage()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState(startId: "room_a");
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "north"), state, io);

        // Assert — some error should be printed
        Assert.NotEmpty(io.Lines);
    }

    [Fact]
    public void Go_NoNoun_PrintsError()
    {
        // Arrange
        var state = WorldFactory.TwoRoomState();
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", null), state, io);

        // Assert — stays put, outputs something
        Assert.Equal("room_a", state.CurrentRoomId);
        Assert.NotEmpty(io.Lines);
    }

    [Theory]
    [InlineData("north", "n")]
    [InlineData("south", "s")]
    [InlineData("east", "e")]
    [InlineData("west", "w")]
    public void Go_DirectionAlias_BehavesLikeFullDirection(string direction, string alias)
    {
        // Arrange: build a state with exit named after the full direction word
        var roomA = new Room { Id = "a", Name = "A", Description = "Room A" };
        var roomB = new Room { Id = "b", Name = "B", Description = "Room B" };
        roomA.Exits[direction] = new Exit { Direction = direction, TargetRoomId = "b" };
        roomB.Exits[direction] = new Exit { Direction = direction, TargetRoomId = "a" }; // reverse not needed for test

        var state = new GameState
        {
            CurrentRoomId = "a",
            Rooms = new Dictionary<string, Room> { ["a"] = roomA, ["b"] = roomB }
        };
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act: use alias as verb (no noun), which GoCommand should support
        cmd.Execute(new ParsedCommand(alias, null), state, io);

        // Assert: moved to room b
        Assert.Equal("b", state.CurrentRoomId);
    }

    [Fact]
    public void Go_LockedExit_DoesNotMovePlayer()
    {
        // Arrange
        var roomA = new Room { Id = "a", Name = "A", Description = "Room A" };
        var roomB = new Room { Id = "b", Name = "B", Description = "Server Room" };
        roomA.Exits["north"] = new Exit
        {
            Direction = "north",
            TargetRoomId = "b",
            IsLocked = true,
            RequiredItemId = "keycard"
        };

        var state = new GameState
        {
            CurrentRoomId = "a",
            Rooms = new Dictionary<string, Room> { ["a"] = roomA, ["b"] = roomB }
        };
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "north"), state, io);

        // Assert
        Assert.Equal("a", state.CurrentRoomId);
    }

    [Fact]
    public void Go_LockedExit_PrintsHintAboutRequiredItem()
    {
        // Arrange
        var roomA = new Room { Id = "a", Name = "A", Description = "Room A" };
        var roomB = new Room { Id = "b", Name = "B", Description = "Server Room" };
        roomA.Exits["north"] = new Exit
        {
            Direction = "north",
            TargetRoomId = "b",
            IsLocked = true,
            RequiredItemId = "keycard"
        };

        var state = new GameState
        {
            CurrentRoomId = "a",
            Rooms = new Dictionary<string, Room> { ["a"] = roomA, ["b"] = roomB }
        };
        var io = new FakeInputOutput();
        var cmd = new GoCommand();

        // Act
        cmd.Execute(new ParsedCommand("go", "north"), state, io);

        // Assert — should mention the keycard or that it's locked
        Assert.True(io.OutputContains("keycard") || io.OutputContains("locked"));
    }

    // ──────────────────────────────────────────────
    // TakeCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Take_TakeableItem_AddsToInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.TakeableItem("flyer", "Crumpled Flyer"));
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "flyer"), state, io);

        // Assert
        Assert.Single(state.Inventory);
        Assert.Equal("flyer", state.Inventory[0].Id);
    }

    [Fact]
    public void Take_TakeableItem_RemovesFromRoom()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.TakeableItem("flyer", "Crumpled Flyer"));
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "flyer"), state, io);

        // Assert
        Assert.Empty(state.CurrentRoom.Items);
    }

    [Fact]
    public void Take_NonTakeableItem_FailsWithMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.SceneryItem("terminal", "Broken Terminal"));
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "terminal"), state, io);

        // Assert — item stays in room, inventory empty, message printed
        Assert.Empty(state.Inventory);
        Assert.Contains(state.CurrentRoom.Items, i => i.Id == "terminal");
        Assert.NotEmpty(io.Lines);
    }

    [Fact]
    public void Take_NonexistentItem_FailsWithMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand("take", "dragon"), state, io);

        // Assert
        Assert.Empty(state.Inventory);
        Assert.NotEmpty(io.Lines);
    }

    [Fact]
    public void Take_ByItemName_WorksCaseInsensitively()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.TakeableItem("chip", "Data Chip"));
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act — use item name instead of ID, with different case
        cmd.Execute(new ParsedCommand("take", "DATA CHIP"), state, io);

        // Assert
        Assert.Single(state.Inventory);
    }

    [Theory]
    [InlineData("get")]
    [InlineData("pick")]
    [InlineData("grab")]
    public void Take_Aliases_PickUpItem(string alias)
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(WorldFactory.TakeableItem("keycard", "Corp Keycard"));
        var io = new FakeInputOutput();
        var cmd = new TakeCommand();

        // Act
        cmd.Execute(new ParsedCommand(alias, "keycard"), state, io);

        // Assert
        Assert.Single(state.Inventory);
    }

    // ──────────────────────────────────────────────
    // DropCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Drop_CarriedItem_RemovesFromInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "flyer"), state, io);

        // Assert
        Assert.Empty(state.Inventory);
    }

    [Fact]
    public void Drop_CarriedItem_AppearsInCurrentRoom()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "flyer"), state, io);

        // Assert
        Assert.Contains(state.CurrentRoom.Items, i => i.Id == "flyer");
    }

    [Fact]
    public void Drop_ItemNotInInventory_PrintsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "plasma_rifle"), state, io);

        // Assert
        Assert.NotEmpty(io.Lines);
    }

    [Fact]
    public void Drop_NullNoun_PrintsDropWhat()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Drop what?"));
    }

    [Fact]
    public void Drop_ByItemName_RemovesFromInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "Crumpled Flyer"), state, io);

        // Assert
        Assert.Empty(state.Inventory);
    }

    [Fact]
    public void Drop_ByPartialName_RemovesFromInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "Crumpled"), state, io);

        // Assert
        Assert.Empty(state.Inventory);
    }

    [Fact]
    public void Drop_CaseInsensitive_RemovesFromInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "FLYER"), state, io);

        // Assert
        Assert.Empty(state.Inventory);
    }

    [Fact]
    public void Drop_SuccessMessage_ContainsItemName()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "flyer"), state, io);

        // Assert
        Assert.True(io.OutputContains("Crumpled Flyer"));
    }

    [Fact]
    public void Drop_ItemNotInInventory_ErrorContainsNoun()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "plasma_rifle"), state, io);

        // Assert
        Assert.True(io.OutputContains("plasma_rifle"));
    }

    [Fact]
    public void Drop_OneOfMultipleItems_OtherItemRemainsInInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item1 = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        var item2 = WorldFactory.TakeableItem("key", "Rusty Key");
        state.Inventory.Add(item1);
        state.Inventory.Add(item2);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "flyer"), state, io);

        // Assert
        Assert.Single(state.Inventory);
        Assert.Contains(state.Inventory, i => i.Id == "key");
    }

    [Fact]
    public void Drop_ByName_ItemNoLongerInInventory()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("flyer", "Crumpled Flyer");
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new DropCommand();

        // Act
        cmd.Execute(new ParsedCommand("drop", "Crumpled Flyer"), state, io);

        // Assert
        Assert.DoesNotContain(state.Inventory, i => i.Id == "flyer");
    }

    [Fact]
    public void Drop_VerbAndHelpText_AreCorrect()
    {
        // Arrange
        var cmd = new DropCommand();

        // Assert
        Assert.Equal("drop", cmd.Verb);
        Assert.NotEmpty(cmd.HelpText);
    }

    // ──────────────────────────────────────────────
    // InventoryCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Inventory_EmptyInventory_PrintsEmptyMessage()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new InventoryCommand();

        // Act
        cmd.Execute(new ParsedCommand("inventory", null), state, io);

        // Assert — some "carrying nothing" message
        Assert.NotEmpty(io.Lines);
    }

    [Fact]
    public void Inventory_WithItems_ListsAllItems()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.Inventory.Add(WorldFactory.TakeableItem("keycard", "Corp Keycard"));
        state.Inventory.Add(WorldFactory.TakeableItem("flyer", "Crumpled Flyer"));
        var io = new FakeInputOutput();
        var cmd = new InventoryCommand();

        // Act
        cmd.Execute(new ParsedCommand("inventory", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Corp Keycard"));
        Assert.True(io.OutputContains("Crumpled Flyer"));
    }

    [Theory]
    [InlineData("i")]
    [InlineData("inv")]
    public void Inventory_Aliases_ListInventory(string alias)
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.Inventory.Add(WorldFactory.TakeableItem("chip", "Data Chip"));
        var io = new FakeInputOutput();
        var cmd = new InventoryCommand();

        // Act
        cmd.Execute(new ParsedCommand(alias, null), state, io);

        // Assert
        Assert.True(io.OutputContains("Data Chip"));
    }

    // ──────────────────────────────────────────────
    // Examine — via LookCommand with noun
    // ──────────────────────────────────────────────

    [Fact]
    public void Examine_ItemInRoom_ShowsDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "drive",
            Name = "Data Drive",
            Description = "The prize. Packed with stolen SynthCorp research.",
            CanPickUp = true
        };
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act — "look drive" is examine
        cmd.Execute(new ParsedCommand("look", "drive"), state, io);

        // Assert
        Assert.True(io.OutputContains("The prize."));
    }

    [Fact]
    public void Examine_ItemInInventory_ShowsDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "keycard",
            Name = "Corp Keycard",
            Description = "Grants access to restricted areas.",
            CanPickUp = true
        };
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new LookCommand();

        // Act
        cmd.Execute(new ParsedCommand("look", "keycard"), state, io);

        // Assert
        Assert.True(io.OutputContains("Grants access"));
    }

    // ──────────────────────────────────────────────
    // HelpCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Help_PrintsAvailableCommands()
    {
        // Arrange
        var registry = BuildRegistry();
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new HelpCommand(registry);

        // Act
        cmd.Execute(new ParsedCommand("help", null), state, io);

        // Assert — basic verbs should appear
        Assert.True(io.OutputContains("look") || io.OutputContains("go") || io.OutputContains("help"));
    }

    [Fact]
    public void Help_PrintsHelpTextForCommands()
    {
        // Arrange
        var registry = BuildRegistry();
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new HelpCommand(registry);

        // Act
        cmd.Execute(new ParsedCommand("help", null), state, io);

        // Assert — output should be non-trivial
        Assert.True(io.Lines.Count > 2);
    }

    // ──────────────────────────────────────────────
    // QuitCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Quit_SetsIsRunningFalse()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new QuitCommand();

        // Act
        cmd.Execute(new ParsedCommand("quit", null), state, io);

        // Assert
        Assert.False(state.IsRunning);
    }

    [Theory]
    [InlineData("exit")]
    [InlineData("q")]
    public void Quit_Aliases_AlsoStopGame(string alias)
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var registry = BuildRegistry();

        // Act — go through registry to test alias dispatch
        registry.Execute(new ParsedCommand(alias, null), state, io);

        // Assert
        Assert.False(state.IsRunning);
    }

    // ──────────────────────────────────────────────
    // Unknown command
    // ──────────────────────────────────────────────

    [Fact]
    public void UnknownCommand_PrintsHelpfulError()
    {
        // Arrange
        var registry = BuildRegistry();
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();

        // Act
        registry.Execute(new ParsedCommand("xyzzy", null), state, io);

        // Assert — "help" hint should appear
        Assert.True(io.OutputContains("help") || io.OutputContains("unknown") || io.OutputContains("xyzzy"),
            $"Expected helpful error message but got: {io.AllOutput}");
    }

    [Theory]
    [InlineData("flibble")]
    [InlineData("zap")]
    [InlineData("dance")]
    public void UnknownCommand_Various_AllPrintError(string verb)
    {
        // Arrange
        var registry = BuildRegistry();
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();

        // Act
        registry.Execute(new ParsedCommand(verb, null), state, io);

        // Assert
        Assert.NotEmpty(io.Lines);
    }

    // ──────────────────────────────────────────────
    // UseCommand
    // ──────────────────────────────────────────────

    [Fact]
    public void Use_KeycardOnLockedExit_UnlocksExit()
    {
        // Arrange
        var roomA = new Room { Id = "lobby", Name = "Corp Lobby", Description = "Sterile lobby." };
        var roomB = new Room { Id = "server", Name = "Server Room", Description = "Hum of servers." };
        roomA.Exits["north"] = new Exit
        {
            Direction = "north",
            TargetRoomId = "server",
            IsLocked = true,
            RequiredItemId = "keycard"
        };

        var keycard = new Item
        {
            Id = "keycard",
            Name = "Corp Keycard",
            Description = "Grants access.",
            CanPickUp = true,
            UseTargetId = "north",      // matches the exit direction in current room
            UseMessage = "The door clicks open."
        };

        var state = new GameState
        {
            CurrentRoomId = "lobby",
            Rooms = new Dictionary<string, Room> { ["lobby"] = roomA, ["server"] = roomB }
        };
        state.Inventory.Add(keycard);

        var io = new FakeInputOutput();
        var cmd = new UseCommand();

        // Act
        cmd.Execute(new ParsedCommand("use", "keycard"), state, io);

        // Assert — exit is now unlocked
        Assert.False(roomA.Exits["north"].IsLocked);
    }

    [Fact]
    public void Use_ItemNotInInventory_PrintsError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new UseCommand();

        // Act
        cmd.Execute(new ParsedCommand("use", "laser"), state, io);

        // Assert
        Assert.NotEmpty(io.Lines);
    }
}
