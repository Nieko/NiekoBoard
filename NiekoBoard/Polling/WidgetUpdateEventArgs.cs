using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    public class WidgetUpdateEventArgs : EventArgs
    {
        public IList<ScriptConfig> Removed { get; }
        public IList<ScriptConfig> Added { get; }
        internal WidgetUpdateEventArgs(IList<ScriptConfig> removed, IList<ScriptConfig> added) 
        {
            Removed = removed;
            Added = added;
        }
    }
}
