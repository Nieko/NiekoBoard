using Grpc.Net.Client;
using NiekoBoard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiekoBoard.Gprc;
using NiekoBoard.Service;
using NiekoBoard.Data;

namespace NiekoBoard.Client
{
    public class GprcScriptSource : IIisScriptSource
    {
        private Func<ClientWork> _WorkFactory;

        public GprcScriptSource(Func<ClientWork> workFactory)
        {
            _WorkFactory = workFactory;
        }

        public IList<IScript> GetScripts()
        {
            using (var worker = GetClientWorker())
            {
                return new List<IScript>(worker.Scripts.GetScripts(new ScriptsRequest())
                        .Scripts
                        .Select(s => new ProxyScript(i => ExecuteAction(s.ScriptId, i), () => GetStatus(s.ScriptId))
                        {
                            ScriptId = s.ScriptId,
                            Name = s.Name,
                            Description = s.Description,
                            LastChanged = DateTime.Parse(s.LastChanged),
                            Actions = s.Actions.Count == 0 ? default(IList<string>) : s.Actions,
                            CustomResultsRange = s.CustomResultsRange.Count == 0 ? default(IList<string>) : s.CustomResultsRange,
                            Features = (ScriptFeature)(s.Features)
                        }));
            }
        }

        private string ExecuteAction(string scriptid, int actionId)
        {
            using (var worker = GetClientWorker())
            {
                var result = worker.Scripts.ExecuteAction(new ExecuteScriptActionRequest { ScriptId = scriptid, ActionIndex = actionId });

                return result.Result;
            }
        }

        private string GetStatus(string scriptid)
        {
            using (var worker = GetClientWorker())
            {
                var result = worker.Scripts.GetLatestUpdate(new ScriptUpdateRequest { ScriptId = scriptid });

                return result.Result;
            }
        }

        private ClientWork GetClientWorker()
        {
            return _WorkFactory();
        }
    }
}
