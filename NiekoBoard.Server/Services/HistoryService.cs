using Grpc.Core;
using NiekoBoard.Gprc;

namespace NiekoBoard.Server.Services
{
    public class HistoryService : HistoryStore.HistoryStoreBase
    {
        private IScriptHistoryStore _HistoryStore;

        public HistoryService(IScriptHistoryStore historyStore) 
        {
            _HistoryStore = historyStore;
        }
        public override Task<GetHistoryResponse> GetHistory(GetHistoryRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = new GetHistoryResponse();

                    response.History.AddRange(
                        _HistoryStore.GetHistory(request.ScriptId)
                            .Results
                            .Select(h => new Gprc.ScriptHistory
                            {
                                AsAt = h.AsAt.ToString(),
                                Result = h.Result
                            }));

                    return response;
                }
                catch (Exception ex)
                {
                    return new GetHistoryResponse
                    {
                        Error = ex.ToString()
                    };
                }
            });
        }
        public override Task<EditHistoryResponse> Save(EditHistoryRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = new EditHistoryResponse();

                    _HistoryStore.Save(request.ScriptId, new HistoricResult
                    {
                        AsAt = DateTime.Parse(request.ScriptEvent.AsAt),
                        Result = request.ScriptEvent.Result
                    });

                    return response;
                }
                catch (Exception ex)
                {
                    return new EditHistoryResponse { Error = ex.ToString() };
                }
            });
        }

        public override Task<EditHistoryResponse> Delete(EditHistoryRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = new EditHistoryResponse();

                    _HistoryStore.Delete(request.ScriptId, new HistoricResult
                    {
                        AsAt = DateTime.Parse(request.ScriptEvent.AsAt),
                        Result = request.ScriptEvent.Result
                    });

                    return response;
                }
                catch (Exception ex)
                {
                    return new EditHistoryResponse
                    {
                        Error = ex.ToString()
                    };
                }
            });
        }

        public override Task<DeleteAllHistoryResponse> DeleteAll(DeleteAllHistoryRequest request, ServerCallContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var response = new DeleteAllHistoryResponse();

                    _HistoryStore.DeleteAll(request.ScriptId);

                    return response;
                }
                catch (Exception ex)
                {
                    return new DeleteAllHistoryResponse
                    {
                        Error = ex.ToString()
                    };
                }
            });
        }
    }
}
