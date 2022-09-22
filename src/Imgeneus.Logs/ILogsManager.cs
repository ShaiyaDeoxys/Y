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
    }
}
