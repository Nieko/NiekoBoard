using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    public class ScriptUpdateFrequency
    {
        public string ScriptId { get; set; } = string.Empty;
        public TimeSpan Frequency { get; set; } = TimeSpan.Zero;
        public DateTime LastUpdated { get; set; } = DateTime.MinValue;
    }
}
