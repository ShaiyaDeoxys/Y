using System.Threading.Tasks;

namespace Imgeneus.Logs
{
    public interface ILogsManager
    {
        /// <summary>
        /// Inits connection to logs storage.
        /// </summary>
        void Connect(string connectionString);

        void LogChat(uint senderId, string messageType, string message, string target);
        void LogAddItem(uint ownerId, byte type, byte typeId, byte count, int gem1, int gem2, int gem3, int gem4, int gem5, int gem6, bool isEnabledDye, byte r, byte g, byte b, string craftname, string source);
        void LogRemoveItem(uint senderId, byte type, byte typeId, byte count, int gem1, int gem2, int gem3, int gem4, int gem5, int gem6, bool isEnabledDye, byte r, byte g, byte b, string craftname, string source);
    }
}
