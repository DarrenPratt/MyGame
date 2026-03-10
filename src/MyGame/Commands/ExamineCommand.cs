namespace MyGame.Commands;

using MyGame.Engine;
using MyGame.Models;

public class ExamineCommand : ICommand
{
    public string Verb => "examine";
    public string[] Aliases => ["x", "inspect", "read"];
    public string HelpText => "Examine an item closely. Usage: examine <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is null)
        {
            io.WriteLine(ColorConsole.Error("Examine what?"));
            return;
        }

        var item = FindItem(command.Noun, state);
        if (item is not null)
        {
            io.WriteLine(item.Description);
            return;
        }

        io.WriteLine(ColorConsole.Error($"You don't see any \"{command.Noun}\" to examine."));
    }

    private static Item? FindItem(string noun, GameState state)
    {
        return state.CurrentRoom.Items
            .Concat(state.Inventory)
            .FirstOrDefault(i =>
                i.Id.Equals(noun, StringComparison.OrdinalIgnoreCase) ||
                i.Name.Contains(noun, StringComparison.OrdinalIgnoreCase));
    }
}
