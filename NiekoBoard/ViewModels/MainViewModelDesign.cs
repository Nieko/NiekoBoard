using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.ViewModels
{
    public class MainViewModelDesign : IMainViewModel
    {
        public string Title => "Nieko Board Testing";

        public int WidgetRows => 2;

        public int WidgetColumns => 3;

        public IList<IWidgetChartViewModel> Widgets { get; private set; } = new List<IWidgetChartViewModel>();

        public string ServerErrors => "Server Errors have been returned";

        public MainViewModelDesign()
        {
            foreach(var widget in new[]
            {
                new WidgetChartViewModelDesign
                {
                    Widget = WidgetName.Status,
                    Script = new WidgetChartViewModelDesign.ScriptDesign
                    {
                        Name = "Disk Space",
                        Description = "C and D drives"
                    }
                },
                new WidgetChartViewModelDesign
                {
                    Widget = WidgetName.Status,
                    Script = new WidgetChartViewModelDesign.ScriptDesign
                    {
                        Name = "CPU Usage",
                        Description = "Polled CPU Usage"
                    }
                },
                new WidgetChartViewModelDesign
                {
                    Widget = WidgetName.Doughnut,
                    Script = new WidgetChartViewModelDesign.ScriptDesign
                    {
                        Name = "Amazo Script Awesome",
                        Description = "You wouldn't believe it if I told you"
                    }
                }
            })
            {
                Widgets.Add(widget);
            }
        }

        public void StartUpdating() { }

        public void StopUpdating() { }
    }
}
