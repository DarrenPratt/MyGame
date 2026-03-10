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

### Session 9 — Fix Save/Load State Corruption (Issue #35)

- **Three fields were silently dropped on save/load**: `DroneThreatLevel`, `DroneThreatThreshold`, and per-room exit `IsLocked` states were not included in the `SaveData` record.
- **`DroneThreatThreshold` changed from `init` to `set`**: Required to allow `LoadCommand` to restore it on an existing `GameState` object. No tests relied on it being immutable.
- **`SaveCommand` now captures all exit states**: Builds a `Dictionary<string, Dictionary<string, bool>>` of `roomId → direction → isLocked` for all rooms that have exits.
- **`LoadCommand` restores with backward-compat defaults**: `DroneThreatLevel` defaults to 0, `DroneThreatThreshold` keeps the world default if saved value is 0, `ExitLockStates` is null-safe — old saves load without errors.
- **River's 6 pre-written tests all passed immediately** after the fix. Total tests: 211.
- **PR**: #60 on DarrenPratt/MyGame

### Session 10 — Fix viktor_met Narrator Flag (Issue #46)

- **Root cause found immediately**: `GameState.Flags` (`HashSet<string>`) already existed; `NarratorEngine.GetVariant()` already checked it. TalkCommand simply never set any flag after a dialogue interaction.
- **Narrator variant system**: `NarratorVariant.RequiredFlags` is a list of flag strings. `NarratorEngine` selects the most-specific matching variant (highest `RequiredFlags.Count + RequiredInventoryItems.Count` among all whose conditions are fully satisfied). Falls back to `Room.Description` if no variant matches.
- **Rooms using `viktor_met`**: `bar` has a variant keyed on `viktor_met` that changes narration after meeting Viktor. No other room uses this flag.
- **Fix**: One line added to `TalkCommand.Execute()` — `state.Flags.Add($"{npc.Id}_met")` — placed immediately after confirming the NPC has dialogue, before the conversation loop. Generic: sets `viktor_met`, `mox_met`, `guard_met` for any NPC with dialogue, no special-casing.
- **ExamineCommandTests.cs**: River had already written 16 new ExamineCommand tests locally; they were untracked and got picked up in the commit. 227 tests pass.
- **PR**: #61 on DarrenPratt/MyGame

### Session 11 — Scribe Orchestration (2026-03-10T19:15:00Z)

- **Orchestration logs created**: Recorded Judy's viktor_met flag fix and River's ExamineCommand test coverage
- **Session log written**: Comprehensive summary of parallel Issue #46 and Issue #38 work (227 tests passing)
- **Decisions merged**: All four inbox decision files (judy-viktor-met-flag, judy-save-load-fix, river-examine-tests, river-save-load-tests) merged into decisions.md; inbox files deleted
- **Agent histories updated**: Judy and River histories append with session 11 summaries

### Session 12 — Issue #32: Delete Parser.cs (unblock Judy)

**River rewrote ParserTests.cs** to call `CommandParser.Parse()` directly, then deleted `Parser.cs`.

**What Parser.cs was:** A 6-line instance-method wrapper — `public ParsedCommand Parse(string input) => CommandParser.Parse(input);`. No added logic; pure pass-through.

**What replaced it in tests:** `CommandParser.Parse(input)` called as a static method directly. All 6 tests kept identical assertions; only `new Parser()` removed.

**How CommandParser works:** Static class in `MyGame.Engine`. `Parse(string input)` trims/lowercases, splits on first space for Verb + rest, then scans rest for `" on "` to populate `Target`. Returns `ParsedCommand(Verb, Noun?, Target?)` record.

All 227 tests pass. `Parser.cs` is deleted. Decision merged into decisions.md.

### Session 12 — Extract Hardcoded Strings to GameMessages (Issue #34)

