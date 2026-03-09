namespace MyGame.Content;

using MyGame.Engine;
using MyGame.Models;

public static class WorldBuilder
{
    public static GameState Build()
    {
        var rooms = CreateRooms();
        LinkRooms(rooms);
        PlaceItems(rooms);

        return new GameState
        {
            CurrentRoomId = "alley",
            Rooms = rooms
        };
    }

    private static Dictionary<string, Room> CreateRooms()
    {
        return new Dictionary<string, Room>
        {
            ["alley"] = new()
            {
                Id = "alley",
                Name = "Neon Alley",
                Description =
                    "Narrow, grimy, reeking of fermented synth-fuel and old blood. Fire escapes crisscross overhead, " +
                    "draped with dried cables and abandoned server boxes. A faded graffito glows faintly: \"THE LATTICE REMEMBERS.\" " +
                    "Puddles reflect fractured neon from the street beyond. This is where runners hide when the heat gets too close. " +
                    "You can see a bar sign glowing to the east."
            },
            ["bar"] = new()
            {
                Id = "bar",
                Name = "The Byte Bar",
                Description =
                    "A cramped hole-in-the-wall bar wedged between two corporate transit towers. " +
                    "Holographic drink menus flicker across scratched tables, casting sickly blue and pink light across everything. " +
                    "The bartender—a chrome-jawed synth with dead eyes—polishes glasses that never quite get clean. " +
                    "Rain drums against reinforced windows overlooking the street below. A fire escape leads up to the rooftop."
            },
            ["rooftop"] = new()
            {
                Id = "rooftop",
                Name = "Rooftop Spans — Runner's Route",
                Description =
                    "A network of interconnected rooftops spanning three city blocks. Wind buffets the structure, " +
                    "carrying sounds of traffic and distant sirens. Makeshift bridges and zip-lines connect the buildings, " +
                    "weathered by constant use. From here, you can see all of Night City—neon towers stretching to the horizon, " +
                    "corporate megastructures looming like glass mountains. The freedom of the open air contrasts with the oppression below."
            },
            ["lobby"] = new()
            {
                Id = "lobby",
                Name = "Corp Lobby",
                Description =
                    "All chrome and cold steel, a brutalist monument to corporate power. The lobby spans three levels, " +
                    "filled with workers in expensive suits and security personnel in tactical gear. " +
                    "Holographic advertisements for neural implants pulse from every surface. " +
                    "A security door to the north leads deeper into the facility. Security cameras track every movement."
            },
            ["server"] = new()
            {
                Id = "server",
                Name = "Server Room — SynthCorp Research Archive",
                Description =
                    "A labyrinthine archive hidden beneath the corporate tower, sealed behind layers of encryption. " +
                    "Servers hum in endless rows, their light panels casting everything in harsh white and blue. " +
                    "Files and data are stored on holographic displays. This is where your target waits—the drive containing research " +
                    "that could bring down SynthCorp's entire neural implant division. The air is cold, precise, sterile."
            }
        };
    }

    private static void LinkRooms(Dictionary<string, Room> rooms)
    {
        // alley ↔ bar (east/west)
        rooms["alley"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "bar" };
        rooms["bar"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "alley" };

        // bar ↔ lobby (east/west)
        rooms["bar"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "lobby" };
        rooms["lobby"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "bar" };

        // bar ↔ rooftop (up/down)
        rooms["bar"].Exits["up"] = new Exit { Direction = "up", TargetRoomId = "rooftop" };
        rooms["rooftop"].Exits["down"] = new Exit { Direction = "down", TargetRoomId = "bar" };

        // lobby ↔ server (north/south) — north is LOCKED until keycard is used
        rooms["lobby"].Exits["north"] = new Exit
        {
            Direction = "north",
            TargetRoomId = "server",
            IsLocked = true,
            RequiredItemId = "keycard",
            Description = "A heavy security door. A card reader glows red beside it."
        };
        rooms["server"].Exits["south"] = new Exit { Direction = "south", TargetRoomId = "lobby" };
    }

    private static void PlaceItems(Dictionary<string, Room> rooms)
    {
        // alley — a crumpled flyer hints at the bar
        rooms["alley"].Items.Add(new Item
        {
            Id = "flyer",
            Name = "Crumpled Flyer",
            Description =
                "A crumpled flyer wedged under a fire escape. The logo reads 'THE BYTE BAR — one block east.' " +
                "Someone's scrawled underneath: 'Ask for the back room. Bring the key.' " +
                "Useful for orientation, useless for anything else.",
            CanPickUp = true
        });

        // bar — a fried terminal, scenery only
        rooms["bar"].Items.Add(new Item
        {
            Id = "terminal",
            Name = "Broken Terminal",
            Description =
                "A heavy terminal from decades past, still glowing faintly but fried beyond repair. " +
                "The screen loops a SynthCorp logo from twenty years ago. Bolted to the bar, immovable. " +
                "The runner community uses it as a landmark.",
            CanPickUp = false
        });

        // rooftop — the keycard needed to unlock the server room
        rooms["rooftop"].Items.Add(new Item
        {
            Id = "keycard",
            Name = "Corp Keycard",
            Description =
                "A corporate keycard, worn but functional. SynthCorp Security Division — badge number erased with acid. " +
                "The RFID chip inside still works. Someone dropped this in a hurry. " +
                "It looks like it would open a security door.",
            CanPickUp = true,
            UseMessage = "The keycard slides into the reader. The light flicks from red to green. A soft click—the security door north is open."
        });

        // server — the prize
        rooms["server"].Items.Add(new Item
        {
            Id = "drive",
            Name = "Data Drive",
            Description =
                "A sleek data drive etched with corporate security markers. It contains the research data " +
                "that could expose SynthCorp's illegal neural implant experiments. " +
                "This is what you came for. Years of work, dead contacts, all leading to this moment.",
            CanPickUp = true
        });
    }
}
