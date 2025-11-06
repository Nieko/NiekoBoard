using Microsoft.Extensions.Logging;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public class ServerErrors : IServerErrors, ILibraryObject
    {
        private static IServerErrors? _Implementation;
        internal static IServerErrors Implementation
        {
            get
            {
                if (_Implementation == null)
                {
                    throw new NullReferenceException(nameof(_Implementation));
                }

                return _Implementation;
            }
        }
        public IServerErrors AddError(string message)
        {
            return Implementation.AddError(message);
        }

        public IServerErrors AddErrors(IEnumerable<string> messages)
        {
            return Implementation.AddErrors(messages);
        }

        public IList<string> GetCurrentError()
        {
            return Implementation.GetCurrentError();
        }

        public LibraryObjDef Define()
        {
            return LibraryObjDef.Define(this, () => _Implementation, se => _Implementation = se);
        }
    }
}
