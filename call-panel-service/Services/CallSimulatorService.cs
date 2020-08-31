using common_lib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace call_panel_service.Services
{
    public sealed class CallSimulatorService : BackgroundService
    {
        Thread simThread = null;

        private readonly ILogger<CallSimulatorService> _logger;
        private readonly ICallPanelService _callPanelService;

        public CallSimulatorService(ILogger<CallSimulatorService> logger, ICallPanelService callPanelService)
        {
            _logger = logger;
            _callPanelService = callPanelService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            simThread = new Thread(SimulateElevatorCalls);
            simThread.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            simThread.Abort();
            await base.StopAsync(cancellationToken);
        }

        private void SimulateElevatorCalls()
        {
            var rand = new Random();
            while (true)
            {
                var tripRequest = new TripRequest();
                tripRequest.origin = rand.Next(Config.FloorCount) + 1;

                do
                {
                    tripRequest.destination = rand.Next(Config.FloorCount) + 1;
                } while (tripRequest.origin == tripRequest.destination);

                _logger.LogInformation($"Simulating new elevator call - origin: {tripRequest.origin} \tdestination:{tripRequest.destination}");
                _callPanelService.CallElevator(tripRequest).Wait();

                Thread.Sleep(rand.Next(2, 20) * 1000);
            }
        }
    }
}