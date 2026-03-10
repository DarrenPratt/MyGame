namespace MyGame.Models;

public class Npc
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public List<DialogueNode> Dialogue { get; init; } = new();
}

public class DialogueNode
{
    public required string Id { get; init; }
    public required string Text { get; init; }
    public string? SetsFlag { get; init; }
    public List<DialogueResponse> Responses { get; init; } = new();
}

public class DialogueResponse
{
    public required string Text { get; init; }
    public string? NextNodeId { get; init; }
}
