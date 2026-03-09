namespace MyGame.Engine;

public static class ColorConsole
{
    private const string Reset = "\x1b[0m";

    public static string Cyan(string text) => $"\x1b[36m{text}{Reset}";
    public static string Yellow(string text) => $"\x1b[33m{text}{Reset}";
    public static string Green(string text) => $"\x1b[32m{text}{Reset}";
    public static string Red(string text) => $"\x1b[31m{text}{Reset}";
    public static string DarkGray(string text) => $"\x1b[90m{text}{Reset}";
    public static string Magenta(string text) => $"\x1b[35m{text}{Reset}";
    public static string Bold(string text) => $"\x1b[1m{text}{Reset}";
    public static string BoldCyan(string text) => $"\x1b[1;36m{text}{Reset}";
}
