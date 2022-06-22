using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Imgeneus.Monitoring
{
    public class SignalRLoggerProvider : ILoggerProvider
    {
        private readonly SignalRLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, SignalRLogger> _loggers = new ConcurrentDictionary<string, SignalRLogger>();

        public SignalRLoggerProvider(SignalRLoggerConfiguration config)
            => _config = config;

        public ILogger CreateLogger(string categoryName)
            => _loggers.GetOrAdd(categoryName, name => new SignalRLogger(_config));

        public void Dispose()
            => _loggers.Clear();
    }
}
