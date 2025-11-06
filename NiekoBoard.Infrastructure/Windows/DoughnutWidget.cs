using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Windows
{
    public class DoughnutWidget : IMultiValueWidget
    {
        public string Name => "Doughnut Graph";

        public string Description => "Doughnut graph for one or more single valued series";
    }
}
