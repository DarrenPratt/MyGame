using MyGame.Commands;
using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for ExamineCommand — close inspection of items in the room or inventory.
/// </summary>
public class ExamineCommandTests
{
    // ──────────────────────────────────────────────
    // Registration / metadata
    // ──────────────────────────────────────────────

    [Fact]
    public void Verb_IsExamine()
    {
        var cmd = new ExamineCommand();
        Assert.Equal("examine", cmd.Verb);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("inspect")]
    [InlineData("read")]
    public void Aliases_ContainExpectedShortcuts(string alias)
    {
        var cmd = new ExamineCommand();
        Assert.Contains(alias, cmd.Aliases);
    }

    // ──────────────────────────────────────────────
    // Missing noun
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_NoNoun_ShowsUsageError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", null), state, io);

        // Assert
        Assert.True(io.OutputContains("Examine what?"));
    }

    [Fact]
    public void Execute_NoNoun_DoesNotCrash_AndWritesOutput()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", null), state, io);

        // Assert — at least one line of output written
        Assert.NotEmpty(io.Lines);
    }

    // ──────────────────────────────────────────────
    // Item in room
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_ItemInRoom_ById_ShowsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "terminal",
            Name = "Cracked Terminal",
            Description = "A fried machine from decades past.",
            CanPickUp = false
        };
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "terminal"), state, io);

        // Assert
        Assert.True(io.OutputContains("A fried machine from decades past."));
    }

    [Fact]
    public void Execute_ItemInRoom_ByName_ShowsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "flyer",
            Name = "Crumpled Flyer",
            Description = "An old recruitment ad for SynthCorp.",
            CanPickUp = true
        };
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act — search by partial name
        cmd.Execute(new ParsedCommand("examine", "crumpled"), state, io);

        // Assert
        Assert.True(io.OutputContains("An old recruitment ad for SynthCorp."));
    }

    [Fact]
    public void Execute_ItemInRoom_ByName_IsCaseInsensitive()
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
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act — different casing for both id and name lookups
        cmd.Execute(new ParsedCommand("examine", "KEYCARD"), state, io);

        // Assert
        Assert.True(io.OutputContains("Grants access to restricted areas."));
    }

    // ──────────────────────────────────────────────
    // Item in inventory
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_ItemInInventory_ShowsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "drive",
            Name = "Data Drive",
            Description = "Packed with stolen SynthCorp research.",
            CanPickUp = true
        };
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "drive"), state, io);

        // Assert
        Assert.True(io.OutputContains("Packed with stolen SynthCorp research."));
    }

    [Fact]
    public void Execute_ItemInInventory_ByName_ShowsItemDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "keycard",
            Name = "Corp Keycard",
            Description = "Unlocks the lobby north exit.",
            CanPickUp = true
        };
        state.Inventory.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act — use partial name
        cmd.Execute(new ParsedCommand("examine", "keycard"), state, io);

        // Assert
        Assert.True(io.OutputContains("Unlocks the lobby north exit."));
    }

    // ──────────────────────────────────────────────
    // Item not found
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_ItemNotInRoomOrInventory_ShowsNotFoundError()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "phantom"), state, io);

        // Assert — error message contains the noun
        Assert.True(io.OutputContains("phantom"));
    }

    [Fact]
    public void Execute_ItemNotFound_DoesNotPrintAnyDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "real",
            Name = "Real Item",
            Description = "This item exists.",
            CanPickUp = true
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "phantom"), state, io);

        // Assert — item that exists should NOT have its description printed
        Assert.False(io.OutputContains("This item exists."));
    }

    // ──────────────────────────────────────────────
    // Room-vs-inventory priority
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_SameIdInRoomAndInventory_ReturnsRoomItemFirst()
    {
        // Arrange — room item has one description, inventory item has another
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "widget",
            Name = "Widget",
            Description = "The room copy.",
            CanPickUp = true
        });
        state.Inventory.Add(new Item
        {
            Id = "widget",
            Name = "Widget",
            Description = "The inventory copy.",
            CanPickUp = true
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "widget"), state, io);

        // Assert — room item wins (room searched first)
        Assert.True(io.OutputContains("The room copy."));
        Assert.False(io.OutputContains("The inventory copy."));
    }

    // ──────────────────────────────────────────────
    // NPC examine support
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_NpcInRoom_ById_ShowsNpcDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Npcs.Add(new Npc
        {
            Id = "viktor",
            Name = "Viktor",
            Description = "A weathered data broker.",
            Dialogue = new()
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "viktor"), state, io);

        // Assert — NPC found; description is shown
        Assert.True(io.OutputContains("A weathered data broker."));
    }

    [Fact]
    public void Execute_NpcInRoom_ByPartialName_ShowsNpcDescription()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Npcs.Add(new Npc
        {
            Id = "viktor",
            Name = "Viktor the Broker",
            Description = "A weathered data broker.",
            Dialogue = new()
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act — partial name match
        cmd.Execute(new ParsedCommand("examine", "broker"), state, io);

        // Assert
        Assert.True(io.OutputContains("A weathered data broker."));
    }

    [Fact]
    public void Execute_NpcInRoom_ByName_IsCaseInsensitive()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Npcs.Add(new Npc
        {
            Id = "viktor",
            Name = "Viktor",
            Description = "A weathered data broker.",
            Dialogue = new()
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "VIKTOR"), state, io);

        // Assert
        Assert.True(io.OutputContains("A weathered data broker."));
    }

    [Fact]
    public void Execute_NpcNotInRoom_ShowsNotFoundError()
    {
        // Arrange — no matching item or NPC
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Npcs.Add(new Npc
        {
            Id = "viktor",
            Name = "Viktor",
            Description = "A weathered data broker.",
            Dialogue = new()
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act — search for someone who is not here
        cmd.Execute(new ParsedCommand("examine", "ghost"), state, io);

        // Assert — not found
        Assert.True(io.OutputContains("ghost"));
        Assert.False(io.OutputContains("A weathered data broker."));
    }

    [Fact]
    public void Execute_ItemNameTakesPriorityOverNpcName()
    {
        // Arrange — item and NPC share overlapping names; item is checked first
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "chip",
            Name = "Data Chip",
            Description = "Item description.",
            CanPickUp = true
        });
        state.CurrentRoom.Npcs.Add(new Npc
        {
            Id = "chip_npc",
            Name = "Chip",
            Description = "NPC description.",
            Dialogue = new()
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "chip"), state, io);

        // Assert — item wins over NPC
        Assert.True(io.OutputContains("Item description."));
        Assert.False(io.OutputContains("NPC description."));
    }

    // ──────────────────────────────────────────────
    // State is not mutated by examine
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_DoesNotMutateInventoryOrRoom()
    {
        // Arrange
        var state = WorldFactory.SingleRoomState();
        var item = WorldFactory.TakeableItem("widget", "Widget");
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        var roomItemsBefore = state.CurrentRoom.Items.Count;
        var inventoryBefore = state.Inventory.Count;

        // Act
        cmd.Execute(new ParsedCommand("examine", "widget"), state, io);

        // Assert — examine is read-only
        Assert.Equal(roomItemsBefore, state.CurrentRoom.Items.Count);
        Assert.Equal(inventoryBefore, state.Inventory.Count);
    }

    // ──────────────────────────────────────────────
    // Empty-string noun edge case
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_EmptyStringNoun_MatchesFirstRoomItemByNameContains()
    {
        // Arrange — empty string passes the null guard but "" is contained by every non-empty name,
        // so FindItem("") returns the first item in the room (Name.Contains("") is always true).
        var state = WorldFactory.SingleRoomState();
        var item = new Item
        {
            Id = "chip",
            Name = "Cred Chip",
            Description = "Worth more than it looks.",
            CanPickUp = true
        };
        state.CurrentRoom.Items.Add(item);
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", ""), state, io);

        // Assert — empty string matches via Name.Contains(""), returns first item's description
        Assert.True(io.OutputContains("Worth more than it looks."));
    }

    [Fact]
    public void Execute_EmptyStringNoun_EmptyRoom_ShowsNotFoundError()
    {
        // Arrange — empty string noun with no items in room or inventory
        var state = WorldFactory.SingleRoomState();
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", ""), state, io);

        // Assert — nothing to find; error output written
        Assert.NotEmpty(io.Lines);
    }

    // ──────────────────────────────────────────────
    // Alias verbs route to the same behavior
    // ──────────────────────────────────────────────

    [Theory]
    [InlineData("x")]
    [InlineData("inspect")]
    [InlineData("read")]
    public void Execute_WithAliasVerb_StillFindsItem(string aliasVerb)
    {
        // Arrange — aliases all delegate to the same Execute; verb field is irrelevant at runtime
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "note",
            Name = "Scrawled Note",
            Description = "Coordinates and a warning.",
            CanPickUp = true
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand(aliasVerb, "note"), state, io);

        // Assert
        Assert.True(io.OutputContains("Coordinates and a warning."));
    }

    // ──────────────────────────────────────────────
    // Multiple matching items — first match returned
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_PartialNameMatchesMultipleItems_ReturnsFirstAddedItem()
    {
        // Arrange — both items match the partial noun "chip"; room order wins
        var state = WorldFactory.SingleRoomState();
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "blue_chip",
            Name = "Blue Chip",
            Description = "First chip description.",
            CanPickUp = true
        });
        state.CurrentRoom.Items.Add(new Item
        {
            Id = "red_chip",
            Name = "Red Chip",
            Description = "Second chip description.",
            CanPickUp = true
        });
        var io = new FakeInputOutput();
        var cmd = new ExamineCommand();

        // Act
        cmd.Execute(new ParsedCommand("examine", "chip"), state, io);

        // Assert — first item added to room is returned
        Assert.True(io.OutputContains("First chip description."));
        Assert.False(io.OutputContains("Second chip description."));
    }
}
