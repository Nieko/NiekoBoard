using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public interface IAvaloniaWidgetCharts : IWidgetCharts
    {
        void AddWidget<TView, TViewModel>(WidgetName name)
            where TView : UserControl
            where TViewModel : IWidgetChartViewModel;
    }
}
