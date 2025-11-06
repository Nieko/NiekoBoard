using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Polling
{
    public interface IChartUpdater
    {
        event EventHandler<WidgetUpdateEventArgs> ConfigsChanged;
        void Start(Action<Action<ICollection<IWidgetChartViewModel>>> widgetsWorker);
        void Stop();
    }
}
