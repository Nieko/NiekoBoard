using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    internal class WidgetChart : IWidgetFactory
    {
        public WidgetName Widget { get; internal set; }

        public Func<object> BuildView { get; internal set; } = () => new object();

        public Func<IScript, IWidgetChartViewModel> BuildViewModel { get; internal set; } = s => throw new NullReferenceException("WidgetChart not initialized");
    }
}
