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
        
        //GET api/schedule
        [HttpGet]
        public async Task<AirportSchedule> Get(string airportCode, DateTime day)
        {
            AirportSchedule airportSchedule = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"SELECT Airport.Id, Airport.Code, Airport.City, Airport.ProvinceState, Airport.Country
FROM Airport
	WHERE Airport.Code = @AirportCode

SELECT f.Id, f.ScheduledFlightId, f.Day, f.ScheduledDeparture, f.ActualDeparture, f.ScheduledArrival, f.ActualArrival,
sf.*
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.ArrivalAirportId = a.Id
WHERE f.Day = @Day
AND a.Code = @AirportCode

SELECT f.Id, f.ScheduledFlightId, f.Day, f.ScheduledDeparture, f.ActualDeparture, f.ScheduledArrival, f.ActualArrival,
sf.*
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.DepartureAirportId = a.Id
WHERE f.Day = @Day
AND a.Code = @AirportCode";

                using (var multi = await connection.QueryMultipleAsync(query, new { AirportCode = airportCode, Day = day.Date }))
                {
                    airportSchedule = new AirportSchedule
                    {
                        Airport = await multi.ReadFirstAsync<Airport>(),
                        Day = day
                    };

                    airportSchedule.Arrivals = 
                        multi.Read<Flight, ScheduledFlight, Flight>((f, sf) =>
                        {
                            f.ScheduledFlight = sf;
                            return f;
                        }).ToList();
                    airportSchedule.Departures =
                        multi.Read<Flight, ScheduledFlight, Flight>((f, sf) =>
                        {
                            f.ScheduledFlight = sf;
                            return f;
                        }).ToList();


                }
            }

            return airportSchedule;
        }
    }
}
