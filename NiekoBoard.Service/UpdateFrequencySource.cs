using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiekoBoard.Polling;

namespace NiekoBoard.Service
{
    public class UpdateFrequencySource: IUpdateFrequencySource
    {
        private WindowsServiceConfig _Config;
        private IScriptSource _ScriptSource;
        private IScriptConfigStore _ScriptConfigStore;

        public UpdateFrequencySource(WindowsServiceConfig config, IScriptSource scriptSource, IScriptConfigStore scriptConfigStore) 
        {
            _Config = config;
            _ScriptSource = scriptSource;
            _ScriptConfigStore = scriptConfigStore;
        }

        public TimeSpan ConfigFrequency => _Config.ConfigUpdateFrequency;

        public IList<ScriptUpdateFrequency> GetScriptUpdateFrequencies(IDictionary<string, ScriptUpdateFrequency> existingLastUpdates)
        {
            var existingConfig = _ScriptConfigStore.GetConfigurations();
            var configuredScripts = new HashSet<string>(existingConfig.Select(ec => ec.ScriptId));

            var missingScripts = _ScriptSource
                    .GetScripts()
                    .Where(ss => !configuredScripts.Contains(ss.ScriptId));

            foreach(var missingScript in missingScripts)
            {
                _ScriptConfigStore.Save(new ScriptConfig
                {
                    ScriptId = missingScript.ScriptId,
                    Script = missingScript,
                    PollingFrequency = _Config.DefaultScriptUpdateFrequency,
                    Position = new System.Drawing.Rectangle(0, 0, 1, 1),
                    Widgets = new() { new() }
                });
            }

            return existingConfig
                .Select(ec => new ScriptUpdateFrequency
                {
                    ScriptId = ec.ScriptId,
                    Frequency = ec.PollingFrequency,
                    LastUpdated = existingLastUpdates.ContainsKey(ec.ScriptId) ? existingLastUpdates[ec.ScriptId].LastUpdated : DateTime.MinValue
                })
                .Concat(missingScripts
                    .Select(ss => new ScriptUpdateFrequency
                    {
                        ScriptId = ss.ScriptId,
                        Frequency = _Config.DefaultScriptUpdateFrequency,
                        LastUpdated = existingLastUpdates.ContainsKey(ss.ScriptId) ? existingLastUpdates[ss.ScriptId].LastUpdated : DateTime.MinValue
                    })
                    .ToList())
                .ToList();
        }
    }
}
