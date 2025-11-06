using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public class UISettings
    {
        public string Title { get; set; } = string.Empty;
        public TimeSpan SettingsPollFrequency { get; set; } = new TimeSpan(0, 2, 0);
        public TimeSpan StatusPollFrequencyDefault { get; set; } = new TimeSpan(0, 2, 0);
        public TimeSpan ServerErrorLifetime { get; set; } = new TimeSpan(0, 0, 10);
    }
}
