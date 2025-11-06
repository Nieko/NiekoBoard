using Microsoft.Extensions.Logging;
using NiekoBoard.Composition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Logging
{
    public class NiekoLogger : ILogger, ILibraryObject
    {
        private static ILogger? _Implementation;
        internal static ILogger Implementation 
        {
            get
            {
                if(_Implementation == null )
                {
                    throw new NullReferenceException(nameof(_Implementation));
                }

                return _Implementation;
            }
        }
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return Implementation.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Implementation.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            Implementation.Log(logLevel, eventId, state, exception, formatter);
        }

        public LibraryObjDef Define()
        {
            return LibraryObjDef.Define(this, () => _Implementation, i => _Implementation = i);
        }
    }
}
