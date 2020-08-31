using System.Threading;
using common_lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using scheduling_service.Hubs;

namespace scheduling_service.Services
{
    public class ElevatorService : IElevatorService
    {
        private readonly ILogger<ElevatorService> _logger;
        private readonly IHubContext<ElevatorTrackingHub> _hub;

        public ElevatorService(ILogger<ElevatorService> logger, IHubContext<ElevatorTrackingHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        public void Start(Elevator elevator)
        {
            _logger.LogInformation($"Starting elevator {elevator.Id}");

            while (true)
            {
                Trip trip;
                while (elevator.queue.TryDequeue(out trip))
                {
                    elevator.CurrentTrip = trip;
                    ExecuteCurrentTrip(elevator);
                }

                Thread.Sleep(10);
            }
        }

        private void ExecuteCurrentTrip(Elevator elevator)
        {
            elevator.CurrentTrip.Status = TripStatus.InProgress;
            _logger.LogInformation($"Elevator-{elevator.Id} - Executing trip {elevator.CurrentTrip.Id}");
            _hub.Clients.All.SendAsync("tripStarted", elevator);

            if (elevator.CurrentTrip.Origin == elevator.CurrentFloor)
            {
                _logger.LogInformation($"Elevator-{elevator.Id} - Already at origin ({elevator.CurrentTrip.Origin})");
            }
            else
            {
                _logger.LogInformation($"Elevator-{elevator.Id} - Going to origin ({elevator.CurrentTrip.Origin})");
                GoToFloor(elevator, elevator.CurrentTrip.Origin);
            }

            OperateDoors(elevator);

            _logger.LogInformation($"Elevator-{elevator.Id} - Going to destination ({elevator.CurrentTrip.Destination})");
            GoToFloor(elevator, elevator.CurrentTrip.Destination);

            OperateDoors(elevator);

            elevator.CurrentTrip.Status = TripStatus.Complete;
            elevator.CurrentTrip = null;
            _hub.Clients.All.SendAsync("tripEnded", elevator);
        }

        private void GoToFloor(Elevator elevator, int floor)
        {
            var seconds = elevator.GetTimeInSecondsToFloor(floor);
            elevator.CurrentDirection = elevator.GetDirection(floor);
            _logger.LogInformation($"Elevator-{elevator.Id} - Travelling: ({elevator.CurrentFloor} --> {floor})");
            _hub.Clients.All.SendAsync("tripUpdate", elevator);

            TravelDistance(elevator, seconds * Config.ElevatorSpeed);

            elevator.CurrentFloor = floor;
            elevator.CurrentDirection = Direction.None;
            _logger.LogInformation($"Elevator-{elevator.Id} - Arrived at ({floor})");
            _hub.Clients.All.SendAsync("tripUpdate", elevator);
        }

        private void OperateDoors(Elevator elevator)
        {
            elevator.DoorsOpen = true;
            _logger.LogInformation($"Elevator-{elevator.Id} - Doors open");
            _hub.Clients.All.SendAsync("tripUpdate", elevator);

            Thread.Sleep(Config.DoorsOpenDuration * 1000);

            elevator.DoorsOpen = false;
            _logger.LogInformation($"Elevator-{elevator.Id} - Doors closed");
            _hub.Clients.All.SendAsync("tripUpdate", elevator);

            Thread.Sleep(100);
        }

        private void TravelDistance(Elevator elevator, int distance)
        {
            var floors = distance / Config.FloorHeight;
            _logger.LogInformation($"Elevator-{elevator.Id} - Floors to cross {floors}");
            for (int i = 0; i < floors; i++)
            {
                Thread.Sleep(Config.FloorHeight / Config.ElevatorSpeed * 1000);
                if (elevator.CurrentDirection == Direction.Down)
                {
                    elevator.CurrentFloor--;
                }
                else
                {
                    elevator.CurrentFloor++;
                }
                _hub.Clients.All.SendAsync("tripUpdate", elevator);
            }

        }
    }
}