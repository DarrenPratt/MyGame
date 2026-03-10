# River — History

## Project Context

**Project:** MyGame — simple text adventure game in C#  
**Stack:** C#, .NET 8+, console application  
**User:** Jynx_Protocol  
**Theme:** Cyberpunk — dark future, neon-lit streets, corporate dystopia  
**Created:** 2026-03-09

## Learnings

### Session 2026-03-09 — Initial Test Suite

**Wrote comprehensive xUnit test suite** at `src/MyGame.Tests/` covering:

- **GameStateTests.cs** — 17 tests covering starting state, inventory add/remove, room changes, and flags
- **CommandTests.cs** — 40+ tests for every command (look, go, take, drop, inventory, examine, help, quit, unknown, use) including edge cases like aliases, case-insensitivity, locked exits, and graceful failure paths
- **GameWorldTests.cs** — 25 tests verifying world integrity: all rooms exist, exits are bidirectional, items are in correct rooms, locked doors have correct requirements
- **GameIntegrationTests.cs** — End-to-end tests of the full winning path, quit flow, error handling, and edge cases (empty input, unknown commands, drop/retake cycle)
- **Helpers/FakeInputOutput.cs** — Test double implementing `IInputOutput` with queued inputs and captured output
- **Helpers/WorldFactory.cs** — Factory for minimal test GameState objects (single room, two rooms, item constructors)

**Architecture observed:** Tests are written against `ARCHITECTURE.md` V1 World Map (5 rooms: alley/bar/rooftop/lobby/server, 4 items: keycard/flyer/terminal/drive). Win condition: player enters server room. Keycard unlocks lobby north exit.

**Key design decisions:**
- Tests use `FakeInputOutput` and minimal `GameState` objects — no global state, fully isolated
- `WorldFactory` helpers reduce boilerplate without coupling to `WorldBuilder`
- `examine` is tested via `LookCommand` with a noun (matching architecture spec)
- Integration tests exercise `GameEngine.Run()` with injected input sequences
- `[Theory]` + `[InlineData]` used for aliases, direction variants, and error cases

**Risk areas identified:**
- `UseCommand` behavior (UseTargetId matching exit direction vs exit id) may need adjustment once Judy's implementation is reviewed
- Direction aliases for `GoCommand` (e.g. "n" as verb with null noun) need careful handling in implementation

### Session 2026-03-10 — Anticipatory Tests for New Engine Features

**Wrote anticipatory test suite** for Judy's parallel development work. Tests ready to validate implementation when code lands.

**New test files created:**
- **ParserTests.cs** — 6 tests for new Target field in ParsedCommand, covering "use X on Y" syntax, talk command parsing, empty input edge case
- **NarratorEngineTests.cs** — 8 tests for dynamic room descriptions based on flags/inventory, variant matching logic, specificity rules, partial condition handling
- **TalkCommandTests.cs** — 8 tests for NPC dialogue system, "to" prefix stripping, dialogue navigation with user input, NPC lookup by ID/name
- **SaveLoadTests.cs** — 8 tests for game persistence (save/load commands), file handling, state restoration (room, flags, inventory), error handling for missing/corrupt files
- **JsonWorldLoaderTests.cs** — 10 tests for JSON world loading, room/exit wiring, item/NPC placement, catalog population, narrator variant deserialization
- **GameStateTests.cs** — Added 3 tests for new fields (ItemCatalog, NpcCatalog, WinRoomId)

**Test patterns established:**
- Anticipatory tests written against spec, not implementation — tests compile once features land
- Used `IDisposable` pattern in SaveLoad/JsonLoader tests for temp file cleanup
- FakeInputOutput's queue pattern supports multi-step interactive commands (dialogue choices)
- Test helpers (WorldFactory, CreateTestNpc) keep tests focused and readable

**Edge cases identified:**
- Parser: "on" keyword splits target from noun; "talk to X" doesn't strip "to" (TalkCommand's job)
- NarratorEngine: Multiple matching variants choose most specific (most conditions); empty requirements = always match
- TalkCommand: Must handle both ID and Name lookups, "to" prefix optional
- SaveLoad: Inventory restored from ItemCatalog (items reconstructed by ID), not serialized directly
- JsonWorldLoader: NarratorVariants, NPCs, and catalog population critical for dynamic content

