# Johnny — History

## Project Context

**Project:** MyGame — simple text adventure game in C#  
**Stack:** C#, .NET 8+, console application  
**User:** Jynx_Protocol  
**Theme:** Cyberpunk — dark future, neon-lit streets, corporate dystopia  
**Created:** 2026-03-09

## Learnings

- **2025-07-14 — Architecture designed.** Created ARCHITECTURE.md with full class design (Room, Item, Exit, GameState, GameEngine, CommandParser, ICommand, CommandRegistry, WorldBuilder), project structure (src/MyGame + tests/MyGame.Tests), game loop flow, command dispatch pattern, and a 5-room cyberpunk world map. Key decisions: IInputOutput abstraction for testability, command pattern with registry for extensibility, hardcoded WorldBuilder for simplicity, flat string flags for state, room-entry win condition. Decision record written to `.squad/decisions/inbox/johnny-architecture.md`.
- **2026-03-10 — Full codebase review completed.** Reviewed all source files, 12 commands, 10 rooms, 205 tests. Created 12 GitHub issues (#32–#51). Key findings: Save/Load is broken (doesn't persist drone threat or exit state), TalkCommand never sets game flags (narrator variants keyed on dialogue are dead), duplicate FindItem logic across commands, hardcoded narrative in GoCommand/TakeCommand bypasses JSON content pipeline. Architecture is sound — command pattern and IInputOutput abstraction are paying dividends. Main concern is GameEngine accumulating inline logic (drone system should be extracted). ARCHITECTURE.md needs a full refresh to match current implementation.
- **2026-03-10 — Second codebase review (expanded).** Created 14 new GitHub issues across all dimensions. Critical findings: (1) Save/Load missing drone threat level is a game-breaking exploit (#47); (2) TalkCommand dialogue has no state effect — viktor_met and guard_bribed flags are never set despite NarratorVariants referencing them (#52); (3) Cannot examine NPCs despite them having Description fields (#45); (4) Parser.cs is dead code (#39); (5) Test helpers (BuildRegistry) duplicated across 3 test files with inconsistent command registration; (6) ARCHITECTURE.md severely outdated — missing NPC system, NarratorEngine, Save/Load, drone mechanics (#48). Content gaps: 6 rooms have zero NarratorVariants, repair_kit/flyer items serve no mechanical purpose. Architectural concern: GameEngine has grown to handle UI, game loop, win/lose, and restart — consider extraction (#59).

## Team Updates

- **2026-03-09 — Rogue completed content design:** 9-room cyberpunk world with 8 items and rich narrative. Used your architecture as the framework and added atmospheric flavor text.
- **2026-03-09 — River completed test suite:** 114 comprehensive xUnit tests written against ARCHITECTURE.md spec. Tests cover all commands, world integrity, state management, and integration flows. Your IInputOutput abstraction proved essential for testability.
- **2026-03-09 — Judy completed implementation:** Full C# game implementation matching your architecture exactly. All 114 tests passing. Your ICommand pattern and CommandRegistry enabled clean, extensible command dispatch. Game title: "Neon Ledger".
