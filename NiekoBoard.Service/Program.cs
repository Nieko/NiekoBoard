using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.EventLog;
using NiekoBoard;
using NiekoBoard.Composition;
using NiekoBoard.Polling;
using NiekoBoard.Service;
using System.Web.Services.Description;

var serviceName = "NiekoBoardUpdateService";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
#pragma warning disable CA1416 // Validate platform compatibility
        logging.AddEventLog(new EventLogSettings
        {
            SourceName = serviceName,
            LogName = "Service Logs"
        });
#pragma warning restore CA1416 // Validate platform compatibility
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IUpdateFrequencySource, UpdateFrequencySource>();
        services.AddModules(new IModule[]
        {
            new CoreModule(),
            new ServiceModule()
        });
    })
    .UseWindowsService(options =>
    {
        options.ServiceName = serviceName;
    })
    .Build();

host.Services.GetRequiredService<LibraryInitializer>().Initialize();
host.Run();
