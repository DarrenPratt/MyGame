# Decisions — MyGame Project

**Last Updated:** 2026-03-09  
**Project:** MyGame (Neon Ledger)

---

## Architecture Decisions — Johnny (Lead & Architect)

**Date:** 2025-07-14  
**Status:** Approved  
**Scope:** Full game architecture for v1

### 1. Hardcoded content via WorldBuilder (not data-driven)

**Chose:** A static `WorldBuilder` class that constructs rooms, items, and exits in code.  
**Alternatives considered:** JSON/YAML data files, a DSL, or a database.  
**Why:** For v1 with ~5 rooms and ~4 items, data files add complexity (parsing, validation, file loading) with no real benefit. Hardcoded content is debuggable, type-safe, and instantly testable. We can extract to data files in v2 if the world grows.

### 2. Command pattern with registry (not switch/case)

**Chose:** `ICommand` interface + `CommandRegistry` dictionary dispatch.  
**Why:** Adding a new command is one class + one line of registration. No giant switch statement to maintain. Each command is independently testable. Aliases are trivial to add.

### 3. IInputOutput abstraction (not direct Console)

**Chose:** Inject an `IInputOutput` interface into GameEngine and all commands.  
**Why:** Makes the entire game testable without Console. Tests provide a stub that feeds canned input and captures output. This is the single most important testability decision.

### 4. Flat flags for game state (not an event system)

**Chose:** `HashSet<string>` flags on GameState (e.g., "server_unlocked").  
**Why:** A handful of boolean conditions is all v1 needs. An event/trigger system is overkill. Flags are inspectable, serializable, and dead simple to test.

### 5. Win condition is room-based

**Chose:** Entering the `server` room triggers victory.  
**Why:** Simple, clear, easy to test. The locked door provides the puzzle gate. No complex quest tracking needed for v1.

### 6. Project structure: src/ and tests/ separation

**Chose:** `src/MyGame/` for the game, `tests/MyGame.Tests/` for xUnit tests.  
**Why:** Standard .NET convention. Clean separation. River (test author) has a dedicated project to work in.

### v1 Scope

- 5 rooms with a connected map
- 4 items (1 key item, rest are flavor)
- 8 commands: look, go, take, drop, inventory, use, help, quit
- 1 locked door puzzle (keycard unlocks server room)
- 1 win condition (reach server room)
- Full test coverage via xUnit
- Cyberpunk theme in all descriptions

### Out of Scope for v1

- Save/load game
- NPC dialogue trees
- Combat system
- Data-driven content (JSON/YAML)
- Multiple endings
- Score tracking
- Sound or color/ANSI formatting

---

## Content Design Decisions — Rogue (Content Designer)

**Date:** 2026-03-09  
**Status:** Complete

### 1. Narrative Structure

**Decision:** Linear story arc with optional exploration.
- **Why:** Keeps gameplay focused while allowing player agency through different routes (Street → Tech Den vs. Street → Undercity).
- **Impact:** 9 rooms with clear progression from Flux Bar → The Lattice → Escape Platform.

### 2. Objective System

**Decision:** Single clear win condition (retrieve data chip + escape).
- **Why:** Creates urgency and narrative stakes. Player must balance gathering clues with avoiding time/drone escalation.
- **Impact:** Motivates exploration but prevents indefinite wandering.

### 3. Item Design

**Decision:** 8 items total—half essential (maze_key, data_chip, access_code), half utility/optional (repair_kit, neon_jack, grappling_hook).
- **Why:** Gives players meaningful choices. Some items accelerate victory; others provide safety nets.
- **Impact:** Replayability—different routes can use different items.

### 4. Tone & Atmosphere

**Decision:** Cyberpunk gritty + dark humor, NOT grimdark or hopeless.
- **Why:** Makes NightCity feel lived-in and dangerous but allows moments of victory to feel earned and meaningful.
- **Impact:** Text focuses on environmental detail, not suffering. Win text emphasizes player agency.

