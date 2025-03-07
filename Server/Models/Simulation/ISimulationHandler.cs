using Common.Models;

namespace Server.Models.Simulation;

public interface ISimulationHandler
{
	public Queue<Unit> IncomingQueue { get; set; }

	public Queue<Unit> OutputData();
	
	public bool StartHandler();
	public bool StopHandler();
	
}