using MyGame.Engine;
using MyGame.Tests.Helpers;
using Xunit;

namespace MyGame.Tests;

/// <summary>
/// Tests for the drone threat / lose condition (Issue #8).
///
/// Drone threat logic (in GameEngine.Run):
///   After each command, if the player is in a high-risk room (plaza, checkpoint),
///   DroneThreatLevel is incremented. At threshold (3), HasLost = true, IsRunning = false.
///   Warnings are printed at DroneThreatLevel 1 and 2.
///
/// Navigation from start (alley) to plaza:
///   "go down" → tunnel (safe)
///   "go north" → plaza (high-risk: DroneThreatLevel becomes 1)
/// </summary>
public class DroneTests
{
    // ──────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────

    private static GameState BuildState()
    {
        var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");
        return new JsonWorldLoader().Load(worldPath).State;
    }

    private static (GameEngine engine, GameState state) BuildEngineWithInputs(params string[] inputs)
    {
        var io = new FakeInputOutput(inputs);
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);
        return (engine, state);
    }

    private static (GameEngine engine, GameState state, FakeInputOutput io) BuildEngineWithCapture(params string[] inputs)
    {
        var io = new FakeInputOutput(inputs);
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);
        return (engine, state, io);
    }

    // ──────────────────────────────────────────────
    // State-level drone threat tests
    // ──────────────────────────────────────────────

    [Fact]
    public void DroneThreat_OneCommandInPlaza_DoesNotTriggerLose()
    {
        // Navigate to plaza (that arrival turn counts as the one command in plaza), then quit.
        var (engine, state) = BuildEngineWithInputs(
            "go down",   // alley → tunnel (safe, no increment)
            "go north",  // tunnel → plaza (high-risk: DroneThreatLevel = 1)
            "quit"       // stop before more turns accumulate
        );

        engine.Run();

        Assert.False(state.HasLost, "One turn in a high-risk room should not trigger lose.");
        Assert.False(state.IsRunning);
    }

    [Fact]
    public void DroneThreat_TwoCommandsInPlaza_DoesNotTriggerLose()
    {
        // Arrival in plaza = turn 1; look = turn 2. Still under threshold.
        var (engine, state) = BuildEngineWithInputs(
            "go down",   // alley → tunnel
            "go north",  // tunnel → plaza (DroneThreatLevel = 1)
            "look",      // still in plaza (DroneThreatLevel = 2)
            "quit"
        );

        engine.Run();

        Assert.False(state.HasLost, "Two turns in a high-risk room should not trigger lose.");
        Assert.False(state.IsRunning);
    }

    [Fact]
    public void DroneThreat_ThreeCommandsInPlaza_TriggersLose()
    {
        // Four turns in a high-risk room hits the threshold (4) and triggers the lose condition.
        // Threshold is 4 so the winning path (max 2 turns in any single room) is still viable.
        var (engine, state) = BuildEngineWithInputs(
            "go down",   // alley → tunnel
            "go north",  // tunnel → plaza (DroneThreatLevel = 1)
            "look",      // plaza (DroneThreatLevel = 2)
            "look",      // plaza (DroneThreatLevel = 3)
            "look"       // plaza (DroneThreatLevel = 4 → HasLost = true, engine stops)
        );

        engine.Run();

        Assert.True(state.HasLost, "Four turns in a high-risk room should trigger lose (threshold = 4).");
        Assert.False(state.IsRunning);
    }

    [Fact]
    public void DroneThreat_ThreeCommandsInPlaza_ThreatLevelIsThree()
    {
        var (engine, state) = BuildEngineWithInputs(
            "go down",
            "go north",  // DroneThreatLevel = 1
            "look",      // DroneThreatLevel = 2
            "look"       // DroneThreatLevel = 3
        );

        engine.Run();

        Assert.Equal(3, state.DroneThreatLevel);
    }

    [Fact]
    public void DroneThreat_TurnsAcrossHighRiskRooms_Accumulate()
    {
        // Threat increments carry over from plaza to checkpoint.
        // After "go north" to plaza (DL=1), "look" in plaza (DL=2),
        // "go north" to checkpoint (DL=3), "look" in checkpoint (DL=4 → lose).
        var (engine, state) = BuildEngineWithInputs(
            "go down",   // alley → tunnel
            "go north",  // tunnel → plaza (DroneThreatLevel = 1)
            "look",      // still in plaza (DroneThreatLevel = 2)
            "go north",  // plaza → checkpoint (high-risk: DroneThreatLevel = 3)
            "look"       // still in checkpoint (DroneThreatLevel = 4 → HasLost = true)
        );

        engine.Run();

        Assert.True(state.HasLost,
            "Threat level should accumulate across high-risk rooms: 2 turns in plaza + 2 in checkpoint = 4 → lose.");
    }

    [Fact]
    public void DroneThreat_MovingToSafeRoom_StopsCounting()
    {
        // After 2 turns in plaza, retreating to tunnel (safe) prevents further accumulation.
        var (engine, state) = BuildEngineWithInputs(
            "go down",   // alley → tunnel
            "go north",  // tunnel → plaza (DroneThreatLevel = 1)
            "look",      // plaza (DroneThreatLevel = 2)
            "go south",  // plaza → tunnel (safe — no increment)
            "look",      // tunnel (safe — no increment)
            "look",      // tunnel (safe — no increment)
            "quit"
        );

        engine.Run();

        Assert.False(state.HasLost, "Commands in a safe room should not increment the threat level.");
        Assert.Equal(2, state.DroneThreatLevel);
    }

    // ──────────────────────────────────────────────
    // Output / engine message tests
    // ──────────────────────────────────────────────

    [Fact]
    public void DroneThreat_EngineOutputContainsWarningAtTurnOne()
    {
        // First turn in a high-risk room should print a scanner/drone warning.
        var (engine, state, io) = BuildEngineWithCapture(
            "go down",   // alley → tunnel
            "go north",  // tunnel → plaza (DroneThreatLevel = 1 — warning should print)
            "quit"
        );

        engine.Run();

        Assert.True(
            io.OutputContains("drone") || io.OutputContains("scanner"),
            $"Expected a drone/scanner warning at threat level 1. Output was:\n{io.AllOutput}");
    }

    [Fact]
    public void DroneThreat_EngineOutputContainsLoseText_WhenCaptured()
    {
        // After reaching the threshold (4), the engine should print the lose narrative.
        var (engine, state, io) = BuildEngineWithCapture(
            "go down",
            "go north",  // DroneThreatLevel = 1
            "look",      // DroneThreatLevel = 2
            "look",      // DroneThreatLevel = 3
            "look"       // DroneThreatLevel = 4 → lose
        );

        engine.Run();

        Assert.True(state.HasLost);
        Assert.True(
            io.OutputContains("drone") || io.OutputContains("captured") || io.OutputContains("SynthCorp"),
            $"Expected lose narrative in output. Got:\n{io.AllOutput}");
    }

    // ──────────────────────────────────────────────
    // Winning path regression
    // ──────────────────────────────────────────────

    /// <summary>
    /// The full winning path passes through plaza (1 turn) and checkpoint (1–2 turns) and must
    /// complete before reaching the drone threat threshold. This test guards against regressions
    /// where the drone threat incorrectly fires during a correct play-through.
    ///
    /// NOTE: This test currently FAILS because the winning path accumulates exactly 3 turns in
    /// high-risk rooms (plaza arrival, checkpoint arrival, use cred_chip in checkpoint) and the
    /// threshold is 3 — triggering lose on the "use cred_chip" command. The threshold needs to
    /// be raised to 4, or the UseCommand needs to move the player out of checkpoint after
    /// unlocking the door. Tracked in Issue #8 — flag for Judy.
    /// </summary>
    [Fact]
    public void WinningPath_NotBlockedByDroneThreat()
    {
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
            "go north",       // tunnel → plaza       (DroneThreatLevel = 1)
            "go north",       // plaza → checkpoint   (DroneThreatLevel = 2)
            "use cred_chip",  // unlock checkpoint→north (DroneThreatLevel = 3 — BUG: triggers lose at threshold 3)
            "go north",       // checkpoint → lobby
            "use keycard",    // unlock lobby→north
            "go north"        // lobby → server (WIN)
        );
        var state = BuildState();
        var registry = RegistryFactory.BuildRegistry();
        var engine = new GameEngine(state, registry, io);

        engine.Run();

        Assert.True(state.HasWon, "Winning path should complete successfully without triggering drone lose condition.");
        Assert.False(state.HasLost, "Winning path should not trigger the drone lose condition.");
    }
}
