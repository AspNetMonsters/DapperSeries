using System;
using System.Collections.Generic;

namespace DapperSeries.Entities 
{
    public class AirportSchedule 
    {
        public Airport Airport { get; set; }
        public DateTime Day { get; set; }
        public IList<Flight> Departures { get; set; }
        public IList<Flight> Arrivals { get; set; }
    }
}
