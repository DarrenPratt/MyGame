namespace MyGame.Commands;

using MyGame.Engine;

public class HelpCommand : ICommand
{
    private readonly CommandRegistry _registry;

    public HelpCommand(CommandRegistry registry)
    {
        _registry = registry;
    }

    public string Verb => "help";
    public string[] Aliases => ["?", "commands"];
    public string HelpText => "Show available commands.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        io.WriteLine(GameMessages.Help.Header);
        foreach (var cmd in _registry.AllCommands.OrderBy(c => c.Verb))
            io.WriteLine($"  {cmd.Verb,-12} {cmd.HelpText}");
        io.WriteLine(GameMessages.Help.Directions);
    }
}
