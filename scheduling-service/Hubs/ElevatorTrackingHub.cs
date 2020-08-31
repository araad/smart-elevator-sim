using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace scheduling_service.Hubs
{
    public sealed class ElevatorTrackingHub : Hub
    {
        private readonly ILogger<ElevatorTrackingHub> _logger;
        public ElevatorTrackingHub(ILogger<ElevatorTrackingHub> logger)
        {
            _logger = logger;
        }

        public async Task UpdateElevatorPosition(int tripId, int elevatorId)
        {
            await Clients.All.SendAsync("updateState", tripId, elevatorId);
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client Connected: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client Disonnected: {Context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}