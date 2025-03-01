using System.Collections.Concurrent;
using Server.Network;
using Server.Resources.Interfaces;
using Server.ViewModels;

namespace Server.Resources.Models;

public class TcpRpcServerModel : ModelBase, IServerModel
{
	private MainWindowViewModel viewModel;
	
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
	private gRPCImportHandler GRpcImportHandler { get; init; }
	public ConcurrentQueue<Unit> UpdateQueue { get; set; }
	
	public TcpRpcServerModel(MainWindowViewModel viewModel)
	{
		this.viewModel = viewModel;
		
		GRpcImportHandler = new gRPCImportHandler(viewModel.Config.DcsServerSettings); //Import from DCS
		TcpServerHandler = new TcpServerHandler(this, viewModel.Config); // Export to Clients
	}
	
	public bool StartServer()
	{
		bool tcpActive = TcpServerHandler.Start();
		bool gRpcActive = GRpcImportHandler.Start();

		if (gRpcActive && !tcpActive) { GRpcImportHandler.Stop(); } // If TCP fails to start, Stop the other.
		if (tcpActive && !gRpcActive) { _ = TcpServerHandler.StopAsync(); }	 // If gRPC fails to start, stop the other.
		
		return tcpActive && gRpcActive;
	}

	public bool StopServer()
	{
		_ = TcpServerHandler.StopAsync();
		return false;
	}
	

}