using MyGame.Commands;
using MyGame.Engine;

namespace MyGame.Tests.Helpers;

/// <summary>
/// Factory helper for building a standard CommandRegistry in tests.
/// </summary>
public static class RegistryFactory
{
    /// <summary>
    /// Creates a <see cref="CommandRegistry"/> with all standard game commands registered.
    /// </summary>
    public static CommandRegistry BuildRegistry()
    {
        var registry = new CommandRegistry();
        registry.Register(new LookCommand());
        registry.Register(new GoCommand());
        registry.Register(new TakeCommand());
        registry.Register(new DropCommand());
        registry.Register(new InventoryCommand());
        registry.Register(new UseCommand());
        registry.Register(new HelpCommand(registry));
        registry.Register(new QuitCommand());
        return registry;
    }
}
