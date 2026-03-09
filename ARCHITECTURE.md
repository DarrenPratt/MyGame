# MyGame — Architecture Document

> **Cyberpunk Text Adventure** · C# / .NET 8 · Console Application
> Architect: Johnny · Created: 2025-07-14

---

## 1. Project Structure

```
MyGame/
├── ARCHITECTURE.md
├── src/
│   └── MyGame/
│       ├── MyGame.csproj              # Console app, net8.0
│       ├── Program.cs                 # Entry point
│       ├── Engine/
│       │   ├── GameState.cs           # All mutable game state
│       │   ├── GameEngine.cs          # Main game loop & orchestration
│       │   └── CommandParser.cs       # Raw input → parsed command
│       ├── Commands/
│       │   ├── ICommand.cs            # Command interface
│       │   ├── CommandRegistry.cs     # Maps verbs → ICommand
│       │   ├── LookCommand.cs
│       │   ├── GoCommand.cs
│       │   ├── TakeCommand.cs
│       │   ├── DropCommand.cs
│       │   ├── InventoryCommand.cs
│       │   ├── UseCommand.cs
│       │   ├── HelpCommand.cs
│       │   └── QuitCommand.cs
│       ├── Models/
│       │   ├── Room.cs                # Room definition
│       │   ├── Item.cs                # Item definition
│       │   └── Exit.cs                # Directional link between rooms
│       └── Content/
│           └── WorldBuilder.cs        # Builds all rooms, items, exits
└── tests/
    └── MyGame.Tests/
        ├── MyGame.Tests.csproj        # xUnit test project
        ├── Engine/
        │   ├── GameStateTests.cs
        │   ├── GameEngineTests.cs
        │   └── CommandParserTests.cs
        ├── Commands/
        │   ├── LookCommandTests.cs
        │   ├── GoCommandTests.cs
        │   ├── TakeCommandTests.cs
        │   ├── DropCommandTests.cs
        │   ├── InventoryCommandTests.cs
        │   └── UseCommandTests.cs
        └── Content/
            └── WorldBuilderTests.cs
```

### Project files

**src/MyGame/MyGame.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**tests/MyGame.Tests/MyGame.Tests.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\MyGame\MyGame.csproj" />
  </ItemGroup>
</Project>
```

---

## 2. Models

### Room

```csharp
namespace MyGame.Models;

public class Room
{
    public required string Id { get; init; }           // e.g. "alley", "bar", "rooftop"
    public required string Name { get; init; }         // e.g. "Neon Alley"
    public required string Description { get; init; }  // Full room description
    public Dictionary<string, Exit> Exits { get; } = new();  // "north" → Exit
    public List<Item> Items { get; } = new();          // Items on the ground
}
```

### Exit

```csharp
namespace MyGame.Models;

public class Exit
{
    public required string Direction { get; init; }     // "north", "south", "east", "west", "up", "down"
    public required string TargetRoomId { get; init; }  // Id of destination room
    public string? Description { get; init; }           // Optional: "A rusty fire escape leads up."
    public bool IsLocked { get; set; }                  // Can be unlocked by game logic
    public string? RequiredItemId { get; set; }         // Item needed to unlock (null = not locked)
}
```

### Item

```csharp
namespace MyGame.Models;

public class Item
{
    public required string Id { get; init; }            // e.g. "keycard", "pistol"
    public required string Name { get; init; }          // e.g. "Corp Keycard"
    public required string Description { get; init; }   // Examine text
    public bool CanPickUp { get; init; } = true;        // Some items are scenery
    public string? UseTargetId { get; init; }           // What this item interacts with (exit id, item id, etc.)
    public string? UseMessage { get; init; }            // Message shown when item is used successfully
}
```

---

## 3. Engine

### GameState

Holds **all** mutable state. Pure data — no logic. This makes it easy to test and serialize later.

```csharp
namespace MyGame.Engine;

using MyGame.Models;

public class GameState
{
    public required string CurrentRoomId { get; set; }
    public List<Item> Inventory { get; } = new();
    public HashSet<string> Flags { get; } = new();      // e.g. "door_unlocked", "talked_to_fixer"
    public bool IsRunning { get; set; } = true;
    public bool HasWon { get; set; }

