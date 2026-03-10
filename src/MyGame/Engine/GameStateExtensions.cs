namespace MyGame.Engine;

using MyGame.Models;

public static class GameStateExtensions
{
    /// <summary>
    /// Searches the current room first, then the player's inventory.
    /// Match is by exact ID (case-insensitive) or partial name (case-insensitive).
    /// </summary>
    public static Item? FindItem(this GameState state, string noun) =>
        state.FindRoomItem(noun) ?? state.FindInventoryItem(noun);

    /// <summary>
    /// Searches only the current room's items.
    /// Match is by exact ID (case-insensitive) or partial name (case-insensitive).
    /// </summary>
    public static Item? FindRoomItem(this GameState state, string noun) =>
        state.CurrentRoom.Items.FirstOrDefault(i => MatchesNoun(i, noun));

    /// <summary>
    /// Searches only the player's inventory.
    /// Match is by exact ID (case-insensitive) or partial name (case-insensitive).
    /// </summary>
    public static Item? FindInventoryItem(this GameState state, string noun) =>
        state.Inventory.FirstOrDefault(i => MatchesNoun(i, noun));

    private static bool MatchesNoun(Item item, string noun) =>
        item.Id.Equals(noun, StringComparison.OrdinalIgnoreCase) ||
        item.Name.Contains(noun, StringComparison.OrdinalIgnoreCase);
}
