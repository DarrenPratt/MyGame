# Squad Decisions

## Active Decisions

### Issue Backlog Creation — Johnny
**Date:** 2026-03-09  
**Author:** Johnny (Lead & Architect)

Created 19 GitHub issues across 3 milestones for the Neon Ledger cyberpunk text adventure project.

- **Milestone v0.1 – Core Engine (6 issues):** Program.cs wiring, CommandParser, ColorConsole, NarratorEngine, WorldBuilder, smoke test
- **Milestone v0.2 – World & Content (7 issues):** Win/lose conditions, NPC interactions, use commands, flavor text, command tests
- **Milestone v0.3 – Polish & Release (6 issues):** Save/load, full test suite, edge case hardening, JSON loading, final playthrough, README
- **Squad Assignments:** Judy (10), Rogue (3), River (6)
- **Status:** ✅ Complete — All 19 issues created and assigned

### Engine Expansion Decisions — Judy
**Author:** Judy (Core Engineer)

- Implemented `JsonWorldLoader` in `MyGame.Engine` with an instance `Load` method returning `LoadedWorld`, and added forwarder properties for GameState access while keeping metadata for the UI.
- Save/Load commands accept an optional base directory, append `.json` to filenames, and use System.Text.Json for serialization.
- Room descriptions and NPC handling rely on `NarratorVariants` and `Npcs` collections, with `NarratorEngine` selecting the most specific variant.
- GameEngine now renders a dynamic, colored title banner from LoadedWorld metadata and retains fallback defaults.

### Test Coverage: New Engine Systems — River
**Author:** River (Tester)  
**Date:** 2026-03-10  

Wrote comprehensive anticipatory test suite (43 new tests) for Judy's parallel feature development:
- **ParserTests.cs (6 tests):** ParsedCommand.Target field for "use X on Y" syntax
- **NarratorEngineTests.cs (8 tests):** Dynamic room descriptions based on game state with flag/inventory variants
- **TalkCommandTests.cs (8 tests):** NPC dialogue system with choice navigation
- **SaveLoadTests.cs (8 tests):** Game persistence with JSON serialization
- **JsonWorldLoaderTests.cs (10 tests):** World file loading with full data deserialization
- **GameStateTests.cs (+3 tests):** New GameState fields (ItemCatalog, NpcCatalog, WinRoomId)

**Status:** Tests written and await implementation (currently non-compiling by design). Ready to validate Judy's feature work.
### ColorConsole Design (Issue #3) — 2026-03-10

**Semantic colour palette (cyberpunk theme)**
| Role | Method | ANSI code | Rationale |
|---|---|---|---|
| Room descriptions | `RoomDescription` | `\x1b[96m` bright cyan | Environment is the neon world — should glow |
| Errors | `Error` | `\x1b[31m` red | Hard stop, immediate danger |
| Input prompt | `Prompt` | `\x1b[2;36m` dim cyan | Quietly waiting, doesn't compete with output |
| Flavour / atmosphere | `Flavor` | `\x1b[35m` magenta | Cyberpunk neon sign colour |

Existing primitives (`Cyan`, `BoldCyan`, `Yellow`, `Green`, `Magenta`, `DarkGray`) retained unchanged — backwards compatibility.

**Windows ANSI support**
Used `kernel32.dll` P/Invoke (`GetStdHandle`/`GetConsoleMode`/`SetConsoleMode`) with `ENABLE_VIRTUAL_TERMINAL_PROCESSING (0x0004)`. Guarded with `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`. Graceful degradation on unsupported terminals.

**Initialization**
`Initialize()` called from `Program.cs` top-level statements as the first line before any console output. Tests unaffected (ANSI codes are transparent to `OutputContains` substring assertions).

### PR #20 ColorConsole Review (2026-03-10)

**Reviewer:** River (QA)  
**Status:** APPROVED

Implementation verified:
- All 164 tests pass
- Windows P/Invoke guards (`ENABLE_VIRTUAL_TERMINAL_PROCESSING`) implemented correctly with graceful degradation
- `Flavor()` method defined but unused (awaiting atmospheric text implementation)
- Not all commands updated (intentional — future PRs to follow pattern)
- ANSI codes transparent to test assertions

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
