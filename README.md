# Neon Ledger

*Data runs through the streets. You run the streets.*

A cyberpunk text adventure in a rain-soaked megacity, where every command counts and the clock is always ticking.

## Quick Start

**Requirements:** .NET 10 SDK

```bash
cd src/MyGame
dotnet run
```

That's it. The game loads. Your job begins.

## The Story

Your fixer went dark. All you've got is a cryptic message and a deadline. SynthCorp controls three districts. The data chip you need is buried in their networks. Navigate the neon-soaked streets, talk to shady contacts, avoid the drones, solve the puzzles, and get out alive.

## Commands

| Command | Aliases | What it does |
|---------|---------|-------------|
| `go <direction>` | `north`, `south`, `east`, `west`, `n`, `s`, `e`, `w`, `up`, `down`, `u`, `d` | Move in a direction |
| `look` | `l` | Describe the room you're in |
| `look <item>` | — | Examine an item in the room |
| `take <item>` | `get`, `grab`, `pick` | Pick up an item |
| `drop <item>` | — | Drop an item from inventory |
| `examine <item>` | `x`, `inspect`, `read` | Examine an item closely |
| `use <item>` | — | Use an item (may unlock exits or trigger events) |
| `talk <npc>` | `speak` | Talk to an NPC |
| `inventory` | `inv`, `i` | List items you're carrying |
| `help` | `?`, `commands` | Show all available commands |
| `save [filename]` | — | Save your game to a JSON file |
| `load [filename]` | — | Load a saved game |
| `quit` | `exit`, `q` | Exit the game |

## Saving & Loading

Save your progress anytime:

```bash
save
save mysave
load mysave
```

## Tips

- **Examine everything.** Items in rooms reveal secrets. Use the `examine` command.
- **Talk to everyone.** NPCs have information. Some won't repeat themselves.
- **The drones are watching.** Linger too long and your cover's blown.
- **Map it out.** Rooms connect in ways that matter. Pay attention to exits.

## Development

**Run tests:**

```bash
cd src/MyGame.Tests
dotnet test
```

**Stack:**
- C# / .NET 10
- xUnit (tests)
- Newtonsoft.Json

## Engine

- **World loader** — rooms, items, NPCs, and exits from JSON
- **Command parser** — full set of commands for navigation and interaction
- **Narrator engine** — room descriptions shift based on your inventory and actions
- **Save / load** — game state serialized to JSON
- **Colored output** — ANSI terminal colors for immersion
