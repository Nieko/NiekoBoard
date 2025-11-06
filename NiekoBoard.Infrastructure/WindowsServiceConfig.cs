using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public class WindowsServiceConfig
    {
        public static TimeSpan MinimumUpdateFrequency = new TimeSpan(0, 0, 1);
        public string PipeName { get; set; } = "NiekoBoardService";
        public string ClientUserName { get; set; } = "IIS_IUSRS";
        public uint Retries { get; set; } = 8;
        public uint MaxConnections { get; set; } = 10;
        public TimeSpan ConfigUpdateFrequency { get; set; } = new TimeSpan(0, 1, 0);
        public TimeSpan DefaultScriptUpdateFrequency { get; set; } = new TimeSpan(0, 1, 0);
    }
}
