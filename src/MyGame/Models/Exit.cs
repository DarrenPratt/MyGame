namespace MyGame.Models;

public class Exit
{
    public required string Direction { get; init; }
    public required string TargetRoomId { get; init; }
    public string? Description { get; init; }
    public bool IsLocked { get; set; }
    public string? RequiredItemId { get; set; }
}
