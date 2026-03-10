using MyGame.Engine;
using MyGame.Models;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for JsonWorldLoader — loads game world from JSON files.
/// </summary>
public class JsonWorldLoaderTests : IDisposable
{
    private readonly string _testDirectory;

    public JsonWorldLoaderTests()
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

    private string CreateTestJsonFile(string json, string filename = "world.json")
    {
        var path = Path.Combine(_testDirectory, filename);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void Load_MinimalValidJson_NoExceptions()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""A test room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        Assert.NotNull(state);
    }

    [Fact]
    public void Load_ReturnsCorrectStartRoomId()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""alley"",
            ""winRoomId"": ""server"",
            ""rooms"": [
                {
                    ""id"": ""alley"",
                    ""name"": ""Dark Alley"",
                    ""description"": ""A dark alley."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        Assert.Equal("alley", state.CurrentRoomId);
    }

    [Fact]
    public void Load_RoomsDictionary_ContainsAllRoomsFromJson()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room2"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room One"",
                    ""description"": ""First room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                },
                {
                    ""id"": ""room2"",
                    ""name"": ""Room Two"",
                    ""description"": ""Second room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        Assert.Equal(2, state.Rooms.Count);
        Assert.True(state.Rooms.ContainsKey("room1"));
        Assert.True(state.Rooms.ContainsKey("room2"));
    }

