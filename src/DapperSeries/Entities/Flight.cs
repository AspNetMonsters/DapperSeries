using System;

namespace DapperSeries.Entities 
{
    public class Flight 
    {
        public int Id {get; set;}
        public int ScheduledFlightId {get; set;}
        public ScheduledFlight ScheduledFlight { get; set;}
        public DateTime Day {get; set;}
        public DateTime ScheduledDeparture {get; set;}
        public DateTime? ActualDeparture {get; set;}
        public DateTime ScheduledArrival {get; set;}
        public DateTime? ActualArrival {get; set;}   
    }
}