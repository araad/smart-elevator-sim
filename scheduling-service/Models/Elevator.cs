using System;
using System.Collections.Concurrent;
using common_lib;

namespace scheduling_service
{
    public class Elevator
    {
        public int Id { get; set; }
        public int CurrentFloor { get; set; } = 1;
        public Direction CurrentDirection { get; set; } = Direction.None;

        public Trip CurrentTrip { get; set; } = null;

        public Boolean DoorsOpen { get; set; } = false;

        public ConcurrentQueue<Trip> queue = new ConcurrentQueue<Trip>();

        public int GetTimeInSecondsToFloor(int floor)
        {
            return (Math.Abs(floor - CurrentFloor) * Config.FloorHeight) / Config.ElevatorSpeed;
        }

        public Direction GetDirection(int destination)
        {
            return destination - CurrentFloor > 0 ? Direction.Up : Direction.Down;
        }
    }
}