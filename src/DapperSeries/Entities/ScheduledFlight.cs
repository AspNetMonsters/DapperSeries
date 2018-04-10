using System;

namespace DapperSeries.Entities 
{
    public class ScheduledFlight 
    {
        public int Id {get; set;}
        public string FlightNumber {get; set;}

        public int DepartureAirportId {get; set;}
        public Airport DepartureAirport {get; set;}
        public int DepartureHour {get; set;}
        public int DepartureMinute {get; set;}

        public int ArrivalAirportId {get; set;}
        public Airport ArrivalAirport {get; set;}        
        public int ArrivalHour {get; set;}
        public int ArrivalMinute {get; set;}

        public bool IsSundayFlight {get; set;}
        public bool IsMondayFlight {get; set;}
        public bool IsTuesdayFlight {get; set;}
        public bool IsWednesdayFlight {get; set;}
        public bool IsThursdayFlight {get; set;}
        public bool IsFridayFlight {get; set;}
        public bool IsSaturdayFlight {get; set;}
    }
}