### 5. Room Interconnectivity

**Decision:** Multiple paths to The Lattice (direct via access_code, or via Undercity + maze_key).
- **Why:** Players can choose risk vs. safety (corporate areas are faster but watched; Undercity is slower but safe).
- **Impact:** Multiple valid strategies, replayable exploration.

### 6. Time Pressure Mechanism

**Decision:** Implicit, text-based (drones appear more frequently in certain zones, no explicit turn counter).
- **Why:** Builds tension without mechanical overhead. Fits narrative (corporate response scales).
- **Impact:** Encourages brisk gameplay while respecting narrative pacing.

---

## Testing Decisions — River (Tester)

**Date:** 2026-03-09  
**Status:** Complete

### Decision 1: Test project location

**Decision:** Place test project at `src/MyGame.Tests/` (not `tests/MyGame.Tests/` as ARCHITECTURE.md shows).  
**Reason:** Jynx_Protocol explicitly specified `src/MyGame.Tests/`. The `.csproj` ProjectReference path is `../MyGame/MyGame.csproj`.

### Decision 2: FakeInputOutput captures both Write and WriteLine

**Decision:** Both `Write()` and `WriteLine()` calls are captured in the same `Lines` list in `FakeInputOutput`.  
**Reason:** The game engine uses `Write()` for the prompt (`"> "`) and `WriteLine()` for content. Tests that check `OutputContains()` should see both without needing to distinguish.

### Decision 3: Examine is tested via LookCommand with noun

**Decision:** There is no separate `ExamineCommand` in the architecture. Examine functionality is provided by `LookCommand` when called with a noun argument (e.g., `look terminal`).  
**Reason:** ARCHITECTURE.md LookCommand spec: "If noun matches an item (in room or inventory): show item description." Tests reflect this behavior.

### Decision 4: WorldFactory for isolated unit tests, WorldBuilder for integration

**Decision:** Command tests (`CommandTests.cs`) use `WorldFactory` helpers to build minimal, controlled `GameState` objects. World/integration tests use `WorldBuilder.Build()` directly.  
**Reason:** Unit tests should not depend on WorldBuilder correctness. Integration tests intentionally validate the full stack.

### Decision 5: Direction alias tests for GoCommand use verb-as-direction pattern

**Decision:** When testing direction shortcuts like "n", "s", "e", "w", tests pass the alias as the *verb* with a null noun — matching the ARCHITECTURE.md spec: `Aliases => ["north", "south", ..., "n", "s", ...]`.  
**Reason:** GoCommand supports using direction words directly as commands (e.g., user types "north" not "go north").

### Decision 6: Win condition checked via state, not output parsing

**Decision:** Integration tests assert `state.HasWon == true` and `state.CurrentRoomId == "server"` rather than parsing win message strings.  
**Reason:** More robust — output text may change, but the state contract should not.

### Decision 7: UseCommand test assumes UseTargetId matches exit direction

**Decision:** The `UseCommand` unit test sets `UseTargetId = "north"` on the keycard item, assuming the command matches item's `UseTargetId` against exit direction keys in the current room.  
**Reason:** This is the most logical interpretation of the architecture spec: "UseTargetId — What this item interacts with (exit id, item id, etc.)" combined with the dispatch example showing keycard unlocking the "north" exit.  
**Risk:** If Judy implements `UseTargetId` as matching room IDs instead of direction keys, this test will need updating.

---

## Implementation Decisions — Judy (C# Developer)

**Date:** 2026-03-09  
**Status:** Complete

### Decision 1: World Map Follows ARCHITECTURE.md (Not CONTENT.md)

**Context:** ARCHITECTURE.md defines a 5-room V1 world (alley, bar, rooftop, lobby, server). CONTENT.md defines a richer 9-room cyberpunk world. River's pre-existing tests lock in the ARCHITECTURE.md room IDs.

