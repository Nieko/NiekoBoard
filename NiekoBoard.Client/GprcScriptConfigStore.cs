using Azure;
using NiekoBoard.Data;
using NiekoBoard.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Client
{
    public class GprcScriptConfigStore : IIisScriptConfigStore
    {
        private Func<ClientWork> _WorkFactory;

        public GprcScriptConfigStore(Func<ClientWork> workFactory)
        {
            _WorkFactory = workFactory;
        }
        public void Delete(ScriptConfig config)
        {
            using var worker = GetWorker();
            
            var response = worker.Configs.DeleteConfig(new Gprc.DeleteConfigRequest
            {
                ScriptId = config.ScriptId
            });

            if (!string.IsNullOrEmpty(response.Error))
            {
                new ServerErrors().AddError(response.Error);
            }
        }

        public IList<ScriptConfig> GetConfigurations()
        {
            using var worker = GetWorker();
            var response = worker.Configs.GetConfigs(new Gprc.GetConfigsRequest());

            if(!string.IsNullOrEmpty(response.Error))
            {
                new ServerErrors().AddError(response.Error);
            }

            return response.Configs
                .Select(c => new ScriptConfig
                {
                    ScriptId = c.ScriptId,
                    Script = new ScriptHeader
                    {
                        ScriptId = c.ScriptHeader.ScriptId,
                        Name = c.ScriptHeader.Name,
                        Description = c.ScriptHeader.Description,
                        LastChanged = DateTime.Parse(c.ScriptHeader.LastChanged),
                        Actions = c.ScriptHeader.Actions.Any() ? null : c.ScriptHeader.Actions,
                        CustomResultsRange = c.ScriptHeader.CustomResultsRange.Any() ? null : c.ScriptHeader.CustomResultsRange,
                        Features = (ScriptFeature)c.ScriptHeader.Features
                    },
                    PollingFrequency = TimeSpan.Parse(c.PollingFrequency),
                    Position = new System.Drawing.Rectangle(c.Position.X, c.Position.Y, c.Position.Width, c.Position.Height),
                    Widgets = c.Widgets
                        .Select(w => new ScriptWidget
                        {
                            WidgetName = w.WidgetName,
                            ConfigData = w.ConfigData,
                            ConfigUI = w.ConfigUI,
                            LastChanged = DateTime.Parse(w.LastChanged)
                        })
                        .ToList()
                })
                .ToList();
        }

        public void Save(ScriptConfig config)
        {
            using var worker = GetWorker();

            var request = new Gprc.SaveConfigRequest
            {
                Config = new Gprc.ScriptConfig
                {
                    ScriptId = config.ScriptId,
                    ScriptHeader = new Gprc.Script
                    {
                        ScriptId = config.ScriptId,
                        Name = config.Script.Name,
                        Description = config.Script.Description,
                        LastChanged = config.Script.LastChanged.ToString(),
                        Features = (int)config.Script.Features
                    },
                    PollingFrequency = config.PollingFrequency.Ticks.ToString(),
                    Position = new Gprc.PositionRectangle
                    {
                        X = config.Position.X,
                        Y = config.Position.Y,
                        Width = config.Position.Width,
                        Height = config.Position.Height
                    }
                }
            };

            request.Config.ScriptHeader.Actions.AddRange(config.Script?.Actions ?? []);
            request.Config.ScriptHeader.CustomResultsRange.AddRange(config.Script?.CustomResultsRange ?? []);
            request.Config.Widgets.AddRange(config.Widgets
                .Select(w => new Gprc.ScriptWidget
                {
                    WidgetName = w.WidgetName,
                    LastChanged = w.LastChanged.ToString(),
                    ConfigData = w.ConfigData,
                    ConfigUI = w.ConfigUI
                }));

            var response = worker.Configs.SaveConfig(request);

            if (!string.IsNullOrEmpty(response.Error))
            {
                new ServerErrors().AddError(response.Error);
            }
        }

        private ClientWork GetWorker()
        {
            return _WorkFactory();
        }
    }
}
