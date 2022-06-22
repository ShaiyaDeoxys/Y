using Microsoft.AspNetCore.SignalR;

namespace Imgeneus.Monitoring
{
    public class MonitoringHub : Hub
    {
        public const string HubUrl = "/monitoring";
    }
}
