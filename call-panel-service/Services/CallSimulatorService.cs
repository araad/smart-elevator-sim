using call_panel_service.Configuration;
using common_lib;
using common_lib.Configuration;
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
        private readonly BuildingConfiguration _buildingConfiguration;
        private readonly CallPanelConfiguration _callPanelConfiguration;

        public CallSimulatorService(ILogger<CallSimulatorService> logger,
                                    ICallPanelService callPanelService,
                                    BuildingConfiguration buildingConfiguration,
                                    CallPanelConfiguration callPanelConfiguration)
        {
            _logger = logger;
            _callPanelService = callPanelService;
            _buildingConfiguration = buildingConfiguration;
            _callPanelConfiguration = callPanelConfiguration;
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
            var retries = 0;
            var minDelay = _callPanelConfiguration.MinDelayBetweenSimulatedCalls;
            var maxDelay = _callPanelConfiguration.MaxDelayBetweenSimulatedCalls;

            while (true)
            {
                var tripRequest = new TripRequest();
                tripRequest.origin = rand.Next(_buildingConfiguration.FloorCount) + 1;

                do
                {
                    tripRequest.destination = rand.Next(_buildingConfiguration.FloorCount) + 1;
                } while (tripRequest.origin == tripRequest.destination);

                _logger.LogInformation($"Simulating new elevator call - origin: {tripRequest.origin} \tdestination:{tripRequest.destination}");
                try
                {
                    _callPanelService.CallElevator(tripRequest).Wait();
                    Thread.Sleep(rand.Next(minDelay, maxDelay) * 1000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());

                    if (retries++ > 1)
                    {
                        _logger.LogInformation("Too many timeouts, stopping");
                        break;
                    }

                    _logger.LogInformation("Retrying in 60 seconds...");
                    Thread.Sleep(6000);
                }
            }
        }
    }
}