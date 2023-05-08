namespace mBus
{
    public class BookingSchema
    {
        //Source,Destination,Date,Price,Id,BusId
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Date { get; set; }
        public int Price { get; set; }
        public string Id { get; set; }

        public string BusId { get; set; }

    }
}
