using Imgeneus.Authentication.Entities;
using Imgeneus.Database.Entities;
using Imgeneus.Network.PacketProcessor;
using Imgeneus.Network.Packets;
using Imgeneus.Network.Packets.Login;
using System.Linq;

namespace Imgeneus.Login.Packets
{
    internal class LoginPacketFactory : ILoginPacketFactory
    {
        private readonly ILoginServer _server;

        public LoginPacketFactory(ILoginServer server)
        {
            _server = server;
        }

        public void SendLoginHandshake(LoginClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.LOGIN_HANDSHAKE);

            packet.Write<byte>(0);
            packet.Write((byte)client.CryptoManager.RSAPublicExponent.Length); // Exponent length
            packet.Write((byte)client.CryptoManager.RSAModulus.Length);
            packet.WritePaddedBytes(client.CryptoManager.RSAPublicExponent, 64);
            packet.WritePaddedBytes(client.CryptoManager.RSAModulus, 128);

            client.Send(packet, false);
        }

        public void AuthenticationFailed(LoginClient client, AuthenticationResult result)
        {
            using var packet = new ImgeneusPacket(PacketType.LOGIN_REQUEST);

            packet.Write((byte)result);
            packet.Write(new byte[21]);

            client.Send(packet);
        }

        public void AuthenticationSuccess(LoginClient client, AuthenticationResult result, DbUser user, bool isAdmin)
        {
            using var packet = new ImgeneusPacket(PacketType.LOGIN_REQUEST);

            packet.Write((byte)result);
            packet.Write(user.Id);
            packet.Write(isAdmin ? (byte)0 : (byte)255);
            packet.Write(client.Id.ToByteArray());

            client.Send(packet);

            SendServerList(client);
        }

        public void SendServerList(LoginClient client)
        {
            using var packet = new ImgeneusPacket(PacketType.SERVER_LIST);

            var worlds = _server.GetConnectedWorlds();

            packet.Write((byte)worlds.Count());

            foreach (var world in worlds)
            {
                packet.Write(world.Id);
                packet.Write((byte)world.WorldStatus);
                packet.Write(world.ConnectedUsers);
                packet.Write(world.MaxAllowedUsers);
                packet.WriteString(world.Name, 32);
            }

            client.Send(packet);
        }

        public void SelectServerFailed(LoginClient client, SelectServer error)
        {
            using var packet = new ImgeneusPacket(PacketType.SELECT_SERVER);

            packet.Write((sbyte)error);
            packet.Write(new byte[4]);

            client.Send(packet);
        }

        public void SelectServerSuccess(LoginClient client, byte[] worldIp)
        {
            using var packet = new ImgeneusPacket(PacketType.SELECT_SERVER);

            packet.Write((sbyte)SelectServer.Success);
            packet.Write(worldIp);

            client.Send(packet);
        }
    }
}
