using Grpc.Core;
using NiekoBoard;
using NiekoBoard.Gprc;

namespace NiekoBoard.Server.Services
{
    public class ConfigService : ConfigStore.ConfigStoreBase
    {
        private IScriptConfigStore _ConfigStore;

        public ConfigService(IScriptConfigStore configStore) 
        {
            _ConfigStore = configStore;
        }

        public override Task<GetConfigsResponse> GetConfigs(GetConfigsRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            { 
                try
                {
                    var response = new GetConfigsResponse();

                    response.Configs.AddRange(_ConfigStore.GetConfigurations()
                        .Select(c =>
                        {
                            var item = new NiekoBoard.Gprc.ScriptConfig
                            {
                                ScriptId = c.ScriptId,
                                ScriptHeader = new Script
                                {
                                    ScriptId = c.ScriptId,
                                    Name = c.Script.Name,
                                    Description = c.Script.Description,
                                    LastChanged = c.Script.LastChanged.ToString()
                                },
                                PollingFrequency = c.PollingFrequency.ToString(),
                                Position = new PositionRectangle
                                {
                                    X = c.Position.X,
                                    Y = c.Position.Y,
                                    Width = c.Position.Width,
                                    Height = c.Position.Height
                                }
                            };

                            item.ScriptHeader.Actions.AddRange(c.Script.Actions ?? Array.Empty<string>());
                            item.ScriptHeader.CustomResultsRange.AddRange(c.Script.CustomResultsRange ?? Array.Empty<string>());
                            item.ScriptHeader.Features = (int)(c.Script.Features);

                            item.Widgets.AddRange(c.Widgets
                                .Select(w => new NiekoBoard.Gprc.ScriptWidget
                                {
                                    WidgetName = w.WidgetName,
                                    ConfigData = w.ConfigData,
                                    ConfigUI = w.ConfigUI,
                                    LastChanged = w.LastChanged.ToString()
                                }));

                            return item;
                        }));

                    return response;
                }
                catch (Exception ex)
                {
                    return new GetConfigsResponse
                    {
                        Error = ex.ToString()
                    };
                }
            });
        }

        public override Task<SaveConfigResponse> SaveConfig(SaveConfigRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    if (request.Config == null)
                    {
                        throw new NullReferenceException(nameof(request.Config));
                    }

                    var response = new SaveConfigResponse();

                    var existingConfig = _ConfigStore.GetConfigurations()
                        .FirstOrDefault(c => c.ScriptId == request?.Config.ScriptId);

                    if(existingConfig == null)
                    {
                        return new SaveConfigResponse();
                    }

                    var position = request?.Config?.Position;

                    if(position == null)
                    {
                        throw new NullReferenceException(nameof(request.Config.Position));
                    }

                    existingConfig.Position = new System.Drawing.Rectangle(position.X, position.Y, position.Width, position.Height);
                    existingConfig.PollingFrequency = TimeSpan.Parse(request?.Config?.PollingFrequency??string.Empty);
                    existingConfig.Widgets.Clear();

                    return response;
                }
                catch (Exception ex)
                {
                    return new SaveConfigResponse
                    {
                        Error = ex.ToString()
                    };
                }
            });
        }
    }
}