**Risk areas for implementation:**
- SaveCommand/LoadCommand need save directory configuration (constructor injection pattern used in tests)
- Dialogue system needs robust input validation for choice selection
- NarratorEngine specificity algorithm: more conditions = more specific (count RequiredFlags + RequiredInventoryItems)

### Session 2026-03-10 — PR #20 Review: ColorConsole Themed ANSI Output

**Reviewed PR #20** — "Implement ColorConsole — themed ANSI output" (closes #3)
### Session 2026-03-10 — PR #21 Review: NarratorEngine Atmospheric Variants

**Reviewed PR #21** — "feat: complete NarratorEngine — atmospheric variant output (#4)"

**Verdict: APPROVED** (submitted as review comment — GitHub prevents self-approval)

**What was reviewed:**
- `src/MyGame/Engine/ColorConsole.cs` — new semantic methods (`RoomDescription`, `Error`, `Prompt`, `Flavor`) + Windows P/Invoke `Initialize()` method
- `src/MyGame/Commands/ExamineCommand.cs`, `GoCommand.cs`, `LookCommand.cs`, `UseCommand.cs` — error paths wrapped in `ColorConsole.Error()`
- `src/MyGame/Engine/GameEngine.cs` — prompt wrapped in `ColorConsole.Prompt()`
- `src/MyGame/Program.cs` — `Initialize()` added as first call

**Key findings:**
- All 164 tests pass — ANSI codes are transparent to `FakeInputOutput.OutputContains` substring assertions by design
- ANSI codes match `decisions.md` exactly: `\x1b[96m` RoomDescription, `\x1b[31m` Error, `\x1b[2;36m` Prompt, `\x1b[35m` Flavor
- Windows P/Invoke guards are complete: null handle, `INVALID_HANDLE_VALUE`, `GetConsoleMode` failure, OS platform check
- Silent ignore of `SetConsoleMode` failure is correct (graceful degradation)
- `Flavor` method defined but not yet called — palette method for future use, not a blocker
- No dedicated ColorConsole unit tests needed — simple string formatters, covered by transparent ANSI behavior
- `src/MyGame/Engine/NarratorEngine.cs` — new `GetVariant()` method returning `NarratorVariant?`
- `src/MyGame/Commands/LookCommand.cs` — uses `Flavor()` for variants, `RoomDescription()` for base
- `src/MyGame/Content/worlds/neon-ledger.json` — bar room gets unconditional atmospheric variant
- `src/MyGame.Tests/NarratorEngineTests.cs` — 2 new tests for GetVariant() behavior

**Key findings:**
- All 166 tests pass (164 existing + 2 new)
- `GetVariant()` cleanly extracts variant-matching logic, returns null when no match
- LookCommand correctly branches on variant vs base for color rendering
- Empty requirements `[]` in JSON = always matches (score 0) — pattern works as designed
- Higher-specificity variants (e.g. viktor_met with score 1) correctly override atmospheric
- Atmospheric text is appropriately cyberpunk: neon, chrome bartender, mysterious terminal message

**Pattern observed:** The empty-requirements variant as "default atmospheric" is now an established content pattern. Any room can use this to avoid showing static base descriptions while still allowing flag-conditional overrides.

### Session 2026-03-10 — TryAgainTests for Issue #29

**Wrote 6 new tests** in `src/MyGame.Tests/TryAgainTests.cs` validating Judy's restart/retry feature.

**Tests added:**
- `Death_WithFactory_ShowsTryAgainPrompt` — factory present + death → "try again" appears in output
- `Death_WithFactory_AnswerNo_ExitsCleanly` — "no" → IsRunning false, HasLost true
- `Death_WithFactory_AnswerYes_RestartsGame` — "yes" → banner appears twice (two full sessions)
- `Death_WithoutFactory_NoTryAgainPrompt` — no factory → no "try again" (backward compat)
- `Win_DoesNotShowTryAgainPrompt` — win path + factory → prompt never shown
- `Quit_DoesNotShowTryAgainPrompt` — voluntary quit + factory → prompt never shown

