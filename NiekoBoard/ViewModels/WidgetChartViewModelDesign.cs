using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.ViewModels
{
    public class WidgetChartViewModelDesign : IWidgetChartViewModel
    {
        public class ScriptDesign : IScript
        {
            public string ScriptId => Name;
            public string Name { get; internal set; }

            public string Description { get; internal set; }

            public DateTime LastChanged { get; internal set; }

            public ScriptFeature Features { get; internal set; }

            public IList<string>? CustomResultsRange { get; internal set; }

            public IList<string>? Actions { get; internal set; }

            public string ExecuteAction(int actionIndex)
            {
                return ExecuteAction();
            }

            public string ExecuteAction()
            {
                return "Random Text";
            }

            public string GetLatestStatus()
            {
                return nameof(ScriptResult.Success);
            }
        }

        public WidgetName Widget { get; internal set; }

        public IScript Script { get; set; }

        public string Status => nameof(ScriptResult.Info);

        public string Message => "Some errors encountered";

        public Task Update() { return Task.CompletedTask; }
    }
}
