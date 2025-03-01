using System.Collections.Concurrent;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using RurouniJones.Dcs.Grpc.V0.Mission;
using Server.Models;
using Server.Resources.Models;
using Position = Server.Resources.Models.Position;
using Unit = Server.Resources.Models.Unit;

namespace Server.Network;

public class gRPCImportHandler
{
	// Import from DCS
	private DcsServerSettings ServerSettings { get; init; }
	
	private GrpcChannel Channel { get; init; }
	private MissionService.MissionServiceClient MissionDataImport { get; init; }
	
	public ConcurrentQueue<Unit> UpdateQueue { get; set; }
	
	public gRPCImportHandler(DcsServerSettings serverSettings)
	{
		ServerSettings = serverSettings;

		Channel = CreateChannel(ServerSettings.HostName, ServerSettings.SrcToDcsPort.ToString(),
			ServerSettings.Password);
		
		MissionDataImport = new MissionService.MissionServiceClient(Channel);
		
	}

	public bool Start()
	{
		//TODO: See if the Channel is valid after a dispose/stop and start in the same session.
		
		var missionDataImport = new MissionService.MissionServiceClient(Channel);
	
		try
		{
			missionDataImport.StreamUnits( new StreamUnitsRequest(){ PollRate = 10, MaxBackoff = 60 } );
			var response = missionDataImport.GetScenarioCurrentTime(new GetScenarioCurrentTimeRequest { });
			Console.WriteLine($"Current mission time: {response.Datetime}");
			
			
			
			return true;
		}
		catch (Exception ex)
		{
			// Timeout or hanging occurs when the server is not Running or the Mission is paused.
			Console.WriteLine(ex.Message);
			return false;
		}
	}

	public void Stop()
	{
		Channel.Dispose();
	}
	
	private GrpcChannel CreateChannel(string host, string port, string? apiKey)
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
	
	public async Task StreamUnitsAsync(uint pollRate, CancellationToken stoppingToken)
    {
        try
        {
            var units = MissionDataImport.StreamUnits(new StreamUnitsRequest
            {
                PollRate = pollRate,
                MaxBackoff = 30
            }, null, null, stoppingToken);
            await foreach (var update in units.ResponseStream.ReadAllAsync(stoppingToken))
            {
                switch (update.UpdateCase)
                {
                    case StreamUnitsResponse.UpdateOneofCase.None:
                        //No-op
                        break;
                    case StreamUnitsResponse.UpdateOneofCase.Unit:
                        var sourceUnit = update.Unit;
                        UpdateQueue.Enqueue(new Unit
                        {
                            Coalition = (int)sourceUnit.Coalition,
                            Id = sourceUnit.Id,
                            Name = sourceUnit.Name,
                            Position = new Position(sourceUnit.Position.Lat, sourceUnit.Position.Lon),
                            Altitude = sourceUnit.Position.Alt,
                            Callsign = sourceUnit.Callsign,
                            Type = sourceUnit.Type,
                            Player = sourceUnit.PlayerName,
                            GroupName = sourceUnit.Group.Name,
                            Speed = sourceUnit.Velocity.Speed,
                            Heading = sourceUnit.Orientation.Heading
                        });
						Logger.Info("gRPCImportHandler", $"Enqueue unit update {sourceUnit}" );
                        break;
                    case StreamUnitsResponse.UpdateOneofCase.Gone:
                        var deletedUnit = update.Gone;
                        UpdateQueue.Enqueue(new Unit
                        {
                            Id = deletedUnit.Id,
                            Name = deletedUnit.Name,
                            Deleted = true
                        });
						Logger.Info("gRPCImportHandler", $"Enqueue unit deletion {deletedUnit}");
                        break;
                    default:
						Logger.Error("gRPCImportHandler", $"Unexpected UnitUpdate case of {update.UpdateCase}");
                        break;
                }
            }
        }
        catch (RpcException ex)
        {
            if (ex.Status.StatusCode == StatusCode.Cancelled)
            {
				Logger.Info("gRPCImportHandler", $"Shutting down gRPC connection due to {ex.Status.Detail}" );
            }
            else
            {
				Logger.Warning("gRPCImportHandler", $"gRPC Exception: {ex}");
            }
        }
        catch (Exception ex)
        {
			Logger.Warning("gRPCImportHandler", $"gRPC Exception: {ex}");
        }
    }
}