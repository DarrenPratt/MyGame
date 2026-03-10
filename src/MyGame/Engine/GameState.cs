namespace MyGame.Engine;

using MyGame.Models;

public class GameState
{
    public required string CurrentRoomId { get; set; }
    public List<Item> Inventory { get; } = new();
    public HashSet<string> Flags { get; } = new();
    public bool IsRunning { get; set; } = true;
    public bool HasWon { get; set; }
    public bool HasLost { get; set; } = false;
    public int DroneThreatLevel { get; set; } = 0;
    public HashSet<string> HighRiskRoomIds { get; init; } = new(StringComparer.OrdinalIgnoreCase) { "plaza", "checkpoint" };
    public int DroneThreatThreshold { get; init; } = 4;
    public string? WinRoomId { get; set; } = "server";

    private Dictionary<string, Room> _rooms = new();
    private Dictionary<string, Item> _itemCatalog = new();
    private Dictionary<string, Npc> _npcCatalog = new();

    public Dictionary<string, Item> ItemCatalog
    {
        get => _itemCatalog;
        init => _itemCatalog = value ?? new Dictionary<string, Item>();
    }

    public Dictionary<string, Npc> NpcCatalog
    {
        get => _npcCatalog;
        init => _npcCatalog = value ?? new Dictionary<string, Npc>();
    }

    public required Dictionary<string, Room> Rooms
    {
        get => _rooms;
        init
        {
            _rooms = value ?? new Dictionary<string, Room>();
            if (_itemCatalog.Count == 0)
            {
                foreach (var item in _rooms.Values.SelectMany(room => room.Items))
                {
                    if (!_itemCatalog.ContainsKey(item.Id))
                        _itemCatalog[item.Id] = item;
                }
            }

            if (_npcCatalog.Count == 0)
            {
                foreach (var npc in _rooms.Values.SelectMany(room => room.Npcs))
                {
                    if (!_npcCatalog.ContainsKey(npc.Id))
                        _npcCatalog[npc.Id] = npc;
                }
            }
        }
    }

    public Room CurrentRoom => Rooms[CurrentRoomId];
}
