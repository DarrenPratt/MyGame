# MyGame: Neon Ledger
## A Cyberpunk Text Adventure

---

## Narrative Setup

**Title:** Neon Ledger

**Tagline:** "Data runs through the streets. You run the streets."

**Opening Text:**
You slide into the shadowed booth of Flux, a dive bar that exists in the cracks between corporate towers. The street outside flickers with neon signs—synthetic rain patters against the window. Your fixer contact was supposed to meet you twenty minutes ago with a data chip containing stolen research from SynthCorp, the megacorporation that controls three city districts. The chip is worth 50,000 credits on the black market—enough to disappear for a year. Instead, there's only a cryptic message on your wrist terminal: "Package at The Lattice. Maze key in the old place. Watch the drones."

**Win Condition:**
Retrieve the data chip from The Lattice and escape to safety with the chip intact.

**Win Text:**
You palm the data chip and feel its weight—real, tangible proof of what SynthCorp is trying to hide. Somewhere across the sprawl, your contact's final dead-drop location awaits. As you slip out the back exit of The Lattice, you catch sight of corporate security drones sweeping the upper levels two blocks over. They haven't spotted you. Not yet. In your pocket, the chip pulses with a faint green light. You smile—this changes everything.

**Lose Condition:**
Get caught by SynthCorp security or fail to escape The Lattice before drones lock down the perimeter.

**Lose Text:**
Red warning lights flood the street. SynthCorp security drones converge on your position, their scanner locks painting you in deadly light. Your wrist terminal screams alerts. You've lost the game—and possibly much worse. The last thing you see is a drone's targeting reticle zeroing in. SynthCorp doesn't take data theft lightly.

---

## Rooms

### Room 1
**Room ID:** flux_bar  
**Name:** Flux Bar  
**Description:** A cramped hole-in-the-wall bar wedged between two corporate transit towers. Holographic drink menus flicker across scratched tables, casting sickly blue and pink light across everything. The bartender—a chrome-jawed synth with dead eyes—polishes glasses that never quite get clean. Rain drums against reinforced windows overlooking the street below, where hover-bikes zip between neon-soaked alleys.  
**Exits:** east → street_level, north → back_alley, behind_bar → (blocked by bartender)  
**Items:** cryptic_message (on table), empty_glass  
**Starting Position:** Player begins here

### Room 2
**Room ID:** street_level  
**Name:** Neon Street — Market District  
**Description:** The pulse of the city beats here. Market stalls hawking black-market implants, fake IDs, and stolen tech crowd the sides of the street. Holographic signs tower overhead advertising SynthCorp products while genuine dealers whisper deals from shadowed doorways. A drone sweeps past overhead, its red scanning light painting the pavement red. The air tastes of ozone and fried circuits.  
**Exits:** west → flux_bar, north → corporate_plaza, south → undercity_entrance, east → tech_den  
**Items:** data_pad (dropped by merchant), neon_jack  
**Notes:** Drones appear here periodically—player should eventually move on or risk capture.

### Room 3
**Room ID:** back_alley  
**Name:** Alley Behind Flux  
**Description:** Narrow, grimy, reeking of fermented synth-fuel and old blood. Fire escapes crisscross overhead, draped with dried cables and abandoned server boxes. A faded graffito glows faintly: "THE LATTICE REMEMBERS." Puddles reflect fractured neon from the street beyond. This is where runners hide when the heat gets too close.  
**Exits:** south → street_level, up → rooftop_network, down → underground_passage  
**Items:** maze_key, broken_terminal  
**Notes:** The maze_key is essential—without it, The Lattice is impassable.

### Room 4
**Room ID:** corporate_plaza  
**Name:** SynthCorp Plaza  
**Description:** All chrome and cold steel, a brutalist monument to corporate power. The plaza spans three levels, filled with corporate workers in expensive suits and security personnel in tactical gear. Holographic advertisements for neural implants and synthetic organs pulse from every surface. A fountain in the center runs with something that isn't quite water. Security cameras are everywhere—being here draws unwanted attention.  
**Exits:** south → street_level, east → security_checkpoint, north → data_tower_lobby  
**Items:** corp_badge (fake, useless on close inspection)  
**Notes:** High-risk area. Extended time here increases drone response.

