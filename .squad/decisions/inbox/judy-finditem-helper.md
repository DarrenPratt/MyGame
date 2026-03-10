# Decision: GameState Extension Methods for Item Lookup

**Date:** 2026-03-10  
**Author:** Judy (C# Developer)  
**Issue:** #33 — FindItem duplication across commands

## Decision

Add `GameStateExtensions.cs` in `MyGame.Engine` providing three extension methods on `GameState` for item lookup:

```csharp
public static Item? FindItem(this GameState state, string noun)         // room then inventory
public static Item? FindRoomItem(this GameState state, string noun)     // room only
public static Item? FindInventoryItem(this GameState state, string noun) // inventory only
```

All three use a shared private predicate: exact ID match (case-insensitive) OR partial name contains (case-insensitive).

## Rationale

- **Single source of truth**: The same predicate logic was inlined in four separate command classes. Any future change to match semantics (e.g., adding aliases) now requires only one edit.
- **Extension method over static class**: `state.FindItem(noun)` reads naturally at call sites, consistent with idiomatic C# and the existing GameState API.
- **Three scopes preserve existing semantics**: `TakeCommand` only searches the room (can't take what you're holding); `DropCommand` and `UseCommand` only search inventory; `ExamineCommand` searches both. Separate methods make scope intent explicit rather than hiding it in a flags parameter.

## Files Changed

- `src/MyGame/Engine/GameStateExtensions.cs` — new file
- `src/MyGame/Commands/TakeCommand.cs` — uses `FindRoomItem`
- `src/MyGame/Commands/DropCommand.cs` — uses `FindInventoryItem`
- `src/MyGame/Commands/ExamineCommand.cs` — uses `FindItem`, private static removed
- `src/MyGame/Commands/UseCommand.cs` — uses `FindInventoryItem`

## Status

All 227 tests pass.
