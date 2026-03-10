# Decision: Parser.cs Deleted — Tests Repointed to CommandParser

**Author:** River (Tester)  
**Date:** 2026-03-10  
**Closes:** Issue #32

## What Happened

`Parser.cs` was a 6-line wrapper that only existed to expose `CommandParser.Parse(string input)` as an instance method:

```csharp
public class Parser
{
    public ParsedCommand Parse(string input) => CommandParser.Parse(input);
}
```

The only thing keeping it alive was `ParserTests.cs`, which instantiated `new Parser()` in all 6 tests.

## Change Made

- **`src/MyGame.Tests/ParserTests.cs`** — all 6 tests rewritten to call `CommandParser.Parse(input)` directly (static call, no instantiation).
- **`src/MyGame/Engine/Parser.cs`** — deleted.

## Behavioral Coverage Preserved

All 6 test behaviors retained verbatim:
1. `use keycard` → Verb=use, Noun=keycard, Target=null
2. `use keycard on door` → Verb=use, Noun=keycard, Target=door
3. `use item on target with spaces` → multi-word target captured
4. `talk to viktor` → "to viktor" kept as Noun (stripping "to" is TalkCommand's job)
5. `go north` → standard two-part command, no target
6. `""` (empty) → Verb="", Noun=null, Target=null

## CommandParser Design

`CommandParser` is a `static class` in `MyGame.Engine`. Its `Parse(string input)` method:
- Trims and lowercases input
- Splits on first space to get Verb + rest
- Scans rest for `" on "` keyword to split Noun from Target
- Returns a `ParsedCommand` record (Verb, Noun?, Target?)

## Status

All 227 tests pass. `Parser.cs` is gone from the codebase.
