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

## Summary

All major decisions documented and deduped. Team achieved:
- ✅ Clean architecture with testability first
- ✅ Rich cyberpunk narrative integrated with core mechanics
- ✅ Comprehensive test coverage (114 tests)
- ✅ Full implementation matching architecture and passing all tests
