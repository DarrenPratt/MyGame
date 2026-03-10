# Decision: Static const class for narrative strings (Issue #34)

**Date:** 2026-03-10  
**Author:** Judy (C# Developer)

## Context

`GameEngine.cs` and the ten command files contained hardcoded player-facing string literals scattered throughout the implementation. Death messages, win/lose banners, drone warnings, command-error messages, and UI prompts were all embedded inline — making them hard to find, update consistently, or reference from tests.

## Decision

Extract all player-facing narrative and UI strings into a single static class `GameMessages` (`src/MyGame/Engine/GameMessages.cs`), using `public const string` members grouped by context in nested static classes.

```
GameMessages.Defaults    — title, subtitle, intro text
GameMessages.Prompts     — command input prompt, dialogue prompt, try-again prompt
GameMessages.Drone       — warning messages at threat levels 1–3
GameMessages.Win         — server-room entry lines, default win narrative, win banner
GameMessages.Lose        — default lose narrative, lose banner
GameMessages.Quit        — quit message, quit banner
GameMessages.Go          — no-direction error, "The way is locked."
GameMessages.Take        — take-what error, data_chip pickup flavour
GameMessages.Use         — use-what error
GameMessages.Look        — "Items here:", "You see here:", "Exits:"
GameMessages.Help        — commands header, directions footer
GameMessages.Drop        — drop-what error
GameMessages.Examine     — examine-what error
GameMessages.Inventory   — empty message, carrying header
GameMessages.Talk        — to-whom prompt, end-conversation, goodbye
```

## Rationale

- **Single place to find and update any player-facing string** — no grep needed.
- **Simple**: one file, const strings, no localization framework, no resource files.
- **Pure refactor** — zero behaviour change, all 227 tests pass unchanged.
- **Consolidation pattern**: multi-line prose previously written as sequential `_io.WriteLine` calls is now a single const with `\n` delimiters, split via the existing `SplitLines` helper — matching the pattern already used for JSON world messages.

## Files Changed

- `src/MyGame/Engine/GameMessages.cs` — new file, all constants
- `src/MyGame/Engine/GameEngine.cs` — references to constants
- `src/MyGame/Commands/GoCommand.cs`, `TakeCommand.cs`, `UseCommand.cs`, `LookCommand.cs`, `HelpCommand.cs`, `QuitCommand.cs`, `TalkCommand.cs`, `DropCommand.cs`, `ExamineCommand.cs`, `InventoryCommand.cs` — references to constants
