using NiekoBoard.Views;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Composition
{
    public class WidgetViewModelConverterInitializer : IUIComponentInitializer
    {
        private IWidgetCharts _Charts;

        public WidgetViewModelConverterInitializer(IWidgetCharts charts) 
        {
            _Charts = charts;
        }

        public void InitializeComponent()
        {
            WidgetViewModelConverter.Charts = _Charts;
        }
    }
}
