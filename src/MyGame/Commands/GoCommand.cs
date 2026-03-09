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
            io.WriteLine("Go where? Specify a direction (north, south, east, west, up, down, etc.).");
            return;
        }

        if (Abbreviations.TryGetValue(direction, out var expanded))
            direction = expanded;

        var room = state.CurrentRoom;
        if (!room.Exits.TryGetValue(direction, out var exit))
        {
            io.WriteLine($"You can't go {direction} from here.");
            return;
        }

        if (exit.IsLocked)
        {
            var needed = exit.RequiredItemId is not null
                ? $"You need the {exit.RequiredItemId.Replace('_', ' ')} to proceed."
                : "The way is locked.";
            io.WriteLine($"The way {direction} is blocked. {needed}");
            return;
        }

        state.CurrentRoomId = exit.TargetRoomId;
        io.WriteLine($"You move {direction}.");

        // Win condition: entering the server room
        var winRoomId = state.WinRoomId ?? "server";
        if (state.CurrentRoomId == winRoomId)
        {
            state.HasWon = true;
            state.IsRunning = false;
            io.WriteLine("The server room hums around you. Rows of data towers stretch into the dark.");
            io.WriteLine("You find the drive. Your hand trembles as you pocket it.");
            return;
        }

        LookCommand.DescribeRoom(state.CurrentRoom, io, state);
    }
}
