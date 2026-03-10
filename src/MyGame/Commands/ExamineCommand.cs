namespace MyGame.Commands;

using MyGame.Engine;

public class ExamineCommand : ICommand
{
    public string Verb => "examine";
    public string[] Aliases => ["x", "inspect", "read"];
    public string HelpText => "Examine an item closely. Usage: examine <item>";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is null)
        {
            io.WriteLine(ColorConsole.Error(GameMessages.Examine.NoItem));
            return;
        }

        var item = state.FindItem(command.Noun);
        if (item is not null)
        {
            io.WriteLine(item.Description);
            return;
        }

        var npc = state.CurrentRoom.Npcs.FirstOrDefault(n =>
            n.Id.Equals(command.Noun, StringComparison.OrdinalIgnoreCase)
            || n.Name.Contains(command.Noun, StringComparison.OrdinalIgnoreCase));
        if (npc is not null)
        {
            io.WriteLine(ColorConsole.Flavor(npc.Description));
            return;
        }

        io.WriteLine(ColorConsole.Error($"You don't see any \"{command.Noun}\" here."));
    }
}