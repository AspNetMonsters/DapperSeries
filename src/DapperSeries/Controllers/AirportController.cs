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
    [Route("api/airport")]
    public class AirportController : Controller
    {
        private string _connectionString;

        public AirportController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        //GET api/airport/schedule
        [HttpGet("schedule")]
        public async Task<AirportSchedule> Get(string airportCode, DateTime day)
        {
            AirportSchedule airportSchedule = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT Airport.Id, Airport.Code, Airport.City, Airport.ProvinceState, Airport.Country
FROM Airport
	WHERE Airport.Code = @AirportCode;


SELECT Airport.Id, Airport.Code, Airport.City, Airport.ProvinceState, Airport.Country
FROM Airport
	WHERE Airport.Id IN (SELECT sf.DepartureAirportId
	                                FROM Flight f
									JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
									JOIN Airport a ON sf.ArrivalAirportId = a.Id
									WHERE a.Code = @AirportCode
										  AND f.Day = @Day)
		OR Airport.Id IN (SELECT sf.ArrivalAirportId
							FROM Flight f
							JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
							JOIN Airport a ON sf.DepartureAirportId = a.Id
							WHERE a.Code = @AirportCode
									AND f.Day = @Day);

SELECT f.Id, f.ScheduledFlightId, f.Day, f.ScheduledDeparture, f.ActualDeparture, f.ScheduledArrival, f.ActualArrival
, sf.*
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.ArrivalAirportId = a.Id
WHERE f.Day = @Day
AND a.Code = @AirportCode;

SELECT f.Id, f.ScheduledFlightId, f.Day, f.ScheduledDeparture, f.ActualDeparture, f.ScheduledArrival, f.ActualArrival
, sf.*
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.DepartureAirportId = a.Id
WHERE f.Day = @Day
AND a.Code = @AirportCode;
";

                using (var multi = await connection.QueryMultipleAsync(query, new { AirportCode = airportCode, Day = day.Date }))
                {
                    airportSchedule = new AirportSchedule
                    {
                        Airport = await multi.ReadFirstAsync<Airport>(),
                        Day = day
                    };


                    var airports = multi.Read<Airport>().ToDictionary(a => a.Id);
                    airports.Add(airportSchedule.Airport.Id, airportSchedule.Airport);

                    airportSchedule.Arrivals = 
                        multi.Read<Flight, ScheduledFlight, Flight>((f, sf) =>
                                             {
                                                 f.ScheduledFlight = sf;
                                                 sf.ArrivalAirport = airports[sf.ArrivalAirportId];
                                                 sf.DepartureAirport = airports[sf.DepartureAirportId];
                                                 return f;
                                             });
                    airportSchedule.Departures = multi.Read<Flight, ScheduledFlight, Flight>((f, sf) =>
                    {
                        f.ScheduledFlight = sf;
                        sf.ArrivalAirport = airports[sf.ArrivalAirportId];
                        sf.DepartureAirport = airports[sf.DepartureAirportId];
                        return f;
                    });



                }
            }

            return airportSchedule;
        }
    }
}