    [Fact]
    public void Load_RoomExits_AreCorrectlyWired()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room2"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room One"",
                    ""description"": ""First room."",
                    ""exits"": [
                        {
                            ""direction"": ""north"",
                            ""targetRoomId"": ""room2"",
                            ""isLocked"": false
                        }
                    ],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                },
                {
                    ""id"": ""room2"",
                    ""name"": ""Room Two"",
                    ""description"": ""Second room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        var room1 = state.Rooms["room1"];
        Assert.True(room1.Exits.ContainsKey("north"));
        Assert.Equal("room2", room1.Exits["north"].TargetRoomId);
    }

    [Fact]
    public void Load_Items_ArePlacedInCorrectRooms()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""A test room."",
                    ""exits"": [],
                    ""items"": [""keycard""],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [
                {
                    ""id"": ""keycard"",
                    ""name"": ""Keycard"",
                    ""description"": ""A security keycard."",
                    ""canPickUp"": true
                }
            ],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        var room = state.Rooms["room1"];
        Assert.Single(room.Items);
        Assert.Equal("keycard", room.Items[0].Id);
    }

    [Fact]
    public void Load_Npcs_ArePlacedInCorrectRooms()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""A test room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [""viktor""],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": [
                {
                    ""id"": ""viktor"",
                    ""name"": ""Viktor"",
                    ""description"": ""A data broker."",
                    ""dialogue"": []
                }
            ]
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        var room = state.Rooms["room1"];
        Assert.Single(room.Npcs);
        Assert.Equal("viktor", room.Npcs[0].Id);
    }

    [Fact]
    public void Load_NarratorVariants_AreLoadedIntoRooms()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""Base description."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": [
                        {
                            ""requiredFlags"": [""flag1""],
                            ""requiredInventoryItems"": [],
                            ""description"": ""Variant description.""
                        }
                    ]
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        var room = state.Rooms["room1"];
        Assert.Single(room.NarratorVariants);
        Assert.Contains("flag1", room.NarratorVariants[0].RequiredFlags);
    }

    [Fact]
    public void Load_ItemCatalog_ContainsAllItemsFromJson()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""A test room."",
                    ""exits"": [],
                    ""items"": [""keycard"", ""flyer""],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [
                {
                    ""id"": ""keycard"",
                    ""name"": ""Keycard"",
                    ""description"": ""A security keycard."",
                    ""canPickUp"": true
                },
                {
                    ""id"": ""flyer"",
                    ""name"": ""Flyer"",
                    ""description"": ""A crumpled flyer."",
                    ""canPickUp"": true
                }
            ],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        Assert.Equal(2, state.ItemCatalog.Count);
        Assert.True(state.ItemCatalog.ContainsKey("keycard"));
        Assert.True(state.ItemCatalog.ContainsKey("flyer"));
    }

    [Fact]
    public void Load_WinRoomId_IsSetFromJson()
    {
        // Arrange
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""server"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Test Room"",
                    ""description"": ""A test room."",
                    ""exits"": [],
                    ""items"": [],
                    ""npcs"": [],
                    ""narratorVariants"": []
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);
        var loader = new JsonWorldLoader();

        // Act
        var state = loader.Load(path);

        // Assert
        Assert.Equal("server", state.WinRoomId);
    }

    // ── Metadata forwarding ───────────────────────────────────────────────

    [Fact]
    public void Load_WinMessage_IsForwardedFromJson()
    {
        var json = MinimalJsonWith(@"""winMessage"": ""You cracked the vault. The data is yours."",");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("You cracked the vault. The data is yours.", world.WinMessage);
    }

    [Fact]
    public void Load_Title_IsForwardedFromJson()
    {
        var json = MinimalJsonWith(@"""title"": ""Neon Ledger"",");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("Neon Ledger", world.Title);
    }

    [Fact]
    public void Load_Subtitle_IsForwardedFromJson()
    {
        var json = MinimalJsonWith(@"""subtitle"": ""A cyberpunk heist"",");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("A cyberpunk heist", world.Subtitle);
    }

    [Fact]
    public void Load_IntroText_IsForwardedFromJson()
    {
        var json = MinimalJsonWith(@"""introText"": ""Rain hammers the alley outside."",");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("Rain hammers the alley outside.", world.IntroText);
    }

    [Fact]
    public void Load_LoseMessage_IsForwardedFromJson()
    {
        var json = MinimalJsonWith(@"""loseMessage"": ""The drones have you. Game over."",");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("The drones have you. Game over.", world.LoseMessage);
    }

    [Fact]
    public void Load_AllMetadataFields_ForwardedCorrectly()
    {
        var json = @"{
            ""startRoomId"": ""alley"",
            ""winRoomId"": ""vault"",
            ""winMessage"": ""Data secured."",
            ""loseMessage"": ""Caught."",
            ""title"": ""Neon Ledger"",
            ""subtitle"": ""A cyberpunk heist"",
            ""introText"": ""Rain falls."",
            ""rooms"": [
                { ""id"": ""alley"", ""name"": ""Alley"", ""description"": ""Dark."" }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("alley", world.CurrentRoomId);
        Assert.Equal("vault", world.WinRoomId);
        Assert.Equal("Data secured.", world.WinMessage);
        Assert.Equal("Caught.", world.LoseMessage);
        Assert.Equal("Neon Ledger", world.Title);
        Assert.Equal("A cyberpunk heist", world.Subtitle);
        Assert.Equal("Rain falls.", world.IntroText);
    }

    [Fact]
    public void Load_StartRoomId_ForwardedAsCurrentRoomId()
    {
        var json = @"{
            ""startRoomId"": ""plaza"",
            ""winRoomId"": ""vault"",
            ""rooms"": [
                { ""id"": ""plaza"", ""name"": ""Plaza"", ""description"": ""Busy."" }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Equal("plaza", world.CurrentRoomId);
        Assert.Equal("plaza", world.State.CurrentRoomId);
    }

    // ── Edge cases ────────────────────────────────────────────────────────

    [Fact]
    public void Load_NarratorVariants_MissingKey_DefaultsToEmptyList()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                { ""id"": ""room1"", ""name"": ""Room"", ""description"": ""A room."" }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Empty(world.Rooms["room1"].NarratorVariants);
    }

    [Fact]
    public void Load_ItemReferencedInRoom_NotInItemsList_IsSkipped()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room"",
                    ""description"": ""A room."",
                    ""items"": [""ghost_item""]
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Empty(world.Rooms["room1"].Items);
    }

    [Fact]
    public void Load_NpcReferencedInRoom_NotInNpcsList_IsSkipped()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room"",
                    ""description"": ""A room."",
                    ""npcs"": [""phantom_npc""]
                }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Empty(world.Rooms["room1"].Npcs);
    }

    [Fact]
    public void Load_ItemIds_AlternativeKey_PlacesItemsInRoom()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room"",
                    ""description"": ""A room."",
                    ""itemIds"": [""keycard""]
                }
            ],
            ""items"": [
                { ""id"": ""keycard"", ""name"": ""Keycard"", ""description"": ""A card."", ""canPickUp"": true }
            ],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Single(world.Rooms["room1"].Items);
        Assert.Equal("keycard", world.Rooms["room1"].Items[0].Id);
    }

    [Fact]
    public void Load_NpcIds_AlternativeKey_PlacesNpcsInRoom()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room"",
                    ""description"": ""A room."",
                    ""npcIds"": [""viktor""]
                }
            ],
            ""items"": [],
            ""npcs"": [
                { ""id"": ""viktor"", ""name"": ""Viktor"", ""description"": ""Broker."", ""dialogue"": [] }
            ]
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Single(world.Rooms["room1"].Npcs);
        Assert.Equal("viktor", world.Rooms["room1"].Npcs[0].Id);
    }

    [Fact]
    public void Load_DuplicateItemIds_DeduplicatedInRoom()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""rooms"": [
                {
                    ""id"": ""room1"",
                    ""name"": ""Room"",
                    ""description"": ""A room."",
                    ""items"": [""keycard""],
                    ""itemIds"": [""keycard""]
                }
            ],
            ""items"": [
                { ""id"": ""keycard"", ""name"": ""Keycard"", ""description"": ""A card."", ""canPickUp"": true }
            ],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Single(world.Rooms["room1"].Items);
    }

    [Fact]
    public void Load_HighRiskRoomIds_LoadedFromJson()
    {
        var json = @"{
            ""startRoomId"": ""room1"",
            ""winRoomId"": ""room1"",
            ""highRiskRoomIds"": [""danger_zone"", ""hot_sector""],
            ""rooms"": [
                { ""id"": ""room1"", ""name"": ""Room"", ""description"": ""A room."" }
            ],
            ""items"": [],
            ""npcs"": []
        }";
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Contains("danger_zone", world.State.HighRiskRoomIds);
        Assert.Contains("hot_sector", world.State.HighRiskRoomIds);
        Assert.DoesNotContain("plaza", world.State.HighRiskRoomIds);
    }

    [Fact]
    public void Load_HighRiskRoomIds_DefaultsToPlazaAndCheckpoint_WhenMissing()
    {
        var json = MinimalJsonWith("");
        var path = CreateTestJsonFile(json);

        var world = new JsonWorldLoader().Load(path);

        Assert.Contains("plaza", world.State.HighRiskRoomIds, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("checkpoint", world.State.HighRiskRoomIds, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void Load_MissingFile_ThrowsFileNotFoundException()
    {
        var loader = new JsonWorldLoader();
        var missingPath = Path.Combine(_testDirectory, "does_not_exist.json");

        Assert.Throws<FileNotFoundException>(() => loader.Load(missingPath));
    }

    [Fact]
    public void Load_MalformedJson_ThrowsJsonException()
    {
        var path = CreateTestJsonFile("{ this is not valid json }", "bad.json");
        var loader = new JsonWorldLoader();

        Assert.Throws<JsonException>(() => loader.Load(path));
    }

    [Fact]
    public void Load_NullWorldDefinition_ThrowsInvalidOperationException()
    {
        var path = CreateTestJsonFile("null", "null.json");
        var loader = new JsonWorldLoader();

        Assert.Throws<InvalidOperationException>(() => loader.Load(path));
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static string MinimalJsonWith(string extra) => $$"""
        {
            {{extra}}
            "startRoomId": "room1",
            "winRoomId": "room1",
            "rooms": [
                { "id": "room1", "name": "Room", "description": "A room." }
            ],
            "items": [],
            "npcs": []
        }
        """;
}
