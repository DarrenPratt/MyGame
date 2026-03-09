namespace MyGame.Models;

public class NarratorVariant
{
    public List<string> RequiredFlags { get; init; } = new();
    public List<string> RequiredInventoryItems { get; init; } = new();
    public required string Description { get; init; }
}
