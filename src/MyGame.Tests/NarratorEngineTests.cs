using MyGame.Engine;
using MyGame.Models;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for NarratorEngine — dynamic room descriptions based on flags and inventory.
/// </summary>
public class NarratorEngineTests
{
    [Fact]
    public void GetDescription_NoVariants_ReturnsBaseDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
        };
        var state = WorldFactory.SingleRoomState();

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Base description.", description);
    }

    [Fact]
    public void GetDescription_VariantRequiresFlag_FlagNotSet_ReturnsBaseDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    Description = "Variant description."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Base description.", description);
    }

    [Fact]
    public void GetDescription_VariantRequiresFlag_FlagSet_ReturnsVariantDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    Description = "Variant description."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Variant description.", description);
    }

    [Fact]
    public void GetDescription_VariantRequiresFlagAndInventory_OnlyFlagSet_ReturnsBaseDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    RequiredInventoryItems = new() { "keycard" },
                    Description = "Variant description."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Base description.", description);
    }

    [Fact]
    public void GetDescription_VariantRequiresFlagAndInventory_BothMet_ReturnsVariantDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    RequiredInventoryItems = new() { "keycard" },
                    Description = "Variant description."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");
        var keycard = new Item { Id = "keycard", Name = "Keycard", Description = "A keycard.", CanPickUp = true };
        state.Inventory.Add(keycard);

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Variant description.", description);
    }

    [Fact]
    public void GetDescription_MultipleVariants_MostSpecificWins()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    Description = "One condition met."
                },
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1", "flag2" },
                    Description = "Two conditions met."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");
        state.Flags.Add("flag2");

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Two conditions met.", description);
    }

    [Fact]
    public void GetDescription_VariantWithEmptyRequirements_AlwaysMatches()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new(),
                    RequiredInventoryItems = new(),
                    Description = "Default override."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Default override.", description);
    }

    [Fact]
    public void GetVariant_BarRoom_HasAtmosphericVariant_ReturnsItWhenNoFlagsSet()
    {
        // Arrange — simulate bar room with both its variants
        var room = new Room
        {
            Id = "bar",
            Name = "The Byte Bar",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new(),
                    RequiredInventoryItems = new(),
                    Description = "Neon flickers across the bar."
                },
                new NarratorVariant
                {
                    RequiredFlags = new() { "viktor_met" },
                    RequiredInventoryItems = new(),
                    Description = "Viktor works the bar tonight."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();

        // Act — no flags set, the unconditional atmospheric variant should win
        var variant = NarratorEngine.GetVariant(room, state);

        // Assert
        Assert.NotNull(variant);
        Assert.Equal("Neon flickers across the bar.", variant.Description);
    }

    [Fact]
    public void GetVariant_BarRoom_ViktorMetFlag_ReturnsViktorVariant()
    {
        // Arrange — simulate bar room with both its variants
        var room = new Room
        {
            Id = "bar",
            Name = "The Byte Bar",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new(),
                    RequiredInventoryItems = new(),
                    Description = "Neon flickers across the bar."
                },
                new NarratorVariant
                {
                    RequiredFlags = new() { "viktor_met" },
                    RequiredInventoryItems = new(),
                    Description = "Viktor works the bar tonight."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("viktor_met");

        // Act — viktor_met flag raises specificity score, that variant should win
        var variant = NarratorEngine.GetVariant(room, state);

        // Assert
        Assert.NotNull(variant);
        Assert.Equal("Viktor works the bar tonight.", variant.Description);
    }

    [Fact]
    public void GetDescription_VariantRequiresTwoFlags_OnlyOneSet_ReturnsBaseDescription()
    {
        // Arrange
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1", "flag2" },
                    Description = "Variant description."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");

        // Act
        var description = NarratorEngine.GetDescription(room, state);

        // Assert
        Assert.Equal("Base description.", description);
    }

    // ── GetVariant null-path tests ──────────────────────────────────────────

    [Fact]
    public void GetVariant_NoVariants_ReturnsNull()
    {
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
        };
        var state = WorldFactory.SingleRoomState();

        var variant = NarratorEngine.GetVariant(room, state);

        Assert.Null(variant);
    }

    [Fact]
    public void GetVariant_VariantPresent_RequiredFlagNotSet_ReturnsNull()
    {
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "secret_flag" },
                    Description = "Secret variant."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();

        // No flags set — variant requires "secret_flag" so nothing matches
        var variant = NarratorEngine.GetVariant(room, state);

        Assert.Null(variant);
    }

    // ── Inventory-only variant tests ────────────────────────────────────────

    [Fact]
    public void GetDescription_InventoryOnlyVariant_ItemPresent_ReturnsVariantDescription()
    {
        // Variant requires only an inventory item (no flags)
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredInventoryItems = new() { "datakey" },
                    Description = "You sense the datakey humming."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Inventory.Add(new Item { Id = "datakey", Name = "Data Key", Description = "A chip.", CanPickUp = true });

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("You sense the datakey humming.", description);
    }

    [Fact]
    public void GetDescription_InventoryOnlyVariant_ItemAbsent_ReturnsBaseDescription()
    {
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredInventoryItems = new() { "datakey" },
                    Description = "You sense the datakey humming."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        // datakey NOT in inventory — variant should not match

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("Base description.", description);
    }

    // ── Inventory matching is by item ID, not item Name ─────────────────────

    [Fact]
    public void GetDescription_InventoryVariant_MatchedByItemId_NotByItemName()
    {
        // The variant requires inventory item with Id "chip", NOT by Name
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredInventoryItems = new() { "chip" },
                    Description = "The chip pulses."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        // Item has Id="chip" but a completely different Name
        state.Inventory.Add(new Item { Id = "chip", Name = "Neural Interface Chip", Description = "Rare tech.", CanPickUp = true });

        var description = NarratorEngine.GetDescription(room, state);

        // Match is by Id — should find it despite the name mismatch
        Assert.Equal("The chip pulses.", description);
    }

    [Fact]
    public void GetDescription_InventoryVariant_ItemWithMatchingName_ButWrongId_ReturnsBase()
    {
        // Variant requires Id "chip"; item has name "chip" but different Id — should NOT match
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredInventoryItems = new() { "chip" },
                    Description = "The chip pulses."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Inventory.Add(new Item { Id = "neural_chip", Name = "chip", Description = "Has name chip but wrong Id.", CanPickUp = true });

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("Base description.", description);
    }

    // ── High-specificity fallback when best variant doesn't match ───────────

    [Fact]
    public void GetDescription_HigherSpecificityNotMatched_LowerSpecificityMatched_ReturnsLower()
    {
        // Two variants: high-specificity (2 flags) is NOT met; low-specificity (1 flag) IS met.
        // Engine should return the lower-specificity variant, NOT the base description.
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1" },
                    Description = "One condition met."
                },
                new NarratorVariant
                {
                    RequiredFlags = new() { "flag1", "flag2" },
                    Description = "Two conditions met."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flag1");
        // flag2 NOT set — high-specificity variant doesn't match

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("One condition met.", description);
    }

    // ── Multiple flags all required ─────────────────────────────────────────

    [Fact]
    public void GetDescription_VariantRequiresThreeFlags_AllPresent_ReturnsVariantDescription()
    {
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "alpha", "beta", "gamma" },
                    Description = "Triple gate unlocked."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("alpha");
        state.Flags.Add("beta");
        state.Flags.Add("gamma");

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("Triple gate unlocked.", description);
    }

    // ── Tie-breaking: equal specificity, first in list wins ─────────────────

    [Fact]
    public void GetDescription_TwoMatchingVariantsSameScore_FirstInListReturned()
    {
        // Both variants have score 1 (one condition each) and both match.
        // LINQ stable sort means the first variant in the list is returned.
        var room = new Room
        {
            Id = "test_room",
            Name = "Test Room",
            Description = "Base description.",
            NarratorVariants = new()
            {
                new NarratorVariant
                {
                    RequiredFlags = new() { "flagA" },
                    Description = "Variant A — listed first."
                },
                new NarratorVariant
                {
                    RequiredFlags = new() { "flagB" },
                    Description = "Variant B — listed second."
                }
            }
        };
        var state = WorldFactory.SingleRoomState();
        state.Flags.Add("flagA");
        state.Flags.Add("flagB");

        var description = NarratorEngine.GetDescription(room, state);

        Assert.Equal("Variant A — listed first.", description);
    }
}
