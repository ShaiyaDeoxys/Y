using Azure.Data.Tables;
using Imgeneus.Logs.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Imgeneus.Logs
{
    public class LogsManager : ILogsManager
    {
        private readonly ILogger<LogsManager> _logger;
        private bool _isConnected;

        private TableServiceClient _tableServiceClient;

        private TableClient _chatTable;

        public LogsManager(ILogger<LogsManager> logger)
        {
            _logger = logger;
        }


        public void Connect(string connectionString)
        {
            try
            {
                _tableServiceClient = new TableServiceClient(connectionString);

                _chatTable = _tableServiceClient.GetTableClient("chatlogs");
                _chatTable.CreateIfNotExists();

                _isConnected = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                _isConnected = false;
            }
        }

        public void LogChat(uint senderId, string messageType, string message, string target)
        {
            if (!_isConnected)
                return;

            Task.Run(async () =>
            {
                var record = new ChatRecord(senderId, messageType, message, target);
                await _chatTable.AddEntityAsync(record);
            });
        }
    }
}
