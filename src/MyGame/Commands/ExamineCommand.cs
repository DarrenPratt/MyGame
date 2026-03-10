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

        io.WriteLine(ColorConsole.Error($"You don't see any \"{command.Noun}\" to examine."));
    }
}