- **`GameMessages.cs` created** at `src/MyGame/Engine/GameMessages.cs`: one static class, twelve nested static classes, all `public const string` members.
- **Strings extracted from `GameEngine.cs`**: default title/subtitle/introText, `"\n> "` prompt, `"\nTry again? (yes/no) "` retry prompt, all three drone warning messages, win/lose default narrative blocks (consolidated from separate `_io.WriteLine` calls into single const strings split via `SplitLines`), and win/lose/quit banners.
- **Strings extracted from command files**: `GoCommand` (no-direction error, "The way is locked.", server-room win lines), `TakeCommand` (take-what error, data_chip flavour line), `UseCommand` (use-what error), `LookCommand` ("Items here:", "You see here:", "Exits:"), `HelpCommand` (header, directions footer), `QuitCommand` ("Jacking out…"), `TalkCommand` ("Talk to whom?", end-conversation, goodbye, dialogue prompt `"> "`), `DropCommand` (drop-what), `ExamineCommand` (examine-what), `InventoryCommand` (empty/header).
- **Grouping**: `Defaults`, `Prompts`, `Drone`, `Win`, `Lose`, `Quit`, `Go`, `Take`, `Use`, `Look`, `Help`, `Drop`, `Examine`, `Inventory`, `Talk`.
- **All 227 tests pass** after the refactor — pure behaviour-preserving change.
- **Pattern noted**: multi-line prose previously written as multiple consecutive `_io.WriteLine` calls — consolidated into single const strings using `\n`, then split via the existing `SplitLines` helper. Consistent with how JSON world win/lose messages are already handled.

### Session 12 — FindItem Deduplication (Issue #33)

- **`GameStateExtensions.cs` created in `MyGame.Engine`**: Three public extension methods on `GameState` — `FindItem(noun)` (room then inventory), `FindRoomItem(noun)` (room only), `FindInventoryItem(noun)` (inventory only). All share a private `MatchesNoun` predicate (exact ID or partial name, case-insensitive).
- **Four commands refactored**: `TakeCommand` → `FindRoomItem`, `DropCommand` → `FindInventoryItem`, `ExamineCommand` → `FindItem` (private static helper removed), `UseCommand` → `FindInventoryItem`. Search semantics preserved exactly.
- **All 227 tests pass** unchanged.
- **Git commit prepared**: .squad/ changes staged and committed with CoAuthor trailer

### Session 14 — Issue #32 Parser.cs Investigation

- **Parser.cs NOT deleted**: Investigation found 6 callers in `src/MyGame.Tests/ParserTests.cs` — River's tests use `new Parser()` as the public API surface for `ParsedCommand.Target` field coverage ("use X on Y" syntax).
- **Parser.cs IS a dead wrapper in production**: `GameEngine.cs` calls `CommandParser.Parse()` directly; `Parser.cs` just delegates to the same. But the test contract depends on it.
- **Decision**: Per task instructions, did not delete — reported finding instead. Issue #32 cannot be resolved without migrating ParserTests.cs to use `CommandParser` directly first.

### Session 14 — Remove savegame.json from Git Tracking (Issue #41)

- **`src/MyGame/savegame.json` removed from tracking**: Used `git rm --cached` to untrack the runtime save file without deleting it from disk. File still exists locally for game use.
- **`.gitignore` updated**: Added `savegame.json` under a new `# Runtime artifacts` section. Entry is top-level (no `**/` prefix) because `savegame.json` is written to the working directory and not namespaced.
- **No other save-related artifacts found**: No `*.save` or `*.sav` files present. No additional entries needed.
- **Committed on current branch** (`squad/46-viktor-met-flag`) with no new branch created.

### Session 16 — Issue #41: Complete savegame.json removal (PR #63)

- **Previous session's work already in origin/main**: `savegame.json` was untracked and added to `.gitignore` in session 14, but only `savegame.json` was listed — `*.save.json` was omitted.
- **`*.save.json` glob added**: Covers any future save-file naming variants (e.g. `slot1.save.json`). Section renamed from `# Runtime artifacts` to `# Save files` for specificity.
- **Branch**: `squad/41-remove-savegame` — first time proper branch was created for this issue.
- **Build confirmed**: `dotnet build` passes (0 errors, 0 warnings) on net10.0.
- **PR #63 opened**: `fix: remove savegame.json and add save file patterns to .gitignore`. Closes #41.

### Session 15 — Issue #31: Remove Duplicate FindItem from LookCommand

