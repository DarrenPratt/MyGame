using MyGame.Engine;

namespace MyGame.Tests.Helpers;

/// <summary>
/// Test double for IInputOutput. Feeds pre-queued inputs and captures all output lines.
/// </summary>
public class FakeInputOutput : IInputOutput
{
    private readonly Queue<string?> _inputs;
    private readonly List<string> _lines = new();

    public FakeInputOutput(params string?[] inputs)
    {
        _inputs = new Queue<string?>(inputs);
    }

    public string? ReadLine() => _inputs.Count > 0 ? _inputs.Dequeue() : null;

    public void WriteLine(string text) => _lines.Add(text);

    public void Write(string text) => _lines.Add(text);

    /// <summary>All captured output joined with newlines.</summary>
    public string AllOutput => string.Join("\n", _lines);

    /// <summary>Individual output entries in order.</summary>
    public IReadOnlyList<string> Lines => _lines;

    /// <summary>True if any captured line contains the given substring (case-insensitive).</summary>
    public bool OutputContains(string substring) =>
        _lines.Any(l => l.Contains(substring, StringComparison.OrdinalIgnoreCase));
}
