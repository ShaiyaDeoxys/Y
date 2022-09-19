using Imgeneus.World.Game.Player;

namespace Imgeneus.World.Game.Chat
{
    public interface IChatManager
    {
        /// <summary>
        /// Sends message.
        /// </summary>
        /// <param name="sender">Character, that generated message.</param>
        /// <param name="messageType">type of message</param>
        /// <param name="message">message itself</param>
        /// <param name="targetName">optional, target name</param>
        public void SendMessage(Character sender, MessageType messageType, string message, string targetName = "");

        /// <summary>
        /// Is the next message in normal chat willl be message to server?
        /// </summary>
        public bool IsMessageToServer { get; set; }

        /// <summary>
        /// Character can not write in any chat?
        /// </summary>
        public bool IsMuted { get; set; }
    }
}
