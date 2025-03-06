using Common.Models;

namespace Server.Models.Simulation;

public interface ISimulationHandler
{
	public void InputData(List<Unit> units);
	
	public List<Unit> OutputData();
}