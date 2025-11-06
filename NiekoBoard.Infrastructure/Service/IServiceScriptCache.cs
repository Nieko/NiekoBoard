using NiekoBoard.Polling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public interface IServiceScriptCache : IScriptSource
    {
        IScript GetScript(string scriptId);
        string GetLastStatus(string scriptId);
        void UpdateAll();
    }
}
