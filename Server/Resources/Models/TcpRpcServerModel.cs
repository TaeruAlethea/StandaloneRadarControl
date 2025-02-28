using Server.Resources.Interfaces;

namespace Server.Resources.Models;

public class TcpRpcServerModel : ModelBase, IServerModel
{
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
	
	
	
	public bool StartServer()
	{
		throw new NotImplementedException();
	}

	public bool StopServer()
	{
		throw new NotImplementedException();
	}
}