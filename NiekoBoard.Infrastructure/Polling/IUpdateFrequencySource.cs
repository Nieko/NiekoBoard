using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    public interface IUpdateFrequencySource
    {
        TimeSpan ConfigFrequency { get; }
        IList<ScriptUpdateFrequency> GetScriptUpdateFrequencies(IDictionary<string, ScriptUpdateFrequency> existingLastUpdates);
    }
}
