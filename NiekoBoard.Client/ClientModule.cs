using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Client
{
    public class ClientModule : IModule
    {
        public void Register(IServiceCollection container)
        {
            container.AddTransient<ClientGprcSettings>(sp => sp.GetRequiredService<IConfigurationRoot>()?.GetSection("ScriptSource:Config").Get<ClientGprcSettings>() ?? new ClientGprcSettings())
                .AddSingleton<GprcScriptSource>()
                .AddTransient<IIisScriptSource, GprcScriptSource>()
                .AddTransient<IIisScriptConfigStore, GprcScriptConfigStore>()
                .AddTransient<IIisScriptHistoryStore, GprcScriptHistoryStore>()
                .AddTransient<ClientWork>()
                .AddTransient<Func<ClientWork>>(sp => sp.GetRequiredService<ClientWork>);
        }
    }
}
