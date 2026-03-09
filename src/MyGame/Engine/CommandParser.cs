namespace MyGame.Engine;

public record ParsedCommand(string Verb, string? Noun, string? Target = null);

public static class CommandParser
{
    public static ParsedCommand Parse(string input)
    {
        var trimmed = input.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(trimmed))
            return new ParsedCommand("", null, null);

        var spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex < 0)
            return new ParsedCommand(trimmed, null, null);

        var verb = trimmed[..spaceIndex];
        var noun = trimmed[(spaceIndex + 1)..].Trim();
        if (string.IsNullOrEmpty(noun))
            return new ParsedCommand(verb, null, null);

        string? target = null;
        var onIndex = noun.IndexOf(" on ", StringComparison.Ordinal);
        if (onIndex >= 0)
        {
            target = noun[(onIndex + 4)..].Trim();
            noun = noun[..onIndex].Trim();
        }

        return new ParsedCommand(
            verb,
            string.IsNullOrEmpty(noun) ? null : noun,
            string.IsNullOrEmpty(target) ? null : target);
    }
}
