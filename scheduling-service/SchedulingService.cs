using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using common_lib;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using scheduling_service.Hubs;

namespace scheduling_service
{
    public class SchedulingService : BackgroundService
    {
        private readonly ILogger<SchedulingService> _logger;
        private readonly IHubContext<ElevatorTrackingHub> _hub;

        private List<Elevator> elevators = new List<Elevator>();
        private ConcurrentQueue<Trip> queue = new ConcurrentQueue<Trip>();

        public SchedulingService(ILogger<SchedulingService> logger, IHubContext<ElevatorTrackingHub> hub)
        {
            _logger = logger;

            for (int i = 0; i < Config.ELEVATOR_COUNT; i++)
            {
                var elevator = new Elevator(hub);
                elevator.Id = i;
                elevators.Add(elevator);
            }

            _hub = hub;
        }

        static int tripCount = 0;
        static object elevatorsLocker = new object();

        public int ScheduleTrip(int origin, int destination)
        {
            var trip = new Trip();
            trip.Id = tripCount++;
            trip.origin = origin;
            trip.destination = destination;

            var elevator = getNextAvailableElevator();

            trip.ElevatorId = elevator.Id;
            elevator.queue.Enqueue(trip);
            _logger.LogInformation($"SchedulingService - Scheduling trip {trip.Id} to elevator {elevator.Id}");
            _hub.Clients.All.SendAsync("newTripRequest", elevator, trip);

            return elevator.Id;
        }

        private Elevator getNextAvailableElevator()
        {
            List<Elevator> shallowCopy;
            lock (elevatorsLocker)
            {
                shallowCopy = new List<Elevator>(elevators);

                shallowCopy.Sort((Elevator a, Elevator b) =>
                {
                    var asec = getSecondsUntilFree(a);
                    var bsec = getSecondsUntilFree(b);

                    return asec.CompareTo(bsec);
                });
            }

            return shallowCopy[0];
        }

        private int getSecondsUntilFree(Elevator e)
        {
            int totalDuration = 0;

            if (e.currentTrip != null)
            {
                totalDuration += e.currentTrip.tripDuration;
            }

            foreach (var trip in e.queue)
            {
                totalDuration += trip.tripDuration;
            }

            return totalDuration;
        }

        public List<Elevator> getElevators()
        {
            return elevators;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var elevator in elevators)
            {
                var elevatorThread = new Thread(elevator.Run);
                elevatorThread.Name = $"e{elevator.Id}";
                elevatorThread.Start();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10, stoppingToken);
            }
        }
    }

    public enum TripStatus
    {
        Scheduled,
        InProgress,
        Complete
    }

    public enum Direction
    {
        Down,
        Up
    }

    public class Trip
    {
        public int Id { get; set; }
        public int origin { get; set; }
        public int destination { get; set; }

        public int ElevatorId { get; set; }

        public TripStatus status { get; set; } = TripStatus.Scheduled;
        public int distance => Math.Abs(destination - origin) * Config.FLOOR_HEIGHT;
        public int tripDuration => distance / Config.SPEED;
        public Direction direction => destination - origin > 0 ? Direction.Up : Direction.Down;
    }

    public class Elevator
    {
        public int Id { get; set; }
        public int currentFloor { get; set; } = 1;
        public int currentSpeed { get; set; } = 0;
        public Direction currentDirection { get; set; }

        public Trip currentTrip { get; set; }

        public Boolean doorsOpen { get; set; } = false;

        public ConcurrentQueue<Trip> queue { get; set; } = new ConcurrentQueue<Trip>();

        private IHubContext<ElevatorTrackingHub> _hub;

        public Elevator(IHubContext<ElevatorTrackingHub> hub)
        {
            _hub = hub;
        }

        private int getSecondsToFloor(int floor)
        {
            return (Math.Abs(floor - currentFloor) * Config.FLOOR_HEIGHT) / Config.SPEED;
        }

        private Direction GetDirection(int destination)
        {
            return destination - currentFloor > 0 ? Direction.Up : Direction.Down;
        }

        public void Run()
        {
            Console.WriteLine($"Starting elevator {Id}");

            while (true)
            {
                Trip trip;
                while (queue.TryDequeue(out trip))
                {
                    currentTrip = trip;
                    currentTrip.status = TripStatus.InProgress;
                    Console.WriteLine($"Elevator-{Id} - Executing trip {currentTrip.Id}");
                    _hub.Clients.All.SendAsync("tripStarted", this, currentTrip);

                    if (currentTrip.origin == currentFloor)
                    {
                        Console.WriteLine($"Elevator-{Id} - Already at origin ({currentTrip.origin})");
                    }
                    else
                    {
                        var toOrigin = getSecondsToFloor(currentTrip.origin);
                        currentDirection = GetDirection(currentTrip.origin);
                        Console.WriteLine($"Elevator-{Id} - Going to origin ({currentFloor} --> {currentTrip.origin})");
                        _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);

                        var floors1 = toOrigin * Config.SPEED / Config.FLOOR_HEIGHT;
                        Console.WriteLine($"Elevator-{Id} - Floors to cross {floors1}");
                        for (int i = 0; i < floors1; i++)
                        {
                            Thread.Sleep(Config.FLOOR_HEIGHT / Config.SPEED * 1000);
                            if (currentDirection == Direction.Down)
                            {
                                currentFloor--;
                            }
                            else
                            {
                                currentFloor++;
                            }
                            _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);
                        }

                        currentFloor = currentTrip.origin;
                        Console.WriteLine($"Elevator-{Id} - Arrived at origin ({currentTrip.origin})");
                    }


                    doorsOpen = true;
                    Console.WriteLine($"Elevator-{Id} - Doors open");
                    _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);

                    Thread.Sleep(Config.DOORS_OPEN_DURATION * 1000);

                    doorsOpen = false;
                    var toDestination = getSecondsToFloor(currentTrip.destination);
                    currentDirection = GetDirection(currentTrip.destination);
                    Console.WriteLine($"Elevator-{Id} - Doors closed");
                    Console.WriteLine($"Elevator-{Id} - Going to destination ({currentFloor} --> {currentTrip.destination})");
                    _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);

                    var floors = currentTrip.distance / Config.FLOOR_HEIGHT;
                    Console.WriteLine($"Elevator-{Id} - Floors to cross {floors}");
                    for (int i = 0; i < floors; i++)
                    {
                        Thread.Sleep(Config.FLOOR_HEIGHT / Config.SPEED * 1000);
                        if (currentDirection == Direction.Down)
                        {
                            currentFloor--;
                        }
                        else
                        {
                            currentFloor++;
                        }
                        _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);
                    }

                    currentFloor = currentTrip.destination;
                    Console.WriteLine($"Elevator-{Id} - Arrived at destination ({currentTrip.destination})");

                    doorsOpen = true;
                    Console.WriteLine($"Elevator-{Id} - Doors open");
                    _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);

                    Thread.Sleep(Config.DOORS_OPEN_DURATION * 1000);

                    doorsOpen = false;
                    Console.WriteLine($"Elevator-{Id} - Doors closed");
                    _hub.Clients.All.SendAsync("tripUpdate", this, currentTrip);

                    Thread.Sleep(100);

                    currentTrip.status = TripStatus.Complete;
                    _hub.Clients.All.SendAsync("tripEnded", this, currentTrip);
                    currentTrip = null;
                }

                Thread.Sleep(10);
            }
        }
    }
}
