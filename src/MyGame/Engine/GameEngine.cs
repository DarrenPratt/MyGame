namespace MyGame.Engine;

using MyGame.Commands;

public interface IInputOutput
{
    string? ReadLine();
    void WriteLine(string text);
    void Write(string text);
}

public class ConsoleIO : IInputOutput
{
    public string? ReadLine() => Console.ReadLine();
    public void WriteLine(string text) => Console.WriteLine(text);
    public void Write(string text) => Console.Write(text);
}

public class GameEngine
{
    private readonly GameState _state;
    private readonly CommandRegistry _commands;
    private readonly IInputOutput _io;
    private readonly LoadedWorld? _world;

    public GameEngine(GameState state, CommandRegistry commands, IInputOutput io, LoadedWorld? world = null)
    {
        _state = state;
        _commands = commands;
        _io = io;
        _world = world;
    }

    public void Run()
    {
        var title = _world?.Title ?? "N E O N   L E D G E R";
        var subtitle = _world?.Subtitle ?? "A Cyberpunk Text Adventure";
        var introText = _world?.IntroText
            ?? "You've been hired to infiltrate SynthCorp's data vaults and retrieve stolen research.\n" +
               "Your fixer's last message: \"Package in the corp system. Get in, get the drive, get out.\"\n" +
               "You start in the back alley with nothing but your wits and a job to do.";

        var contentWidth = Math.Max(title.Length, subtitle.Length);
        var topBorder = "╔" + new string('═', contentWidth + 2) + "╗";
        var bottomBorder = "╚" + new string('═', contentWidth + 2) + "╝";

        _io.WriteLine(ColorConsole.Cyan(topBorder));
        _io.WriteLine(ColorConsole.BoldCyan($"║ {CenterText(title, contentWidth)} ║"));
        _io.WriteLine(ColorConsole.Cyan($"║ {CenterText(subtitle, contentWidth)} ║"));
        _io.WriteLine(ColorConsole.Cyan(bottomBorder));
        _io.WriteLine("");

        foreach (var line in SplitLines(introText))
            _io.WriteLine(line);
        _io.WriteLine("");

        _commands.Execute(new ParsedCommand("look", null), _state, _io);

        while (_state.IsRunning)
        {
            _io.Write(ColorConsole.Prompt("\n> "));
            var input = _io.ReadLine();
            if (input is null)
                break;

            var parsed = CommandParser.Parse(input);
            if (string.IsNullOrEmpty(parsed.Verb))
                continue;

            _commands.Execute(parsed, _state, _io);
        }

        _io.WriteLine("");
        if (_state.HasWon)
        {
            if (!string.IsNullOrWhiteSpace(_world?.WinMessage))
            {
                foreach (var line in SplitLines(_world.WinMessage))
                    _io.WriteLine(line);
            }
            else
            {
                _io.WriteLine("You've done it. The SynthCorp data drive is in your hands—real, tangible proof");
                _io.WriteLine("of what they've been hiding. As you slip out through the service corridor, corporate");
                _io.WriteLine("security drones sweep the upper levels. They haven't spotted you. Not yet.");
                _io.WriteLine("In your pocket, the drive pulses with cold data. You smile—this changes everything.");
            }

            _io.WriteLine("");
            _io.WriteLine(ColorConsole.Magenta("*** YOU WIN. The neon city is yours. ***"));
        }
        else
        {
            _io.WriteLine(ColorConsole.DarkGray("*** JACKED OUT. See you in the sprawl. ***"));
        }
        _io.WriteLine("");
    }

    private static IEnumerable<string> SplitLines(string text) =>
        text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text;

        var padding = width - text.Length;
        var padLeft = padding / 2;
        var padRight = padding - padLeft;
        return new string(' ', padLeft) + text + new string(' ', padRight);
    }
}