### Room 5
**Room ID:** tech_den  
**Name:** The Chrome Dealer's Shop  
**Description:** A cramped storefront stuffed with salvaged tech—circuit boards, neural cables, and screens displaying cryptic market data. The dealer, a paranoid netrunner with silver-threaded eyes, nods but doesn't speak. Half the shop is fake inventory; the real deals happen in the back through a door that requires the right payment or password. The air hums with the electromagnetic buzz of a dozen devices running simultaneously.  
**Exits:** west → street_level, back → (requires payment or password)  
**Items:** access_code (clue to The Lattice)  
**Notes:** Dealer has information but is suspicious of outsiders.

### Room 6
**Room ID:** undercity_entrance  
**Name:** Down Into the Undercity  
**Description:** The street-level entrance to what once was the city's subway system. Now it's a sprawling underground network—the Undercity—where those unwanted by the megacorps have built a parallel civilization. Glowing cables strung across makeshift shelters cast everything in electric blue. The sound of machinery, voices, and distant music echoes through tunnels. The air is thick and warm.  
**Exits:** north → street_level, deeper → underground_passage, south → market_hub  
**Items:** repair_kit  
**Notes:** Safe zone. Drones can't reach here, but you need to know where you're going.

### Room 7
**Room ID:** the_lattice  
**Name:** The Lattice — SynthCorp Research Archive  
**Description:** A labyrinthine archive hidden beneath the city, sealed behind corporate encryption. Servers hum in endless rows, their light panels casting everything in harsh white and blue. The maze_key allows you to navigate the shifting security doors. Files and data are stored on holographic displays. This is where your target waits—the chip containing research that could bring down SynthCorp's entire neural implant division. The air is cold, precise, sterile.  
**Exits:** out → (using maze_key), deeper → secure_vault  
**Items:** data_chip (the objective), research_files  
**Notes:** Puzzle room. The maze_key is required to navigate the security layout safely. Time is limited—drones converge once the alarm is triggered.

### Room 8
**Room ID:** rooftop_network  
**Name:** Rooftop Spans — Runner's Route  
**Description:** A network of interconnected rooftops spanning three city blocks. Wind buffets the structure, carrying sounds of traffic and distant sirens. Makeshift bridges and zip-lines connect the buildings, weathered by constant use. From here, you can see all of Night City—neon towers stretching to the horizon, corporate megastructures looming like glass mountains. The freedom of the open air contrasts sharply with the oppression below.  
**Exits:** down → back_alley, west → secondary_tower, east → escape_platform  
**Items:** grappling_hook (optional utility)  
**Notes:** Fast route to escape, but exposed to drones.

### Room 9
**Room ID:** escape_platform  
**Name:** The Drop — Hover-Bike Platform  
**Description:** A makeshift platform jutting from a building edge, rigged with stolen corporate tech. An old hover-bike sits here, barely functional but running. Below is a dizzying drop to the street three hundred meters down. The sound of drone engines grows closer—their searchlights sweep the rooftops. This is your last chance: escape with the chip or be cornered on this roof.  
**Exits:** back → rooftop_network, escape → (with data_chip = win)  
**Items:** none  
**Notes:** Final location. Reaching here with the data_chip triggers the win condition.

---

## Items

### Item 1
**Item ID:** cryptic_message  
**Name:** Scrawled Terminal Message  
**Description:** A message appears on the bartender's terminal, meant for you: "Package at The Lattice. Maze key in the old place. Watch the drones." It's encrypted but cracked—your contact's final instruction before going dark.  
**Takeable:** No (you remember it on sight)  
**Starting Room:** flux_bar  
**Item Type:** Quest clue

### Item 2
**Item ID:** maze_key  
**Name:** Crystalline Maze Key  
**Description:** A small, impossibly complex geometric structure that glows faintly with internal light. It's a physical-digital hybrid key used to navigate The Lattice's security labyrinth. Without it, the archive's shifting doors are impassable.  
**Takeable:** Yes  
**Starting Room:** back_alley  
**Item Type:** Essential quest item

### Item 3
**Item ID:** data_chip  
**Name:** Encrypted Data Chip — SynthCorp Research  
**Description:** A small, iridescent chip etched with corporate security markers. It contains the research data that could expose SynthCorp's illegal neural implant experiments. This is what you came for.  
**Takeable:** Yes  
**Starting Room:** the_lattice  
**Item Type:** Objective/win condition

