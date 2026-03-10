using MyGame.Commands;
using MyGame.Engine;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// End-to-end integration tests that exercise a full game session.
/// Uses FakeInputOutput to inject inputs and capture results without a real console.
///
/// Winning path:
///   alley → east(bar) → up(rooftop) → take keycard → down(bar) → west(alley)
///   → down(tunnel) → south(den) → take cred_chip → north(tunnel) → north(plaza)
///   → north(checkpoint) → use cred_chip → north(lobby) → use keycard → north(server) [WIN]
/// </summary>
public class GameIntegrationTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static GameState BuildState()
    {
        var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");
        return new JsonWorldLoader().Load(worldPath).State;
    }

    private static GameEngine BuildEngine(FakeInputOutput io)
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        return new GameEngine(state, registry, io);
    }

    private static (GameEngine engine, GameState state, CommandRegistry registry) BuildComponents()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();
        var engine = new GameEngine(state, registry, io);
        return (engine, state, registry);
    }

    // ──────────────────────────────────────────────
    // Full winning sequence
    // ──────────────────────────────────────────────

    [Fact]
    public void WinningPath_CompletesWithHasWonTrue()
    {
        // Arrange — feed the complete winning input sequence
        var io = new FakeInputOutput(
            "go east",        // alley → bar
            "go up",          // bar → rooftop
            "take keycard",   // pick up the keycard
            "go down",        // rooftop → bar
            "go west",        // bar → alley
            "go down",        // alley → tunnel
            "go south",       // tunnel → den
            "take cred_chip", // pick up the cred_chip
            "go north",       // den → tunnel
            "go north",       // tunnel → plaza
            "go north",       // plaza → checkpoint
            "use cred_chip",  // unlock checkpoint→north
            "go north",       // checkpoint → lobby
            "use keycard",    // unlock lobby→north
            "go north"        // lobby → server (WIN — engine stops here)
        );
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        // Act
        engine.Run();

        // Assert
        Assert.True(state.HasWon, "Player should have won after completing the winning path.");
    }

    [Fact]
    public void WinningPath_SetsIsRunningFalse()
    {
        var io = new FakeInputOutput(
            "go east", "go up", "take keycard",
            "go down", "go west", "go down", "go south", "take cred_chip",
            "go north", "go north", "go north", "use cred_chip",
            "go north", "use keycard", "go north"
        );
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void WinningPath_PlayerEndsInServerRoom()
    {
        var io = new FakeInputOutput(
            "go east", "go up", "take keycard",
            "go down", "go west", "go down", "go south", "take cred_chip",
            "go north", "go north", "go north", "use cred_chip",
            "go north", "use keycard", "go north"
        );
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        Assert.Equal("server", state.CurrentRoomId);
    }

    // ──────────────────────────────────────────────
    // Attempting to enter server room without keycard
    // ──────────────────────────────────────────────

    [Fact]
    public void GoToServerWithoutKeycard_IsBlocked()
    {
        // Arrange — place player directly in lobby without keycard
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        state.CurrentRoomId = "lobby";

        var io = new FakeInputOutput();

        // Act — attempt to go north into server without keycard
        registry.Execute(new ParsedCommand("go", "north"), state, io);

        // Assert — blocked
        Assert.Equal("lobby", state.CurrentRoomId);
        Assert.False(state.HasWon);
    }

    [Fact]
    public void GoToServerWithoutKeycard_PrintsLockedMessage()
    {
        // Place player directly in lobby without keycard
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        state.CurrentRoomId = "lobby";

        var io = new FakeInputOutput();
        registry.Execute(new ParsedCommand("go", "north"), state, io);

        // Should mention lock or required item
        Assert.True(io.OutputContains("keycard") || io.OutputContains("locked") || io.OutputContains("need"),
            $"Expected a hint about the locked door but got: {io.AllOutput}");
    }

    // ──────────────────────────────────────────────
    // Item pickup and inventory flow
    // ──────────────────────────────────────────────

    [Fact]
    public void PickingUpKeycard_AddsToInventory()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        // Navigate to rooftop
        registry.Execute(new ParsedCommand("go", "east"), state, io); // alley → bar
        registry.Execute(new ParsedCommand("go", "up"), state, io);   // bar → rooftop

        Assert.Equal("rooftop", state.CurrentRoomId);

        var io2 = new FakeInputOutput();
        registry.Execute(new ParsedCommand("take", "keycard"), state, io2);

        Assert.Contains(state.Inventory, i => i.Id == "keycard");
    }

    [Fact]
    public void KeycardTaken_IsRemovedFromRooftop()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);
        registry.Execute(new ParsedCommand("go", "up"), state, io);
        registry.Execute(new ParsedCommand("take", "keycard"), state, io);

        var rooftop = state.Rooms["rooftop"];
        Assert.DoesNotContain(rooftop.Items, i => i.Id == "keycard");
    }

    // ──────────────────────────────────────────────
    // Navigation breadcrumbs
    // ──────────────────────────────────────────────

    [Fact]
    public void Navigation_AlleyToBar_CorrectRoom()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);

        Assert.Equal("bar", state.CurrentRoomId);
    }

    [Fact]
    public void Navigation_BarToRooftop_CorrectRoom()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);
        registry.Execute(new ParsedCommand("go", "up"), state, io);

        Assert.Equal("rooftop", state.CurrentRoomId);
    }

    [Fact]
    public void Navigation_CanReturnToStart()
    {
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io); // → bar
        registry.Execute(new ParsedCommand("go", "west"), state, io); // → alley

        Assert.Equal("alley", state.CurrentRoomId);
    }

    // ──────────────────────────────────────────────
    // Quit during play
    // ──────────────────────────────────────────────

    [Fact]
    public void Quit_DuringPlay_EndsWithHasWonFalse()
    {
        var io = new FakeInputOutput("quit");
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        Assert.False(state.HasWon);
        Assert.False(state.IsRunning);
    }

    // ──────────────────────────────────────────────
    // Win message in output
    // ──────────────────────────────────────────────

    [Fact]
    public void WinningPath_EngineOutputContainsWinMessage()
    {
        var io = new FakeInputOutput(
            "go east", "go up", "take keycard",
            "go down", "go west", "go down", "go south", "take cred_chip",
            "go north", "go north", "go north", "use cred_chip",
            "go north", "use keycard", "go north"
        );
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        // ARCHITECTURE.md states: engine prints win message when HasWon is true
        Assert.True(io.OutputContains("win") || io.OutputContains("WIN") || io.OutputContains("YOU WIN"),
            $"Expected win message in output. Got:\n{io.AllOutput}");
    }

    [Fact]
    public void WinningPath_UsesCustomWinMessageFromJson()
    {
        var io = new FakeInputOutput(
            "go east", "go up", "take keycard",
            "go down", "go west", "go down", "go south", "take cred_chip",
            "go north", "go north", "go north", "use cred_chip",
            "go north", "use keycard", "go north"
        );
        var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");
        var world = new JsonWorldLoader().Load(worldPath);
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(world.State, registry, io, world);

        engine.Run();

        // neon-ledger.json has a custom winMessage — engine should display it, not the default
        Assert.True(io.OutputContains("burning in your pocket") || io.OutputContains("Years of dead contacts"),
            $"Expected custom JSON win message in output. Got:\n{io.AllOutput}");
    }

    [Fact]
    public void QuittingGame_EngineOutputContainsQuitMessage()
    {
        var io = new FakeInputOutput("quit");
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        // Engine prints quit/jacked-out message when IsRunning stops without winning
        Assert.True(io.OutputContains("JACKED") || io.OutputContains("quit") || io.OutputContains("game over"),
            $"Expected quit/game-over message in output. Got:\n{io.AllOutput}");
    }

    // ──────────────────────────────────────────────
    // Edge cases
    // ──────────────────────────────────────────────

    [Fact]
    public void EmptyInput_DoesNotCrash()
    {
        var io = new FakeInputOutput("", "  ", "quit");
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        // Should complete without throwing
        engine.Run();

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void UnknownCommandDuringPlay_DoesNotCrash()
    {
        var io = new FakeInputOutput("fly", "teleport north", "quit");
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        // Game continued and accepted quit
        Assert.False(state.IsRunning);
        Assert.False(state.HasWon);
    }

    [Fact]
    public void DroppingAndRetakingKeycard_WinConditionStillWorks()
    {
        // Navigate to rooftop, take keycard, drop it, retake it, then complete winning path
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);      // → bar
        registry.Execute(new ParsedCommand("go", "up"), state, io);        // → rooftop
        registry.Execute(new ParsedCommand("take", "keycard"), state, io);
        registry.Execute(new ParsedCommand("drop", "keycard"), state, io); // drop on rooftop
        registry.Execute(new ParsedCommand("take", "keycard"), state, io); // pick up again
        registry.Execute(new ParsedCommand("go", "down"), state, io);      // → bar
        registry.Execute(new ParsedCommand("go", "west"), state, io);      // → alley
        registry.Execute(new ParsedCommand("go", "down"), state, io);      // → tunnel
        registry.Execute(new ParsedCommand("go", "south"), state, io);     // → den
        registry.Execute(new ParsedCommand("take", "cred_chip"), state, io);
        registry.Execute(new ParsedCommand("go", "north"), state, io);     // → tunnel
        registry.Execute(new ParsedCommand("go", "north"), state, io);     // → plaza
        registry.Execute(new ParsedCommand("go", "north"), state, io);     // → checkpoint
        registry.Execute(new ParsedCommand("use", "cred_chip"), state, io);
        registry.Execute(new ParsedCommand("go", "north"), state, io);     // → lobby
        registry.Execute(new ParsedCommand("use", "keycard"), state, io);
        registry.Execute(new ParsedCommand("go", "north"), state, io);     // → server

        Assert.True(state.HasWon, "Should win even after drop/retake cycle.");
    }

    // ──────────────────────────────────────────────
    // Registry completeness
    // ──────────────────────────────────────────────

    /// <summary>
    /// Verifies that RegistryFactory registers every verb that Program.cs wires up in production.
    /// Add new verbs here whenever a new command lands in Program.cs.
    /// </summary>
    [Theory]
    [InlineData("look")]
    [InlineData("go")]
    [InlineData("take")]
    [InlineData("drop")]
    [InlineData("inventory")]
    [InlineData("use")]
    [InlineData("examine")]
    [InlineData("help")]
    [InlineData("quit")]
    [InlineData("talk")]
    [InlineData("save")]
    [InlineData("load")]
    public void RegistryFactory_RegistersAllProductionCommands(string verb)
    {
        var registry = RegistryFactory.BuildRegistry();
        var registeredVerbs = registry.AllCommands.Select(c => c.Verb).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.True(registeredVerbs.Contains(verb),
            $"RegistryFactory is missing command with verb \"{verb}\". " +
            $"Add it to RegistryFactory.BuildRegistry() to match Program.cs.");
    }
}