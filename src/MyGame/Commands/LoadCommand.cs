namespace MyGame.Commands;

using System.Text.Json;
using MyGame.Engine;

public class LoadCommand : ICommand
{
    private readonly string _baseDirectory;

    public LoadCommand(string? baseDirectory = null)
    {
        _baseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? Directory.GetCurrentDirectory()
            : baseDirectory;
    }

    public string Verb => "load";
    public string[] Aliases => [];
    public string HelpText => "Load a saved game. Usage: load [filename]";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        var filename = string.IsNullOrWhiteSpace(command.Noun) ? "savegame.json" : command.Noun.Trim();
        if (!filename.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            filename += ".json";
        filename = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(filename))
            filename = "savegame.json";

        var path = Path.Combine(_baseDirectory, filename);
        if (!File.Exists(path))
        {
            io.WriteLine($"Save file not found: {path}");
            return;
        }

        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<SaveData>(json, options);
            if (data is null)
            {
                io.WriteLine("Save file is corrupted.");
                return;
            }

            if (data.CurrentRoomId is null || !state.Rooms.ContainsKey(data.CurrentRoomId))
            {
                io.WriteLine("Saved room no longer exists in this world.");
                return;
            }

            state.CurrentRoomId = data.CurrentRoomId;

            state.Flags.Clear();
            foreach (var flag in data.Flags ?? [])
                state.Flags.Add(flag);

            state.Inventory.Clear();
            foreach (var itemId in data.Inventory ?? [])
            {
                if (state.ItemCatalog.TryGetValue(itemId, out var item))
                    state.Inventory.Add(item);
            }

            io.WriteLine("Game loaded.");
        }
        catch (Exception ex)
        {
            io.WriteLine($"Error loading save file: {ex.Message}");
        }
    }

    private record SaveData(string? CurrentRoomId, List<string>? Inventory, List<string>? Flags);
}
