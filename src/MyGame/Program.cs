using MyGame.Engine;
using MyGame.Commands;

ColorConsole.Initialize();

var worldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "worlds", "neon-ledger.json");

if (!File.Exists(worldPath))
    throw new FileNotFoundException($"World file not found: {worldPath}");

var loader = new JsonWorldLoader();
var loaded = loader.Load(worldPath);
var state = loaded.State;

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
