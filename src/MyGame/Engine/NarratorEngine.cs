namespace MyGame.Engine;

using MyGame.Models;

public static class NarratorEngine
{
    public static NarratorVariant? GetVariant(Room room, GameState state)
    {
        return room.NarratorVariants
            .Where(variant => variant.RequiredFlags.All(flag => state.Flags.Contains(flag))
                && variant.RequiredInventoryItems.All(id => state.Inventory.Any(item => item.Id == id)))
            .OrderByDescending(variant => variant.RequiredFlags.Count + variant.RequiredInventoryItems.Count)
            .FirstOrDefault();
    }

    public static string GetDescription(Room room, GameState state)
    {
        return GetVariant(room, state)?.Description ?? room.Description;
    }
}
