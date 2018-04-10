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
using BenchmarkDotNet.Running;

namespace DapperSeries.Controllers
{
    [Route("api/scheduledflight")]
    public class ScheduledFlightController : Controller
    {
        private string _connectionString;

        public ScheduledFlightController(IConfiguration configuration)
        {
            if (configuration != null)
            {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            }
             else {
                 _connectionString =  "Server=(localdb)\\mssqllocaldb;Database=AirPaquette;ConnectRetryCount=0;Trusted_Connection=True;MultipleActiveResultSets=true";
             }
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

        //GET api/scheduledflight/alt?from=yyc
        [HttpGet("alt")]
        public async Task<IEnumerable<ScheduledFlight>> GetAlt(string from)
        {
            IEnumerable<ScheduledFlight> scheduledFlights;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
SELECT s.Id, s.FlightNumber, s.DepartureHour, s.DepartureMinute, s.ArrivalHour, s.ArrivalMinute, s.IsSundayFlight, s.IsMondayFlight, s.IsTuesdayFlight, s.IsWednesdayFlight, s.IsThursdayFlight, s.IsFridayFlight, s.IsSaturdayFlight,
s.DepartureAirportId, s.ArrivalAirportId
FROM ScheduledFlight s
	INNER JOIN Airport a1
		ON s.DepartureAirportId = a1.Id
    WHERE a1.Code = @FromCode
    
SELECT a1.Id, a1.Code, a1.City, a1.ProvinceState, a1.Country
FROM Airport a1
	WHERE a1.Code = @FromCode
UNION    
SELECT DISTINCT a2.Id, a2.Code, a2.City, a2.ProvinceState, a2.Country
FROM ScheduledFlight s
	INNER JOIN Airport a1
		ON s.DepartureAirportId = a1.Id
    INNER JOIN Airport a2
		ON s.ArrivalAirportId = a2.Id
    WHERE a1.Code = @FromCode";

                using (var multi = await connection.QueryMultipleAsync(query, new{FromCode = from} ))
                {
                    scheduledFlights = multi.Read<ScheduledFlight>();
                    var airports = multi.Read<Airport>().ToDictionary(a => a.Id);
                    foreach(var flight in scheduledFlights)
                    {
                        flight.ArrivalAirport = airports[flight.ArrivalAirportId];
                        flight.DepartureAirport = airports[flight.DepartureAirportId];
                    }

                }
            }
            return scheduledFlights;
        }

        [HttpGet("benchmark")]
        public string Benchmark() 
        {
            var summary = BenchmarkRunner.Run<Benchmarks.OneToManyBenchmarks>();
            return summary.ToString();
        }

    }
}
