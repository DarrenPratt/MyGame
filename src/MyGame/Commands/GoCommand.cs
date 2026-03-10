namespace MyGame.Commands;

using MyGame.Engine;

public class GoCommand : ICommand
{
    public string Verb => "go";
    public string[] Aliases => ["north", "south", "east", "west", "up", "down",
                                "n", "s", "e", "w", "u", "d"];
    public string HelpText => "Move in a direction. Usage: go <direction> or just type the direction.";

    private static readonly Dictionary<string, string> Abbreviations = new(StringComparer.OrdinalIgnoreCase)
    {
        { "n", "north" }, { "s", "south" }, { "e", "east" }, { "w", "west" },
        { "u", "up" }, { "d", "down" }
    };

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        string? direction = command.Verb == "go" ? command.Noun : command.Verb;

        if (direction is null)
        {
            io.WriteLine(ColorConsole.Error(GameMessages.Go.NoDirection));
            return;
        }

        if (Abbreviations.TryGetValue(direction, out var expanded))
            direction = expanded;

        var room = state.CurrentRoom;
        if (!room.Exits.TryGetValue(direction, out var exit))
        {
            io.WriteLine(ColorConsole.Error($"You can't go {direction} from here."));
            return;
        }

        if (exit.IsLocked)
        {
            var needed = exit.RequiredItemId is not null
                ? $"You need the {exit.RequiredItemId.Replace('_', ' ')} to proceed."
                : GameMessages.Go.WayLocked;
            io.WriteLine(ColorConsole.Error($"The way {direction} is blocked. {needed}"));
            return;
        }

        if (!state.Rooms.ContainsKey(exit.TargetRoomId))
        {
            io.WriteLine(ColorConsole.Error($"You can't go {direction} — the path leads nowhere. (World error)"));
            return;
        }

        state.CurrentRoomId = exit.TargetRoomId;
        io.WriteLine($"You move {direction}.");

        if (state.WinRoomId is not null && state.CurrentRoomId == state.WinRoomId)
        {
            state.HasWon = true;
            state.IsRunning = false;
            return;
        }

        LookCommand.DescribeRoom(state.CurrentRoom, io, state);
    }
}