    public required Dictionary<string, Room> Rooms { get; init; }

    public Room CurrentRoom => Rooms[CurrentRoomId];
}
```

**Flags** are simple string tags. Commands set them when events happen (e.g., unlocking a door). This keeps the state model flat and extensible without an event system.

### CommandParser

Splits raw input into a verb and an optional noun. No intelligence — just string splitting.

```csharp
namespace MyGame.Engine;

public record ParsedCommand(string Verb, string? Noun);

public static class CommandParser
{
    public static ParsedCommand Parse(string input)
    {
        // Trim, lowercase, split on first space
        // "go north" → ("go", "north")
        // "look"     → ("look", null)
        // "take keycard" → ("take", "keycard")
        
        var trimmed = input.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(trimmed))
            return new ParsedCommand("", null);

        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex < 0)
            return new ParsedCommand(trimmed, null);

        var verb = trimmed[..spaceIndex];
        var noun = trimmed[(spaceIndex + 1)..].Trim();
        return new ParsedCommand(verb, string.IsNullOrEmpty(noun) ? null : noun);
    }
}
```

### GameEngine

Orchestrates the game loop. Depends on `IInputOutput` for testability — no direct `Console` calls.

```csharp
namespace MyGame.Engine;

using MyGame.Commands;

public interface IInputOutput
{
    string? ReadLine();
    void WriteLine(string text);
    void Write(string text);
}

public class ConsoleIO : IInputOutput
{
    public string? ReadLine() => Console.ReadLine();
    public void WriteLine(string text) => Console.WriteLine(text);
    public void Write(string text) => Console.Write(text);
}

public class GameEngine
{
    private readonly GameState _state;
    private readonly CommandRegistry _commands;
    private readonly IInputOutput _io;

    public GameEngine(GameState state, CommandRegistry commands, IInputOutput io)
    {
        _state = state;
        _commands = commands;
        _io = io;
    }

    public void Run()
    {
        _io.WriteLine("=== NEON SHADOWS ===");
        _io.WriteLine("A cyberpunk text adventure.\n");

        // Show initial room
        _commands.Execute(new ParsedCommand("look", null), _state, _io);

        while (_state.IsRunning)
        {
            _io.Write("\n> ");
            var input = _io.ReadLine();
            if (input is null)
                break;

            var parsed = CommandParser.Parse(input);
            if (string.IsNullOrEmpty(parsed.Verb))
                continue;

            _commands.Execute(parsed, _state, _io);
        }

        if (_state.HasWon)
            _io.WriteLine("\n*** YOU WIN. The neon city is yours. ***\n");
        else
            _io.WriteLine("\n*** JACKED OUT. See you in the sprawl. ***\n");
    }
}
```

---

## 4. Command System

### ICommand

```csharp
namespace MyGame.Commands;

using MyGame.Engine;

public interface ICommand
{
    string Verb { get; }
    string[] Aliases => [];             // Optional aliases (e.g., "l" for "look")
    string HelpText { get; }            // One-line description for help screen
    void Execute(ParsedCommand command, GameState state, IInputOutput io);
}
```

### CommandRegistry

Maps verb strings to `ICommand` implementations. Supports aliases.

```csharp
namespace MyGame.Commands;

using MyGame.Engine;

