using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public interface IServerErrors
    {
        IList<string> GetCurrentError();
        IServerErrors AddError(string message);
        IServerErrors AddErrors(IEnumerable<string> messages);
    }
}
