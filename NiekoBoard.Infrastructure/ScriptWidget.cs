using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public sealed class ScriptWidget
    {
        public DateTime LastChanged { get; set; } = DateTime.Now;
        public string WidgetName { get; set; } = Enum.GetName(Windows.WidgetName.Status) ?? string.Empty;
        public string ConfigData { get; set; } = string.Empty;
        public string ConfigUI { get; set; } = string.Empty;
    }
}
