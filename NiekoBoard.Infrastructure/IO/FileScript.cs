using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;

namespace NiekoBoard.IO
{
    internal class FileScript : IScript
    {
        internal string SourceFile { get; set; } = string.Empty;

        public string ScriptId => SourceFile;

        public required string Name { get; internal set; }

        public required string Description { get; internal set; }

        public DateTime LastChanged { get; internal set; } = DateTime.MinValue;

        public ScriptFeature Features { get; internal set; } = ScriptFeature.ExecuteOnly;

        public IList<string>? CustomResultsRange { get; internal set; } = new List<string>();

        public IList<string>? Actions { get; internal set; } = new List<string>();

        public string ExecuteAction(int actionIndex)
        {
            object result = string.Empty;
            var actionName = Actions?[actionIndex];
            Collection<PSObject>? invokeResults = null;

            ExecuteAgainstScript(s => $@"{s}

RunAction {actionName}
",
            ps => invokeResults = ps.Invoke());

            return (invokeResults?.FirstOrDefault() ?? string.Empty).ToString();
        }

        public string ExecuteAction()
        {
            object result = string.Empty;
            Collection<PSObject>? invokeResults = null;

            ExecuteAgainstScript(s => $@"{s}

RunAction
",
            ps => invokeResults = ps.Invoke());

            return (invokeResults?.FirstOrDefault() ?? string.Empty).ToString();
        }

        public string GetLatestStatus()
        {
            string result = string.Empty;
            Collection<PSObject>? invokeResults = null;

            if ((this.Features & ScriptFeature.ExecuteOnly) == ScriptFeature.ExecuteOnly)
            {
                ExecuteAgainstScript(s => $@"$ErrorActionPreference = 'Stop'
try {{ 
    {s} 
}}
catch {{
    return ""failed""
}}",
                ps => invokeResults = ps.Invoke());

                result = invokeResults?.Any() == false ? nameof(ScriptResult.Success) : nameof(ScriptResult.Error);
            }
            else
            {

                ExecuteAgainstScript(s => $@"{s}

GetStatus 
",
                ps => invokeResults = ps.Invoke());

                result = (invokeResults ?? new Collection<PSObject>()).
                    Where(ir => ir != null).
                    Select(ir => ir.ToString()).
                    FirstOrDefault() ?? string.Empty;
            }

            return result;
        }

        internal void ExecuteAgainstScript(Func<string,string> scriptAugmentation, Action<PowerShell> executeAction)
        {
            var executionFolder = (new FileInfo(SourceFile)).DirectoryName;
            var shortName = Path.GetFileName(SourceFile);
            var psiss = InitialSessionState.CreateDefault2();
            psiss.ExecutionPolicy = Microsoft.PowerShell.ExecutionPolicy.Bypass;

            using (Runspace runspace = RunspaceFactory.CreateRunspace(psiss))
            {
                runspace.Open();
                runspace.SessionStateProxy.Path.SetLocation(executionFolder);

                using (var powershell = PowerShell.Create(runspace))
                {
                    powershell.AddScript(scriptAugmentation(". .\\" + shortName + "\n"));
                    executeAction(powershell);
                }
            }
        }

        internal void UpdateFeatures()
        {
            Collection<PSObject>? invokeResults = null;

            ExecuteAgainstScript(s => $@"try {{ 
        {s}
    }}
catch {{
    
}}"
            , ps =>
            {
                invokeResults = ps.Invoke();
            });

            if (invokeResults != null && invokeResults.Count > 0)
            {
                this.Features = ScriptFeature.ExecuteOnly;

                return;
            }

            ExecuteAgainstScript(s => $@"{s}
get-command GetStatus", ps => invokeResults = ps.Invoke());

            if (invokeResults == null || !invokeResults.Any())
            {
                this.Features = ScriptFeature.ExecuteOnly;

                return;
            }

            var resultType = invokeResults.First().GetType();

            if (!(resultType == typeof(string) ||
                resultType == typeof(int) ||
                resultType.BaseType == typeof(Enum)))
            {
                this.Features = ScriptFeature.MultiValueResult;
            }
            else
            {
                ExecuteAgainstScript(s => $@"{s}
get-command GetResultsRange", ps => invokeResults = ps.Invoke());

                if (invokeResults != null && invokeResults.Any())
                {
                    this.Features = ScriptFeature.CustomResult & ScriptFeature.CustomResultEnum;

                    this.CustomResultsRange?.Clear();

                    foreach (var result in invokeResults)
                    {
                        this.CustomResultsRange?.Add(result.ToString());
                    }
                }
            }

            ExecuteAgainstScript(s => $@"{s}
get-command RunAction", ps => invokeResults = ps.Invoke());

            if (invokeResults != null && invokeResults.Any())
            {
                this.Features &= ScriptFeature.Actions;

                ExecuteAgainstScript(s => $@"{s}
get-command GetRunActions", ps => invokeResults = ps.Invoke());

                if (invokeResults != null && invokeResults.Any())
                {
                    this.Features  &= ScriptFeature.ActionsEnum;

                    this.Actions?.Clear();

                    foreach (var result in invokeResults)
                    {
                        this.Actions?.Add(result.ToString());
                    }
                }
            }
        }
    }
}
