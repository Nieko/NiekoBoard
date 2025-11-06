using NiekoBoard.Composition;

namespace NiekoBoard.Server
{
    public class ServerModule : IModule
    {
        public void Register(IServiceCollection container)
        {
            container.AddSingleton<ServerConfig>(sp => sp.GetRequiredService<IConfiguration>()?.GetSection("NiekoServer")?.Get<ServerConfig>() ?? new ServerConfig());
        }
    }
}
