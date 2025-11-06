using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Logging
{
    public interface IClientLogs
    {
        IEnumerable<string> GetLogs(LogLevel level);
        IEnumerable<string> GetLogs();
        void Clear();
    }
}
