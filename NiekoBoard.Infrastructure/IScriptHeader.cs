using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public interface IScriptHeader
    {
        string ScriptId { get; }
        string Name { get; }
        string Description { get; }
        DateTime LastChanged { get; }
        ScriptFeature Features { get; }
        IList<string>? CustomResultsRange { get; }
        IList<string>? Actions { get; }
    }
}
