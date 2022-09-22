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
        private TableClient _addItemTable;
        private TableClient _removeItemTable;

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

                _addItemTable = _tableServiceClient.GetTableClient("additemlogs");
                _addItemTable.CreateIfNotExists();

                _removeItemTable = _tableServiceClient.GetTableClient("removeitemlogs");
                _removeItemTable.CreateIfNotExists();

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
                try
                {
                    var record = new ChatRecord(senderId, messageType, message, target);
                    await _chatTable.AddEntityAsync(record);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }

        public void LogAddItem(uint senderId, byte type, byte typeId, byte count, int gem1, int gem2, int gem3, int gem4, int gem5, int gem6, bool isEnabledDye, byte r, byte g, byte b, string craftname, string source)
        {
            if (!_isConnected)
                return;

            Task.Run(async () =>
            {
                try
                {
                    var record = new ItemRecord(senderId, type, typeId, count, gem1, gem2, gem3, gem4, gem5, gem6, isEnabledDye, r, g, b, craftname, source);
                    await _addItemTable.AddEntityAsync(record);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }

        public void LogRemoveItem(uint senderId, byte type, byte typeId, byte count, int gem1, int gem2, int gem3, int gem4, int gem5, int gem6, bool isEnabledDye, byte r, byte g, byte b, string craftname, string source)
        {
            if (!_isConnected)
                return;

            Task.Run(async () =>
            {
                try
                {
                    var record = new ItemRecord(senderId, type, typeId, count, gem1, gem2, gem3, gem4, gem5, gem6, isEnabledDye, r, g, b, craftname, source);
                    await _removeItemTable.AddEntityAsync(record);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            });
        }
    }
}
