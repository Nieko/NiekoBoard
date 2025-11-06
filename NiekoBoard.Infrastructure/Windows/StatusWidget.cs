using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public class StatusWidget : IResultWidget
    {
        public string Name => "Current Status Panel";

        public string Description => "Displays a single result from a script";
    }
}
