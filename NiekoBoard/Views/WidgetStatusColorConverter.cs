using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Views
{
    public class WidgetStatusColorConverter : IValueConverter
    {
        private bool _ColorsLoaded = false;
        private Color _StartColor = Colors.Green;
        private Color _WarningColor = Colors.Orange;
        private Color _EndColor = Colors.Red;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var widget = value as IWidgetChartViewModel;

            if (!_ColorsLoaded)
            {
                object? resObj;

                if (Application.Current?.TryGetResource("SuccessStatusColor", null, out resObj) == true && resObj is Color)
                {
                    _StartColor = (Color)resObj;
                }

                if (Application.Current?.TryGetResource("WarningStatusColor", null, out resObj) == true && resObj is Color)
                {
                    _WarningColor = (Color)resObj;
                }

                if (Application.Current?.TryGetResource("FailStatusColor", null, out resObj) == true && resObj is Color)
                {
                    _EndColor = (Color)resObj;
                }

                _ColorsLoaded = true;
            }

            var color = widget == null ? _WarningColor : GetColor(widget);

            if (parameter == null)
            {
                return new SolidColorBrush(color);
            }

            var boxShadowParams = (parameter.ToString() ?? string.Empty).Split(",")
                .Select(s =>
                {
                    double x;
                    if (!double.TryParse(s, out x))
                    {
                        return 0;
                    }

                    return x;
                })
                .ToList();

            while (boxShadowParams.Count < 4)
            {
                boxShadowParams.Add(0);
            }

            return BoxShadows.Parse(boxShadowParams
                .Select(bsp => bsp.ToString())
                .Append(color.ToString())
                .Aggregate(string.Empty, (total, current) => total + (current == string.Empty ? string.Empty : " ") + current));
        }

        private Color GetColor(IWidgetChartViewModel widget)
        {
            var specificValues = new[] {
                new { Result = ScriptResult.Success, Color = _StartColor },
                new { Result = ScriptResult.Warning, Color = _WarningColor },
                new { Result = ScriptResult.Fatal, Color = _EndColor }
            }
            .ToDictionary(rc => (decimal)rc.Result);
            var statusValue = widget.Script.GetStatusValue(widget.Status);
            if (specificValues.ContainsKey(statusValue))
            {
                return specificValues[statusValue].Color;
            }

            var failProgression = statusValue / (int)ScriptResult.Fatal;
            var firstHalf = failProgression < (decimal)ScriptResult.Warning;
            Color lowerEnd = firstHalf ? _StartColor : _WarningColor;
            Color higherEnd = firstHalf ? _WarningColor : _EndColor;

            return new Color(255, (byte)((lowerEnd.R + higherEnd.R) / 2.0M), (byte)((lowerEnd.G + higherEnd.G) / 2.0M), (byte)((lowerEnd.B + higherEnd.B) / 2.0M));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
