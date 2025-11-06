using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.ViewModels
{
    public interface IMainViewModel
    {
        string Title { get; }
        string ServerErrors { get; }
        int WidgetRows { get; }
        int WidgetColumns { get; }
        IList<IWidgetChartViewModel> Widgets { get; }
        void StartUpdating();
        void StopUpdating();
    }
}
