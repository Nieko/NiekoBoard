using Microsoft.Extensions.Logging;
using NiekoBoard.Logging;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public class ServiceServerEndPoint : IServiceServerEndPoint
    {
        private WindowsServiceConfig _Config;
        private IServiceScriptCache _ScriptSource;
        private ScriptSerializer _Serializer = new ScriptSerializer(sd =>
        {
            throw new ArgumentException("Unexpected service request from client with IList<IScript> parameter");
        });

        public ServiceServerEndPoint(WindowsServiceConfig config, IServiceScriptCache scriptSource)
        {
            _Config = config;
            _ScriptSource = scriptSource;
        }

        public async Task Start()
        {
            var pipeSecurity = new PipeSecurity();
            var clientUserName = string.Empty;

#if DEBUG
            clientUserName = "Everyone";
#else
            clientUserName = _Config.ClientUserName;
#endif
            pipeSecurity.AddAccessRule(new PipeAccessRule(clientUserName,
                PipeAccessRights.ReadWrite, AccessControlType.Allow));

            using (var pipeServer = NamedPipeServerStreamAcl.Create(_Config.PipeName,
                PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity:pipeSecurity))
            {
                await pipeServer.WaitForConnectionAsync();
                var streamer = new PipeStringStream(pipeServer);

                try
                {
                    var parameters = await streamer.Read();
                    var request = _Serializer.Deserialize(parameters);
                    string payload = string.Empty;

                    switch(request.MessageType)
                    {
                        case ScriptSerializer.MessageType.GetScripts:
                            {
                                var scripts = _ScriptSource.GetScripts();
                                payload = _Serializer.Serialize(scripts);
                                break;
                            }
                        case ScriptSerializer.MessageType.ExecuteMethod:
                            {
                                var requestData = (ScriptSerializer.ScriptMethod)request.Data;
                                string scriptResult = string.Empty;

                                if (requestData.MethodName == nameof(IScript.ExecuteAction))
                                {
                                    var script = _ScriptSource.GetScript(requestData.ScriptId);
                                    scriptResult = script.ExecuteAction(requestData.ActionIndex);
                                }
                                else
                                {
                                    scriptResult = _ScriptSource.GetLastStatus(requestData.ScriptId);
                                }
                                
                                payload = _Serializer.Serialize(scriptResult);
                                break;
                            }
                        default:
                            {
                                new NiekoLogger().LogError("Invalid request received over named pipe." + Environment.NewLine + parameters);

                                payload = "Invalid request";
                                break;
                            }
                    }

                    await streamer.Write(payload);
                }
                catch (Exception ex)
                {
                    new NiekoLogger().LogError(ex, nameof(ServiceServerEndPoint));
                }
            }
        }
    }
}
