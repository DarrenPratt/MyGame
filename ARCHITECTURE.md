# MyGame — Architecture Document

> **Cyberpunk Text Adventure** · C# / .NET 8+ · Console Application  
> Architect: Johnny · Updated: 2026-03-10

---

## 1. Project Structure

```
MyGame/
├── ARCHITECTURE.md
├── src/
│   ├── MyGame/                       # Main game executable
│   │   ├── MyGame.csproj             # net8.0 console app
│   │   ├── Program.cs                # Entry point
│   │   ├── Engine/
│   │   │   ├── GameState.cs          # All mutable game state
│   │   │   ├── GameEngine.cs         # Main loop, restart-on-death, session mgmt
│   │   │   ├── CommandParser.cs      # Raw input → ParsedCommand
│   │   │   ├── GameMessages.cs       # All UI/narrative strings (centralized)
│   │   │   ├── GameStateExtensions.cs # Shared FindItem, FindRoomItem helpers
│   │   │   ├── NarratorEngine.cs     # Dynamic description selection by flags
│   │   │   ├── ColorConsole.cs       # Colored output + Windows ANSI support
│   │   │   └── JsonWorldLoader.cs    # Load game world from JSON
│   │   ├── Commands/
│   │   │   ├── ICommand.cs           # Command interface
│   │   │   ├── CommandRegistry.cs    # Maps verb → ICommand
│   │   │   ├── GoCommand.cs          # Movement + win condition
│   │   │   ├── LookCommand.cs        # Room inspection + NPC listing
│   │   │   ├── ExamineCommand.cs     # Examine items/NPCs
│   │   │   ├── TakeCommand.cs        # Pick up item
│   │   │   ├── DropCommand.cs        # Drop item
│   │   │   ├── InventoryCommand.cs   # List inventory
│   │   │   ├── UseCommand.cs         # Use item (unlock exits, etc.)
│   │   │   ├── TalkCommand.cs        # NPC dialogue + flag setting
│   │   │   ├── SaveCommand.cs        # Save game state + locks/threat
│   │   │   ├── LoadCommand.cs        # Load saved game
│   │   │   ├── HelpCommand.cs        # Command list
│   │   │   └── QuitCommand.cs        # Graceful exit
│   │   └── Models/
│   │       ├── Room.cs               # Room with Exits, Items, NPCs, Variants
│   │       ├── Exit.cs               # Directional link (locked/unlocked)
│   │       ├── Item.cs               # Pickuppable object
│   │       ├── Npc.cs                # NPC with dialogue tree
│   │       ├── DialogueNode.cs       # Single dialogue message + choices
│   │       ├── DialogueResponse.cs   # Player choice
│   │       └── NarratorVariant.cs    # Conditional room description
│   │
│   └── MyGame.Tests/                 # Test suite (227+ tests)
│       ├── MyGame.Tests.csproj       # net10.0 xUnit project
│       ├── Helpers/
│       │   ├── FakeInputOutput.cs    # Test harness for I/O
│       │   └── WorldFactory.cs       # Test world builder
│       ├── Engine/
│       │   ├── GameStateTests.cs
│       │   ├── ParserTests.cs
│       │   └── NarratorEngineTests.cs
│       ├── Commands/
│       │   ├── CommandTests.cs       # Generic command harness
│       │   ├── ExamineCommandTests.cs
│       │   ├── TalkCommandTests.cs
│       │   └── [other command tests]
│       ├── SaveLoadTests.cs          # Game persistence
│       ├── JsonWorldLoaderTests.cs   # World file loading
│       ├── GameIntegrationTests.cs   # End-to-end flows
│       ├── GameWorldTests.cs         # Room/exit/item integrity
│       ├── DroneTests.cs             # Drone threat system
│       ├── TryAgainTests.cs          # Restart-on-death feature
│       ├── EdgeCaseTests.cs          # Boundary conditions
│       └── [other test files]
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

**src/MyGame.Tests/MyGame.Tests.csproj**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\MyGame\MyGame.csproj" />
  </ItemGroup>
</Project>
```

---

## 2. Core Models

### Room

```csharp
public class Room
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, Exit> Exits { get; } = new();
    public List<Item> Items { get; } = new();
    public List<NarratorVariant> NarratorVariants { get; init; } = new();
    public List<Npc> Npcs { get; init; } = new();
}
```

Rooms hold **exits** (to other rooms), **items** (on the ground), **NPCs** (characters), and **narrator variants** (conditional descriptions based on game state).

### Exit

