using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    internal class WidgetFactory : IWidgetFactory
    {
        public WidgetName Widget { get; internal set;}

        public required Func<object> BuildView { get; internal set; } = () => new object();

        public required Func<IScript, IWidgetChartViewModel> BuildViewModel { get; internal set; }
    }
}
