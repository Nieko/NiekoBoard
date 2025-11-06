using Microsoft.Extensions.DependencyInjection;
using NiekoBoard;
using NiekoBoard.Client;
using NiekoBoard.Composition;
using NiekoBoard.Data;
using System.Text.Json;

var container = new ServiceCollection();

foreach (var module in new IModule[]
{
            new CoreModule(),
            new ClientModule()
})
{
    module.Register(container);
}

var providers = container.BuildServiceProvider();
var commands = new Dictionary<string, Action>
{
    {
        "GetScripts",
        () =>
        {
            var scriptSource = providers.GetRequiredService<IScriptSource>();

            var scriptsJson = JsonSerializer.Serialize(scriptSource.GetScripts().ToList());
            Console.WriteLine(scriptsJson);
        }
    },
    {
        "GetScriptConfigs",
        () =>
        {
            var configStore = providers.GetRequiredService<IScriptConfigStore>();

            var scriptsJson = JsonSerializer.Serialize(configStore.GetConfigurations().ToList());
            Console.WriteLine(scriptsJson);
        }
    }
};

var command = commands.Keys
    .FirstOrDefault(k => k.ToLower() == (args.Length == 0 ? string.Empty : args[0]).ToLower());

if (args.Length != 1 || command == null)
{
    Console.WriteLine("NiekoBoard CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");

    foreach (var commandName in commands.Keys)
    {
        Console.WriteLine(commandName);
    }
}
else
{
    commands[command]();
}

