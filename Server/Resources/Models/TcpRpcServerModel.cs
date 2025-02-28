using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using RurouniJones.Dcs.Grpc.V0.Atmosphere;
using RurouniJones.Dcs.Grpc.V0.Common;
using RurouniJones.Dcs.Grpc.V0.Mission;
using Server.Network;
using Server.Resources.Interfaces;
using Server.ViewModels;

namespace Server.Resources.Models;

public class TcpRpcServerModel : ModelBase, IServerModel
{
	private MainWindowViewModel ViewModel;
	
	private int clientCount;
	public int ClientCount
	{
		get => clientCount;
		set
		{
			clientCount = value; 
			OnPropertyChanged();
		}
	}
	
	private float pingInterval;
	public float PingInterval	{
		get => pingInterval;
		set
		{
			pingInterval = value; 
			OnPropertyChanged();
		}
	}
	
	private int updatesPerSecond;
	public int UpdatesPerSecond	{
		get => updatesPerSecond;
		set
		{
			updatesPerSecond = value; 
			OnPropertyChanged();
		}
	}
	
	private int activePort;
	public int ActivePort	{
		get => activePort;
		set
		{
			activePort = value; 
			OnPropertyChanged();
		}
	}

	private TcpServerHandler TcpServerHandler { get; init; }
	
	public TcpRpcServerModel(MainWindowViewModel viewModel)
	{
		ViewModel = viewModel;
		
		TcpServerHandler = new TcpServerHandler(this, viewModel.Config);

		DcsServerSettings serverSettings = viewModel.Config.DcsServerSettings;
		
		//var channel = CreateChannel("localhost", "50051", "SomeToken");
		var channel = CreateChannel(serverSettings.HostName, 
			serverSettings.ServerToDcsPort.ToString(), 
			serverSettings.Password);
		
		
		var client = new MissionService.MissionServiceClient(channel);
		
		// Timeout or hanging occurs when the server is not Running or the Mission is paused.
		var response = client.GetScenarioCurrentTime(new GetScenarioCurrentTimeRequest { });

		Console.WriteLine($"Current mission time: {response.Datetime}");
	}
	
	public bool StartServer()
	{
		bool tcpActive = TcpServerHandler.Start();
		return tcpActive;
	}

	public bool StopServer()
	{
		_ = TcpServerHandler.StopAsync();
		return false;
	}
	
	public GrpcChannel CreateChannel(string host, string port, string? apiKey)
	{
		GrpcChannelOptions options = new GrpcChannelOptions();
		if (apiKey != null)
		{
			CallCredentials credentials = CallCredentials.FromInterceptor(async (context, metadata) =>
			{
				metadata.Add("X-API-Key", Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(apiKey)) );
			});
			
			Console.WriteLine($"Connecting to {host}:{port} with API Key: {apiKey}");
			
			options.UnsafeUseInsecureChannelCallCredentials = true;
			options.Credentials = ChannelCredentials.Create(ChannelCredentials.Insecure, credentials) ;
		}

		return GrpcChannel.ForAddress($"http://{host}:{port}", options);
	}
}