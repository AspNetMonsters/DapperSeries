using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IO;
using System.Data;
using Dapper;
using DapperSeries.Entities;

namespace DapperSeries.Controllers
{
    [Route("api/scheduledflight")]
    public class ScheduledFlightController : Controller
    {
        private string _connectionString;

        public ScheduledFlightController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        //GET api/scheduledflight?from=yyc
        [HttpGet]
        public async Task<IEnumerable<ScheduledFlight>> Get(string from)
        {
            IEnumerable<ScheduledFlight> scheduledFlights;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                    var query = @"
SELECT s.Id, s.FlightNumber, s.DepartureHour, s.DepartureMinute, s.ArrivalHour, s.ArrivalMinute, s.IsSundayFlight, s.IsMondayFlight, s.IsTuesdayFlight, s.IsWednesdayFlight, s.IsThursdayFlight, s.IsFridayFlight, s.IsSaturdayFlight,
       a1.Id, a1.Code, a1.City, a1.ProvinceState, a1.Country,
	   a2.Id, a2.Code, a2.City, a2.ProvinceState, a2.Country
FROM ScheduledFlight s
	INNER JOIN Airport a1
		ON s.DepartureAirportId = a1.Id
	INNER JOIN Airport a2
		ON s.ArrivalAirportId = a2.Id
    WHERE a1.Code = @FromCode";

                scheduledFlights = 
                    await connection.QueryAsync<ScheduledFlight, Airport, Airport, ScheduledFlight>(query,
                            (flight, departure, arrival ) => {
                                flight.DepartureAirport = departure;
                                flight.ArrivalAirport = arrival;
                                return flight;
                            },
                            new{FromCode = from} );
            }
            return scheduledFlights;
        }

    }
}
