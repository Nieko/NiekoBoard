using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Service
{
    public interface IServiceClientEndPoint
    {
        IList<string> Errors { get; }
        Task<string> Get(string parameters);
    }
}