**Decision:** Implement the ARCHITECTURE.md world map with room IDs (alley, bar, rooftop, lobby, server). Use CONTENT.md for flavor text, descriptions, and narrative tone — not for structure.

**Rationale:** Pre-existing test files are the authoritative spec. Changing room IDs would break 114 tests.

### Decision 2: Win Condition via "server" Room ID (Hardcoded)

**Context:** ARCHITECTURE.md says "The GoCommand checks: if the destination is server, set HasWon = true and IsRunning = false."

**Decision:** GoCommand hardcodes a check for `CurrentRoomId == "server"` AFTER moving the player and AFTER the locked-exit check. The server room is a real room in the world (not a sentinel value).

**Rationale:** Tests expect `state.CurrentRoomId == "server"` and `state.HasWon == true` simultaneously. The lock check must precede the win check so the keycard requirement is enforced.

### Decision 3: UseCommand Unlocks Exits via RequiredItemId Matching

**Context:** Items need to be able to unlock exits. Two approaches: (a) item.UseTargetId matches the exit direction key, or (b) UseCommand searches current room exits for `RequiredItemId == item.Id`.

**Decision:** UseCommand searches current room's exits for any locked exit where `RequiredItemId == item.Id`. This does not require UseTargetId to be set on key items. UseTargetId is reserved for flag-based side effects (informational items).

**Rationale:** Cleaner separation — the Exit knows what it needs, the item just needs to match. Avoids duplicating information between Item and Exit.

### Decision 4: Project Structure Matches Task Spec (src/ layout)

**Context:** Task specified `src\MyGame` and `src\MyGame.Tests`. ARCHITECTURE.md specifies `tests\MyGame.Tests`.

