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
    // NPC present but examine looks only at items
    // ──────────────────────────────────────────────

    [Fact]
    public void Execute_NpcInRoom_ButNoMatchingItem_ShowsNotFoundError()
    {
        // Arrange — NPC is in the room but ExamineCommand only searches items
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

        // Assert — not an item, so not-found error appears
        Assert.True(io.OutputContains("viktor"));
        Assert.False(io.OutputContains("A weathered data broker."));
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
}
