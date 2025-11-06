using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using NiekoBoard.Data;
using System.ComponentModel;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NiekoBoard.IO;
using NiekoBoard.Logging;
using NiekoBoard.Service;
using Microsoft.Extensions.Logging;
using NiekoBoard.Client;

namespace NiekoBoard
{
    public class CoreModule : ILibraryModule
    {
        public IEnumerable<ILibraryObject> GetLibraryObjects()
        {
            return [ new NiekoLogger() ];
        }

        public void Register(IServiceCollection container)
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json")
                    .Build();
            container.AddSingleton<IConfigurationRoot>(config)
                .AddSingleton<ILogger>(sp => sp.GetRequiredService<ILogger<NiekoLogger>>());
            
            var scriptConfig = config?.GetSection("ScriptSource").Get<ScriptSourceConfig>() ?? new ScriptSourceConfig();
            container.AddSingleton(scriptConfig);

            container.AddTransient<FolderSourceSettings>(sp => config?.GetSection("ScriptSource:Config").Get<FolderSourceSettings>() ?? new FolderSourceSettings());
            var windowsServiceConfig = config?.GetSection("WindowsService").Get<WindowsServiceConfig>() ?? new WindowsServiceConfig();

            if(windowsServiceConfig.ConfigUpdateFrequency < WindowsServiceConfig.MinimumUpdateFrequency) windowsServiceConfig.ConfigUpdateFrequency = WindowsServiceConfig.MinimumUpdateFrequency;
            if(windowsServiceConfig.DefaultScriptUpdateFrequency < WindowsServiceConfig.MinimumUpdateFrequency) windowsServiceConfig.DefaultScriptUpdateFrequency = WindowsServiceConfig.MinimumUpdateFrequency;

            container.AddSingleton(windowsServiceConfig);

            var sqlConfig = new SqlStoreConfig();
            var cnxstring = config?.GetConnectionString("storedb");

            if(!string.IsNullOrEmpty(cnxstring))
            {
                sqlConfig.ConnectionString = cnxstring;
            }

            container.AddSingleton(sqlConfig);


            switch (scriptConfig.ScriptSourceName)
            {
                case ScriptSourceName.IISHosted:
                    {
                        container.AddSingleton<IScriptSource>(sp => sp.GetRequiredService<Client.IIisScriptSource>());
                        break;
                    }
                case ScriptSourceName.Service:
                    {
                        container.AddSingleton<Service.ServiceScriptSource>()
                            .AddSingleton<IScriptSource>(sp => sp.GetRequiredService<Service.ServiceScriptSource>());
                        break;
                    }
                case ScriptSourceName.LocalFile:
                default:
                    {
                        container.AddSingleton<IO.FolderScriptSource>()
                            .AddSingleton<IScriptSource>(sp => sp.GetRequiredService<IO.FolderScriptSource>());
                        break;
                    }
            }

            var configStoreName = config?.GetValue<ScriptConfigSourceName>("ScriptConfigStore:StoreType")?? ScriptConfigSourceName.IISHosted;

            switch (configStoreName)
            {
                case ScriptConfigSourceName.IISHosted:
                    {
                        container.AddSingleton<IScriptConfigStore>(sp => sp.GetRequiredService<Client.IIisScriptConfigStore>());
                        break;
                    }
                case ScriptConfigSourceName.SqlDb:
                    {
                        container.AddSingleton<SqlScriptConfigStore>()
                            .AddSingleton<IScriptConfigStore>(sp => sp.GetRequiredService<SqlScriptConfigStore>());
                        break;
                    }
                case ScriptConfigSourceName.JsonLocalFile:
                default:
                    {
                        var configStoreConfig = config?.GetSection("ScriptConfigStore:Config").Get<JsonScriptConfigStoreConfig>() ?? new JsonScriptConfigStoreConfig();

                        container.AddTransient<IJsonScriptConfigStore, JsonScriptConfigStore>()
                            .AddSingleton<JsonScriptConfigStoreConfig>(configStoreConfig)
                            .AddSingleton<IScriptConfigStore>(sp => sp.GetRequiredService<IJsonScriptConfigStore>());
                        break;
                    }
            }

            var historyStoreName = config?.GetValue<HistorySourceName>("HistoryStore:StoreType") ?? HistorySourceName.Volatile;

            switch (historyStoreName)
            {
                case HistorySourceName.Volatile:
                    {
                        container.AddSingleton<IVolatileScriptHistoryStore, VolatileScriptHistoryStore>()
                            .AddSingleton<IScriptHistoryStore>(sp => sp.GetRequiredService<IScriptHistoryStore>());
                        break;
                    }
                case HistorySourceName.IISHosted:
                    {
                        container.AddSingleton<IIisScriptHistoryStore>(sp => sp.GetRequiredService<Client.IIisScriptHistoryStore>());
                        break;
                    }
                case HistorySourceName.SqlDb:
                    {
                        container.AddSingleton<IScriptHistoryStore, SqlScriptHistoryStore>();
                        break;
                    }
            }

            container.AddTransient<LibraryInitializer>();
            container.AddTransient<IServiceClientEndPoint, ServiceClientEndPoint>();
        }
    }
}
