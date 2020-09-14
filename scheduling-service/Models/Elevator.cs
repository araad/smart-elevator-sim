using System;
using System.Collections.Concurrent;

namespace scheduling_service
{
    public class Elevator
    {
        public int Id { get; set; }
        public int CurrentFloor { get; set; } = 1;
        public Direction CurrentDirection { get; set; } = Direction.None;

        public Trip CurrentTrip { get; set; } = null;

        public Boolean DoorsOpen { get; set; } = false;

        public ConcurrentQueue<Trip> Queue { get; } = new ConcurrentQueue<Trip>();

        public Direction GetDirection(int destination)
        {
            return destination - CurrentFloor > 0 ? Direction.Up : Direction.Down;
        }
    }
}