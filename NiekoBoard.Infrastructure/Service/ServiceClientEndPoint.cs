using Microsoft.Management.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NiekoBoard.Service
{
    public class ServiceClientEndPoint : IServiceClientEndPoint
    {
        private object _lock = new object();
        private WindowsServiceConfig _Config;
        private List<string> _Errors = new List<string>();

        public IList<string> Errors { get; private set; }

        public ServiceClientEndPoint(WindowsServiceConfig config)
        {
            _Config = config;
            Errors = new ReadOnlyCollection<string>(_Errors);
        }

        public async Task<string> Get(string parameters)
        {
            using (var pipeClient = new NamedPipeClientStream(".", _Config.PipeName,
                PipeDirection.InOut, PipeOptions.None, System.Security.Principal.TokenImpersonationLevel.None))
            {
                var streamer = new PipeStringStream(pipeClient);
                var completed = false;

                var i = 0;

                while (i++ <= _Config.Retries)
                    try
                    {
                        await pipeClient.ConnectAsync();

                        await streamer.Write(parameters);
                        completed = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        pipeClient.Close();
                        lock (_lock)
                        {
                            _Errors.Add(e.ToString());
                        }
                    }

                if (!completed)
                {
                    return string.Empty;
                }

                i = 0;
                string results = "";
                completed = false;

                while (i++ <= _Config.Retries)
                    try
                    {
                        results = await streamer.Read();
                        completed = true;
                        break;
                    }
                    catch (Exception e)
                    {
                        lock (_lock)
                        {
                            _Errors.Add(e.ToString());
                        }
                    }

                if(!completed)
                {
                    return string.Empty;
                }

                return results;
            }
        }
    }
}
