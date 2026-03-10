namespace MyGame.Commands;

using MyGame.Engine;
using MyGame.Models;

public class UseCommand : ICommand
{
    public string Verb => "use";
    public string HelpText => "Use an item from your inventory. Usage: use <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is null)
        {
            io.WriteLine(ColorConsole.Error("Use what?"));
            return;
        }

        var item = state.FindInventoryItem(command.Noun);

        if (item is null)
        {
            io.WriteLine(ColorConsole.Error($"You don't have \"{command.Noun}\" in your inventory."));
            return;
        }

        // Try to unlock a locked exit in the current room that requires this item
        Exit? matchingExit = null;
        if (!string.IsNullOrWhiteSpace(command.Target))
        {
            matchingExit = state.CurrentRoom.Exits.Values.FirstOrDefault(e =>
                e.IsLocked
                && e.RequiredItemId == item.Id
                && (e.Direction.Equals(command.Target, StringComparison.OrdinalIgnoreCase)
                    || (!string.IsNullOrWhiteSpace(e.Description)
                        && e.Description.Contains(command.Target, StringComparison.OrdinalIgnoreCase))));
        }

        matchingExit ??= state.CurrentRoom.Exits.Values
            .FirstOrDefault(e => e.IsLocked && e.RequiredItemId == item.Id);

        if (matchingExit is not null)
        {
            matchingExit.IsLocked = false;
            state.Flags.Add($"{item.Id}_used");
            io.WriteLine(item.UseMessage ?? $"You use the {item.Name}. The way forward opens.");
            return;
        }

        // Flag-based use (informational/story items)
        if (item.UseTargetId is not null)
        {
            if (state.Flags.Contains(item.UseTargetId))
            {
                io.WriteLine($"You've already used the {item.Name}.");
                return;
            }
            state.Flags.Add(item.UseTargetId);
            io.WriteLine(item.UseMessage ?? $"You use the {item.Name}.");
            return;
        }

        io.WriteLine(ColorConsole.Error($"You're not sure how to use the {item.Name} here."));
    }
}
