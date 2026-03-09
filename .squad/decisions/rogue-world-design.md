# Rogue's World Design Decisions

## Date
2026-03-09 (Neon Ledger - Full World JSON)

## Overview
Complete cyberpunk world definition created for Neon Ledger with 10 rooms, 6 items, 3 NPCs, and 2 puzzles. All content maintains atmospheric cyberpunk aesthetic with gritty corporate dystopia theme.

## Room Structure (10 Total)

### Core 5 Preserved Rooms (Existing Architecture)
1. **alley** - Starting position, entry point to undercity
2. **bar** - Social hub, NPC Viktor, narrative exposition
3. **rooftop** - Alternative route, NPC Mox, keycard location
4. **lobby** - Corporate nexus, puzzle #1 lock (keycard→server)
5. **server** - Goal room, data drive (win condition)

### New 5 Expansion Rooms
1. **plaza** - SynthCorp public face, high surveillance
2. **checkpoint** - Puzzle #2 location, guard NPC, narrative tension
3. **corridor** - Service maintenance route, worker bypass option
4. **tunnel** - Undercity access, atmospheric hub
5. **den** - Hacker's hideout, cred chip location, black market flavor

## Puzzle Architecture

### Puzzle #1: Keycard→Server (Preserved)
- **Mechanism:** Locked exit on lobby→server, requires keycard item
- **Item:** keycard (found on rooftop)
- **Solution Path:** rooftop → pickup keycard → lobby → use keycard on north exit
- **Narrative:** Player must explore to find security access device

### Puzzle #2: Cred Chip→Checkpoint (New)
- **Mechanism:** Locked exit on checkpoint→lobby, requires cred chip item
- **Item:** cred_chip (found in undercity den)
- **Solution Path:** alley → down to tunnel → south to den → pickup cred_chip → north to checkpoint → use cred_chip on north exit
- **Narrative:** Player must navigate undercity black market to bribe corporate security
- **Alternative Puzzle Trigger:** NPC Guard dialogue branches offer bribe/distraction hints

## NPC Design

### Viktor (Bartender/Fixer)
- **Location:** The Byte Bar
- **Function:** Exposition, map hints, cred chip awareness
- **Dialogue Nodes:** 6 (start, synthcorp, rooftops, other_way, passing, thanks)
- **Narrative Role:** Establishes SynthCorp threat level, hints at checkpoint guards' bribability
- **Player Engagement:** Branching dialogue teaches about cred chips without direct mission assignment

### Mox (Rooftop Runner)
- **Location:** Rooftop Spans
- **Function:** Route intel, undercity guidance, drone hazard exposition
- **Dialogue Nodes:** 6 (start, synthcorp, undercity, no_chip, looking, thanks)
- **Narrative Role:** Provides tactical advantage awareness, connects rooftop to undercity path
- **Player Engagement:** Mentor-like figure, warns of drone patrols, reinforces puzzle solution

### Guard (Security Checkpoint)
- **Location:** Security Checkpoint
- **Function:** Gatekeeper, tension/conflict, bribability mechanic
- **Dialogue Nodes:** 8 (start, cred, cred_check, passing, reason, weak_reason, negotiate, bribe_offer)
- **Narrative Role:** Embodiment of corporate corruption, enforces puzzle requirement
- **Player Engagement:** Multiple dialogue paths—player can attempt authorization, reason, or bribe

## Items (6 Total)

### Existing 4 Items
1. **flyer** - Flavor/orientation, alley start
2. **terminal** - Environmental scenery, bar (non-takeable)
3. **keycard** - Puzzle #1 solution, rooftop
4. **drive** - Win condition, server room

### New 2 Items
1. **cred_chip** - Puzzle #2 solution, den (undercity)
2. **repair_kit** - Flavor/future expansion, tunnel

## Narrator Variants (Dynamic Descriptions)

### Alley (2 Variants)
- **Triggered by flag `keycard_used`:** Emphasis on escape routes and urgency
- **Triggered by inventory `drive`:** Heightened paranoia, SynthCorp threat palpable

### Bar (1 Variant)
- **Triggered by flag `viktor_met`:** Recognition of NPC presence, updated dialogue awareness

### Plaza (1 Variant)
- **Triggered by flag `cred_chip_obtained`:** Security personnel acknowledgment, tactical opportunity

## Connectivity (Bidirectional Exits)

```
alley ←→ bar (east/west) [existing]
bar ←→ rooftop (up/down) [existing]
bar ←→ plaza (east/west) [new]
plaza ←→ checkpoint (north/south) [new]
checkpoint ←→ lobby (north/south, locked by cred_chip) [new lock]
lobby ←→ server (north/south, locked by keycard) [existing lock]
lobby ←→ corridor (west/east) [new]
alley ←→ tunnel (down/up) [new]
plaza ←→ tunnel (south/north) [new]
tunnel ←→ den (south/north) [new]
```

## Narrative Flow (Optimal Path)

1. **Setup:** Player starts in alley, picks up flyer hint
2. **Exposition:** Visit bar, meet Viktor, learn about SynthCorp/checkpoint/cred chip
3. **Route Choice:** Either rooftop (keycard) or tunnel (cred chip)
4. **Route A (Rooftop):** bar → rooftop → pickup keycard → meet Mox → gather intel
5. **Route B (Undercity):** alley → tunnel → den → pickup cred_chip
6. **Convergence:** Both routes must eventually complete checkpoint puzzle
7. **Key Retrieval:** Once at lobby, unlock server room with keycard
8. **Victory:** server room → grab drive → win

## Design Rationale

- **Two Puzzles:** Provides puzzle variety (security access vs. bribery) and multiple viable paths
- **Narrator Variants:** Show player progression through story beats, reward exploration
- **NPC Branching:** Guides solution discovery without explicit quests, maintains immersion
- **Undercity Hub:** Thematic black market, justifies puzzle requirement, creates atmosphere
- **Preserved Core:** All existing 5 rooms maintain original connectivity, ensuring compatibility with existing test suite

## Atmospheric Standards Met

- ✅ Room descriptions: 2-4 sentences, evocative, neon/corporate dystopia aesthetic
- ✅ Item descriptions: 1-3 sentences, flavor + utility hints
- ✅ NPC dialogue: Character-voiced, hints without hand-holding, naturalistic banter
- ✅ World scale: 10 distinct spaces create believable city geography
- ✅ Puzzle integration: Both puzzles serve narrative (corporate security, black market bribery)

## JSON Schema Compliance

- ✅ All required fields present (rooms, items, npcs, exits, dialogue)
- ✅ Bidirectional exits validated
- ✅ Narrator variants use requiredFlags/requiredInventoryItems correctly
- ✅ NPC dialogue trees branch logically, end with nextNodeId: null
- ✅ Item useTargetId and useMessage semantically linked to lock mechanisms
- ✅ All room IDs and item IDs unique and lowercase
