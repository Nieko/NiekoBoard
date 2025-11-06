using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public class ScriptHistory
    {
        public string ScriptId { get; set; } = string.Empty;
        public IList<HistoricResult> Results { get; private set; } = [];
    }
}
