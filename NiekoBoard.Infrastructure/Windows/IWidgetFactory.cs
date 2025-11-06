using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public interface IWidgetFactory
    {
        WidgetName Widget { get; }
        Func<object> BuildView { get; }
        Func<IScript, IWidgetChartViewModel> BuildViewModel { get; }
    }
}
