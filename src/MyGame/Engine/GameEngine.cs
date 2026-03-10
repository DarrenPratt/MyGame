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
    private GameState _state;
    private readonly CommandRegistry _commands;
    private readonly IInputOutput _io;
    private readonly LoadedWorld? _world;
    private readonly Func<GameState>? _stateFactory;

    public GameEngine(GameState state, CommandRegistry commands, IInputOutput io, LoadedWorld? world = null, Func<GameState>? stateFactory = null)
    {
        _state = state;
        _commands = commands;
        _io = io;
        _world = world;
        _stateFactory = stateFactory;
    }

    public void Run()
    {
        while (true)
        {
            RunSession();

            if (_state.HasLost && _stateFactory is not null)
            {
                _io.Write(ColorConsole.Yellow(GameMessages.Prompts.TryAgain));
                var answer = _io.ReadLine();
                if (answer is not null && answer.StartsWith("y", StringComparison.OrdinalIgnoreCase))
                {
                    _state = _stateFactory();
                    continue;
                }
            }

            break;
        }
    }

    private void RunSession()
    {
        var title = _world?.Title ?? GameMessages.Defaults.Title;
        var subtitle = _world?.Subtitle ?? GameMessages.Defaults.Subtitle;
        var introText = _world?.IntroText ?? GameMessages.Defaults.IntroText;

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
            _io.Write(ColorConsole.Prompt(GameMessages.Prompts.CommandInput));
            var input = _io.ReadLine();
            if (input is null)
                break;

            var parsed = CommandParser.Parse(input);
            if (string.IsNullOrEmpty(parsed.Verb))
                continue;

            var prevRoomId = _state.CurrentRoomId;
            _commands.Execute(parsed, _state, _io);

            // Drone threat check — runs after every command
            if (_state.IsRunning && _state.HighRiskRoomIds.Contains(_state.CurrentRoomId))
            {
                _state.DroneThreatLevel++;
                if (_state.DroneThreatLevel == 1)
                    _io.WriteLine(ColorConsole.Error(GameMessages.Drone.Warning1));
                else if (_state.DroneThreatLevel == 2)
                    _io.WriteLine(ColorConsole.Error(GameMessages.Drone.Warning2));
                else if (_state.DroneThreatLevel == 3)
                    _io.WriteLine(ColorConsole.Error(GameMessages.Drone.Warning3));
                else if (_state.DroneThreatLevel >= _state.DroneThreatThreshold)
                {
                    _state.HasLost = true;
                    _state.IsRunning = false;
                }
            }
        }

        _io.WriteLine("");
        if (_state.HasWon)
        {
            var winText = !string.IsNullOrWhiteSpace(_world?.WinMessage)
                ? _world!.WinMessage
                : GameMessages.Win.DefaultMessage;
            foreach (var line in SplitLines(winText))
                _io.WriteLine(line);

            _io.WriteLine("");
            _io.WriteLine(ColorConsole.Magenta(GameMessages.Win.Banner));
        }
        else if (_state.HasLost)
        {
            var loseText = !string.IsNullOrWhiteSpace(_world?.LoseMessage)
                ? _world!.LoseMessage
                : GameMessages.Lose.DefaultMessage;
            foreach (var line in SplitLines(loseText))
                _io.WriteLine(line);

            _io.WriteLine("");
            _io.WriteLine(ColorConsole.Error(GameMessages.Lose.Banner));
        }
        else
        {
            _io.WriteLine(ColorConsole.DarkGray(GameMessages.Quit.Banner));
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
