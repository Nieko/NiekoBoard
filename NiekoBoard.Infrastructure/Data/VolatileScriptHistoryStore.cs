using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class VolatileScriptHistoryStore : IVolatileScriptHistoryStore
    {
        Dictionary<string, Dictionary<DateTime, string>> _History = new Dictionary<string, Dictionary<DateTime, string>>();

        public void Delete(string scriptId, HistoricResult result)
        {
            var items = GetScriptHistory(scriptId);

            if(items.ContainsKey(result.AsAt))
            {
                items.Remove(result.AsAt);
            }
        }

        public void DeleteAll(string scriptId)
        {
            GetScriptHistory(scriptId).Clear();
        }

        public ScriptHistory GetHistory(string scriptId)
        {
            var history = new ScriptHistory
            {
                ScriptId = scriptId
            };

            foreach (var item in GetScriptHistory(scriptId)
                .Select(kvp => new HistoricResult
                {
                    AsAt = kvp.Key,
                    Result = kvp.Value
                }))
            {
                history.Results.Add(item);
            }

            return history;
        }

        public void Save(string scriptId, HistoricResult result)
        {
            GetScriptHistory(scriptId)[result.AsAt] = result.Result;
        }

        private Dictionary<DateTime, string> GetScriptHistory(string scriptId)
        {
            Dictionary<DateTime, string> scriptHistory = null;

            if(!_History.TryGetValue(scriptId, out scriptHistory))
            {
                scriptHistory = new();
                _History.Add(scriptId, scriptHistory);
            }

            return scriptHistory;
        }
    }
}
