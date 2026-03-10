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

### Try-Again Restart Feature (Issue #29) — Judy

**Date:** 2026-03-10  
**Author:** Judy (C# Developer)

## Context

When the player's drone threat level exceeds the threshold, `HasLost` is set to `true` and the game loop exits. Previously the engine returned immediately, with no way to restart without re-running the process.

## Decision

Introduce an optional `Func<GameState>? stateFactory` parameter to `GameEngine`'s constructor. The factory is a delegate that produces a fresh `GameState` on demand. When the player dies and the factory is present, the engine prompts for a retry and — if confirmed — calls `_state = _stateFactory()` to replace the entire game state before looping into a new session.

## Design Rationale

- **Delegate over interface**: A `Func<GameState>` is the simplest possible contract. No new interface, no new class — just a lambda at the call site.
- **Optional with `null` default**: Existing tests construct `GameEngine` without a factory and must not regress. `null` means "exit on death", which preserves the prior behaviour exactly.
- **Factory owns construction**: `Program.cs` provides `() => new JsonWorldLoader().Load(worldPath).State`. The engine never needs to know how the world is loaded — it only knows how to run a session.
- **`RunSession()` handles one full game**: Banner, intro, game loop, end message — all encapsulated. Calling it again from the retry loop gives the player an identical fresh-start experience.
- **Prompt style**: `ColorConsole.Yellow()` used for the "Try again?" prompt, consistent with the cyberpunk colour palette (yellow = decision/choice).

## Consequences

- `_state` field changed from `readonly` to mutable — intentional, required for in-place replacement.
- `Run()` now owns the outer retry loop; `RunSession()` owns a single game lifetime.
- No change to any test setup code — all 205 tests pass (199 pre-existing + 6 new).

## Files Changed

- `src/MyGame/Engine/GameEngine.cs` — factory field, mutable state, `Run()` retry loop, `RunSession()` extraction
- `src/MyGame/Program.cs` — factory lambda passed to `GameEngine` constructor

### Test Coverage: Try-Again Restart (Issue #29) — River

**Author:** River (Tester)  
**Date:** 2026-03-10

## Decision: 6 Tests Cover the Restart Feature

### What Was Tested

Six focused tests in `src/MyGame.Tests/TryAgainTests.cs`:

| Test | What it guards |
|---|---|
| `Death_WithFactory_ShowsTryAgainPrompt` | Prompt appears on death when factory provided |
| `Death_WithFactory_AnswerNo_ExitsCleanly` | "no" answer exits cleanly with correct state flags |
| `Death_WithFactory_AnswerYes_RestartsGame` | "yes" answer restarts the full session (two banners in output) |
| `Death_WithoutFactory_NoTryAgainPrompt` | No factory = no prompt (backward compatibility) |
| `Win_DoesNotShowTryAgainPrompt` | Win path never shows prompt, even with factory |
| `Quit_DoesNotShowTryAgainPrompt` | Voluntary quit never shows prompt, even with factory |

### What Was Not Tested

- **Multiple restarts in sequence** (die → yes → die → yes → no): considered low-risk; the retry loop is bounded by user input and uses the same code path on every cycle.
- **Factory throwing an exception**: error handling in the factory is Judy's concern; no behavior is specified for this case.
- **"YES" / "Yes" / "Y" casing variants**: the implementation uses `StartsWith("y", OrdinalIgnoreCase)` and the existing parser tests cover similar case-insensitive input patterns; not duplicated here.

### Design Choices

- **`DeathInputs` static array** shared across all death-triggering tests — single source of truth for the plaza death sequence.
- **Banner line count** used to detect session restart — counts lines containing "╔" (unique to the title banner). This is output-observable without requiring access to the engine's internal `_state` after factory reset.
- **Named parameter** `stateFactory: factory` used at call sites to skip the optional `world` parameter cleanly, matching the test idiom of omitting world for integration tests that don't need world metadata.

### Status

All 205 tests pass (199 pre-existing + 6 new).

### PR #62 Closure: Duplicate Code Removal — Issue #31
**Date:** 2026-03-10  
**Author:** Judy (C# Developer)  
**Status:** MERGED (squash) to main

Removed duplicate `FindItem()` method from `LookCommand` that duplicated `GameStateExtensions.FindItem()` introduced in Issue #33. This was an oversight during the initial refactoring.

**Changes:**
- `LookCommand.cs`: 1 insertion, 9 deletions
- Migrated call to use extension method: `FindItem(command.Noun, state)` → `state.FindItem(command.Noun)`
- Removed duplicate private method

**Test Coverage:** 227 tests pass (no new tests required — refactor only)

**Recommendation:** Future shared-utility extractions should include comprehensive grep across all command files before closure.

### Issue #39: Parser.cs Removal — Verified & Closed

**Date:** 2026-03-10  
**Status:** ✅ RESOLVED (Completed in Session 12, Verified in Session 17)  
**Author:** Judy (C# Developer)

**Summary:** Parser.cs, a 6-line wrapper around `CommandParser.Parse()`, has been successfully removed. All 6 tests in ParserTests.cs migrated to direct `CommandParser.Parse()` calls. Codebase grep confirms zero remaining references to the wrapper. All 227 tests pass with zero failures.

**Resolution:** Issue #39 is fully closed. Parser.cs removal complete, no further action required.

---

### Issue #59: GameEngine Coupling Assessment — Resolved

**Date:** 2026-03-10 (Session 17)  
**Status:** RESOLVED — Current structure acceptable  
**Author:** Johnny (Lead & Architect)

**Summary:** GameEngine previously mixed drone threat escalation, UI rendering, and restart logic. DroneThreatSystem extraction (Issue #44) resolved the escalation coupling. Current GameEngine structure is clean and properly separated: session orchestration (banner → loop → endgame), delegate drone mechanics to DroneThreatSystem, delegate command dispatch to CommandRegistry, delegate content to GameMessages.

**Analysis:** Each responsibility is isolated and single-purpose. Title banner rendering is a presentation concern that belongs in the orchestrator—not a coupling issue. Alternatives (TitleRenderer, GameSession wrappers) rejected as unnecessary abstractions without clarity benefit.

**Design Rationale:**
1. **Separation of Concerns** — Each system has one clear responsibility (GameEngine = session orchestration, DroneThreatSystem = threat mechanics, CommandRegistry = command dispatch, GameMessages = narrative/UI text, NarratorEngine = dynamic descriptions)
2. **No New Abstractions Needed** — Current code is readable and maintainable; extraction would add boilerplate without benefit
3. **Pragmatism** — Engine is 147 lines; all concerns properly delegated
4. **Testability Achieved** — All 227 tests pass; mock I/O captures banner output for assertions

**Consequences:** GameEngine remains the clear orchestrator for game session lifecycle. Future features (pause menu, settings, difficulty) can integrate cleanly into orchestrator without refactoring.

**Recommendation:** Close Issue #59 as RESOLVED (NOT PLANNED). No follow-up work needed.

**Files Affected:** `src/MyGame/Engine/GameEngine.cs` (147 lines), ARCHITECTURE.md, 227+ tests

### Content Decision: Unused Items (Issue #54) — Rogue

**Date:** 2026-03-10  
**Status:** RESOLVED  
**Author:** Rogue (Content Designer)

**Problem:** Two items in the world JSON served no mechanical purpose (repair_kit and flyer). repair_kit description falsely implied mechanical use ("useful for repairing or bypassing locks") with null UseTargetId. flyer was pure flavor with no UseMessage.

**Decision:** Reframe both items as **purely atmospheric flavor** while giving them proper UseMessages that activate when the player uses them. Keep UseTargetId: null (no mechanical hooks).

**repair_kit Redesign:**
- **New description:** Transformed from "broken promise" to "cultural artifact." Emphasizes undercity survival economy, salvage culture, jury-rigged repairs. Removes false mechanical implications.
- **UseMessage:** "You turn the kit over in your hands. Whatever this was meant to fix, it tells a story—scavenged parts, crude welds, ingenuity born from desperation. Down here, broken things become tools. Tools become leverage. Leverage becomes freedom." (Player sees when using item)
- **Tone:** Cyberpunk fatalism emphasizing information/leverage as currency

**flyer Redesign:**
- **New description:** Upgraded from generic orientation hint to undercity propaganda with contraband framing. Establishes information as social currency. "The paper's worn, but the message is clear—even the undercity has its rumor mill."
- **UseMessage:** "You glance at the flyer again. In a city drowning in propaganda—corporate lies plastered on every corner—even a hand-scrawled tip sheet feels like contraband. Information trades fast down here. Faster than bullets." (Player sees when using item)
- **Tone:** Cyberpunk cynicism about information economy and undercity as alternative civilization

**Rationale:**
- Arbitrary mechanical hooks would either create narrative confusion, undermine existing puzzle hierarchy, or require significant world redesign
- UseMessages create moments of flavor that reward player interaction without mechanically overcomplicating world
- Reinforces worldbuilding: salvage economy + information network strengthen central conflict (corporate megastructures vs. underground resistance)
- Consistent with existing pattern (keycard and cred_chip also have UseMessages)

**Implementation:** Content-only change in world JSON. UseCommand already supports null UseTargetId (displays UseMessage). No engine changes required. No test modifications. All 227 tests pass.

**Future Expansion:** If mechanics desired later, add UseTargetId pointing to specific exit—no description changes needed. Current approach leaves door open without committing design.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
