namespace MyGame.Commands;

using MyGame.Engine;

public class CommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void Register(ICommand command)
    {
        _commands[command.Verb] = command;
        foreach (var alias in command.Aliases)
            _commands[alias] = command;
    }

    public void Execute(ParsedCommand parsed, GameState state, IInputOutput io)
    {
        if (_commands.TryGetValue(parsed.Verb, out var command))
            command.Execute(parsed, state, io);
        else
            io.WriteLine($"Unknown command: \"{parsed.Verb}\". Type \"help\" for commands.");
    }

    public IEnumerable<ICommand> AllCommands => _commands.Values.Distinct();
}
