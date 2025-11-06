using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Composition
{
    public interface ILibraryModule : IModule
    {
        IEnumerable<ILibraryObject> GetLibraryObjects();
    }
}
