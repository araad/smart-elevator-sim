using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using common_lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using scheduling_service.Hubs;

namespace scheduling_service.Services
{
    public sealed class SchedulingService : BackgroundService
    {
        private readonly ILogger<SchedulingService> _logger;
        private readonly IHubContext<ElevatorTrackingHub> _hub;

        private List<Elevator> _elevators = new List<Elevator>();
        private ConcurrentQueue<Trip> _queue = new ConcurrentQueue<Trip>();

        private IServiceProvider _serviceProvider;

        public SchedulingService(ILogger<SchedulingService> logger, IHubContext<ElevatorTrackingHub> hub, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            for (int i = 0; i < Config.ElevatorCount; i++)
            {
                var elevator = new Elevator();
                elevator.Id = i;
                _elevators.Add(elevator);
            }

            _hub = hub;
        }

        static int tripCount = 0;
        static object elevatorsLocker = new object();

        public int ScheduleTrip(int origin, int destination)
        {
            var trip = new Trip();
            trip.Id = tripCount++;
            trip.Origin = origin;
            trip.Destination = destination;

            var elevator = GetNextAvailableElevator();

            trip.ElevatorId = elevator.Id;
            elevator.Queue.Enqueue(trip);
            _logger.LogInformation($"SchedulingService - Scheduling trip {trip.Id} to elevator {elevator.Id}");
            _hub.Clients.All.SendAsync("newTripRequest", elevator);

            return elevator.Id;
        }

        private Elevator GetNextAvailableElevator()
        {
            List<Elevator> shallowCopy;
            lock (elevatorsLocker)
            {
                shallowCopy = new List<Elevator>(_elevators);

                shallowCopy.Sort((Elevator a, Elevator b) =>
                {
                    var a_seconds = GetTimeInSecondsUntilAvailable(a);
                    var b_seconds = GetTimeInSecondsUntilAvailable(b);

                    return a_seconds.CompareTo(b_seconds);
                });
            }

            return shallowCopy[0];
        }

        private int GetTimeInSecondsUntilAvailable(Elevator e)
        {
            int totalDuration = 0;

            if (e.CurrentTrip != null)
            {
                totalDuration += e.CurrentTrip.duration;
            }

            foreach (var trip in e.Queue)
            {
                totalDuration += trip.duration;
            }

            return totalDuration;
        }

        public List<Elevator> GetElevators()
        {
            return _elevators;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var elevator in _elevators)
            {

                var elevatorService = _serviceProvider.GetService<IElevatorService>();
                var elevatorThread = new Thread(() =>
                {
                    elevatorService.Start(elevator);
                });
                elevatorThread.Name = $"e{elevator.Id}";
                elevatorThread.Start();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10, stoppingToken);
            }
        }
    }
}