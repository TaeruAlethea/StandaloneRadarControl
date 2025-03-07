using System.Text.Json.Serialization;
using Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

	private CancellationTokenSource cancellationToken = new();
	private Task simulationTask;
	
	public PassthroughSimHandler(MainWindowViewModel viewModel)
	{
		ViewModel = viewModel;
	}

	public bool StartHandler()
	{
		try
		{
			cancellationToken = new();
			
			simulationTask = Task.Run(() => simulationTaskWithCancellation(cancellationToken.Token));
			return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return false;
		}
	}

	public bool StopHandler()
	{
		try
		{
			cancellationToken.Cancel();
			
			simulationTask.Dispose();
			cancellationToken.Dispose();
			
			return true;
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			return false;
		}
	}
	
	public Queue<Unit> OutputData()
	{
		return IncomingQueue;
	}

	private async Task simulationTaskWithCancellation(CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			foreach (var unit in IncomingQueue)
			{
				await ViewModel.DataExportHandler.SendDataToAllClients( unit );
			}
			
		}
	}
}