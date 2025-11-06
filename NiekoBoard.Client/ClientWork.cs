using Grpc.Net.Client;
using NiekoBoard.Gprc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Client
{
    public class ClientWork : IDisposable
    {
        GrpcChannel _Channel;
        
        public ScriptsSource.ScriptsSourceClient Scripts { get; private set; }
        public ConfigStore.ConfigStoreClient Configs { get; private set; }

        public HistoryStore.HistoryStoreClient History { get; private set; }

        public ClientWork(ClientGprcSettings settings)
        {
            _Channel = GrpcChannel.ForAddress(settings.AppServerUrl);
            Scripts = new ScriptsSource.ScriptsSourceClient(_Channel);
            Configs = new ConfigStore.ConfigStoreClient(_Channel);
            History = new HistoryStore.HistoryStoreClient(_Channel);
        }

        public void Dispose()
        {
            _Channel.Dispose();
        }
    }
}
