using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.IO
{
    public class FolderSourceSettings
    {
        public string RootFolder { get; set; } = string.Empty;

        public bool AutoAddNew { get; set; } = false;

        public string GetRootPath()
        {
            return Path.GetFullPath(string.IsNullOrWhiteSpace(RootFolder) ? "." : RootFolder);
        }
    }
}
