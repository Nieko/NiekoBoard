using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public static class IScriptExtensions
    {
        public static string GetDefaultStatus(this IScript script)
        {
            if((script.Features & ScriptFeature.CustomResultEnum) == ScriptFeature.CustomResultEnum)
            {
                return script.CustomResultsRange == null ? nameof(ScriptResult.Success) : script.CustomResultsRange.First();
            }

            return nameof(ScriptResult.Success);
        }

        public static string GetWorstStatus(this IScript script)
        {
            if ((script.Features & ScriptFeature.CustomResultEnum) == ScriptFeature.CustomResultEnum)
            {
                return script.CustomResultsRange == null ? nameof(ScriptResult.Fatal) : script.CustomResultsRange.Last();
            }

            return nameof(ScriptResult.Fatal);
        }

        public static Decimal GetStatusValue(this IScript script, string status)
        {
            List<ScriptResult> standardResultsRange = Enum.GetValues<ScriptResult>().ToList();

            if((script.Features & ScriptFeature.CustomResultEnum) == ScriptFeature.CustomResultEnum)
            {
                if(script.CustomResultsRange == null)
                {
                    return (Decimal)ScriptResult.Fatal;
                }

                var customIndex = script.CustomResultsRange.IndexOf(status);

                if(customIndex == -1)
                {
                    return (Decimal)ScriptResult.Fatal;
                }

                var customRatio = customIndex == 0 ? 0 : ((decimal)customIndex / (decimal)(script.CustomResultsRange.Count - 1));

                return customRatio * (decimal)(standardResultsRange.Count - 1);
            }

            var scriptResultValue = standardResultsRange
                .Select(srr => (Enum.GetName(srr) ?? string.Empty).ToLower())
                .ToList()
                .IndexOf(status.ToLower());

            if(scriptResultValue >= 0)
            {
                return (decimal)standardResultsRange[scriptResultValue];
            }

            Decimal decimalStatus = 0;

            if(!Decimal.TryParse(status, out decimalStatus) || decimalStatus > (standardResultsRange.Select(srr => (int)srr)).Max())
            {
                return (Decimal)(string.IsNullOrWhiteSpace(status) ? ScriptResult.Success : ScriptResult.Fatal);
            }

            return decimalStatus;
        }
    }
}
