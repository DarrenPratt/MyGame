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
        PlaceNpcs(rooms);
        AddNarratorVariants(rooms);

        return new GameState
        {
            CurrentRoomId = "alley",
            WinRoomId = "server",
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
                    "Puddles reflect fractured neon from the street beyond—The Byte Bar glows to the east, and tunnel grates lead down into the undercity."
            },
            ["bar"] = new()
            {
                Id = "bar",
                Name = "The Byte Bar",
                Description =
                    "A cramped hole-in-the-wall bar wedged between two corporate transit towers. " +
                    "Holographic drink menus flicker across scratched tables, casting sickly blue and pink light across everything. " +
                    "The bartender—a chrome-jawed synth with dead eyes—polishes glasses that never quite get clean. " +
                    "Rain drums against reinforced windows overlooking the street below."
            },
            ["rooftop"] = new()
            {
                Id = "rooftop",
                Name = "Rooftop Spans — Runner's Route",
                Description =
                    "A network of interconnected rooftops spanning three city blocks. Wind buffets the structure, " +
                    "carrying sounds of traffic and distant sirens. Makeshift bridges and zip-lines connect the buildings, " +
                    "weathered by constant use. From here, you can see all of Night City—neon towers stretching to the horizon, " +
                    "corporate megastructures looming like glass mountains. The freedom of the open air contrasts sharply with the oppression below."
            },
            ["plaza"] = new()
            {
                Id = "plaza",
                Name = "SynthCorp Plaza",
                Description =
                    "All chrome and cold steel, a brutalist monument to corporate power. The plaza spans three levels, " +
                    "filled with corporate workers in expensive suits and security personnel in tactical gear. " +
                    "Holographic advertisements for neural implants and synthetic organs pulse from every surface. " +
                    "A fountain in the center runs with something that isn't quite water. Security cameras are everywhere—being here draws unwanted attention."
            },
            ["checkpoint"] = new()
            {
                Id = "checkpoint",
                Name = "Security Checkpoint",
                Description =
                    "A fortified security checkpoint marks the boundary between the public plaza and the corporate tower. " +
                    "Heavy blast doors frame the entrance. Two guards in tactical gear man the station, their eyes tracking every movement. " +
                    "Surveillance equipment lines the walls. The air crackles with tension—you can hear the hum of weapons charging, drones circling overhead. " +
                    "This is the choke point."
            },
            ["lobby"] = new()
            {
                Id = "lobby",
                Name = "Corp Lobby",
                Description =
                    "All chrome and cold steel, a brutalist monument to corporate power. The lobby spans three levels, " +
                    "filled with workers in expensive suits and security personnel in tactical gear. " +
                    "Holographic advertisements for neural implants pulse from every surface. " +
                    "A security door to the north leads deeper into the facility. Security cameras track every movement. Elevators hum behind frosted glass."
            },
            ["corridor"] = new()
            {
                Id = "corridor",
                Name = "Service Corridor",
                Description =
                    "A narrow, dimly lit corridor runs behind the main lobby. Pipes and cable conduits line the walls, humming with power. " +
                    "Emergency lighting casts everything in sickly green. The air is warm and stale, smelling of ozone and machine oil. " +
                    "This is the worker's route—maintenance staff and low-level corp drones use these passages to move unseen. " +
                    "A back exit sign glows faintly at the far end."
            },
            ["server"] = new()
            {
                Id = "server",
                Name = "Server Room — SynthCorp Research Archive",
                Description =
                    "A labyrinthine archive hidden beneath the corporate tower, sealed behind layers of encryption. " +
                    "Servers hum in endless rows, their light panels casting everything in harsh white and blue. " +
                    "Files and data are stored on holographic displays. This is where your target waits—the drive containing research " +
                    "that could bring down SynthCorp's entire neural implant division. The air is cold, precise, sterile. Condensation forms on your breath."
            },
            ["tunnel"] = new()
            {
                Id = "tunnel",
                Name = "Underground Tunnel Network",
                Description =
                    "You slip down into the undercity—the sprawling network of tunnels that web beneath the city. " +
                    "Glowing cables strung across makeshift shelters cast everything in electric blue. " +
                    "The sound of machinery, voices, and distant music echoes through the passages. " +
                    "The air is thick and warm, smelling of ozone and grease. Runner dens and black-market tech shops cluster here in the shadows. " +
                    "This is where those unwanted by the megacorps have built a parallel civilization."
            },
            ["den"] = new()
            {
                Id = "den",
                Name = "Hacker's Den",
                Description =
                    "A sprawling underground hideout carved into the undercity's bones. " +
                    "Jury-rigged servers and salvaged neural jacks line the walls, their lights pulsing in rhythm. " +
                    "The smell of fried circuits mingles with cheap synth-coffee and stim-sticks. " +
                    "Runners, netrunners, and black-market dealers move through the shadows like ghosts. " +
                    "Information trades hands here—gossip, credentials, access codes. The runners call it the Den. Everyone else just calls it home."
            }
        };
    }

    private static void LinkRooms(Dictionary<string, Room> rooms)
    {
        // alley ↔ bar (east/west)
        rooms["alley"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "bar" };
        rooms["bar"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "alley" };

        // alley ↔ tunnel (down/up)
        rooms["alley"].Exits["down"] = new Exit { Direction = "down", TargetRoomId = "tunnel" };
        rooms["tunnel"].Exits["up"] = new Exit { Direction = "up", TargetRoomId = "alley" };

        // bar ↔ lobby (east/west) — preserved for test compatibility
        rooms["bar"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "lobby" };
        rooms["lobby"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "bar" };

        // bar ↔ plaza (south/north) — bar's east is taken by lobby; use south instead
        rooms["bar"].Exits["south"] = new Exit { Direction = "south", TargetRoomId = "plaza" };
        rooms["plaza"].Exits["north"] = new Exit { Direction = "north", TargetRoomId = "bar" };

        // bar ↔ rooftop (up/down)
        rooms["bar"].Exits["up"] = new Exit { Direction = "up", TargetRoomId = "rooftop" };
        rooms["rooftop"].Exits["down"] = new Exit { Direction = "down", TargetRoomId = "bar" };

        // plaza ↔ checkpoint (east/west) — adjusted since plaza north is taken by bar
        rooms["plaza"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "checkpoint" };
        rooms["checkpoint"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "plaza" };

        // plaza ↔ tunnel (south/north)
        rooms["plaza"].Exits["south"] = new Exit { Direction = "south", TargetRoomId = "tunnel" };
        rooms["tunnel"].Exits["north"] = new Exit { Direction = "north", TargetRoomId = "plaza" };

        // checkpoint ↔ lobby (north/south) — north is LOCKED until cred_chip is used
        rooms["checkpoint"].Exits["north"] = new Exit
        {
            Direction = "north",
            TargetRoomId = "lobby",
            IsLocked = true,
            RequiredItemId = "cred_chip",
            Description = "Reinforced security door. The guard eyes you with contempt. You'll need something to get past him."
        };
        rooms["lobby"].Exits["south"] = new Exit { Direction = "south", TargetRoomId = "checkpoint" };

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

        // lobby ↔ corridor (east/west) — lobby's west is taken by bar; use east instead
        rooms["lobby"].Exits["east"] = new Exit { Direction = "east", TargetRoomId = "corridor" };
        rooms["corridor"].Exits["west"] = new Exit { Direction = "west", TargetRoomId = "lobby" };

        // tunnel ↔ den (south/north)
        rooms["tunnel"].Exits["south"] = new Exit { Direction = "south", TargetRoomId = "den" };
        rooms["den"].Exits["north"] = new Exit { Direction = "north", TargetRoomId = "tunnel" };
    }

    private static void PlaceItems(Dictionary<string, Room> rooms)
    {
        rooms["alley"].Items.Add(new Item
        {
            Id = "flyer",
            Name = "Crumpled Flyer",
            Description =
                "A crumpled flyer wedged under a fire escape. The logo reads 'THE BYTE BAR — one block east.' " +
                "Someone's scrawled underneath: 'Ask for Viktor. He knows the score.' Useful for orientation.",
            CanPickUp = true
        });

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

        rooms["rooftop"].Items.Add(new Item
        {
            Id = "keycard",
            Name = "Corp Keycard",
            Description =
                "A corporate keycard, worn but functional. SynthCorp Security Division—badge number erased with acid. " +
                "The RFID chip inside still works. Someone dropped this in a hurry. It looks like it would open a security door.",
            CanPickUp = true,
            UseTargetId = "north",
            UseMessage = "The keycard slides into the reader. The light flicks from red to green. A soft click—the security door unlocks with a pneumatic hiss."
        });

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

        rooms["tunnel"].Items.Add(new Item
        {
            Id = "repair_kit",
            Name = "Black-Market Repair Kit",
            Description =
                "A weathered case containing tools and spare parts for fixing damaged tech. " +
                "Found in the undercity, it's useful for repairing equipment or bypassing simple security locks.",
            CanPickUp = true
        });

        rooms["den"].Items.Add(new Item
        {
            Id = "cred_chip",
            Name = "Arasaka Cred Chip",
            Description =
                "A sleek corporate credit chip bearing Arasaka's corporate logo. " +
                "It's loaded with enough credits to bribe most security personnel. " +
                "Black-market value is high, but it's worth more as a ticket past the checkpoint.",
            CanPickUp = true,
            UseTargetId = "north",
            UseMessage = "You slide the cred chip across the desk. The guard's eyes light up—this is enough to make him very cooperative. The locked door to the north slides open silently."
        });
    }

    private static void PlaceNpcs(Dictionary<string, Room> rooms)
    {
        rooms["bar"].Npcs.Add(new Npc
        {
            Id = "viktor",
            Name = "Viktor",
            Description =
                "A grizzled fixer with chrome-plated forearms and eyes that have seen too much. " +
                "He tends bar at The Byte Bar, serving drinks and information in equal measure. " +
                "His chrome catches the neon light, reflecting decades of street work.",
            Dialogue =
            [
                new DialogueNode
                {
                    Id = "start",
                    Text = "Hey runner. Eyes like that, you're either lost or you're running a job. Which is it?",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'm looking for SynthCorp.", NextNodeId = "synthcorp" },
                        new DialogueResponse { Text = "Just passing through.", NextNodeId = "passing" },
                        new DialogueResponse { Text = "Nothing. Forget it.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "synthcorp",
                    Text = "SynthCorp. Ha. You've got stones, runner. Their corporate plaza is east of here. Security checkpoint between the plaza and the lobby—that's where it gets tight. Guards there are greedy corp-sec. Everyone's got a price. Word is they're partial to Arasaka cred chips. You can find one in the undercity if you know where to look.",
                    Responses =
                    [
                        new DialogueResponse { Text = "What about the rooftops?", NextNodeId = "rooftops" },
                        new DialogueResponse { Text = "Any other way in?", NextNodeId = "other_way" },
                        new DialogueResponse { Text = "Thanks for the info.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "rooftops",
                    Text = "Smart thinking. The rooftops are a runner's highway—get to them and you're halfway free. There's a girl up there, goes by Mox. She knows the routes. Careful though—drones patrol at night.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'll check it out.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "other_way",
                    Text = "There's always another way in the undercity. But you'll need something to get past the guards at the checkpoint. Currency works better than bullets down here.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Understood.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "passing",
                    Text = "Sure you are. Buy a drink or move on, choom.",
                    Responses =
                    [
                        new DialogueResponse { Text = "See ya.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "thanks",
                    Text = "Stay low, stay fast. And don't get jacked.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Will do.", NextNodeId = null }
                    ]
                }
            ]
        });

        rooms["rooftop"].Npcs.Add(new Npc
        {
            Id = "mox",
            Name = "Mox",
            Description =
                "A sharp-eyed runner with a shock of electric blue hair and a military-grade neural jack glowing at her temple. " +
                "She moves across the rooftops like she was born there. Leather jacket covered in patches from a dozen corporate wars.",
            Dialogue =
            [
                new DialogueNode
                {
                    Id = "start",
                    Text = "New face on the roofs. You running a job or just sightseeing? Either way, the drones don't care—they shoot first.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'm going after SynthCorp.", NextNodeId = "synthcorp" },
                        new DialogueResponse { Text = "Just looking around.", NextNodeId = "looking" },
                        new DialogueResponse { Text = "None of your business.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "synthcorp",
                    Text = "Balls or brains? SynthCorp's a hard target. The checkpoint guards are your bottleneck—they control access to the corporate heart. You'll need leverage. Get down to the undercity, find the Hacker's Den. There's a cred chip there, black market trade. That'll get you past security.",
                    Responses =
                    [
                        new DialogueResponse { Text = "How do I get to the undercity?", NextNodeId = "undercity" },
                        new DialogueResponse { Text = "What if I can't get the chip?", NextNodeId = "no_chip" },
                        new DialogueResponse { Text = "Thanks for the help.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "undercity",
                    Text = "Drop down from the alley—there's tunnel grates. They'll take you under. Fair warning: the undercity's got its own rules. Respect the runners down there, and they'll respect your heat.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Got it.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "no_chip",
                    Text = "Then you're dead meat at the checkpoint. Those guards won't move without serious juice. Either find the cred chip or find another route. No shortcuts in this game.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'll figure something out.", NextNodeId = "thanks" }
                    ]
                },
                new DialogueNode
                {
                    Id = "looking",
                    Text = "Well, don't look around too long. Corporate drones patrol these roofs on the hour. You get tagged, you're fried.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'll keep moving.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "thanks",
                    Text = "Good luck out there, runner. Try not to get caught.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Will do.", NextNodeId = null }
                    ]
                }
            ]
        });

        rooms["checkpoint"].Npcs.Add(new Npc
        {
            Id = "guard",
            Name = "Guard",
            Description =
                "A hardened security contractor in sleek black tactical armor. SynthCorp pays well. " +
                "His hand rests on a plasma rifle with casual menace. Eyes that track every movement—this one's seen combat.",
            Dialogue =
            [
                new DialogueNode
                {
                    Id = "start",
                    Text = "Stop. Credentials. Now.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I have a credential chip.", NextNodeId = "cred" },
                        new DialogueResponse { Text = "I'm just passing through.", NextNodeId = "passing" },
                        new DialogueResponse { Text = "Look, we can work this out.", NextNodeId = "negotiate" }
                    ]
                },
                new DialogueNode
                {
                    Id = "cred",
                    Text = "Show me. Make it quick.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Here—take a look.", NextNodeId = "cred_check" }
                    ]
                },
                new DialogueNode
                {
                    Id = "cred_check",
                    Text = "Arasaka? That's... acceptable. You're cleared to proceed. Don't cause trouble in there.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Won't be a problem.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "passing",
                    Text = "Passing through? Without clearance? I don't think so. You need credentials, a retinal scan, or a very good reason. What've you got?",
                    Responses =
                    [
                        new DialogueResponse { Text = "Never mind, I'm leaving.", NextNodeId = null },
                        new DialogueResponse { Text = "What if I had a reason?", NextNodeId = "reason" }
                    ]
                },
                new DialogueNode
                {
                    Id = "reason",
                    Text = "A reason? Convince me, runner. But make it good—I'm paid to be skeptical.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'm just trying to get to the other side.", NextNodeId = "weak_reason" }
                    ]
                },
                new DialogueNode
                {
                    Id = "weak_reason",
                    Text = "That's not a reason. That's an excuse. You're not getting through without proper authorization. Move along.",
                    Responses =
                    [
                        new DialogueResponse { Text = "Understood.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "negotiate",
                    Text = "Work it out? I like your style, runner. But SynthCorp pays better than you can probably bribe. Unless... you've got something valuable. Arasaka cred chip, maybe? That would make my shift a lot more interesting.",
                    Responses =
                    [
                        new DialogueResponse { Text = "What if I can get you one?", NextNodeId = "bribe_offer" },
                        new DialogueResponse { Text = "Forget it then.", NextNodeId = null }
                    ]
                },
                new DialogueNode
                {
                    Id = "bribe_offer",
                    Text = "Get me a chip and you're golden. Bring it back, and I'll personally escort you through. Until then, you're blocked. Standard protocol.",
                    Responses =
                    [
                        new DialogueResponse { Text = "I'll be back.", NextNodeId = null }
                    ]
                }
            ]
        });
    }

    private static void AddNarratorVariants(Dictionary<string, Room> rooms)
    {
        rooms["alley"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredFlags = ["keycard_used"],
            Description =
                "The alley is familiar now, your escape route clear in your mind. Sirens wail in the distance—drones are hunting. " +
                "The bar door to the east looks safer than the tunnel descent. Choose fast."
        });
        rooms["alley"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredInventoryItems = ["drive"],
            Description =
                "The alley feels alive with danger now. The data drive in your pocket is like carrying a bomb—SynthCorp's security will tear this city apart to find you. " +
                "The tunnel entrance below glows with blue electric light, promising escape. Or the bar, still."
        });

        rooms["bar"].NarratorVariants.Add(new NarratorVariant
        {
            Description =
                "Neon flickers across the bar. Holographic drink menus cast sickly pink light on empty glasses. " +
                "Someone left a message on the terminal—your name isn't on it, but it might as well be. The chrome-jawed synth bartender watches you without watching you."
        });
        rooms["bar"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredFlags = ["viktor_met"],
            Description =
                "Viktor works the bar tonight, same as always. His chrome forearms catch the neon light as he pours. " +
                "You know he could help, but he'll want to know what you're really after. The rooftop access is up those back stairs. East leads to the corporate plaza."
        });

        rooms["plaza"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredFlags = ["cred_chip_obtained"],
            Description =
                "The plaza is thick with security personnel today. You can feel the tension—corpo enforcers in sleek black armor, drones sweeping overhead. " +
                "The checkpoint to the north is your gateway in, but the guards look hungry. With the cred chip, you might talk your way through."
        });

        rooms["checkpoint"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredFlags = ["guard_distracted"],
            Description =
                "The guards are distracted—their comms are buzzing with activity. Something's happening deeper in the building, pulling their attention away. " +
                "Now's your chance to slip through. The corridor north leads into the corporate heart."
        });
        rooms["checkpoint"].NarratorVariants.Add(new NarratorVariant
        {
            RequiredFlags = ["guard_bribed"],
            Description =
                "The guard at the gate nods slightly—you know where you stand with him now. He'll look the other way when you make your move. " +
                "The lobby entrance north is clear. At least for now."
        });
    }
}