```csharp
public class Exit
{
    public required string Direction { get; init; }   // "north", "south", etc.
    public required string TargetRoomId { get; init; }
    public string? Description { get; init; }
    public bool IsLocked { get; set; }
    public string? RequiredItemId { get; set; }       // Item needed to unlock
}
```

Exits can be locked and require an item to unlock (e.g., keycard).

### Item

```csharp
public class Item
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public bool CanPickUp { get; init; } = true;
    public string? UseTargetId { get; init; }         // Exit or item to interact with
    public string? UseMessage { get; init; }
}
```

### Npc

```csharp
public class Npc
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public List<DialogueNode> Dialogue { get; init; } = new();
}

public class DialogueNode
{
    public required string Id { get; init; }
    public required string Text { get; init; }
    public List<DialogueResponse> Responses { get; init; } = new();
}

public class DialogueResponse
{
    public required string Text { get; init; }
    public string? NextNodeId { get; init; }
}
```

NPCs have **dialogue trees**. When talked to, `TalkCommand` sets the `{npc_id}_met` flag.

### NarratorVariant

```csharp
public class NarratorVariant
{
    public required string Description { get; init; }
    public List<string> RequiredFlags { get; init; } = new();
    public List<string> RequiredInventoryItems { get; init; } = new();
}
```

Allows dynamic room descriptions. `NarratorEngine.GetDescription()` selects the most specific variant (highest combined flag + item count) where all requirements are met.

---

## 3. Engine Systems

### GameState

Holds **all** mutable game state. Pure data, no logic.

```csharp
public class GameState
{
    public required string CurrentRoomId { get; set; }
    public List<Item> Inventory { get; } = new();
    public HashSet<string> Flags { get; } = new();    // e.g. "viktor_met", "door_unlocked"
    public bool IsRunning { get; set; } = true;
    public bool HasWon { get; set; }
    public bool HasLost { get; set; } = false;
    public int DroneThreatLevel { get; set; } = 0;
    public HashSet<string> HighRiskRoomIds { get; init; } = new() { "plaza", "checkpoint" };
    public int DroneThreatThreshold { get; set; } = 4;
    public string? WinRoomId { get; set; } = "server";
    
    public Dictionary<string, Item> ItemCatalog { get; init; }
    public Dictionary<string, Npc> NpcCatalog { get; init; }
    public required Dictionary<string, Room> Rooms { get; init; }
    public Room CurrentRoom => Rooms[CurrentRoomId];
}
```

**Key fields:**
- `DroneThreatLevel` / `DroneThreatThreshold` — drone surveillance mechanic
- `HighRiskRoomIds` — zones where drones increment threat each turn
- `Flags` — dynamic state for NPC meeting, door unlocking, etc.
- `ItemCatalog` / `NpcCatalog` — global lookups for quick access

### GameEngine & Game Loop

The engine now supports **restart-on-death** via an optional `stateFactory` delegate.

```csharp
public class GameEngine
{
    private GameState _state;                           // Mutable, replaced on restart
    private readonly Func<GameState>? _stateFactory;    // Optional restart factory
    private readonly LoadedWorld? _world;               // World metadata + messages
    private readonly CommandRegistry _commands;
    private readonly IInputOutput _io;
    
    public GameEngine(GameState state, CommandRegistry commands, IInputOutput io,
                      LoadedWorld? world = null, Func<GameState>? stateFactory = null)
    { ... }

    public void Run()  // Outer loop: handles restart
    {
        while (true)
        {
            RunSession();  // Play one game
            
            if (_state.HasLost && _stateFactory is not null)
            {
                // Prompt to try again
                var answer = _io.ReadLine();  // "y" to restart
                if (answer?.StartsWith("y", OrdinalIgnoreCase) == true)
                {
                    _state = _stateFactory();  // Fresh state
                    continue;
                }
            }
            break;
        }
    }

    private void RunSession()  // Inner loop: one game lifetime
    {
        // Render banner from LoadedWorld or defaults
        // Show intro text
        // Execute "look" to describe starting room
        
        while (_state.IsRunning)
        {
            var input = _io.ReadLine();  // "> "
            var parsed = CommandParser.Parse(input);
            _commands.Execute(parsed, _state, _io);
            
            // Drone threat check (after every command in high-risk rooms)
            if (_state.HighRiskRoomIds.Contains(_state.CurrentRoomId))
            {
                _state.DroneThreatLevel++;
                // Show warnings at levels 1, 2, 3
                // Die at threshold (≥4)
            }
        }
        
        // End-of-session: show win/lose/quit message
    }
}
```

**Drone Threat System:**
- Each turn in "plaza" or "checkpoint" increments threat counter
- At threshold (default 4), `HasLost = true`, game ends
- Save/load persists threat level

