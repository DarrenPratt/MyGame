namespace MyGame.Commands;

using MyGame.Engine;
using MyGame.Models;

public class LookCommand : ICommand
{
    public string Verb => "look";
    public string[] Aliases => ["l"];
    public string HelpText => "Look around the current room. Usage: look [item]";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (command.Noun is not null)
        {
            var item = FindItem(command.Noun, state);
            if (item is not null)
            {
                io.WriteLine(item.Description);
                return;
            }
            io.WriteLine(ColorConsole.Error($"You don't see any \"{command.Noun}\" here."));
            return;
        }

        DescribeRoom(state.CurrentRoom, io, state);
    }

    internal static void DescribeRoom(Room room, IInputOutput io)
    {
        DescribeRoom(room, io, null);
    }

    internal static void DescribeRoom(Room room, IInputOutput io, GameState? state)
    {
        string description;
        bool isVariant = false;

        if (state is null)
        {
            description = room.Description;
        }
        else
        {
            var variant = NarratorEngine.GetVariant(room, state);
            isVariant = variant is not null;
            description = variant?.Description ?? room.Description;
        }

        io.WriteLine($"\n{ColorConsole.BoldCyan(room.Name)}");
        io.WriteLine(ColorConsole.Cyan(new string('─', room.Name.Length)));
        io.WriteLine(isVariant ? ColorConsole.Flavor(description) : ColorConsole.RoomDescription(description));

        if (room.Items.Count > 0)
        {
            io.WriteLine("");
            io.WriteLine("Items here: " + string.Join(", ", room.Items.Select(i => ColorConsole.Yellow(i.Name))));
        }

        if (room.Npcs.Count > 0)
        {
            io.WriteLine("");
            io.WriteLine("You see here: " + string.Join(", ", room.Npcs.Select(npc => ColorConsole.Yellow(npc.Name))));
        }

        if (room.Exits.Count > 0)
        {
            io.WriteLine("");
            io.WriteLine("Exits: " + string.Join(", ", room.Exits.Keys.OrderBy(k => k)
                .Select(exit => ColorConsole.Green(exit))));
        }
    }

    private static Item? FindItem(string noun, GameState state)
    {
        return state.CurrentRoom.Items
            .Concat(state.Inventory)
            .FirstOrDefault(i =>
                i.Id.Equals(noun, StringComparison.OrdinalIgnoreCase) ||
                i.Name.Contains(noun, StringComparison.OrdinalIgnoreCase));
    }
}
