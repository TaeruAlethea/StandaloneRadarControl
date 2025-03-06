using Common.Models;
using Server.ViewModels;

namespace Server.Models.Simulation;

public class PassthroughSimHandler : ISimulationHandler
{
	// External Stuff (Always a Property.)
	private MainWindowViewModel ViewModel { get; init; }
    
	// Internal Stuff
	private Queue<Unit> incomingQueue = new Queue<Unit>();

	public Queue<Unit> IncomingQueue // The Importer uses this to add new things.
	{
		get => incomingQueue; 
		set => incomingQueue = value;
	}
	
	public PassthroughSimHandler(MainWindowViewModel viewModel)
	{
		ViewModel = viewModel;
	}
	
	public Queue<Unit> OutputData()
	{
		return IncomingQueue;
	}
}