- **Change**: Replaced the call to `LookCommand`'s private `FindItem(noun, state)` with `state.FindItem(noun)` (the shared `GameStateExtensions` extension method), then deleted the private method (8 lines removed).
- **`using MyGame.Models;` retained**: `Room` is still referenced by the `DescribeRoom` overloads — the import was not unused.
- **Pattern**: When `GameStateExtensions.FindItem()` was introduced (Issue #33), `ExamineCommand` was migrated but `LookCommand` was missed. Whenever a shared utility is added to replace duplicates, grep all command files for the old pattern to ensure full coverage in the same PR.
- **PR**: #62 on DarrenPratt/MyGame. 227 tests pass unchanged.

### Session 13 — Parallel Refactoring Work (2026-03-10T19:25:00Z)

- **Issue #34 — GameMessages String Extraction**: Created `GameMessages.cs` (15 nested static classes, 50+ const strings) centralizing all player-facing narrative from `GameEngine.cs` and 10 command files. Consolidation pattern applied: multi-line prose from sequential `WriteLine` calls → single const with `\n` → split via `SplitLines`. Pure refactor, 227 tests pass.
- **Issue #33 — FindItem Extension Methods**: Completed same session — `GameStateExtensions.cs` with three scoped methods sharing one `MatchesNoun` predicate. Eliminates duplication across `TakeCommand`, `DropCommand`, `ExamineCommand`, `UseCommand`. 227 tests pass.
- **Rogue parallel work (Issue #36)**: Added 19 narrator variants across 10 rooms (return-visit variants on all, progression variants for keycard_used/cred_chip_obtained on 4 rooms, item-possession variants on 2 rooms). World now dynamic and reactive to player state.
- **All work on branch squad/46-viktor-met-flag**: Three agents working in parallel, orchestration logs and session summary documented in .squad/. 227 tests passing, ready for merge.
### Session 16 — Issue #42: TakeMessage Field (2026-03-10)

- **Root cause confirmed**: `TakeCommand` had `if (item.Id == "data_chip")` — ID that doesn't exist in neon-ledger.json (real ID is `drive`). Dead code and design inconsistency.
- **`Item.cs`**: Added `public string? TakeMessage { get; init; }` alongside existing `UseMessage`. Simple nullable string, no required, defaults to null.
- **`TakeCommand.cs`**: Replaced hardcoded check with `if (item.TakeMessage is not null) io.WriteLine(ColorConsole.Flavor(item.TakeMessage))`. Generic fallback unchanged. `Flavor()` wrapping is consistent with how UseMessage content is displayed.
- **`GameMessages.cs`**: Removed dead `DataChipPickup` const — no callers after the TakeCommand fix.
- **`neon-ledger.json`**: Added `takeMessage` to `drive` item: "Your hand trembles as you pocket the drive. Years of work, dead contacts, all leading to this moment." (adapted from the original dead-code message, corrected "chip" → "drive").
- **JSON deserialization**: `JsonWorldLoader` uses `PropertyNameCaseInsensitive = true` — camelCase `takeMessage` in JSON maps to `TakeMessage` on `Item` automatically. No loader changes needed.
- **PR**: #64 on DarrenPratt/MyGame. All 227 tests pass.
- **Pattern**: Any item pick-up flavor text belongs in the JSON `takeMessage` field, not in command code. Content designers can now customize per-item without touching C#.

- **2026-03-10 — River validated restart feature:** 6 new TryAgainTests.cs tests cover all restart branches and backward compat. Banner line count detects session restart cleanly. All 205 tests passing.
- **2026-03-10 — Johnny completed codebase review:** Filed 14 improvement issues across squad. 5 assigned to Judy: #31 (duplicate FindItem), #39 (Parser.cs wrapper), #42 (TakeCommand hardcoded text), #47 (Save/Load DroneThreatLevel P1), #52 (TalkCommand flags P1), #55 (GoCommand hardcoded win logic P1). Critical issues identified in save/load and game state persistence.

- **2026-03-10 — River validated restart feature:** 6 new TryAgainTests.cs tests cover all restart branches and backward compat. Banner line count detects session restart cleanly. All 205 tests passing.
- **2026-03-10 — Johnny completed codebase review:** Filed 14 improvement issues across squad. 5 assigned to Judy: #31 (duplicate FindItem), #39 (Parser.cs wrapper), #42 (TakeCommand hardcoded text), #47 (Save/Load DroneThreatLevel P1), #52 (TalkCommand flags P1), #55 (GoCommand hardcoded win logic P1). Critical issues identified in save/load and game state persistence.

- **2026-03-10 — River validated restart feature:** 6 new TryAgainTests.cs tests cover all restart branches and backward compat. Banner line count detects session restart cleanly. All 205 tests passing.
- **2026-03-10 — Johnny completed codebase review:** Filed 14 improvement issues across squad. 5 assigned to Judy: #31 (duplicate FindItem), #39 (Parser.cs wrapper), #42 (TakeCommand hardcoded text), #47 (Save/Load DroneThreatLevel P1), #52 (TalkCommand flags P1), #55 (GoCommand hardcoded win logic P1). Critical issues identified in save/load and game state persistence.
- **2026-03-10 — Johnny completed codebase review:** Filed 12 improvement issues (4 for Judy, 2 for Rogue, 2 for River, 4 for Johnny). P1 save/load corruption identified. All findings documented in decisions.md.

