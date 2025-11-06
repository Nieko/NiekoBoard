using NiekoBoard.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NiekoBoard.Data
{
    [DisplayName("JsonFile")]
    public class JsonScriptConfigStore : IJsonScriptConfigStore
    {
        private FolderSourceSettings _Settings;

        public static string FileName = "NiekoBoardScriptConfig.json";

        public JsonScriptConfigStore(FolderSourceSettings settings) 
        {
            _Settings = settings;
        }

        public IList<ScriptConfig> GetConfigurations()
        {
            var filePath = GetFilePath();

            if(!File.Exists(filePath))
            {
                return new List<ScriptConfig>();
            }

            try
            {
                var jsonData = JsonSerializer.Deserialize<List<ScriptConfigData>>(File.ReadAllText(filePath)) ?? new List<ScriptConfigData>();

                return jsonData.Select(jd => new ScriptConfig
                {
                    ScriptId = jd.ScriptId,
                    Script = new ScriptHeader
                    {
                        ScriptId = jd.ScriptId,
                        Name = jd.Name,
                        Description = jd.Description,
                        LastChanged = jd.LastChanged,
                        Actions = jd.Actions,
                        CustomResultsRange = jd.CustomResultsRange,
                        Features = (ScriptFeature)jd.Features
                    },
                    PollingFrequency = jd.PollingFrequency,
                    Position = new System.Drawing.Rectangle(jd.PositionX, jd.PositionY, jd.PositionWidth, jd.PositionHeight),
                    Widgets = jd.Widgets
                }).ToList();
            }
            catch (Exception)
            {
                return new List<ScriptConfig>();
            }
        }

        public void Save(ScriptConfig config)
        {
            var currentConfigs = GetConfigurations();
            currentConfigs.Where(cc => cc.ScriptId == config.ScriptId)
                .ToList()
                .ForEach(cc => currentConfigs.Remove(cc));

            currentConfigs.Add(config);

            Persist(currentConfigs);
        }
        public void Delete(ScriptConfig config)
        {
            var currentConfigs = GetConfigurations();
            currentConfigs.Where(cc => cc.ScriptId == config.ScriptId)
                .ToList()
                .ForEach(cc => currentConfigs.Remove(cc));

            Persist(currentConfigs);
        }

        private void Persist(IEnumerable<ScriptConfig> items)
        {
            var filePath = GetFilePath();
            var scriptConfigData = items.Select(cc => new ScriptConfigData
            {
                ScriptId = cc.ScriptId,
                Name = cc.Script.Name,
                Description = cc.Script.Description,
                LastChanged = cc.Script.LastChanged,
                Actions = (cc.Script.Actions ?? new string[] { }).ToList(),
                CustomResultsRange = (cc.Script.CustomResultsRange ?? new string[] { }).ToList(),
                Features = (int)cc.Script.Features,
                PollingFrequency = cc.PollingFrequency,
                PositionX = cc.Position.X,
                PositionY = cc.Position.Y,
                PositionHeight = cc.Position.Height,
                PositionWidth = cc.Position.Width,
                Widgets = cc.Widgets
            })
            .ToList();

            var jsonData = JsonSerializer.Serialize<List<ScriptConfigData>>(scriptConfigData);
            File.WriteAllText(filePath, jsonData);
        }

        private string GetFilePath()
        {
            return Path.Combine(_Settings.GetRootPath(), FileName);
        }
    }
}
