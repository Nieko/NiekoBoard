using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Composition
{
    public class LibraryInitializer
    {
        private IEnumerable<Func<ILibraryObject>> _Initializers;

        public LibraryInitializer(IEnumerable<Func<ILibraryObject>> initializers)
        {
            _Initializers = initializers;
        }

        public void Initialize() 
        { 
            foreach(var initializer in _Initializers)
            {
                initializer();
            }
        }
    }
}
