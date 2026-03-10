namespace MyGame.Engine;

/// <summary>
/// All player-facing narrative and UI strings, grouped by context.
/// </summary>
public static class GameMessages
{
    public static class Defaults
    {
        public const string Title = "N E O N   L E D G E R";
        public const string Subtitle = "A Cyberpunk Text Adventure";
        public const string IntroText =
            "You've been hired to infiltrate SynthCorp's data vaults and retrieve stolen research.\n" +
            "Your fixer's last message: \"Package in the corp system. Get in, get the drive, get out.\"\n" +
            "You start in the back alley with nothing but your wits and a job to do.";
    }

    public static class Prompts
    {
        public const string CommandInput = "\n> ";
        public const string DialogueInput = "> ";
        public const string TryAgain = "\nTry again? (yes/no) ";
    }

    public static class Drone
    {
        public const string Warning1 = "A drone sweeps overhead — its scanner lights paint the street.";
        public const string Warning2 = "Drone targeting systems are locking on. You need to move. Now.";
        public const string Warning3 = "CRITICAL: Drone lock acquired. Leave this zone immediately.";
    }

    public static class Win
    {
        public const string ServerRoom1 = "The server room hums around you. Rows of data towers stretch into the dark.";
        public const string ServerRoom2 = "You find the drive. Your hand trembles as you pocket it.";
        public const string DefaultMessage =
            "You've done it. The SynthCorp data drive is in your hands—real, tangible proof\n" +
            "of what they've been hiding. As you slip out through the service corridor, corporate\n" +
            "security drones sweep the upper levels. They haven't spotted you. Not yet.\n" +
            "In your pocket, the drive pulses with cold data. You smile—this changes everything.";
        public const string Banner = "*** YOU WIN. The neon city is yours. ***";
    }

    public static class Lose
    {
        public const string DefaultMessage =
            "Red warning lights flood the street. SynthCorp security drones converge on your position,\n" +
            "their scanner locks painting you in deadly light. Your wrist terminal screams alerts.\n" +
            "You've lost the game—and possibly much worse. The last thing you see is a drone's\n" +
            "targeting reticle zeroing in. SynthCorp doesn't take data theft lightly.";
        public const string Banner = "*** CAPTURED. SynthCorp wins this round. ***";
    }

    public static class Quit
    {
        public const string Message = "Jacking out of the sprawl...";
        public const string Banner = "*** JACKED OUT. See you in the sprawl. ***";
    }

    public static class Go
    {
        public const string NoDirection = "Go where? Specify a direction (north, south, east, west, up, down, etc.).";
        public const string WayLocked = "The way is locked.";
    }

    public static class Take
    {
        public const string NoItem = "Take what?";
        public const string DataChipPickup = "Your hand trembles as you pocket the chip. Years of work, dead contacts, all leading to this moment.";
    }

    public static class Use
    {
        public const string NoItem = "Use what?";
    }

    public static class Look
    {
        public const string ItemsHere = "Items here: ";
        public const string NpcsHere = "You see here: ";
        public const string Exits = "Exits: ";
    }

    public static class Help
    {
        public const string Header = "\nAvailable commands:";
        public const string Directions = "\nDirections: north (n), south (s), east (e), west (w), up (u), down (d)";
    }

    public static class Drop
    {
        public const string NoItem = "Drop what?";
    }

    public static class Examine
    {
        public const string NoItem = "Examine what?";
    }

    public static class Inventory
    {
        public const string Empty = "You're carrying nothing.";
        public const string Header = "You're carrying:";
    }

    public static class Talk
    {
        public const string ToWhom = "Talk to whom?";
        public const string EndConversation = "You end the conversation.";
        public const string Goodbye = "Goodbye.";
    }
}
