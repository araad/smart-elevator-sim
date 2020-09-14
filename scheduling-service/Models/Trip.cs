namespace scheduling_service
{
    public class Trip
    {
        public int Id { get; set; }
        public int Origin { get; set; }
        public int Destination { get; set; }
        public int ElevatorId { get; set; }
        public TripStatus Status { get; set; } = TripStatus.Scheduled;
    }
}