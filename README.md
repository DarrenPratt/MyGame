# Neon Ledger

A cyberpunk text adventure built on a C# engine.

## Play

```bash
cd src/MyGame
dotnet run
```

## Test

```bash
cd src/MyGame.Tests
dotnet test
```

## Engine Features

- **World loader** — rooms, items, NPCs, and exits from JSON (`Content/worlds/*.json`)
- **Command parser** — `go`, `look`, `take`, `drop`, `talk to`, `use`, `examine`, `inventory`, `save`, `load`
- **Narrator engine** — room descriptions vary based on inventory and past actions
- **Save / load** — game state serialized to JSON
- **Colored output** — ANSI terminal colors via `ColorConsole`

## The Adventure — Neon Ledger

10 rooms · 5 items · 3 NPCs · 2 puzzles, set in a near-future cyberpunk city.

> *You wake up in a rain-soaked alley with a dead comms implant and no memory of the last 48 hours.*

## Stack

- C# / .NET 10
- xUnit (tests)
- Newtonsoft.Json
