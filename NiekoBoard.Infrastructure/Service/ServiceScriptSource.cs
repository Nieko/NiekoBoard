using Markdig.Extensions.Tables;
using NiekoBoard.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public class ServiceScriptSource : IScriptSource
    {
        private IServiceClientEndPoint _Client;
        private ScriptSerializer? _Serializer;

        internal ScriptSerializer Serializer
        {
            get
            {
                return _Serializer ??= new ScriptSerializer(sd => new ProxyScript(i => ExecuteAction(sd.ScriptId, i), () => GetStatus(sd.ScriptId))
                {
                    ScriptId = sd.ScriptId,
                    Name = sd.Name,
                    Description = sd.Description,
                    LastChanged = sd.LastChanged,
                    Features = sd.Features,
                    Actions = sd.Actions,
                    CustomResultsRange = sd.CustomResultsRange
                });
            }
        }
            
        public ServiceScriptSource(IServiceClientEndPoint client)
        {
            _Client = client;
        }

        public IList<IScript> GetScripts()
        {
            var request = Serializer.Serialize();
            var response = _Client.Get(request);

            response.Wait();

            return (IList<IScript>)Serializer.Deserialize(response.Result).Data;
        }

        private string ExecuteAction(string scriptId, int index)
        {
            var request = Serializer.Serialize(nameof(IScript.ExecuteAction), index, scriptId);
            var response = _Client.Get(request);

            response.Wait();

            return response.Result;
        }
        private string GetStatus(string scriptId)
        {
            var request = Serializer.Serialize(scriptId);
            var response = _Client.Get(request);

            response.Wait();

            return (string)Serializer.Deserialize(response.Result).Data;
        }
    }
}
