namespace MyGame.Commands;

using MyGame.Engine;

public class InventoryCommand : ICommand
{
    public string Verb => "inventory";
    public string[] Aliases => ["inv", "i"];
    public string HelpText => "Show what you're carrying.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (state.Inventory.Count == 0)
        {
            io.WriteLine(GameMessages.Inventory.Empty);
            return;
        }

        io.WriteLine(GameMessages.Inventory.Header);
        foreach (var item in state.Inventory)
            io.WriteLine($"  - {ColorConsole.Yellow(item.Name)}");
    }
}
