using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class ScriptWidgetSettings
    {
        public bool AutocreateNewScripts { get; set; } = true;
        public bool NotifyUnassignedScripts { get; set; } = true;
        public TimeSpan StatusPollFrequency { get; set; } = TimeSpan.FromSeconds(15);
    }
}
