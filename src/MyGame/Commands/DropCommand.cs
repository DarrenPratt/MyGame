namespace MyGame.Commands;

using MyGame.Engine;

public class DropCommand : ICommand
{
    public string Verb => "drop";
    public string HelpText => "Drop an item from your inventory. Usage: drop <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is null)
        {
            io.WriteLine(ColorConsole.Error(GameMessages.Drop.NoItem));
            return;
        }

        var item = state.FindInventoryItem(command.Noun);

        if (item is null)
        {
            io.WriteLine($"You're not carrying \"{command.Noun}\".");
            return;
        }

        state.Inventory.Remove(item);
        state.CurrentRoom.Items.Add(item);
        io.WriteLine($"You drop the {ColorConsole.Yellow(item.Name)}.");
    }
}
