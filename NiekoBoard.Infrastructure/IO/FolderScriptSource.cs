using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.IO
{
    public class FolderScriptSource(FolderSourceSettings settings, IScriptConfigStore configStore) : IScriptSource
    {
        private readonly FolderSourceSettings _Settings = settings;
        private readonly IScriptConfigStore _ConfigStore = configStore;

        public IList<IScript> GetScripts()
        {
            var scriptFiles = Directory.GetFiles(_Settings.GetRootPath(), "*.ps1")
                .Where(f => !f.ToUpper().Contains(".lib."))
                .Select(f => new FileInfo(f))
                .ToList();
            var scripts = new List<IScript>();
            var lastConfigs = _ConfigStore.GetConfigurations()
                .ToDictionary(c => c.ScriptId);

            foreach (var scriptSource in scriptFiles)
            {
                var fileNameId = Path.GetFileNameWithoutExtension(scriptSource.Name);

                if (lastConfigs.ContainsKey(fileNameId) && lastConfigs[fileNameId].Script.LastChanged == scriptSource.LastWriteTime)
                {
                    var header = lastConfigs[fileNameId].Script;

                    scripts.Add(new FileScript
                    {
                        SourceFile = scriptSource.FullName,
                        Name = header.Name,
                        Description = header.Description,
                        Features = header.Features,
                        LastChanged = header.LastChanged,
                        CustomResultsRange = header.CustomResultsRange,
                        Actions = header.Actions
                    });
                }
                else
                {
                    var fileScript = new FileScript
                    {
                        SourceFile = scriptSource.FullName,
                        Name = fileNameId,
                        Description = string.Empty
                    };

                    fileScript.UpdateFeatures();

                    scripts.Add(fileScript);

                    if(_Settings.AutoAddNew)
                    {
                        var scriptConfig = new ScriptConfig
                        {
                            Script = fileScript,
                            ScriptId = fileScript.ScriptId,
                        };

                        _ConfigStore.Save(scriptConfig);
                    }
                }
            }

            return scripts;
        }
    }
}
