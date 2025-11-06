using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class ScriptHeader : IScriptHeader
    {
        public string ScriptId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime LastChanged { get; set; }
        public ScriptFeature Features { get; set; }
        public IList<string>? CustomResultsRange { get; set; }
        public IList<string>? Actions { get; set; }
    }
}
