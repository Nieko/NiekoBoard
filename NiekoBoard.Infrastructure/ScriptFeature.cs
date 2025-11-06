using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard
{
    public enum ScriptFeature
    {
        None = 0,
        ExecuteOnly = 1,
        ScriptResult = 2,
        CustomResult = 4,
        CustomResultEnum = 8,
        MultiValueResult = 16,
        Actions = 32,
        ActionsEnum = 64,
        DefaultConfig = 128
    }
}
