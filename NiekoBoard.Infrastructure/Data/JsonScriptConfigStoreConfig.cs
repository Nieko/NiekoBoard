using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    public class JsonScriptConfigStoreConfig
    {
        public static string DefaultFileName = "NiekoBoardScriptConfig.json";
        public string Folder { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