### NarratorEngine

Provides **context-aware room descriptions** based on game state.

```csharp
public static class NarratorEngine
{
    public static string GetDescription(Room room, GameState state)
    {
        var variant = GetVariant(room, state);
        return variant?.Description ?? room.Description;
    }

    public static NarratorVariant? GetVariant(Room room, GameState state)
    {
        // Find most specific variant where all RequiredFlags and items match
        return room.NarratorVariants
            .Where(v => v.RequiredFlags.All(f => state.Flags.Contains(f))
                     && v.RequiredInventoryItems.All(id => state.Inventory.Any(i => i.Id == id)))
            .OrderByDescending(v => v.RequiredFlags.Count + v.RequiredInventoryItems.Count)
            .FirstOrDefault();
    }
}
```

Used by `LookCommand` and `GoCommand` to show tailored descriptions.

### GameMessages

All **player-facing strings** centralized in one static class — no hardcoded text in commands.

```csharp
public static class GameMessages
{
    public static class Defaults
    {
        public const string Title = "N E O N   L E D G E R";
        public const string Subtitle = "A Cyberpunk Text Adventure";
        public const string IntroText = "...";
    }
    
    public static class Prompts
    {
        public const string CommandInput = "\n> ";
        public const string DialogueInput = "> ";
        public const string TryAgain = "\nTry again? (yes/no) ";
    }
    
    public static class Drone
    {
        public const string Warning1 = "A drone sweeps overhead...";
        public const string Warning2 = "Drone targeting systems...";
        public const string Warning3 = "CRITICAL: Drone lock acquired...";
    }
    
    public static class Win { ... }
    public static class Lose { ... }
    public static class Talk { ... }
    public static class Go { ... }
    // ... other command-specific messages
}
```

### ColorConsole

Handles **colored output** and **Windows ANSI support**.

```csharp
public static class ColorConsole
{
    public static void Initialize() { /* P/Invoke ENABLE_VIRTUAL_TERMINAL_PROCESSING */ }
    
    public static string Cyan(string text) => $"\x1b[96m{text}\x1b[0m";
    public static string RoomDescription(string text) => Cyan(text);
    public static string Error(string text) => $"\x1b[31m{text}\x1b[0m";
    public static string Yellow(string text) => ...;
    public static string Magenta(string text) => ...;
    // ... and primitives (BoldCyan, DarkGray, Prompt, etc.)
}
```

### GameStateExtensions

Shared **item lookup helpers** to avoid duplication across commands.

```csharp
public static class GameStateExtensions
{
    public static Item? FindItem(this GameState state, string itemId);
    public static Item? FindRoomItem(this GameState state, string itemId);
    public static Item? FindInventoryItem(this GameState state, string itemId);
}
```

### JsonWorldLoader

Loads a **world from JSON** and builds a fresh `GameState`.

```csharp
public class JsonWorldLoader
{
    public LoadedWorld Load(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<WorldData>(json);
        return new LoadedWorld { State = ..., Title = ..., Subtitle = ..., IntroText = ... };
    }
}

public class LoadedWorld
{
    public GameState State { get; init; }
    public string Title { get; init; }
    public string Subtitle { get; init; }
    public string IntroText { get; init; }
    public string WinMessage { get; init; }
    public string LoseMessage { get; init; }
}
```

---

## 4. Command System

### ICommand Interface

```csharp
public interface ICommand
{
    string Verb { get; }
    string[] Aliases => [];
    string HelpText { get; }
    void Execute(ParsedCommand command, GameState state, IInputOutput io);
}
```

All commands implement this simple contract. Aliases support shortcuts (e.g., "n" for "north").

### CommandRegistry

Maps verb strings (including aliases) to ICommand implementations.

