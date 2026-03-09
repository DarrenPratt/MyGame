namespace MyGame.Models;

public class Item
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public bool CanPickUp { get; init; } = true;
    public string? UseTargetId { get; init; }
    public string? UseMessage { get; init; }
}
