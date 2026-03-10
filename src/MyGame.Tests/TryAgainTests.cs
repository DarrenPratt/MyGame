using MyGame.Commands;
using MyGame.Engine;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for the "Try Again?" retry prompt shown after death (Issue #29).
///
/// Prompt appears only when:
///   - HasLost == true at end of session
///   - A state factory was provided to GameEngine
///
/// Death trigger sequence (from start in alley):
///   "go down"  → tunnel (safe, no threat)
///   "go north" → plaza  (high-risk: DroneThreatLevel = 1)
///   "look"               DroneThreatLevel = 2
///   "look"               DroneThreatLevel = 3
///   "look"               DroneThreatLevel = 4 >= threshold → HasLost, !IsRunning
/// </summary>
public class TryAgainTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static Func<GameState> BuildStateFactory()
    {
        var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");
        return () => new JsonWorldLoader().Load(worldPath).State;
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

    // Minimum inputs to reach and die in the plaza.
    private static readonly string[] DeathInputs =
    [
        "go down",   // alley → tunnel (safe — no threat increment)
        "go north",  // tunnel → plaza (high-risk: DroneThreatLevel = 1)
        "look",      // DroneThreatLevel = 2
        "look",      // DroneThreatLevel = 3
        "look",      // DroneThreatLevel = 4 >= threshold → HasLost = true, IsRunning = false
    ];

    // ──────────────────────────────────────────────
    // Try Again prompt behaviour
    // ──────────────────────────────────────────────

    [Fact]
    public void Death_WithFactory_ShowsTryAgainPrompt()
    {
        var factory = BuildStateFactory();
        var state = factory();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput([.. DeathInputs, "no"]);
        var engine = new GameEngine(state, registry, io, stateFactory: factory);

        engine.Run();

        Assert.True(io.OutputContains("try again"),
            $"Expected a 'try again' prompt after death when factory is provided.\nOutput:\n{io.AllOutput}");
    }

    [Fact]
    public void Death_WithFactory_AnswerNo_ExitsCleanly()
    {
        var factory = BuildStateFactory();
        var state = factory();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput([.. DeathInputs, "no"]);
        var engine = new GameEngine(state, registry, io, stateFactory: factory);

        engine.Run();

        Assert.False(state.IsRunning, "IsRunning should be false after answering no.");
        Assert.True(state.HasLost, "HasLost should remain true after answering no.");
    }

    [Fact]
    public void Death_WithFactory_AnswerYes_RestartsGame()
    {
        var factory = BuildStateFactory();
        var state = factory();
        var registry = BuildRegistry(state);
        // After death + yes → engine resets state and runs a fresh session; quit ends it
        var io = new FakeInputOutput([.. DeathInputs, "yes", "quit"]);
        var engine = new GameEngine(state, registry, io, stateFactory: factory);

        engine.Run();

        // The banner border is printed once per session; two sessions means it appears twice
        var bannerLineCount = io.Lines.Count(l => l.Contains("╔", StringComparison.Ordinal));
        Assert.True(bannerLineCount >= 2,
            $"Expected banner header to appear at least twice (death session + restarted session).\nOutput:\n{io.AllOutput}");
    }

    [Fact]
    public void Death_WithoutFactory_NoTryAgainPrompt()
    {
        var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");
        var state = new JsonWorldLoader().Load(worldPath).State;
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput(DeathInputs);
        var engine = new GameEngine(state, registry, io); // no factory → backward-compatible path

        engine.Run();

        Assert.False(io.OutputContains("try again"),
            $"Expected NO 'try again' prompt when no factory is provided.\nOutput:\n{io.AllOutput}");
    }

    [Fact]
    public void Win_DoesNotShowTryAgainPrompt()
    {
        var factory = BuildStateFactory();
        var state = factory();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput(
            "go east",        // alley → bar
            "go up",          // bar → rooftop
            "take keycard",   // pick up keycard
            "go down",        // rooftop → bar
            "go west",        // bar → alley
            "go down",        // alley → tunnel
            "go south",       // tunnel → den
            "take cred_chip", // pick up cred_chip
            "go north",       // den → tunnel
            "go north",       // tunnel → plaza       (DroneThreatLevel = 1)
            "go north",       // plaza → checkpoint   (DroneThreatLevel = 2)
            "use cred_chip",  // unlock checkpoint→north (DroneThreatLevel = 3)
            "go north",       // checkpoint → lobby
            "use keycard",    // unlock lobby→north
            "go north"        // lobby → server (WIN)
        );
        var engine = new GameEngine(state, registry, io, stateFactory: factory);

        engine.Run();

        Assert.True(state.HasWon, "Player should have won on the full winning path.");
        Assert.False(io.OutputContains("try again"),
            $"Expected NO 'try again' prompt after winning — only shown on death.\nOutput:\n{io.AllOutput}");
    }

    [Fact]
    public void Quit_DoesNotShowTryAgainPrompt()
    {
        var factory = BuildStateFactory();
        var state = factory();
        var registry = BuildRegistry(state);
        var io = new FakeInputOutput("quit");
        var engine = new GameEngine(state, registry, io, stateFactory: factory);

        engine.Run();

        Assert.False(io.OutputContains("try again"),
            $"Expected NO 'try again' prompt when player voluntarily quits.\nOutput:\n{io.AllOutput}");
    }
}
