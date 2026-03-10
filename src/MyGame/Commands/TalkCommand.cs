namespace MyGame.Commands;

using MyGame.Engine;
using MyGame.Models;

public class TalkCommand : ICommand
{
    public string Verb => "talk";
    public string[] Aliases => ["speak"];
    public string HelpText => "Talk to someone. Usage: talk [npc]";

    public void Execute(ParsedCommand command, GameState state, IInputOutput io)
    {
        if (string.IsNullOrWhiteSpace(command.Noun))
        {
            io.WriteLine(GameMessages.Talk.ToWhom);
            return;
        }

        var noun = command.Noun.Trim();
        if (noun.StartsWith("to ", StringComparison.OrdinalIgnoreCase))
            noun = noun[3..].Trim();

        if (string.IsNullOrWhiteSpace(noun))
        {
            io.WriteLine(GameMessages.Talk.ToWhom);
            return;
        }

        var npc = state.CurrentRoom.Npcs.FirstOrDefault(n =>
            n.Id.Equals(noun, StringComparison.OrdinalIgnoreCase)
            || n.Name.Contains(noun, StringComparison.OrdinalIgnoreCase));

        if (npc is null)
        {
            io.WriteLine($"No one named \"{noun}\" is here.");
            return;
        }

        var nodes = npc.Dialogue.ToDictionary(node => node.Id, StringComparer.OrdinalIgnoreCase);
        var current = npc.Dialogue.FirstOrDefault();
        if (current is null)
        {
            io.WriteLine($"{ColorConsole.Yellow(npc.Name)} has nothing to say.");
            return;
        }

        state.Flags.Add($"{npc.Id}_met");

        while (true)
        {
            io.WriteLine($"{ColorConsole.Yellow(npc.Name)}: {current.Text}");
            if (current.SetsFlag is not null)
                state.Flags.Add(current.SetsFlag);

            if (current.Responses.Count == 0)
                return;

            for (var i = 0; i < current.Responses.Count; i++)
                io.WriteLine($"  {ColorConsole.Green($"{i + 1}.")} {current.Responses[i].Text}");

            io.Write(GameMessages.Prompts.DialogueInput);
            var input = io.ReadLine();
            if (!int.TryParse(input, out var choice) || choice < 1 || choice > current.Responses.Count)
            {
                io.WriteLine(GameMessages.Talk.EndConversation);
                return;
            }

            var response = current.Responses[choice - 1];
            if (response.NextNodeId is null)
            {
                io.WriteLine(GameMessages.Talk.Goodbye);
                return;
            }

            if (!nodes.TryGetValue(response.NextNodeId, out current))
            {
                io.WriteLine(GameMessages.Talk.Goodbye);
                return;
            }
        }
    }
}
