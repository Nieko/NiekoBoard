using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace NiekoBoard.Logging
{
    public class MemoryLogger : ILoggerProvider, ILogger, IClientLogs
    {
        private class LogDetail
        {
            public EventId EventId { get; set; }
            public DateTime Added { get; set; }
            public string Message { get; set; }
        }

        private static object _Lock = new();
        private static Dictionary<LogLevel, List<LogDetail>> _Logs = new Dictionary<LogLevel, List<LogDetail>>();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return this;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            List<LogDetail> items = null;

            lock (_Lock)
            {
                if(_Logs == null)
                {
                    return;
                }

                if (!_Logs.TryGetValue(logLevel, out items))
                {
                    items = new List<LogDetail>();
                    _Logs.Add(logLevel, items);
                }

                items.Add(new LogDetail
                {
                    EventId = eventId,
                    Message = formatter(state, exception),
                    Added = DateTime.Now
                });
            }
        }

        public IEnumerable<string> GetLogs(LogLevel level)
        {
            List<string> logs = null;
            var targetLevels = System.Enum.GetValues<LogLevel>()
                .Where(ll => level < ll)
                .ToList();

            lock (_Lock)
            {
                return targetLevels
                    .SelectMany(tl => _Logs[tl])
                    .OrderBy(l => l.Added)
                    .Select(l => l.Message)
                    .ToList();
            }
        }

        public IEnumerable<string> GetLogs()
        {
            lock(_Lock)
            {
                return _Logs.Values.SelectMany(l => l)
                    .OrderBy(l => l.Added)
                    .Select(l => l.Message)
                    .ToList();
            }
        }

        public void Clear()
        {
            lock (_Lock)
            {
                _Logs.Clear();
            }
        }

        public void Dispose()
        {
            lock (_Lock)
            {
                _Logs = null;
            }
        }
    }
}
