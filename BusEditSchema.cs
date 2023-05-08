namespace mBus
{
    public class BusEditSchema
    {
        public string BusNumber { get; set; }
        public string Name { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public int Price { get; set; }
        public string BusType { get; set; }

        public string ReferenceId { get; set; }
    }
}