public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void Register(ICommand command)
    {
        _commands[command.Verb] = command;
        foreach (var alias in command.Aliases)
            _commands[alias] = command;
    }

    public void Execute(ParsedCommand parsed, GameState state, IInputOutput io)
    {
        if (_commands.TryGetValue(parsed.Verb, out var command))
            command.Execute(parsed, state, io);
        else
            io.WriteLine($"Unknown command: \"{parsed.Verb}\". Type \"help\" for commands.");
    }

    public IEnumerable<ICommand> AllCommands => _commands.Values.Distinct();
}
```

### Command Implementations

Each command is a small, focused class. Here are the signatures and key behaviors:

#### LookCommand

```csharp
public class LookCommand : ICommand
{
    public string Verb => "look";
    public string[] Aliases => ["l"];
    public string HelpText => "Look around the current room.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // If noun is null: describe room, list exits, list items on ground
        // If noun matches an item (in room or inventory): show item description
    }
}
```

#### GoCommand

```csharp
public class GoCommand : ICommand
{
    public string Verb => "go";
    public string[] Aliases => ["north", "south", "east", "west", "up", "down", "n", "s", "e", "w", "u", "d"];
    public string HelpText => "Move in a direction. Usage: go <direction>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // Determine direction from noun (if verb is "go") or from verb itself (if "north", etc.)
        // Check if exit exists and is not locked
        // If locked, tell player what they need
        // Otherwise move player and auto-look
    }
}
```

#### TakeCommand

```csharp
public class TakeCommand : ICommand
{
    public string Verb => "take";
    public string[] Aliases => ["get", "pick", "grab"];
    public string HelpText => "Pick up an item. Usage: take <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // Find item by id or name (case-insensitive) in current room
        // Check CanPickUp
        // Remove from room, add to inventory
    }
}
```

#### DropCommand

```csharp
public class DropCommand : ICommand
{
    public string Verb => "drop";
    public string HelpText => "Drop an item from your inventory. Usage: drop <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // Find item in inventory, move to current room
    }
}
```

#### InventoryCommand

```csharp
public class InventoryCommand : ICommand
{
    public string Verb => "inventory";
    public string[] Aliases => ["inv", "i"];
    public string HelpText => "Show what you're carrying.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // List items in inventory or "You're carrying nothing."
    }
}
```

#### UseCommand

```csharp
public class UseCommand : ICommand
{
    public string Verb => "use";
    public string HelpText => "Use an item. Usage: use <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // Find item in inventory
        // Check UseTargetId — does it match an exit or flag in current context?
        // If exit: unlock it, set flag, show UseMessage
        // If flag-based: set the flag, show UseMessage
        // Enables win condition checks
    }
}
```

#### HelpCommand

```csharp
public class HelpCommand : ICommand
{
    public string Verb => "help";
    public string[] Aliases => ["?", "commands"];
    public string HelpText => "Show available commands.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        // Iterate AllCommands on the registry, print Verb + HelpText
    }
}
```

*Note: HelpCommand needs a reference to CommandRegistry. Pass it in via constructor.*

#### QuitCommand

```csharp
public class QuitCommand : ICommand
{
    public string Verb => "quit";
    public string[] Aliases => ["exit", "q"];
    public string HelpText => "Quit the game.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        state.IsRunning = false;
    }
}
```

---

## 5. Content / World Building

### WorldBuilder

A static factory that constructs the game world. Hardcoded for v1 — simple and easy to test.

```csharp
namespace MyGame.Content;

using MyGame.Models;
using MyGame.Engine;

public static class WorldBuilder
{
    public static GameState Build()
    {
        var rooms = CreateRooms();
        LinkRooms(rooms);
        PlaceItems(rooms);

        return new GameState
        {
            CurrentRoomId = "alley",
            Rooms = rooms
        };
    }

    private static Dictionary<string, Room> CreateRooms() { ... }
    private static void LinkRooms(Dictionary<string, Room> rooms) { ... }
    private static void PlaceItems(Dictionary<string, Room> rooms) { ... }
}
```

### V1 World Map

```
                    ┌─────────────┐
                    │  Rooftop    │
                    │  (rooftop)  │
                    └──────┬──────┘
                           │ down
                           │
┌─────────────┐  east  ┌──┴──────────┐  east  ┌─────────────┐
│  Neon Alley │───────►│  The Byte   │───────►│  Corp Lobby │
│  (alley)    │◄───────│  Bar (bar)  │◄───────│  (lobby)    │
│  [START]    │  west  └─────────────┘  west  └──────┬──────┘
└─────────────┘                                      │ north
                                                     │ (LOCKED)
                                              ┌──────┴──────┐
                                              │  Server Room│
                                              │  (server)   │
                                              │  [WIN]      │
                                              └─────────────┘
