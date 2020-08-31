using common_lib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CallSimulatorService : BackgroundService
{
    Thread simThread = null;

    private readonly ILogger<CallSimulatorService> _logger;
    private readonly ICallPanel _panelService;

    public CallSimulatorService(ILogger<CallSimulatorService> logger, ICallPanel panelService)
    {
        _logger = logger;
        _panelService = panelService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        simThread = new Thread(simulateElevatorCalls);
        simThread.Start();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private void simulateElevatorCalls()
    {
        var rand = new Random();
        while (true)
        {
            var trip = new TripRequest();
            trip.origin = rand.Next(Config.FLOOR_COUNT) + 1;

            do
            {
                trip.destination = rand.Next(Config.FLOOR_COUNT) + 1;
            } while (trip.origin == trip.destination);

            _logger.LogInformation($"Simulating new elevator call - origin: {trip.origin} \tdestination:{trip.destination}");
            _panelService.CallElevator(trip).Wait();

            Thread.Sleep(rand.Next(2, 30) * 1000);
        }
    }
}