using Microsoft.Extensions.Logging;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Logging
{
    public class ClientLogs : IClientLogs, ILibraryObject
    {
        private static IClientLogs? _Implementation;

        internal IClientLogs Implementation
        {
            get 
            { 
                if(_Implementation == null)
                {
                    throw new NullReferenceException(nameof(Implementation));
                }

                return _Implementation;
            }
        }

        public IEnumerable<string> GetLogs(LogLevel level)
        {
            return Implementation.GetLogs(level);
        }
        public IEnumerable<string> GetLogs()
        {
            return Implementation.GetLogs();
        }
        public void Clear()
        {
            Implementation.Clear();
        }
        public LibraryObjDef Define()
        {
            return LibraryObjDef.Define(this, () => _Implementation, cl => _Implementation = cl);
        }
    }
}