**Decision:** Both projects live under `src\`. The test project reference uses `<ProjectReference Include="..\MyGame\MyGame.csproj" />`.

**Rationale:** Task spec takes precedence over architecture doc for project layout. The architecture doc's content (classes, interfaces) was followed faithfully.

### Decision 5: ExamineCommand Added Beyond Architecture Spec

**Context:** Task requested `examine` command but ARCHITECTURE.md does not list ExamineCommand as a file.

**Decision:** Added `ExamineCommand.cs` with aliases `x`, `inspect`, `read`. Shows item description, searches both room items and inventory.

**Rationale:** The task explicitly requested it. Implemented as additive extension — does not break existing tests.

---

## World Architecture Decisions — Judy (C# Developer)

**Date:** 2026-03-10  
**Status:** Complete  
**PR:** #23

### Decision: WorldBuilder Removed — JSON is the Sole World Source

**Background:** `WorldBuilder.cs` was a fallback in `Program.cs` for when `neon-ledger.json` didn't exist. However, the JSON file is committed to the repo and always loaded, making `WorldBuilder.Build()` unreachable in practice.

**Decision:** Delete `WorldBuilder.cs` and make the JSON world file required.

**Rationale:** Eliminates maintenance burden of keeping two world sources in sync. Failing fast with `FileNotFoundException` is clearer than silently loading a fallback that can drift from the authoritative JSON.

**Changes:**
- Deleted `src/MyGame/Content/WorldBuilder.cs`
- `Program.cs`: replaced if/else logic with explicit file-not-found guard
- `GameWorldTests` and `GameIntegrationTests`: migrated to `JsonWorldLoader`, paths now match actual JSON map

**Impact:**
- `MyGame.Content` namespace no longer exists
- All 168 tests pass against JSON world
- JSON world map has two different connections than old WorldBuilder fallback: `bar→east→plaza` (was `lobby`) and `lobby→west→corridor` (was `bar`)
- Winning path requires both `keycard` and `cred_chip`

---

## Codebase Review Findings — Johnny (Lead & Architect)

**Date:** 2026-03-10  
**Status:** Complete  
**Scope:** Full architectural audit of v0.2 codebase

### Critical Issues (P1)

**Save/Load Corruption (#35)**
- **Finding:** Save/Load commands persist only room, inventory, and flags. DroneThreatLevel and unlocked exit state are lost.
- **Impact:** Mid-game saves are silently corrupted. Reloading loses all drone threat progress and puzzle-solving effort.
- **Action:** Judy to extend SaveCommand/LoadCommand to include exit state and threat level persistence.

### Significant Issues (P2)

**TalkCommand Dialogue Doesn't Set Flags (#46)**
- **Finding:** Dialogue trees in Npcs run but never mutate game state. Narrator variants keyed on `viktor_met` and `guard_bribed` cannot trigger.
- **Action:** Judy to add flag-setting capability to dialogue choice system.

**Hardcoded Narrative in Engine Code (#34)**
- **Finding:** GoCommand and TakeCommand contain magic strings that bypass JSON content pipeline.
- **Action:** Judy to extract hardcoded text to data-driven approach (JSON metadata or command properties).

**Integration Tests Missing 4 Commands (#40)**
- **Finding:** Test helpers register 4 core commands but not examine, talk, save, load.
- **Action:** River to add missing command registration to test helpers.

**Six Rooms Lack Narrator Variants (#36)**
- **Finding:** Only alley, bar, plaza have narrator variants. World feels static on revisit.
- **Action:** Rogue to add atmospheric or contextual variants to rooftop, lobby, server, tunnel, checkpoint, corridor.

**repair_kit and corridor Are Dead Content (#37)**
- **Finding:** repair_kit has no use case. Corridor is a dead-end with no purpose or reward.
- **Action:** Rogue to design repair_kit use-case (NPC reward, puzzle unlock) and flesh out corridor (hidden exit, lore item).

**ExamineCommand Untested (#38)**
- **Finding:** Only command without dedicated test coverage. Edge cases (non-existent items, inventory vs. room) may have gaps.
- **Action:** River to write comprehensive ExamineCommand unit tests.

### Housekeeping Issues (P3)

**Dead Parser.cs Wrapper (#32)**
- **Finding:** Parser.cs delegates entirely to CommandParser with no added value.
- **Action:** Judy to remove class and update imports.

---

## Code Organization Decisions — Judy (C# Developer)

**Date:** 2026-03-10  
**Status:** Approved  
**Scope:** String centralization and item lookup deduplication

### 1. Static const class for narrative strings (Issue #34)

**Context:** `GameEngine.cs` and ten command files contained hardcoded player-facing string literals scattered throughout—death messages, win/lose banners, drone warnings, command errors, and UI prompts. Difficult to find, update consistently, or reference from tests.

**Decision:** Extract all player-facing narrative and UI strings into a single static class `GameMessages` (`src/MyGame/Engine/GameMessages.cs`), using `public const string` members grouped by context in nested static classes.

**Structure:**
- `GameMessages.Defaults` — title, subtitle, intro text
- `GameMessages.Prompts` — command input prompt, dialogue prompt, try-again prompt
- `GameMessages.Drone` — warning messages at threat levels 1–3
- `GameMessages.Win` — server-room entry lines, default win narrative, win banner
- `GameMessages.Lose` — default lose narrative, lose banner
- `GameMessages.Quit` — quit message, quit banner
- `GameMessages.Go`, `Take`, `Use`, `Look`, `Help`, `Drop`, `Examine`, `Inventory`, `Talk` — command-specific strings

**Rationale:**
- Single place to find and update any player-facing string—no grep needed
- Simple: one file, const strings, no localization framework needed
- Pure refactor—zero behaviour change, all 227 tests pass unchanged
- Consolidation pattern: multi-line prose previously written as sequential `_io.WriteLine` calls is now a single const with `\n` delimiters, split via existing `SplitLines` helper. Matches existing JSON world message pattern.

**Impact:**
- Maintainability improved: all UI text centralized
- Consistency guaranteed: shared messaging across game engine and all commands
- Testability unchanged: ANSI codes in output remain transparent to tests

### 2. GameState Extension Methods for Item Lookup (Issue #33)

**Context:** Four command classes (`TakeCommand`, `DropCommand`, `ExamineCommand`, `UseCommand`) each implemented identical item search logic (find by exact ID or partial name match, case-insensitive). Duplicated logic created maintenance risk.

**Decision:** Add `GameStateExtensions.cs` in `MyGame.Engine` providing three extension methods on `GameState` for item lookup:
```csharp
public static Item? FindItem(this GameState state, string noun)
public static Item? FindRoomItem(this GameState state, string noun)
public static Item? FindInventoryItem(this GameState state, string noun)
```

All three use a shared private predicate: exact ID match (case-insensitive) OR partial name contains (case-insensitive).

**Rationale:**
- Single source of truth: The same predicate logic was inlined in four command classes. Any future change to match semantics (e.g., adding aliases) now requires only one edit.
- Extension method over static class: `state.FindItem(noun)` reads naturally at call sites, consistent with idiomatic C# and existing GameState API.
- Three scopes preserve existing semantics: `TakeCommand` only searches the room (can't take what you're holding); `DropCommand` and `UseCommand` only search inventory; `ExamineCommand` searches both. Separate methods make scope intent explicit rather than hiding it in flags parameter.

**Impact:**
- Code clarity: Commands now express intent explicitly via method name (FindRoomItem vs. FindInventoryItem)
- Maintainability: Predicate changes propagate instantly to all callers
- Extensibility: Adding new search semantics (aliases, fuzzy matching) requires only changes to the predicate

---

## Narrator Variant Coverage — Rogue (Content Designer)

**Date:** 2026-03-10  
**Status:** Approved  
**Scope:** Dynamic room descriptions for all 10 rooms

### Narrator Variants Implementation (Issue #36)

**Context:** Most rooms had only static descriptions. No dynamic narration based on player progress, location familiarity, or mission state. World felt static despite rich underlying game mechanics (flags, item state, visit counts).

**Decision:** All 10 rooms now include dynamic narrator variants triggered by game state flags. Static descriptions have been replaced with context-aware alternatives reflecting player progress, location familiarity, and mission status.

**Variant Categories & Coverage:**

1. **Return-Visit Variants (visit_count_gt_1)** — All 10 rooms
   - When player revisits, description shifts from discovery/wonder to familiarity and tactical awareness
   - Example: Rooftop base description emphasizes visual scale; return visit emphasizes danger ("green emergency lights feel less alien, more like camouflage")

2. **Progression-Based Variants**
   - `keycard_used`: Rooftop, lobby, corridor, server (escalating corporate alarm, urgency, danger)
   - `cred_chip_obtained`: Tunnel, den (leverage gained, reputation earned in undercity)

3. **Item-Possession Variants**
   - `drive in inventory`: Server, corridor (emotional weight of success, extraction danger; corridor variant combines keycard_used + drive-in-inventory for final-escape context)

4. **Existing Variants** (Maintained)
   - `alley`: keycard_used, drive in inventory
   - `bar`: viktor_met
   - `plaza`: cred_chip_obtained
   - `checkpoint`: guard_distracted, guard_bribed

**Narrative Consistency:**
- All variants maintain cyberpunk voice (gritty, neon-soaked, emotional, 2–4 sentences)
- Preserve base description themes while adding emotional/tactical layers
- Use present-tense perspective consistent with existing content
- Avoid redundancy (no duplicate flags within a room)

**Implementation Notes:**
- Flag choices leverage existing game state: `visit_count_gt_1`, `keycard_used`, `cred_chip_obtained`, `viktor_met`
- All variants follow existing JSON schema with `requiredFlags` and `requiredInventoryItems`
- No engine changes required; variants are additive entries in `narratorVariants` array
- Variants selected by NarratorEngine at runtime based on most-specific match (highest combined flag + inventory match count)

**Impact:**
- Player Experience: World feels dynamic and reactive—rooms acknowledge progress and emotional journey
- Replayability: Variants provide different perspectives on familiar spaces across playthroughs
- Story Pacing: Return visits and progression variants reinforce narrative beats (escalation → success → extraction)
- Content Density: 19 new variant entries distributed across 6 rooms, 10 rooms with complete variant coverage

**savegame.json Committed to Repo (#41)**
- **Finding:** Runtime artifact accidentally committed; creates noise in diffs.
- **Action:** Judy to remove file and add to .gitignore.

**ARCHITECTURE.md Stale (#51)**
- **Finding:** Doc doesn't reflect NarratorEngine, ColorConsole, JsonWorldLoader, save/load, drone system, or NPC dialogue.
- **Action:** Johnny to update architecture doc with current implementation details.

**Drone Threat Logic Inline (#44)**
- **Finding:** Threat calculation and drone appearance logic scattered in GameEngine.Run().
- **Action:** Johnny to extract into DroneThreatSystem class for testability and reuse.

**Duplicated FindItem Logic (#33)**
- **Finding:** Identical method in LookCommand and ExamineCommand.
- **Action:** Judy to extract into shared ItemRepository utility (or use existing if present).

### Architectural Strengths

- Command pattern with registry — trivial to add new commands
- IInputOutput abstraction — enables full test coverage without Console coupling
- JSON world loading — content separated from code
- NarratorEngine variant system — elegant dynamic descriptions
- ColorConsole semantic palette — accessible color without hardcoding ANSI codes

### Architectural Weaknesses

- GameEngine has too many concerns (drone logic, banner rendering, win/lose text) — refactoring opportunities
- Content pipeline gaps — dialogue can't set flags, items can't have custom pickup text
- Save system incomplete — will silently corrupt game state if thread level or exit state is modified

---

## Fix Decisions — Judy (C# Developer)

**Date:** 2026-03-10  
**Status:** Complete

### Decision 1: Persist Drone Threat and Exit Lock State in Save/Load (Issue #35)

**Problem:** Save/Load commands persisted only room, inventory, and flags. DroneThreatLevel, DroneThreatThreshold, and exit IsLocked states were silently dropped on reload.

**Decision:**
1. Change `GameState.DroneThreatThreshold` from `init` to `set` to allow LoadCommand to restore it
2. Extend `SaveData` record with `DroneThreatLevel`, `DroneThreatThreshold`, and `ExitLockStates` (roomId → direction → isLocked)
3. Implement backward compatibility: old saves missing fields default to 0/0/null

**Rationale:**
- Players can no longer exploit save/load to reset drone threat
- Door unlock progress persists across save/load cycles
- Minimal changes to GameState (one modifier change)
- Full backward compatibility with existing save files

**Consequences:**
- All 211 tests pass (205 pre-existing + 6 new from River)
- Mid-game saves no longer silently corrupt

### Decision 2: Set NPC-Met Flags in TalkCommand (Issue #46)

**Problem:** Narrator variants keyed on flags like `viktor_met` and `guard_bribed` were unreachable because TalkCommand never set any flags during dialogue interactions.

**Decision:** Add `state.Flags.Add($"{npc.Id}_met")` in `TalkCommand.Execute()` immediately after confirming the NPC has dialogue, before entering the conversation loop. Generic approach: sets `viktor_met`, `mox_met`, `guard_met` for any NPC dialogue.

**Rationale:**
- **No new infrastructure:** `GameState.Flags` and `NarratorEngine` were already wired correctly
- **Generic over specific:** Using `npc.Id` instead of hardcoding `"viktor"` means all NPCs benefit automatically
- **Trigger point:** Setting flag before loop means even minimal dialogue counts as having met the NPC
- **Flag naming convention:** `{npc.Id}_met` mirrors existing world-file conventions (`keycard_used`, `cred_chip_obtained`)

**Consequences:**
- `viktor_met` narrator variant in bar is now reachable after any Viktor dialogue
- `mox_met` and `guard_met` flags also set (no current room variants use them, available for future content)
- All 227 tests pass (211 pre-existing + 16 new ExamineCommand tests from River)

---

## Test Decisions — River (Tester)

**Date:** 2026-03-10  
**Status:** Complete

### Decision 1: Save/Load State Corruption Tests (Issue #35)

**What Was Tested:** Six tests in `SaveLoadTests.cs` proving Judy's fix to SaveCommand/LoadCommand:
- `SaveLoad_PreservesDroneThreatLevel` — DroneThreatLevel=3 restored after reload
- `SaveLoad_PreservesDroneThreatThreshold` — Non-default threshold (6) restored after reload
- `SaveLoad_PreservesUnlockedExit` — Unlocked exit remains unlocked after reload
- `SaveLoad_DroneThreatZeroByDefault_NotCorrupted` — Zero value not silently dropped
- `SaveLoad_NonZeroThreatSurvivesRoundtrip` — High threat cannot be reset via save/load exploit
- `SaveLoad_LockedExitRemainsLockedAfterReload` — Locked exit stays locked (baseline)

**Key Design Decisions:**
1. Private `TwoRoomStateWithLockedExit()` helper — not added to WorldFactory (save/load-specific concept)
2. Unique filename per test within shared `_testDirectory` — prevents test interference
3. Separate fresh `newState` objects — no state sharing between save and load phases
4. `DroneThreatThreshold` test requires `init` → `set` change (Judy's fix enables this)
5. Backward compatibility — old saves not in test scope (implementation detail handled by Judy)

**Total Tests:** 211 (205 pre-existing + 6 new)

### Decision 2: ExamineCommand Test Coverage (Issue #38)

**What Was Tested:** 16 new tests in `ExamineCommandTests.cs` providing comprehensive coverage.

**Tests Written:**
- **Registration:** Verb is "examine"; aliases [x, inspect, read]
- **Error Handling:** Null noun → "Examine what?"; always produces output
- **Room Discovery:** By exact ID, by partial name, case-insensitive search
- **Inventory Discovery:** By ID, by partial name
- **Edge Cases:** Not-found error, no description leakage, room-before-inventory priority, NPC boundary, state mutation guard

**Key Design Decisions:**
1. **Dedicated test file vs. append:** Created `ExamineCommandTests.cs` (not appended to `CommandTests.cs`). Reason: New `ExamineCommand` is standalone, deserves its own file consistent with `TalkCommandTests.cs`

2. **NPC behavior: not-found error is correct:** `ExamineCommand.FindItem` only searches `CurrentRoom.Items` and `Inventory`. NPCs are not items. Decision: **do not add NPC examine behavior** — that is out of scope for this command. Test `Execute_NpcInRoom_ButNoMatchingItem_ShowsNotFoundError` guards this boundary.

3. **Room-before-inventory priority:** Test `Execute_SameIdInRoomAndInventory_ReturnsRoomItemFirst` documents and locks in the `Concat(Inventory)` behavior.

4. **ANSI transparency via OutputContains:** Tests use `io.OutputContains()` which does case-insensitive substring matching. ANSI codes in output don't interfere with assertions.

5. **State mutation test added:** Test `Execute_DoesNotMutateInventoryOrRoom` guards against accidental side effects. Examine is a pure read operation.

**Total Tests:** 227 (211 pre-existing + 16 new ExamineCommand tests)

---

## Summary

All major decisions documented and deduped. Team achieved:
- ✅ Clean architecture with testability first
- ✅ Rich cyberpunk narrative integrated with core mechanics
- ✅ Comprehensive test coverage (227 tests)
- ✅ Full implementation matching architecture and passing all tests
- ✅ JSON as sole world source, dead code removed
- ✅ Save/load state corruption fixed
- ✅ NPC-met narrator flags functional
- ✅ ExamineCommand fully tested
- ⚠️ 12 improvement issues identified for v0.3 and beyond
