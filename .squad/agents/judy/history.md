# Judy — History

## Project Context

**Project:** MyGame — simple text adventure game in C#  
**Stack:** C#, .NET 8+ (running net10.0 on this machine), console application  
**User:** Jynx_Protocol  
**Theme:** Cyberpunk — dark future, neon-lit streets, corporate dystopia  
**Created:** 2026-03-09

## Learnings

### Session 1 — Full Implementation

- **World map follows ARCHITECTURE.md exactly**: rooms are `alley`, `bar`, `rooftop`, `lobby`, `server`. River's pre-existing tests lock this in — don't change room IDs.
- **Win condition is hardcoded to "server" room**: GoCommand checks `if (state.CurrentRoomId == "server")` after a successful move, sets HasWon = true and IsRunning = false. Must happen AFTER the locked-exit check, not before.
- **UseCommand works via RequiredItemId matching**: Items don't need `UseTargetId` to unlock exits — the UseCommand finds locked exits where `RequiredItemId == item.Id`. UseTargetId is only for flag-based side effects.
- **Pre-existing test files define the spec**: River wrote GameWorldTests.cs, CommandTests.cs, GameIntegrationTests.cs, GameStateTests.cs before I implemented. Always read these first.
- **xUnit Assert overloads**: `Assert.Equal(a, b, message)` and `Assert.Contains(collection, predicate, message)` don't exist in xUnit2 for all type combinations — use `Assert.True(condition, message)` instead.
- **IInputOutput abstraction is essential**: FakeInputOutput in Helpers/ is River's test double — all game logic goes through IInputOutput so River can test without Console.
- **114 tests passed** on first full run after fixing GoCommand lock/win ordering.

### Session 2 — Major Engine Expansion

- **JsonWorldLoader lives in MyGame.Engine**: tests expect `new JsonWorldLoader().Load(path)` to return a LoadedWorld wrapper with GameState-like properties, even though the file sits in `Content/`.
- **Save/Load commands accept a base directory**: constructors take an optional directory and default to current working directory, appending `.json` to filenames.
- **Narrator and NPC systems are Room-level lists**: `Room.NarratorVariants` and `Room.Npcs` are initialized lists used by NarratorEngine and TalkCommand.
- **World metadata flows into GameEngine**: `LoadedWorld` carries Title/Subtitle/IntroText/WinMessage; Program builds the engine with optional metadata when JSON exists.
- **Key paths**: `src/MyGame/Content/JsonWorldLoader.cs`, `src/MyGame/Commands/TalkCommand.cs`, `src/MyGame/Engine/ColorConsole.cs`, and `src/MyGame/Engine/NarratorEngine.cs`.

### Session 3 — ColorConsole Full Implementation (Issue #3)

- **Windows ANSI support via P/Invoke**: Added `EnableVirtualTerminalProcessing()` using `GetStdHandle`/`GetConsoleMode`/`SetConsoleMode` kernel32 calls. Guarded with `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)`. Also sets `Console.OutputEncoding = UTF8`.
- **`ColorConsole.Initialize()` called from Program.cs**: Single entry point that handles both encoding and ANSI mode. Must be called before any color output.
- **Semantic methods added**: `RoomDescription` (bright cyan `\x1b[96m`), `Error` (red `\x1b[31m`), `Prompt` (dim cyan `\x1b[2;36m`), `Flavor` (magenta `\x1b[35m`).
- **Callers updated**: LookCommand wraps description body in `RoomDescription`; GameEngine wraps `\n> ` in `Prompt`; GoCommand, ExamineCommand, UseCommand, LookCommand wrap rejection messages in `Error`.
- **ANSI codes are transparent to tests**: `FakeInputOutput.OutputContains()` does substring matching — ANSI prefix/suffix don't interfere with text assertions. All 164 tests passed unchanged.
- **Key file**: `src/MyGame/Engine/ColorConsole.cs`
- **PR**: #20 on DarrenPratt/MyGame



