using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Client
{
    public class GprcScriptHistoryStore : IIisScriptHistoryStore
    {
        private Func<ClientWork> _WorkFactory;

        public GprcScriptHistoryStore(Func<ClientWork> workFactory) 
        {
            _WorkFactory = workFactory;
        }

        public ScriptHistory GetHistory(string scriptId)
        {
            var history = new ScriptHistory
            {
                ScriptId = scriptId
            };
            
            using var worker = GetWorker();

            foreach(var historicItem in worker.History.GetHistory(new Gprc.GetHistoryRequest { ScriptId = scriptId }).History
                .Select(h => new HistoricResult
                {
                    AsAt = DateTime.Parse(h.AsAt),
                    Result = h.Result
                }))
            {
                history.Results.Add(historicItem);
            }

            return history;
        }

        public void Save(string scriptId, HistoricResult result)
        {
            using var worker = GetWorker();

            worker.History.Save(new Gprc.EditHistoryRequest
            {
                ScriptEvent = new Gprc.ScriptHistory
                {
                    AsAt = result.AsAt.ToString(),
                    Result = result.Result
                }
            });
        }
        public void Delete(string scriptId, HistoricResult result)
        {
            using var worker = GetWorker();

            worker.History.Delete(new Gprc.EditHistoryRequest
            {
                ScriptEvent = new Gprc.ScriptHistory
                {
                    AsAt = result.AsAt.ToString(),
                    Result = result.Result
                }
            });
        }

        public void DeleteAll(string scriptId)
        {
            using var worker = GetWorker();

            worker.History.DeleteAll(new Gprc.DeleteAllHistoryRequest { ScriptId = scriptId });
        }

        private ClientWork GetWorker()
        {
            return _WorkFactory();
        }
    }
}
