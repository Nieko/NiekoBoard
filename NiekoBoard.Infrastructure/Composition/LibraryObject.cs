using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Composition
{
    public sealed class LibraryObjDef
    {
        private Action<object> _Initialization = i => { };

        private LibraryObjDef() { }

        public Type ImplementationType { get; private set; } = typeof(LibraryObjDef);

        internal void Initialize(object implementation)
        {
            _Initialization(implementation);
        }

        public static LibraryObjDef Define<T, TImplementation>(T definer, Func<TImplementation> getImplementation, Action<TImplementation> setImplementation)
            where T : ILibraryObject
        {
            var objDef = new LibraryObjDef
            {
                ImplementationType = typeof(TImplementation),

                _Initialization = (object i) =>
                {
                    if (!(i is TImplementation))
                    {
                        throw new InvalidCastException();
                    }

                    if(i.GetType() == typeof(T))
                    {
                        throw new InvalidCastException(i.GetType().Name + " must implement " + typeof(TImplementation).Name + " but cannot be the library type " + typeof(T).Name);
                    }

                    if (getImplementation() != null)
                    {
                        throw new InvalidOperationException("Implementation is already set");
                    }

                    setImplementation((TImplementation)i);
                }
            };

            return objDef;
        }
    }
}
