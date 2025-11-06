using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public interface IScriptHistoryStore
    {
        ScriptHistory GetHistory(string scriptId);
        void Save(string scriptId, HistoricResult result);
        void Delete(string scriptId, HistoricResult result);
        void DeleteAll(string scriptId);
    }
}
