using Imgeneus.Database.Entities;
using Imgeneus.Network.Packets.Login;

namespace Imgeneus.Login.Packets
{
    public interface ILoginPacketFactory
    {
        void SendLoginHandshake(LoginClient client);
        void AuthenticationFailed(LoginClient client, AuthenticationResult result);
        void AuthenticationSuccess(LoginClient client, AuthenticationResult result, DbUser user);
        void SelectServerFailed(LoginClient client, SelectServer error);
        void SelectServerSuccess(LoginClient client, byte[] worldIp);
    }
}
