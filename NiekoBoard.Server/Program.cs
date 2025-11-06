using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using NiekoBoard;
using NiekoBoard.Composition;
using NiekoBoard.Server;
using NiekoBoard.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddModules(new IModule[]
{
    new CoreModule(),
    new ServerModule()
});
#if DEBUG
builder.WebHost.ConfigureKestrel(options =>
{
    var config = options.ApplicationServices.GetRequiredService<ServerConfig>();

    options.Listen(System.Net.IPAddress.Loopback, config.ListenPort, listenOption =>
    {
        listenOption.Protocols = HttpProtocols.Http2;
        listenOption.UseHttps();
    });
});
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ScriptService>();
app.MapGrpcService<ConfigService>();
app.MapGrpcService<HistoryService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

var scriptService = app.Services.GetRequiredService<IScriptSource>();

app.Run();
