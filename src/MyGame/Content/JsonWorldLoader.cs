namespace MyGame.Engine;

using System.Text.Json;
using MyGame.Models;

public record LoadedWorld(
    GameState State,
    string WinRoomId,
    string WinMessage,
    string Title,
    string Subtitle,
    string IntroText)
{
    public string CurrentRoomId => State.CurrentRoomId;
    public Dictionary<string, Room> Rooms => State.Rooms;
    public Dictionary<string, Item> ItemCatalog => State.ItemCatalog;
    public Dictionary<string, Npc> NpcCatalog => State.NpcCatalog;
}

public class JsonWorldLoader
{
    public LoadedWorld Load(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var definition = JsonSerializer.Deserialize<WorldDefinition>(json, options)
            ?? throw new InvalidOperationException("World definition could not be loaded.");

        var itemCatalog = definition.Items.ToDictionary(item => item.Id);
        var npcCatalog = definition.Npcs.ToDictionary(npc => npc.Id);
        var rooms = new Dictionary<string, Room>();

        foreach (var roomDefinition in definition.Rooms)
        {
            var room = new Room
            {
                Id = roomDefinition.Id,
                Name = roomDefinition.Name,
                Description = roomDefinition.Description,
                NarratorVariants = roomDefinition.NarratorVariants ?? new List<NarratorVariant>(),
                Npcs = new List<Npc>()
            };

            if (roomDefinition.Exits is not null)
            {
                foreach (var exit in roomDefinition.Exits)
                    room.Exits[exit.Direction] = exit;
            }

            var itemIds = new List<string>();
            if (roomDefinition.ItemIds is not null)
                itemIds.AddRange(roomDefinition.ItemIds);
            if (roomDefinition.Items is not null)
                itemIds.AddRange(roomDefinition.Items);

            foreach (var itemId in itemIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (itemCatalog.TryGetValue(itemId, out var item))
                    room.Items.Add(item);
            }

            var npcIds = new List<string>();
            if (roomDefinition.NpcIds is not null)
                npcIds.AddRange(roomDefinition.NpcIds);
            if (roomDefinition.Npcs is not null)
                npcIds.AddRange(roomDefinition.Npcs);

            foreach (var npcId in npcIds.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (npcCatalog.TryGetValue(npcId, out var npc))
                    room.Npcs.Add(npc);
            }

            rooms[room.Id] = room;
        }

        var state = new GameState
        {
            CurrentRoomId = definition.StartRoomId,
            ItemCatalog = itemCatalog,
            NpcCatalog = npcCatalog,
            Rooms = rooms,
            WinRoomId = definition.WinRoomId
        };

        return new LoadedWorld(
            state,
            definition.WinRoomId,
            definition.WinMessage,
            definition.Title,
            definition.Subtitle,
            definition.IntroText);
    }

    private class WorldDefinition
    {
        public string StartRoomId { get; set; } = string.Empty;
        public string WinRoomId { get; set; } = string.Empty;
        public string WinMessage { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string IntroText { get; set; } = string.Empty;
        public List<RoomDefinition> Rooms { get; set; } = new();
        public List<Item> Items { get; set; } = new();
        public List<Npc> Npcs { get; set; } = new();
    }

    private class RoomDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<NarratorVariant>? NarratorVariants { get; set; } = new();
        public List<Exit>? Exits { get; set; } = new();
        public List<string>? ItemIds { get; set; } = new();
        public List<string>? Items { get; set; } = new();
        public List<string>? NpcIds { get; set; } = new();
        public List<string>? Npcs { get; set; } = new();
    }
}
