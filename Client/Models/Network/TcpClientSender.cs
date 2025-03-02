using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Client.Models.Network
{
    public class TcpClientSender
    {
        public static async Task AsyncSendJsonToServer(NetworkStream stream, JObject jObject)
        {
            try
            {
                if (stream != null)
                {
                    string serialized = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                    byte[] bytes = Encoding.UTF8.GetBytes(serialized);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("TcpClientSender.SendToServer", $"Error in: {ex}");
            }
        }
    }
}