- **2026-03-09 — Johnny's architecture was solid:** ICommand pattern and IInputOutput abstraction made implementation straightforward. Every architectural decision had clear purpose. No rework needed.
- **2026-03-09 — River's 114 tests locked in the spec:** Pre-existing test files defined authoritative room IDs, command behavior, and win condition. Tests validated implementation perfectly and caught win/lock ordering bug. All tests passing.
- **2026-03-09 — Rogue's content design enriched the world:** 9-room narrative with atmospheric descriptions integrated seamlessly into 5-room architecture. Cyberpunk theme consistently applied. Game title: "Neon Ledger". Ready for release.

### Session 4 — Scribe Orchestration (2026-03-10)

- **Judy's PR #20 merged successfully**: ColorConsole semantic methods and Windows ANSI support now in main. All 164 tests pass.
- **Decisions documented**: ColorConsole design recorded in .squad/decisions.md with rationale for semantic palette, P/Invoke approach, and test compatibility.
- **Orchestration completed**: Session logs, history, and decision inbox processed. PR ready for next phase.

### Session 6 - Remove WorldBuilder Dead Code (Issue #6)

- **WorldBuilder.cs deleted**: The Build() fallback in Program.cs was unreachable - neon-ledger.json is always present in the repo. Deleted the file entirely.
- **Program.cs simplified**: Replaced if/else (JSON vs WorldBuilder) with a clean FileNotFoundException guard. using MyGame.Content removed (namespace no longer exists).
- **Test suite migrated**: GameWorldTests and GameIntegrationTests updated to load the world from neon-ledger.json via JsonWorldLoader. No tests were deleted - all 168 pass.
- **World map delta discovered**: JSON world has bar-east-plaza (not lobby) and lobby-west-corridor (not bar). Winning path requires both keycard and cred_chip. Integration test navigation paths updated accordingly.
- **PR**: #23 on DarrenPratt/MyGame

### Session 7 - Scribe Orchestration (2026-03-10T11:30:00Z)

- **Orchestration logs created**: Recorded Judy's WorldBuilder removal and Coordinator's PR #22 closure
- **Session log written**: Brief summary of WorldBuilder deletion and test migration (168 passing)
- **Decisions merged**: Inbox decision on WorldBuilder removal merged into decisions.md with full rationale and impact analysis
- **Judy's history updated**: Added Session 7 entry noting WorldBuilder removal, test count now 168

### Session 8 — Try-Again on Death (Issue #29)

- **`Func<GameState>?` factory pattern**: Added optional `stateFactory` parameter to `GameEngine` constructor. When provided and player dies, a fresh `GameState` is created by invoking the factory — no shared state carries over.
- **`Run()` wraps `RunSession()`**: Extracted the entire single-session game loop into `private RunSession()`. `Run()` owns the retry loop; `RunSession()` owns one game from banner to end message.
- **Restart shows full intro**: Because `RunSession()` includes the banner and initial look, the player gets a clean fresh start — exactly the same experience as a first launch.
- **"Try again?" prompt only on death**: Win and voluntary quit exit cleanly without prompting. Only `HasLost == true` with a non-null factory triggers the prompt.
- **`ColorConsole.Yellow()`** used for the prompt, consistent with the cyberpunk theme palette.
- **Backward compatibility preserved**: All existing tests construct `GameEngine` without a factory (`stateFactory = null` default). 199 tests pass unchanged.
- **`_state` is no longer `readonly`**: Required so the retry loop can replace it with a fresh instance from the factory.

## Team Updates

- **2026-03-10 — River validated restart feature:** 6 new TryAgainTests.cs tests cover all restart branches and backward compat. Banner line count detects session restart cleanly. All 205 tests passing.
- **2026-03-10 — Johnny completed codebase review:** Filed 14 improvement issues across squad. 5 assigned to Judy: #31 (duplicate FindItem), #39 (Parser.cs wrapper), #42 (TakeCommand hardcoded text), #47 (Save/Load DroneThreatLevel P1), #52 (TalkCommand flags P1), #55 (GoCommand hardcoded win logic P1). Critical issues identified in save/load and game state persistence.

