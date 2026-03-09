namespace MyGame.Engine;

public class Parser
{
    public ParsedCommand Parse(string input) => CommandParser.Parse(input);
}
