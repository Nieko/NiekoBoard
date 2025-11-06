using Avalonia.Data.Converters;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Views
{
    public class WidgetViewModelConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(Charts ==  null) return null;

            var viewModel = value as IWidgetChartViewModel;

            if(viewModel == null) return null;

            return Charts.GetChart(viewModel.Widget).BuildView();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static IWidgetCharts? Charts { get; set; }
    }
}
