using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public class ServiceModule : IModule
    {
        public void Register(IServiceCollection container)
        {
            container.AddSingleton<IServiceScriptCache, ServiceScriptCache>();
            container.AddSingleton<WindowsServiceConfig>(sp => sp.GetRequiredService<IConfigurationRoot>()
                ?.GetSection("ScriptSource").Get<WindowsServiceConfig>() ?? new WindowsServiceConfig())
                .AddTransient<IServiceServerEndPoint, ServiceServerEndPoint>()
                .AddTransient<Func<IServiceServerEndPoint>>(sp => sp.GetRequiredService<IServiceServerEndPoint>);
        }
    }
}
