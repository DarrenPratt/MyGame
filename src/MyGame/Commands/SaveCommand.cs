namespace MyGame.Commands;

using System.Text.Json;
using MyGame.Engine;

public class SaveCommand : ICommand
{
    private readonly string _baseDirectory;

    public SaveCommand(string? baseDirectory = null)
    {
        _baseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Directory.GetCurrentDirectory()
            : baseDirectory;
    }

    public string Verb => "save";
    public string[] Aliases => [];
    public string HelpText => "Save your game. Usage: save [filename]";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        var filename = string.IsNullOrWhiteSpace(command.Noun) ? "savegame.json" : command.Noun.Trim();
        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            filename += ".json";
        filename = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(filename))
            filename = "savegame.json";

        var path = Path.Combine(_baseDirectory, filename);
        var exitLockStates = state.Rooms
            .Where(kvp => kvp.Value.Exits.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Exits.ToDictionary(e => e.Key, e => e.Value.IsLocked));

        var data = new SaveData(
            state.CurrentRoomId,
            state.Inventory.Select(item => item.Id).ToList(),
            state.Flags.ToList(),
            state.DroneThreatLevel,
            state.DroneThreatThreshold,
            exitLockStates);
        var options = new JsonSerializerOptions { WriteIndented = true };

        try
        {
            var json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(path, json);
            io.WriteLine($"Game saved to {path}.");
        }
        catch (Exception ex)
        {
            io.WriteLine($"Failed to save game: {ex.Message}");
        }
    }

    private record SaveData(
        string CurrentRoomId,
        List<string> Inventory,
        List<string> Flags,
        int DroneThreatLevel,
        int DroneThreatThreshold,
        Dictionary<string, Dictionary<string, bool>> ExitLockStates);
}