```csharp
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

### Available Commands

| Command | Aliases | Purpose |
|---------|---------|---------|
| **look** | `l` | Describe current room, list exits/items/NPCs |
| **go** | `north, south, east, west, up, down, n, s, e, w, u, d` | Move between rooms (checks locks) |
| **examine** | `x` | Examine item or NPC in detail |
| **take** | `get, pick, grab` | Pick up an item (respects `CanPickUp`) |
| **drop** | | Drop item from inventory |
| **inventory** | `inv, i` | List what you're carrying |
| **use** | | Use an item (unlocks exits, triggers flags) |
| **talk** | `speak` | Talk to NPC, navigate dialogue tree; sets `{npc_id}_met` flag |
| **save** | | Save game to JSON (persists threat, locks, flags) |
| **load** | | Load game from JSON |
| **help** | `?, commands` | List all commands |
| **quit** | `exit, q` | Exit the game |

**Key behaviors:**
- **LookCommand** uses `NarratorEngine.GetDescription()` for dynamic room text
- **GoCommand** checks exit locks, handles win condition (entering server room), calls `LookCommand` to auto-describe new room
- **TalkCommand** navigates dialogue tree and **sets `{npc_id}_met` flag** (Issue #46)
- **ExamineCommand** shows `Item.Description` or `Npc.Description`
- **SaveCommand** persists `DroneThreatLevel`, `DroneThreatThreshold`, `ExitLockStates` (Issue #35)
- **LoadCommand** restores all state from JSON

---

## 5. Save/Load Format

SaveCommand serializes to JSON. The format includes:

```json
{
  "CurrentRoomId": "server",
  "Inventory": ["keycard"],
  "Flags": ["viktor_met", "server_unlocked"],
  "DroneThreatLevel": 3,
  "DroneThreatThreshold": 4,
  "ExitLockStates": {
    "lobby": { "north": false },
    "bar": { "east": true }
  }
}
```

LoadCommand deserializes and rebuilds the `GameState`:
- Restores inventory references via `ItemCatalog`
- Restores exit lock states
- Preserves all flags (narrator variants will re-evaluate)

---

## 6. Game Flow

### Startup (Program.cs)

```csharp
var worldPath = "world.json";
var loader = new JsonWorldLoader();
var world = loader.Load(worldPath);
var state = world.State;

var registry = new CommandRegistry();
registry.Register(new LookCommand());
registry.Register(new GoCommand());
registry.Register(new TakeCommand());
registry.Register(new DropCommand());
registry.Register(new InventoryCommand());
registry.Register(new ExamineCommand());
registry.Register(new UseCommand());
registry.Register(new TalkCommand());
registry.Register(new SaveCommand());
registry.Register(new LoadCommand());
registry.Register(new HelpCommand(registry));
registry.Register(new QuitCommand());

var engine = new GameEngine(
    state,
    registry,
    new ConsoleIO(),
    world: world,
    stateFactory: () => new JsonWorldLoader().Load(worldPath).State
);
engine.Run();
```

### Game Loop (One Session)

```
1. Render title banner (from LoadedWorld or defaults)
2. Print intro text
3. Execute "look" to describe starting room
4. LOOP while IsRunning:
   a. Print prompt "> "
   b. Read input
   c. Parse → CommandRegistry.Execute()
   d. Post-command: Drone threat check
      - If in HighRiskRoomId, increment DroneThreatLevel
      - Show warning at levels 1, 2, 3
      - Die (HasLost = true) at threshold
   e. If HasWon or HasLost: break loop
5. Print win/lose/quit message
6. Return control to Run() for potential restart
```

### Restart Flow (Try-Again)

When a player dies (`HasLost = true`) and `stateFactory` is provided:

```
1. Show "Try again? (yes/no) " prompt
2. If "yes": Call stateFactory() → new GameState
3. Loop back to step 1 (RunSession)
4. If "no": Break outer loop, exit game
```

If no factory: game exits immediately on death (backward compatible).

---

## 7. Test Coverage

**227+ tests** organized by concern:

- **GameStateTests** — State initialization, flags, inventory
- **ParserTests** — Command parsing (verb, noun extraction)
- **NarratorEngineTests** — Dynamic descriptions by flags/items
- **CommandTests** — Generic test harness for all commands
- **ExamineCommandTests** — Item/NPC examination
- **TalkCommandTests** — Dialogue navigation, flag setting
- **SaveLoadTests** — Persistence (threat, locks, flags)
- **JsonWorldLoaderTests** — World file loading
- **GameIntegrationTests** — End-to-end game flows
- **GameWorldTests** — Room/exit/item integrity
- **DroneTests** — Threat system mechanics
- **TryAgainTests** — Restart-on-death feature
- **EdgeCaseTests** — Boundary conditions

All tests use `FakeInputOutput` (test harness) to mock I/O and capture output without touching the console.

---

## 8. Design Principles

1. **Centralized strings** — All UI text in `GameMessages` (no hardcoding in commands)
2. **I/O abstraction** — `IInputOutput` interface enables testability
3. **Pure state** — `GameState` has no methods; commands read and mutate it
4. **Command registry** — Open/closed principle: add commands without modifying engine
5. **Narrator variants** — Room descriptions adapt to game state (flags + inventory)
6. **Restart loop** — Optional `stateFactory` supports retry without process restart
7. **Persistence** — Save/load includes all mutable state (threat, flags, locks)
8. **No dead code** — `Parser.cs` (old wrapper) was removed; all parsing via `CommandParser`