### Item 4
**Item ID:** data_pad  
**Name:** Merchant's Data Pad  
**Description:** Dropped by a panicked street vendor. Contains scattered market data, a few credits, and strangely, a partial map to The Lattice's entrance hidden in the Undercity.  
**Takeable:** Yes  
**Starting Room:** street_level  
**Item Type:** Helpful clue

### Item 5
**Item ID:** access_code  
**Name:** Chrome Dealer's Access Code  
**Description:** A string of alphanumeric characters written on a small card. The dealer whispers it to you: the code needed to reach The Lattice's entrance from the tech den's back room.  
**Takeable:** Yes  
**Starting Room:** tech_den  
**Item Type:** Access key

### Item 6
**Item ID:** neon_jack  
**Name:** Glowing Neon Jack (Cyberware Tool)  
**Description:** A small, pulsing tool used for hot-wiring security systems. Glows neon blue in your hand. Useful for bypassing locked doors, but risky—using it alerts nearby drones.  
**Takeable:** Yes  
**Starting Room:** street_level  
**Item Type:** Utility tool

### Item 7
**Item ID:** repair_kit  
**Name:** Black-Market Repair Kit  
**Description:** A weathered case containing tools and spare parts for fixing damaged tech. Found in the Undercity, it's useful for repairing the hover-bike on the escape platform if it's been damaged by drone fire.  
**Takeable:** Yes  
**Starting Room:** undercity_entrance  
**Item Type:** Utility tool

### Item 8
**Item ID:** broken_terminal  
**Name:** Fried Terminal (Ancient Machine)  
**Description:** A terminal from decades past, still glowing faintly but fried beyond repair. The Undercity runner community uses it as a landmark. No useful function, but atmospheric.  
**Takeable:** No  
**Starting Room:** back_alley  
**Item Type:** Environmental detail

---

## Command Flavor & Atmosphere

### "Look" Command Variations
- **In Flux Bar:** "Neon flickers across empty glasses. Someone left a message on the terminal."
- **On Street Level:** "The street pulses with life and danger. Drones overhead, deals whispered from shadows."
- **At The Lattice:** "Servers hum all around you. The maze of security doors shifts subtly."

### "Examine" Special Cases
- **Examining a drone:** "Red targeting laser paints the ground. You're being scanned. Move or hide."
- **Examining the hover-bike:** "Ancient tech, barely held together, but the engine's running. It's your way out."

### Action Flavor
- **Taking the data chip:** "Your hand trembles as you pocket the chip. Years of work, dead contacts, all leading to this moment."
- **Entering escape_platform:** "Wind howls around you. The city spreads out below like a circuit board. Behind you, drones converge."
- **Winning:** "You gun the hover-bike's engine. The rooftop falls away beneath you. Freedom tastes like ozone and adrenaline."

### Environmental Dangers
- **Time pressure:** Every turn spent in corporate-controlled areas (corporate_plaza, security_checkpoint) increases the risk of drone encounters.
- **Drones appear randomly** in street_level and corporate_plaza to raise tension.
- **Only safe zones:** flux_bar, back_alley (initially), undercity areas, and rooftop_network.

---

## Story Beat Structure

1. **Setup** (Flux Bar): Player learns they need the data chip from The Lattice.
2. **Navigation** (Street/Undercity): Player collects items and clues to locate The Lattice entrance.
3. **Access** (Tech Den or Undercity): Player obtains the maze_key and access codes.
4. **Infiltration** (The Lattice): Player navigates the security labyrinth and retrieves the chip.
5. **Escape** (Rooftops): Player flees corporate drones and reaches the hover-bike.
6. **Victory** (Escape Platform): Player escapes with the chip and completes the mission.

---

## Cyberpunk Aesthetics & Tone

- **Visual:** Neon blues, pinks, and electric greens. Chrome and weathered concrete. Holographic overlays everywhere.
- **Audio Cues (for text):** Hum of circuits, rain on metal, wind between buildings, distant sirens.
- **Dialogue Style:** Clipped, professional, street-smart. Corps speak in cold corporate jargon. Street runners use slang and shortcuts.
- **Humor:** Dark and cynical. References to the futility of resistance, the pervasiveness of corporate control, and the small rebellions that matter anyway.

---

*Content designed for Jynx_Protocol's cyberpunk text adventure. All rooms, items, and story beats ready for implementation.*
