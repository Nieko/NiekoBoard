using Grpc.Core;
using NiekoBoard;
using NiekoBoard.Gprc;

namespace NiekoBoard.Server.Services
{
    public class ScriptService : ScriptsSource.ScriptsSourceBase
    {
        private IScriptSource _ScriptsSource;

        public ScriptService(IScriptSource scriptSource)
        {
            _ScriptsSource = scriptSource;
        }
        public override Task<ScriptsResponse> GetScripts(ScriptsRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {

                var response = new ScriptsResponse();

                try
                {
                    response.Scripts.AddRange(_ScriptsSource.GetScripts()
                        .Select(s =>
                        {
                            var item = new Script
                            {
                                ScriptId = s.ScriptId,
                                Name = s.Name,
                                Description = s.Description,
                                LastChanged = s.LastChanged.ToString(),
                                Features = (int)s.Features
                            };

                            item.CustomResultsRange.AddRange(s.CustomResultsRange ?? Array.Empty<string>());
                            item.Actions.AddRange(s.Actions ?? Array.Empty<string>());

                            return item;
                        }));
                }
                catch (Exception ex)
                {
                    response.Scripts.Clear();
                    response.Error = ex.ToString();
                }

                return response;
            });     
        }

        public override Task<ScriptUpdateResponse> GetLatestUpdate(ScriptUpdateRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var script = _ScriptsSource.GetScripts()
                        .FirstOrDefault(s => s.ScriptId == request.ScriptId);

                    if (script == null)
                    {
                        return new ScriptUpdateResponse
                        {
                            Result = string.Empty
                        };
                    }

                    return new ScriptUpdateResponse { Result = script.GetLatestStatus() };
                }
                catch (Exception ex)
                {
                    return new ScriptUpdateResponse() { Error = ex.ToString() };
                }
            });
        }
        public override Task<ExecuteScriptActionResponse> ExecuteAction(ExecuteScriptActionRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var script = _ScriptsSource.GetScripts()
                        .FirstOrDefault(s => s.ScriptId == request.ScriptId);

                    if (script == null)
                    {
                        return new ExecuteScriptActionResponse
                        {
                            Result = string.Empty
                        };
                    }

                    return new ExecuteScriptActionResponse { Result = request.ActionIndex == 0 ? script.ExecuteAction() : script.ExecuteAction(request.ActionIndex) };
                }
                catch (Exception ex)
                {
                    return new ExecuteScriptActionResponse() { Error = ex.ToString() };
                }
            });
        }
    }
}
