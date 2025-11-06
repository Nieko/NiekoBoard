using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class ProxyScript : IScript
    {
        internal Func<int, string> ActionExecutor { get; set; } = i => string.Empty;

        internal Func<string> StatusGetter { get; set; } = () => string.Empty;

        public string ScriptId { get;  set; } = string.Empty;

        public string Name { get;  set; } = string.Empty;

        public string Description { get;  set; } = string.Empty;

        public DateTime LastChanged { get;  set; } = DateTime.MinValue;

        public ScriptFeature Features { get;  set; } = ScriptFeature.ExecuteOnly;

        public IList<string>? CustomResultsRange { get;  set; } = new List<string>();

        public IList<string>? Actions { get;  set; } = new List<string>();

        public ProxyScript(Func<int, string> actionExecutor, Func<string> statusGetter)
        {
            ActionExecutor = actionExecutor;
            StatusGetter = statusGetter;
        }

        public string ExecuteAction(int actionIndex)
        {
            return ActionExecutor(actionIndex);
        }

        public string ExecuteAction()
        {
            return ActionExecutor(0);
        }

        public string GetLatestStatus()
        {
            return  StatusGetter();
        }
    }
}
