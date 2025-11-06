using NiekoBoard.Logging;

namespace NiekoBoard.Service
{
    public class Worker : BackgroundService
    {
        private static object _lock = new object();
        private static int _TotalListeners = 0;
        private WindowsServiceConfig _Config;
        private Func<IServiceServerEndPoint> _EndPointFactory;
        private IServiceScriptCache _ScriptCache;

        public Worker(WindowsServiceConfig config, Func<IServiceServerEndPoint> endPointFactory, IServiceScriptCache scriptCache)
        {
            _Config = config;
            _EndPointFactory = endPointFactory;
            _ScriptCache = scriptCache;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logger = new NiekoLogger();
            Task[] runningTasks = Array.Empty<Task>();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            lock (_lock)
            {
                _ScriptCache.UpdateAll();
            }

            runningTasks = new Task[]
            {
                Task.Factory.StartNew(async () => {
                    while(!stoppingToken.IsCancellationRequested)
                    {
                        _ScriptCache.UpdateAll();
                        await Task.Delay(1000, stoppingToken);
                    }
                }, stoppingToken)
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                int newListenersRequired = 0;

                lock (_lock)
                {
                    newListenersRequired = (int)_Config.MaxConnections - _TotalListeners;
                }

                if (newListenersRequired > 0)
                {
                    lock (_lock)
                    {
                        _TotalListeners = (int)_Config.MaxConnections;
                    }

                    while (newListenersRequired > 0)
                    {
                        var pipeConnection = _EndPointFactory().Start();
                        newListenersRequired--;

                        runningTasks[runningTasks.Length - 1] = pipeConnection
                            .ContinueWith(epf =>
                            {
                                lock (_lock)
                                {
                                    newListenersRequired++;
                                }
                            }, stoppingToken);
                    }
                }
                    
                await Task.Delay(1000, stoppingToken);
            }

            await Task.WhenAll(runningTasks);
        }
    }
}
