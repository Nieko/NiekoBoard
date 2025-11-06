using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public interface IScriptConfigStore
    {
        IList<ScriptConfig> GetConfigurations();
        void Save(ScriptConfig config);
        void Delete(ScriptConfig config);
    }
}
