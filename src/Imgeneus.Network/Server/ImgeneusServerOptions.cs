using Imgeneus.Network.PacketProcessor;
using LiteNetwork.Server;

namespace Imgeneus.Network.Server
{
    public class ImgeneusServerOptions : LiteServerOptions
    {
        public ImgeneusServerOptions() : base()
        {
            PacketProcessor = new ImgeneusPacketProcessor();
        }
    }
}
