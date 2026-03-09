namespace MyGame.Commands;

using MyGame.Engine;

public class TakeCommand : ICommand
{
    public string Verb => "take";
    public string[] Aliases => ["get", "pick", "grab"];
    public string HelpText => "Pick up an item. Usage: take <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is null)
        {
            io.WriteLine("Take what?");
            return;
        }

        var item = state.CurrentRoom.Items.FirstOrDefault(i =>
            i.Id.Equals(command.Noun, StringComparison.OrdinalIgnoreCase) ||
            i.Name.Contains(command.Noun, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            io.WriteLine($"There's no \"{command.Noun}\" here to take.");
            return;
        }

        if (!item.CanPickUp)
        {
            io.WriteLine($"The {item.Name} can't be taken.");
            return;
        }

        state.CurrentRoom.Items.Remove(item);
        state.Inventory.Add(item);

        if (item.Id == "data_chip")
            io.WriteLine("Your hand trembles as you pocket the chip. Years of work, dead contacts, all leading to this moment.");
        else
            io.WriteLine($"You pick up the {ColorConsole.Yellow(item.Name)}.");
    }
}
