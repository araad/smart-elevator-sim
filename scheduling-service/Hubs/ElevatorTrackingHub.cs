using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace scheduling_service.Hubs
{
    public sealed class ElevatorTrackingHub : Hub
    {
        public async Task UpdateElevatorPosition(int tripId, int elevatorId)
        {
            await Clients.All.SendAsync("updateState", tripId, elevatorId);
        }

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"Client Connected: {Context.ConnectionId}");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"Client Disonnected: {Context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}