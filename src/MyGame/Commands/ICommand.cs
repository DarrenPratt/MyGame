namespace MyGame.Commands;

using MyGame.Engine;

public interface ICommand
{
    string Verb { get; }
    string[] Aliases => [];
    string HelpText { get; }
    void Execute(ParsedCommand command, GameState state, IInputOutput io);
}
