using Common.Models;

namespace Server.Models.Simulation;

public class PassthroughSimHandler : ISimulationHandler
{
	private List<Unit> units { get; set; }
	
	public void InputData(List<Unit> inputUnits)
	{
		units = inputUnits;
	}

	public List<Unit> OutputData()
	{
		return units;
	}
}