**Patterns for restart/retry testing:**
- `DeathInputs` static array shared across tests keeps death trigger DRY
- Named parameter `stateFactory: factory` skips `world` (null) cleanly
- Banner line count (counting "╔") detects two sessions without needing access to the restarted `GameState` object
- `FakeInputOutput` collection expression spread `[.. DeathInputs, "no"]` composes input sequences cleanly
- All 205 tests pass (199 baseline + 6 new)

**Findings:**
- Judy's implementation already landed and compiles; Program.cs already passes factory
- `Run()` retry loop only prompts when `HasLost && _stateFactory != null` — clean gate
- Quit and win both fall through to `break` without prompting — no edge cases needed beyond the 6 tests

## Team Updates

- **2026-03-10 — Judy completed Try-Again feature:** Factory pattern cleanly separates state creation (Program.cs) from execution (GameEngine). Backward compatible. 205 tests passing.

## Team Updates

- **2026-03-09 — Johnny's architecture delivered:** IInputOutput abstraction made your test strategy possible. CommandRegistry pattern gave you clean test points. Your 114-test suite locked in the authoritative spec for the team.
- **2026-03-09 — Rogue completed content design:** 9 rooms, 8 items, rich narrative. Your test suite validated all content flows and mechanics. Flavor text integrates seamlessly with tested command behavior.
- **2026-03-09 — Judy implemented full game:** All 114 tests passing. Implementation matched your spec exactly. Your tests caught win condition ordering issue (lock check before win check) — correctly handled by Judy. Game complete and fully validated.
- **2026-03-10 — Johnny filed improvement issues:** #38 (ExamineCommand tests), #40 (integration test helper registration). 205 tests passing. Room for test coverage expansion.
- **2026-03-10 — Johnny completed codebase review:** Filed 14 improvement issues across squad. 3 assigned to River: #38 (ExamineCommand test coverage), #40 (integration test helper registration), #53 (LoadedWorld test coverage), #43 (NarratorEngine edge cases). Test coverage backlog identified; anticipatory test patterns from Session 2 provide foundation for implementation.

### Session 2026-03-10 — Issue #35: Save/Load State Corruption Tests

**Wrote 6 new tests** in `src/MyGame.Tests/SaveLoadTests.cs` for Judy's save/load corruption fix.

**Tests added (all 13 SaveLoad tests pass — 6 pre-existing + 7 new):**

