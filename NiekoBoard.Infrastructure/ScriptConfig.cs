using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public sealed class ScriptConfig
    {
        public string ScriptId { get; set; } = string.Empty;
        public required IScriptHeader Script { get; set; }
        public TimeSpan PollingFrequency { get; set; } = TimeSpan.FromMinutes(1);
        public Rectangle Position { get; set; }
        public List<ScriptWidget> Widgets { get; set; } = new List<ScriptWidget>();
        public override bool Equals(object? obj)
        {
            if (obj is not ScriptConfig other) return false;

            if (Widgets.Count != other.Widgets.Count) return false;

            if(! (ScriptId == other.ScriptId ||
                Script.LastChanged == other.Script.LastChanged))
            {
                return false;
            }

            var i = 0;
            foreach (var widget in Widgets)
            {
                if (widget.LastChanged != other.Widgets[i].LastChanged)
                {
                    return false;
                }

                i++;
            }

            return true;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 29;
                hash = hash * 13 + (ScriptId??string.Empty).GetHashCode();
                
                foreach(var widget in Widgets)
                {
                    hash = hash * 13 + widget.WidgetName.GetHashCode();
                }

                return hash;
            }
        }
    }
}
