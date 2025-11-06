using NiekoBoard.Polling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public class ServiceScriptCache(IScriptSource scriptSource, IUpdateFrequencySource frequencySource, IScriptHistoryStore historyStore) : IServiceScriptCache
    {
        private object _lock = new object();
        private IScriptSource _ScriptSource = scriptSource;
        private IUpdateFrequencySource _FrequencySource = frequencySource;
        private IScriptHistoryStore _HistoryStore = historyStore;
        private DateTime _LastUpdateTime = DateTime.MinValue;
        private DateTime _LastConfigUpdate = DateTime.MinValue;
        private TimeSpan _PollFrequency = TimeSpan.Zero;
        private Dictionary<string, ScriptUpdateFrequency> _Frequencies = new Dictionary<string, ScriptUpdateFrequency>();
        private Dictionary<string, string> _LastStatusById = new Dictionary<string, string>();
        private Dictionary<string, IScript> _ScriptsById = new Dictionary<string, IScript>();

        public string GetLastStatus(string scriptId)
        {
            string? status = null;

            lock (_lock)
            {
                if (_LastStatusById.TryGetValue(scriptId, out status))
                {
                    return status;
                }
            }

            return string.Empty;
        }

        public IScript GetScript(string scriptId)
        {
            IScript? script = null;

            lock (_lock)
            {
                if (_ScriptsById.TryGetValue(scriptId, out script))
                {
                    return script;
                }
            }

#pragma warning disable CS8603 // Possible null reference return.
            return default;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public IList<IScript> GetScripts()
        {
            lock (_lock)
            {
                return _ScriptsById.Values.ToList();
            }
        }

        public TimeSpan GetPollFrequency()
        {
            DateTime updateStart = DateTime.Now;

            if (_PollFrequency == TimeSpan.Zero || _LastConfigUpdate.Add(_FrequencySource.ConfigFrequency) < updateStart)
            {
                var frequencies = _FrequencySource.GetScriptUpdateFrequencies(_Frequencies);
                
                if(!frequencies.Any())
                {
                    _PollFrequency = new TimeSpan(1,0,0,0);

                    return _PollFrequency;
                }

                _PollFrequency = new TimeSpan(frequencies
                    .SelectMany(c1 => frequencies
                        .Select(c2 => long.Abs((c1.Frequency - c2.Frequency).Ticks)))
                    .Where(d => d != 0)
                    .Append(frequencies.Min(f => f.Frequency).Ticks)
                    .Min(d => d));

                foreach(var frequency in frequencies)
                {
                    if(!_Frequencies.ContainsKey(frequency.ScriptId))
                    {
                        _Frequencies[frequency.ScriptId] = frequency;
                    }
                }
            }

            return _PollFrequency;
        }

        public void UpdateAll()
        {
            DateTime updateStart = DateTime.Now;

            lock (_lock)
            {
                var updateFrequency = GetPollFrequency();

                if(_LastUpdateTime.Add(updateFrequency) > updateStart)
                {
                    return;
                }


                var staleScripts = _Frequencies.Values
                    .Where(slu => slu.LastUpdated.Add(slu.Frequency) < updateStart)
                    .ToList();

                if(!staleScripts.Any())
                {
                    return;
                }

                var currentScripts = _ScriptSource.GetScripts()
                    .ToDictionary(s => s.ScriptId);

                foreach (var script in currentScripts.Values)
                {
                    _ScriptsById[script.ScriptId] = script;
                }

                foreach(var removedScript in _ScriptsById.Keys
                    .Where(k => !currentScripts.ContainsKey(k))
                    .ToList())
                {
                    _ScriptsById.Remove(removedScript);
                }   

                foreach (var staleScript in staleScripts)
                {
                    var status = _ScriptsById[staleScript.ScriptId].GetLatestStatus();
                    _LastStatusById[staleScript.ScriptId] = status;
                    staleScript.LastUpdated = updateStart;

                    _HistoryStore.Save(staleScript.ScriptId, new HistoricResult
                    {
                        AsAt = updateStart,
                        Result = status
                    });
                }
            }
        }
    }
}
