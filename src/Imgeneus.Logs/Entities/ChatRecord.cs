
using System;

namespace Imgeneus.Logs.Entities
{
    public record ChatRecord : BaseTableEntity
    {
        public string MessageType { get; init; }

        public string Message { get; init; }

        public string Target { get; init; }

        public ChatRecord(uint senderId, string messageType, string message, string target) : base(senderId)
        {
            MessageType = messageType;
            Message = message;
            Target = target;
        }
    }
}
