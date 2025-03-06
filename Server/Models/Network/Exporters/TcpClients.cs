using System.Net.Sockets;

namespace Server.Models.Network.Exporters
{
    public class TcpClients
    {
        public static Dictionary<string, NetworkStream> clientStreams = new Dictionary<string, NetworkStream>();
    }
}
