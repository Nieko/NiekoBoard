using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public interface IWidgetChartViewModel
    {
        string Status { get; }
        string Message { get; }
        WidgetName Widget { get; }
        IScript Script { get; set; }
        Task Update();
    }
}
