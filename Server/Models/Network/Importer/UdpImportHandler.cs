using System.Net.Sockets;
using System.Text;
using Common.Models;
using Newtonsoft.Json.Linq;
using Server.Models.Utils;
using Server.ViewModels;

namespace Server.Models.Network.Importer
{
    public class UdpImportHandler : IDataImportHandler
    {
        // External Stuff (Always a Property.)
        private MainWindowViewModel ViewModel { get; init; }
        
        // Internal Stuff
        public bool HandlerActive { get; private set; }
        public float UpdatesPerSecond { get; } //Unimplimented
        public string DcsHostName { get; init; } //Unimplimented
        public int SrcToDcsPort { get; init; } //Undeclared
        public int DcsToSrcPort { get; init; }

        private CancellationTokenSource? cancellationTokenSource;
        private UdpClient? udpClient;
        private Task? udpTask;
        
        public UdpImportHandler(MainWindowViewModel mainWindowViewModel)
        {
            this.ViewModel = mainWindowViewModel;
            
            UpdatesPerSecond = 0;
            DcsHostName = "localhost";
            
            DcsToSrcPort = ViewModel.Config.DCS_SERVER_SETTINGS.DCS_TO_SRC_PORT;
            Console.WriteLine(DcsToSrcPort);
        }

        public bool StartHandler()
        {
            try
            {
                cancellationTokenSource = new CancellationTokenSource();
                udpClient = new UdpClient(DcsToSrcPort);

                udpTask = Task.Run(() => DCSListener(cancellationTokenSource.Token));

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("UdpServerHandler.Start", ex.ToString());
                return false;
            }
        }

        public bool StopHandler()
        {
            try
            {
                cancellationTokenSource?.Cancel();
                udpClient?.Close();
                udpClient?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error("UdpServerHandler.Stop", ex.ToString());
                return false;
            }

            return true;
        }

        private async Task DCSListener(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        Console.WriteLine($"Starting UDP server at {DcsToSrcPort}");
                        if (udpClient == null) return;
                        Console.WriteLine($"udpClient Valid");

                        UdpReceiveResult result = await udpClient.ReceiveAsync();
                        string json = Encoding.UTF8.GetString(result.Buffer);

                        Console.WriteLine(json);
                        
                        if (string.IsNullOrWhiteSpace(json))
                            continue;

                        JObject? receivedJson = JObject.Parse(json);

                        if (receivedJson.TryGetValue("callback", out JToken? callbackToken) &&
                            callbackToken?.Type == JTokenType.String)
                        {
                            string callback = callbackToken.ToString();
                            if (callback == "OnGlobalContactExport")
                            {
                                Console.WriteLine(receivedJson);
                                
                                Unit recievedUnit = new Unit{
                                    Name = receivedJson["name"].Value<string>(),
                                    Player = receivedJson["player"].Value<string>(),
                                    GroupName = receivedJson["type"].Value<string>(),
                                    Coalition = receivedJson["side"].Value<int>(),
                                    Type = receivedJson["type"].Value<string>(),
                                    Position = new Position(receivedJson["lat"].Value<double>(), receivedJson["lon"].Value<double>()),
                                    Altitude = receivedJson["alt"].Value<double>(),
                                    Heading = receivedJson["heading"].Value<double>(),
                                    Speed = double.Sqrt(
                                        (receivedJson["velocity"]["x"].Value<double>() * receivedJson["velocity"]["x"].Value<double>()) +
                                        (receivedJson["velocity"]["y"].Value<double>() * receivedJson["velocity"]["y"].Value<double>()) +
                                        (receivedJson["velocity"]["z"].Value<double>() * receivedJson["velocity"]["z"].Value<double>())
                                        ), // Get the Magnitude of the vector
                                    Velocity = new Velocity()
                                    {
                                        X = receivedJson["velocity"]["x"].Value<double>(),
                                        Y = receivedJson["velocity"]["y"].Value<double>(),
                                        Z = receivedJson["velocity"]["z"].Value<double>()
                                    }
                                };

                                ViewModel.SimulationHandler.IncomingQueue.Enqueue(recievedUnit);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("UdpServerHandler.DCSListener", ex.ToString());
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Debug("UdpServerHandler.DCSListener", "Listener stopped.");
            }
        }
    }
}
