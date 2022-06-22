using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Imgeneus.Monitoring
{
    public class SignalRLoggerConfiguration
    {
        public IHubContext<MonitoringHub> HubContext { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
        public int EventId { get; set; } = 0;
    }
}
