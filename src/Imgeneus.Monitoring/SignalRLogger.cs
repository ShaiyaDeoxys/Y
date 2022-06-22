using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Imgeneus.Monitoring
{
    public class SignalRLogger : ILogger
    {
        private readonly SignalRLoggerConfiguration _config;
        public SignalRLogger(SignalRLoggerConfiguration config)
        {
            _config = config;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _config.LogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                        Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            if (_config.EventId == 0 || _config.EventId == eventId.Id)
                _config.HubContext.Clients.All.SendAsync("Broadcast", string.Format("[{0}] {1}-UTC: {2}", logLevel.ToString().ToUpper(), DateTimeOffset.UtcNow.ToString("T"), formatter(state, exception)));
        }
    }
}

