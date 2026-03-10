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
            io.WriteLine(ColorConsole.Error(GameMessages.Take.NoItem));
            return;
        }

        var item = state.FindRoomItem(command.Noun);

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
            io.WriteLine(GameMessages.Take.DataChipPickup);
        else
            io.WriteLine($"You pick up the {ColorConsole.Yellow(item.Name)}.");
    }
}
