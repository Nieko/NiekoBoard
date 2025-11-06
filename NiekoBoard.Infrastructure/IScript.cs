using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public interface IScript : IScriptHeader
    {
        string GetLatestStatus();
        string ExecuteAction(int actionIndex);
        string ExecuteAction();
    }
}
