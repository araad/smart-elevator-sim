using System;
using common_lib;

namespace scheduling_service
{
    public class Trip
    {
        public int Id { get; set; }
        public int Origin { get; set; }
        public int Destination { get; set; }
        public int ElevatorId { get; set; }
        public TripStatus Status { get; set; } = TripStatus.Scheduled;
        public int distance => Math.Abs(Destination - Origin) * Config.FloorHeight;
        public int duration => distance / Config.ElevatorSpeed;
    }
}