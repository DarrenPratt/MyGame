using MyGame.Engine;
using MyGame.Models;
using System;
using System.IO;
using System.Linq;
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
}
