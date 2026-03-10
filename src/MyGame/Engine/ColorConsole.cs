namespace MyGame.Engine;

using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Cyberpunk-themed ANSI colour helpers for console output.
/// Call <see cref="Initialize"/> once at startup to enable ANSI processing on Windows.
/// </summary>
public static class ColorConsole
{
    private const string Reset = "\x1b[0m";

    // ── Low-level colour primitives ──────────────────────────────────────────

    public static string Cyan(string text)     => $"\x1b[36m{text}{Reset}";
    public static string Yellow(string text)   => $"\x1b[33m{text}{Reset}";
    public static string Green(string text)    => $"\x1b[32m{text}{Reset}";
    public static string Red(string text)      => $"\x1b[31m{text}{Reset}";
    public static string DarkGray(string text) => $"\x1b[90m{text}{Reset}";
    public static string Magenta(string text)  => $"\x1b[35m{text}{Reset}";
    public static string Bold(string text)     => $"\x1b[1m{text}{Reset}";
    public static string BoldCyan(string text) => $"\x1b[1;36m{text}{Reset}";

    // ── Semantic cyberpunk theme ─────────────────────────────────────────────

    /// <summary>Room description body — bright cyan, the neon glow of the world.</summary>
    public static string RoomDescription(string text) => $"\x1b[96m{text}{Reset}";

    /// <summary>Error feedback — red, hard and immediate.</summary>
    public static string Error(string text) => $"\x1b[31m{text}{Reset}";

    /// <summary>Input prompt — dim cyan, quietly waiting in the dark.</summary>
    public static string Prompt(string text) => $"\x1b[2;36m{text}{Reset}";

    /// <summary>Flavour / atmospheric text — magenta, the colour of neon signs and corporate logos.</summary>
    public static string Flavor(string text) => $"\x1b[35m{text}{Reset}";

    // ── Windows initialisation ───────────────────────────────────────────────

    /// <summary>
    /// Sets UTF-8 output encoding and, on Windows, enables ANSI virtual terminal
    /// processing so that ANSI escape codes render as colours instead of literal text.
    /// Safe to call on all platforms.
    /// </summary>
    public static void Initialize()
    {
        Console.OutputEncoding = Encoding.UTF8;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            EnableVirtualTerminalProcessing();
    }

    // ── P/Invoke for Windows ENABLE_VIRTUAL_TERMINAL_PROCESSING ─────────────

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private static void EnableVirtualTerminalProcessing()
    {
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        var handle = GetStdHandle(STD_OUTPUT_HANDLE);
        if (handle == IntPtr.Zero || handle == new IntPtr(-1))
            return;

        if (!GetConsoleMode(handle, out var mode))
            return;

        SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
    }
}
