using System;
using System.Collections.Generic;

namespace DapperSeries.Entities
{
    public class ScheduledFlight
    {
        public int Id { get; set; }
        public string FlightNumber { get; set; }

        public int DepartureAirportId { get; set; }
        public Airport DepartureAirport { get; set; }
        public int DepartureHour { get; set; }
        public int DepartureMinute { get; set; }

        public int ArrivalAirportId { get; set; }
        public Airport ArrivalAirport { get; set; }
        public int ArrivalHour { get; set; }
        public int ArrivalMinute { get; set; }

        public bool IsSundayFlight { get; set; }
        public bool IsMondayFlight { get; set; }
        public bool IsTuesdayFlight { get; set; }
        public bool IsWednesdayFlight { get; set; }
        public bool IsThursdayFlight { get; set; }
        public bool IsFridayFlight { get; set; }
        public bool IsSaturdayFlight { get; set; }


        public IEnumerable<Flight> GenerateFlights(DateTime startDate, DateTime endDate)
        {
            var flights = new List<Flight>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (IsOnDayOfWeek(currentDate.DayOfWeek))
                {
                    var departureTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, DepartureHour, DepartureMinute, 0);
                    var arrivalTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, ArrivalHour, ArrivalMinute, 0);
                    var flight = new Flight
                    {
                        ScheduledFlightId = Id,
                        ScheduledDeparture = departureTime,
                        ScheduledArrival = arrivalTime,
                        Day = currentDate.Date
                    };
                    flights.Add(flight);
                }
                currentDate = currentDate.AddDays(1);
            }
            return flights;
        }
        public bool IsOnDayOfWeek(DayOfWeek dayOfWeek)
        {
            return     (dayOfWeek == DayOfWeek.Sunday && IsSundayFlight)
                    || (dayOfWeek == DayOfWeek.Monday && IsMondayFlight)
                    || (dayOfWeek == DayOfWeek.Tuesday && IsTuesdayFlight)
                    || (dayOfWeek == DayOfWeek.Wednesday && IsWednesdayFlight)
                    || (dayOfWeek == DayOfWeek.Thursday && IsThursdayFlight)
                    || (dayOfWeek == DayOfWeek.Friday && IsFridayFlight)
                    || (dayOfWeek == DayOfWeek.Saturday && IsSaturdayFlight);
        }
    }


}