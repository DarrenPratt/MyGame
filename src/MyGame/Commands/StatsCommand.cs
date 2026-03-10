namespace MyGame.Commands;

using MyGame.Engine;

public class StatsCommand : ICommand
{
    public string Verb => "stats";
    public string[] Aliases => ["status", "progress"];
    public string HelpText => "Show current game stats and progress.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        io.WriteLine(ColorConsole.BoldCyan("══ STATS ══════════════════════════════"));

        var room = state.CurrentRoom;
        io.WriteLine($"  {"Location",-14} {ColorConsole.Cyan(room.Name)}");

        var threatColor = state.DroneThreatLevel == 0
            ? ColorConsole.Green($"{state.DroneThreatLevel}/{state.DroneThreatThreshold}")
            : state.DroneThreatLevel >= state.DroneThreatThreshold - 1
                ? ColorConsole.Error($"{state.DroneThreatLevel}/{state.DroneThreatThreshold}")
                : ColorConsole.Yellow($"{state.DroneThreatLevel}/{state.DroneThreatThreshold}");
        io.WriteLine($"  {"Drone Threat",-14} {threatColor}");

        var invLabel = state.Inventory.Count == 0
            ? ColorConsole.DarkGray("none")
            : ColorConsole.Yellow($"{state.Inventory.Count} item{(state.Inventory.Count == 1 ? "" : "s")}");
        io.WriteLine($"  {"Inventory",-14} {invLabel}");
        foreach (var item in state.Inventory)
            io.WriteLine($"    - {ColorConsole.Yellow(item.Name)}");

        var flagsLabel = state.Flags.Count > 0
            ? ColorConsole.Green(string.Join(", ", state.Flags.OrderBy(f => f)))
            : ColorConsole.DarkGray("none");
        io.WriteLine($"  {"Flags",-14} {flagsLabel}");

        io.WriteLine(ColorConsole.BoldCyan("═══════════════════════════════════════"));
    }
}