```

**Rooms:**
| Id | Name | Notes |
|---|---|---|
| `alley` | Neon Alley | Start room. Gritty back alley. |
| `bar` | The Byte Bar | Seedy dive bar. Fixer NPC flavor text. |
| `rooftop` | Rooftop | Above the bar. Has the keycard. |
| `lobby` | Corp Lobby | Sterile corporate building. Locked door north. |
| `server` | Server Room | Win condition room. Reaching here = victory. |

**Items:**
| Id | Name | Location | Notes |
|---|---|---|---|
| `keycard` | Corp Keycard | rooftop | Unlocks lobby→server door |
| `flyer` | Crumpled Flyer | alley | Flavor item, hints at bar |
| `terminal` | Broken Terminal | bar | Scenery (CanPickUp = false) |
| `drive` | Data Drive | server | Flavor — the prize |

**Win Condition:** Player enters the `server` room. The `GoCommand` checks: if the destination is `server`, set `HasWon = true` and `IsRunning = false`.

---

## 6. Game Loop Flow

```
┌────────────────────────────────────────────────────┐
│                    Program.Main()                   │
│  1. WorldBuilder.Build() → GameState               │
│  2. Create CommandRegistry, register all commands   │
│  3. Create GameEngine(state, registry, ConsoleIO)   │
│  4. engine.Run()                                    │
└───────────────────────┬────────────────────────────┘
                        ▼
┌────────────────────────────────────────────────────┐
│                  GameEngine.Run()                   │
│  1. Print banner                                    │
│  2. Execute "look" (show starting room)             │
│  3. LOOP while state.IsRunning:                     │
│     a. Print prompt "> "                            │
│     b. Read input                                   │
│     c. CommandParser.Parse(input) → ParsedCommand   │
│     d. CommandRegistry.Execute(parsed, state, io)   │
│  4. Print win/quit message                          │
└────────────────────────────────────────────────────┘
```

### Command Dispatch Flow

```
User types: "use keycard"
          ↓
CommandParser.Parse("use keycard")
  → ParsedCommand(Verb: "use", Noun: "keycard")
          ↓
CommandRegistry.Execute(parsed, state, io)
  → Looks up "use" → UseCommand
  → UseCommand.Execute(parsed, state, io)
    → Finds "keycard" in inventory
    → Checks UseTargetId → matches exit "server" in lobby
    → Unlocks the exit, sets flag "server_unlocked"
    → Prints "You swipe the keycard. The door clicks open."
```

---

## 7. Program.cs (Entry Point)

```csharp
using MyGame.Engine;
using MyGame.Commands;
using MyGame.Content;

var state = WorldBuilder.Build();

var registry = new CommandRegistry();
registry.Register(new LookCommand());
registry.Register(new GoCommand());
registry.Register(new TakeCommand());
registry.Register(new DropCommand());
registry.Register(new InventoryCommand());
registry.Register(new UseCommand());
registry.Register(new HelpCommand(registry));
registry.Register(new QuitCommand());

var engine = new GameEngine(state, registry, new ConsoleIO());
engine.Run();
```

---

## 8. Testing Strategy

All game logic is testable without `Console`:

- **IInputOutput** is injected — tests provide a mock/stub that feeds input and captures output.
- **GameState** is a plain object — construct it directly in tests with custom rooms/items.
- **Commands** are tested individually: create a minimal GameState, call `Execute()`, assert state changes and output.
- **CommandParser** is a pure function — straightforward input/output tests.
- **WorldBuilder** can be tested by calling `Build()` and asserting the world is valid (all exits point to real rooms, start room exists, etc.).

Example test pattern:
```csharp
[Fact]
public void GoCommand_MovesPlayerToConnectedRoom()
{
    var rooms = new Dictionary<string, Room> { ... };
    var state = new GameState { CurrentRoomId = "alley", Rooms = rooms };
    var io = new TestIO();
    var cmd = new GoCommand();

    cmd.Execute(new ParsedCommand("go", "east"), state, io);

    Assert.Equal("bar", state.CurrentRoomId);
}
```

---

## 9. Design Principles

1. **Separation of concerns**: Engine (loop, parsing) is decoupled from content (rooms, items, world).
2. **Dependency injection via interfaces**: `IInputOutput` enables testing without Console.
3. **Open/closed for commands**: Adding a new command = one new class + one `Register()` call.
4. **State is centralized**: `GameState` is the single source of truth. Commands read and mutate it.
5. **Content is isolated**: `WorldBuilder` is the only place that knows what rooms/items exist.
6. **No over-engineering**: No events, no ECS, no scripting engine. Just classes and methods.
