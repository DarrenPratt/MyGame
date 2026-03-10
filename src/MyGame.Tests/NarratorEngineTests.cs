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
}
