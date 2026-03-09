namespace MyGame.Models;

public class Room
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public Dictionary<string, Exit> Exits { get; } = new();
    public List<Item> Items { get; } = new();
    public List<NarratorVariant> NarratorVariants { get; init; } = new();
    public List<Npc> Npcs { get; init; } = new();
}
