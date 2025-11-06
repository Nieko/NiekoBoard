using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    internal class WidgetCharts : IAvaloniaWidgetCharts
    {
        private Func<WidgetName, Type, Type, IWidgetFactory> _Builder;
        private Dictionary<WidgetName, IWidgetFactory> _Builders = new Dictionary<WidgetName, IWidgetFactory>();

        public WidgetCharts(Func<WidgetName, Type, Type, IWidgetFactory> chartBuilder) 
        {
            _Builder = chartBuilder;
        }

        public IWidgetFactory GetChart(WidgetName name)
        {
            return _Builders[name];
        }

        public void AddWidget<TView, TViewModel>(WidgetName name)
            where TView : UserControl
            where TViewModel : IWidgetChartViewModel
        {
            _Builders[name] = _Builder(name, typeof(TView), typeof(TViewModel));
        }
    }
}
