using MyGame.Engine;
using MyGame.Commands;
using MyGame.Content;

var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");

LoadedWorld? loaded = null;
GameState state;

if (File.Exists(worldPath))
{
    var loader = new JsonWorldLoader();
    loaded = loader.Load(worldPath);
    state = loaded.State;
}
else
{
    state = WorldBuilder.Build();
}

var registry = new CommandRegistry();
registry.Register(new LookCommand());
registry.Register(new GoCommand());
registry.Register(new TakeCommand());
registry.Register(new DropCommand());
registry.Register(new InventoryCommand());
registry.Register(new UseCommand());
registry.Register(new ExamineCommand());
registry.Register(new HelpCommand(registry));
registry.Register(new QuitCommand());
registry.Register(new TalkCommand());
registry.Register(new SaveCommand());
registry.Register(new LoadCommand());

var engine = new GameEngine(state, registry, new ConsoleIO(), loaded);
engine.Run();
