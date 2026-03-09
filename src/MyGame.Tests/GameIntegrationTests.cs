using MyGame.Commands;
using MyGame.Content;
using MyGame.Engine;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// End-to-end integration tests that exercise a full game session.
/// Uses FakeInputOutput to inject inputs and capture results without a real console.
///
/// Winning path:
///   alley → east(bar) → up(rooftop) → take keycard → down(bar) → east(lobby)
///   → use keycard → north(server) [WIN]
/// </summary>
public class GameIntegrationTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static GameEngine BuildEngine(FakeInputOutput io)
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        return new GameEngine(state, registry, io);
    }

    private static (GameEngine engine, GameState state, CommandRegistry registry) BuildComponents()
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();
        var engine = new GameEngine(state, registry, io);
        return (engine, state, registry);
    }

    private static CommandRegistry BuildRegistry(GameState state)
    {
        var registry = new CommandRegistry();
        registry.Register(new LookCommand());
        registry.Register(new GoCommand());
        registry.Register(new TakeCommand());
        registry.Register(new DropCommand());
        registry.Register(new InventoryCommand());
        registry.Register(new UseCommand());
        registry.Register(new HelpCommand(registry));
        registry.Register(new QuitCommand());
        return registry;
    }

    // ──────────────────────────────────────────────
    // Full winning sequence
    // ──────────────────────────────────────────────

    [Fact]
    public void WinningPath_CompletesWithHasWonTrue()
    {
        // Arrange — feed the complete winning input sequence then quit to end run()
        var io = new FakeInputOutput(
            "go east",      // alley → bar
            "go up",        // bar → rooftop
            "take keycard", // pick up the keycard
            "go down",      // rooftop → bar
            "go east",      // bar → lobby
            "use keycard",  // unlock north exit
            "go north"      // lobby → server (WIN — engine stops here)
        );
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
            "go down", "go east", "use keycard", "go north"
        );
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void WinningPath_PlayerEndsInServerRoom()
    {
        var io = new FakeInputOutput(
            "go east", "go up", "take keycard",
            "go down", "go east", "use keycard", "go north"
        );
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
        // Arrange — navigate to lobby without picking up keycard
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();

        // Manually navigate to lobby
        registry.Execute(new ParsedCommand("go", "east"), state, io);  // alley → bar
        registry.Execute(new ParsedCommand("go", "east"), state, io);  // bar → lobby

        Assert.Equal("lobby", state.CurrentRoomId);
        var io2 = new FakeInputOutput();

        // Act — attempt to go north into server without keycard
        registry.Execute(new ParsedCommand("go", "north"), state, io2);

        // Assert — blocked
        Assert.Equal("lobby", state.CurrentRoomId);
        Assert.False(state.HasWon);
    }

    [Fact]
    public void GoToServerWithoutKeycard_PrintsLockedMessage()
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();
        registry.Execute(new ParsedCommand("go", "east"), state, io); // → bar
        registry.Execute(new ParsedCommand("go", "east"), state, io); // → lobby

        var io2 = new FakeInputOutput();
        registry.Execute(new ParsedCommand("go", "north"), state, io2);

        // Should mention lock or required item
        Assert.True(io2.OutputContains("keycard") || io2.OutputContains("locked") || io2.OutputContains("need"),
            $"Expected a hint about the locked door but got: {io2.AllOutput}");
    }

    // ──────────────────────────────────────────────
    // Item pickup and inventory flow
    // ──────────────────────────────────────────────

    [Fact]
    public void PickingUpKeycard_AddsToInventory()
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);

        Assert.Equal("bar", state.CurrentRoomId);
    }

    [Fact]
    public void Navigation_BarToRooftop_CorrectRoom()
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io);
        registry.Execute(new ParsedCommand("go", "up"), state, io);

        Assert.Equal("rooftop", state.CurrentRoomId);
    }

    [Fact]
    public void Navigation_CanReturnToStart()
    {
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
            "go down", "go east", "use keycard", "go north"
        );
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        // ARCHITECTURE.md states: engine prints win message when HasWon is true
        Assert.True(io.OutputContains("win") || io.OutputContains("WIN") || io.OutputContains("YOU WIN"),
            $"Expected win message in output. Got:\n{io.AllOutput}");
    }

    [Fact]
    public void QuittingGame_EngineOutputContainsQuitMessage()
    {
        var io = new FakeInputOutput("quit");
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
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
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var engine = new GameEngine(state, registry, io);

        // Should complete without throwing
        engine.Run();

        Assert.False(state.IsRunning);
    }

    [Fact]
    public void UnknownCommandDuringPlay_DoesNotCrash()
    {
        var io = new FakeInputOutput("fly", "teleport north", "quit");
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        // Game continued and accepted quit
        Assert.False(state.IsRunning);
        Assert.False(state.HasWon);
    }

    [Fact]
    public void DroppingAndRetakingKeycard_WinConditionStillWorks()
    {
        // Navigate to rooftop, take keycard, drop it, retake it, then win
        var state = WorldBuilder.Build();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput();

        registry.Execute(new ParsedCommand("go", "east"), state, io); // → bar
        registry.Execute(new ParsedCommand("go", "up"), state, io);   // → rooftop
        registry.Execute(new ParsedCommand("take", "keycard"), state, io);
        registry.Execute(new ParsedCommand("drop", "keycard"), state, io); // drop on rooftop
        registry.Execute(new ParsedCommand("take", "keycard"), state, io); // pick up again
        registry.Execute(new ParsedCommand("go", "down"), state, io); // → bar
        registry.Execute(new ParsedCommand("go", "east"), state, io); // → lobby
        registry.Execute(new ParsedCommand("use", "keycard"), state, io);  // unlock
        registry.Execute(new ParsedCommand("go", "north"), state, io); // → server

        Assert.True(state.HasWon, "Should win even after drop/retake cycle.");
    }
}
