namespace MyGame.Commands;

using MyGame.Engine;

public class QuitCommand : ICommand
{
    public string Verb => "quit";
    public string[] Aliases => ["exit", "q"];
    public string HelpText => "Quit the game.";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        io.WriteLine(GameMessages.Quit.Message);
        state.IsRunning = false;
    }
}
