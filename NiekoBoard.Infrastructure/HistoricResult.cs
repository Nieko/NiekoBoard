using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public sealed class HistoricResult
    {
        public DateTime AsAt { get; set; } = DateTime.MinValue;
        public string Result { get; set; } = string.Empty;
    }
}
