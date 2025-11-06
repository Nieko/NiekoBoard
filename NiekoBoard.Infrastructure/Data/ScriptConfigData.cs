using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class ScriptConfigData
    {
        public string ScriptId { get; set; } = string.Empty;
        public TimeSpan PollingFrequency { get; set; } = TimeSpan.FromMinutes(1);
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int PositionWidth { get; set; }
        public int PositionHeight { get; set; }
        public List<ScriptWidget> Widgets { get; set; } = new List<ScriptWidget>();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime LastChanged { get; set; } = DateTime.MinValue;
        public int Features { get; set; } = 0;
        public List<string> CustomResultsRange { get; set; } = new List<string>();
        public List<string> Actions { get; set; } = new List<string>();
    }
}