| Test | What it guards |
|---|---|
| `SaveLoad_PreservesDroneThreatLevel` | DroneThreatLevel=3 survives save/load (player can't reset threat by reloading) |
| `SaveLoad_PreservesDroneThreatThreshold` | Non-default threshold (6) restored after load |
| `SaveLoad_PreservesUnlockedExit` | Unlocked exit stays unlocked after reload |
| `SaveLoad_DroneThreatZeroByDefault_NotCorrupted` | Zero threat value not silently dropped during round-trip |
| `SaveLoad_NonZeroThreatSurvivesRoundtrip` | High threat (5) survives round-trip; no exploit via save/load |
| `SaveLoad_LockedExitRemainsLockedAfterReload` | Locked exit baseline — still locked after reload (regression guard) |

**Architecture confirmed (from Judy's fix commit `70a62b2`):**
- `DroneThreatThreshold` changed from `init` to `set` in GameState to allow LoadCommand to restore it
- `SaveData` record extended with `DroneThreatLevel`, `DroneThreatThreshold`, `ExitLockStates` (roomId → direction → isLocked)
- Old save files missing new fields gracefully default to 0/0/null (backward compatible)

**Test design patterns used:**
- `TwoRoomStateWithLockedExit()` private helper in test class creates minimal locked-exit world for exit tests
- Each test uses the class-scoped `_testDirectory` (unique per `IDisposable` instance) — no file collisions
- Separate filename per test (e.g. `"threat-level"`, `"unlock-exit"`) prevents cross-test file interference
- Tests prove Judy's fix correct: all previously failing assertions now pass against the patched code


### Session 2026-03-10 — Issue #38: ExamineCommand Test Coverage

**Wrote 16 new tests** in `src/MyGame.Tests/ExamineCommandTests.cs` — dedicated coverage for `ExamineCommand`.

**What ExamineCommand does:**
- Searches `state.CurrentRoom.Items` then `state.Inventory` (via `Concat`) for an item matching by `Id` (exact, case-insensitive) or `Name.Contains` (partial, case-insensitive)
- If `command.Noun` is null → prints `ColorConsole.Error("Examine what?")`
- If item found → prints `item.Description`
- If not found → prints `ColorConsole.Error("You don't see any \"{noun}\" to examine.")`
- **Does NOT search room NPCs** — examining an NPC by name returns the not-found error
- Verb: `"examine"`; Aliases: `["x", "inspect", "read"]`

**Tests written:**
- `Verb_IsExamine` — verb registration
- `Aliases_ContainExpectedShortcuts` — Theory for x/inspect/read
- `Execute_NoNoun_ShowsUsageError` — null noun → "Examine what?"
- `Execute_NoNoun_DoesNotCrash_AndWritesOutput` — always writes at least one line
- `Execute_ItemInRoom_ById_ShowsItemDescription` — exact ID match in room
- `Execute_ItemInRoom_ByName_ShowsItemDescription` — partial Name.Contains match
- `Execute_ItemInRoom_ByName_IsCaseInsensitive` — UPPERCASE ID still finds item
- `Execute_ItemInInventory_ShowsItemDescription` — item in inventory found by ID
- `Execute_ItemInInventory_ByName_ShowsItemDescription` — inventory item found by partial name
- `Execute_ItemNotInRoomOrInventory_ShowsNotFoundError` — error contains the noun
- `Execute_ItemNotFound_DoesNotPrintAnyDescription` — other items' descriptions not leaked
- `Execute_SameIdInRoomAndInventory_ReturnsRoomItemFirst` — room searched before inventory (Concat order)
- `Execute_NpcInRoom_ButNoMatchingItem_ShowsNotFoundError` — NPCs not examined, error returned
- `Execute_DoesNotMutateInventoryOrRoom` — examine is read-only (no side effects)

**Total tests:** 227 (205 existing + 22 new)

**Edge cases identified:**
- NPC in room is not findable via examine (only items) — examining Viktor returns not-found error
- Room item takes priority over identical inventory item (Concat order: room first)
- Case-insensitive search covers both ID and Name lookups
- Partial name match (`Name.Contains`) means "corp" finds "Corp Keycard"

## Session 11 — Scribe Orchestration (2026-03-10T19:15:00Z)

- **Orchestration logs created**: Documented Judy's viktor_met flag fix (Issue #46) and your ExamineCommand test coverage (Issue #38)
- **Session log written**: Comprehensive summary of parallel work; 227 tests passing
- **Decisions merged**: All inbox files (judy-viktor-met-flag, judy-save-load-fix, river-examine-tests, river-save-load-tests) merged into decisions.md with full details and deduplicated
- **Inbox cleaned**: All merged files deleted from .squad/decisions/inbox/
- **History updated**: Judy and River histories appended with Session 11 and earlier summaries
- **Git commit prepared**: .squad/ orchestration, logs, and updated decisions committed

### Session — Issue #32: Delete Parser.cs (unblock Judy)

**Rewrote ParserTests.cs** to call `CommandParser.Parse()` directly, then deleted `Parser.cs`.

**What Parser.cs was:** A 6-line instance-method wrapper — `public ParsedCommand Parse(string input) => CommandParser.Parse(input);`. No added logic; pure pass-through.

**What replaced it in tests:** `CommandParser.Parse(input)` called as a static method directly. All 6 tests kept identical assertions; only `new Parser()` removed.

**How CommandParser works:** Static class in `MyGame.Engine`. `Parse(string input)` trims/lowercases, splits on first space for Verb + rest, then scans rest for `" on "` to populate `Target`. Returns `ParsedCommand(Verb, Noun?, Target?)` record.

All 227 tests pass. `Parser.cs` is deleted. Decision inbox: `.squad/decisions/inbox/river-parser-cleanup.md`.

## Team Updates

- **2026-03-10 — Judy fixed viktor_met flag:**Generic flag-setting in TalkCommand now sets `{npc.Id}_met` for all NPCs. 227 tests passing (including your 16 new ExamineCommand tests unexpectedly committed).
- **2026-03-10 — River completed ExamineCommand test coverage:** 16 comprehensive tests covering all behaviors, priorities, and boundaries. Dedicated test file confirms ExamineCommand is standalone command (not noun on LookCommand).
- **2026-03-10 — River validated save/load state corruption fix:** 6 new SaveLoadTests validate Judy's persistence of DroneThreatLevel, DroneThreatThreshold, and exit lock states.

### Session 2026-03-10 — Issue #32: Delete Parser.cs (unblock Judy)

**Rewrote ParserTests.cs** to call `CommandParser.Parse()` directly, then deleted `Parser.cs`.

**What Parser.cs was:** A 6-line instance-method wrapper — `public ParsedCommand Parse(string input) => CommandParser.Parse(input);`. No added logic; pure pass-through.

**What replaced it in tests:** `CommandParser.Parse(input)` called as a static method directly. All 6 tests kept identical assertions; only `new Parser()` removed.

**How CommandParser works:** Static class in `MyGame.Engine`. `Parse(string input)` trims/lowercases, splits on first space for Verb + rest, then scans rest for `" on "` to populate `Target`. Returns `ParsedCommand(Verb, Noun?, Target?)` record.

All 227 tests pass. `Parser.cs` is deleted. Decision inbox: `.squad/decisions/inbox/river-parser-cleanup.md`.


## Learnings

### Session 2026-03-10 — Issue #43: NarratorEngine Edge Case Coverage

**Added 9 edge case tests** to `src/MyGame.Tests/NarratorEngineTests.cs` (file now has 19 tests total). PR #68 opened against main.

**New tests added:**

| Test | Edge case covered |
|---|---|
| `GetVariant_NoVariants_ReturnsNull` | Direct null return from `GetVariant` with empty list |
| `GetVariant_VariantPresent_RequiredFlagNotSet_ReturnsNull` | Direct null return when no variant matches |
| `GetDescription_InventoryOnlyVariant_ItemPresent_ReturnsVariantDescription` | Inventory-only variant activates when item carried |
| `GetDescription_InventoryOnlyVariant_ItemAbsent_ReturnsBaseDescription` | Missing inventory item → base description |
| `GetDescription_InventoryVariant_MatchedByItemId_NotByItemName` | Matching uses item.Id, not item.Name |
| `GetDescription_InventoryVariant_ItemWithMatchingName_ButWrongId_ReturnsBase` | Name match alone is not sufficient |
| `GetDescription_HigherSpecificityNotMatched_LowerSpecificityMatched_ReturnsLower` | Falls back to lower-specificity variant when best is unmet |
| `GetDescription_VariantRequiresThreeFlags_AllPresent_ReturnsVariantDescription` | 3-flag gate all satisfied |
| `GetDescription_TwoMatchingVariantsSameScore_FirstInListReturned` | Tied specificity → first in list wins (stable LINQ sort) |

**Key findings from reading NarratorEngine.cs:**
- `GetVariant` is only 13 lines — LINQ Where + OrderByDescending + FirstOrDefault
- Inventory match is `state.Inventory.Any(item => item.Id == id)` — strictly by Id, not Name
- Score = `RequiredFlags.Count + RequiredInventoryItems.Count`
- `GetDescription` null-coalesces: `GetVariant(room, state)?.Description ?? room.Description`

**Gaps that existed in prior tests:**
- `GetVariant` null return was only tested indirectly through `GetDescription`
- No test for the high-specificity-unmet/lower-specificity-matched fallback scenario
- Id vs Name distinction for inventory matching was entirely untested

**Test count:** 227 baseline → 236 after this session (all passing).
