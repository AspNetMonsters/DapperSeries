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
            else
            {
                _connectionString = "Server=(localdb)\\mssqllocaldb;Database=AirPaquette;ConnectRetryCount=0;Trusted_Connection=True;MultipleActiveResultSets=true";
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
                            (flight, departure, arrival) =>
                            {
                                flight.DepartureAirport = departure;
                                flight.ArrivalAirport = arrival;
                                return flight;
                            },
                            new { FromCode = from });
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
DECLARE @DepartureAirportId INT
SELECT @DepartureAirportId = Id FROM Airport WHERE Code = @FromCode

SELECT s.Id, s.FlightNumber, s.DepartureHour, s.DepartureMinute, s.ArrivalHour, s.ArrivalMinute, s.IsSundayFlight, s.IsMondayFlight, s.IsTuesdayFlight, s.IsWednesdayFlight, s.IsThursdayFlight, s.IsFridayFlight, s.IsSaturdayFlight,
s.DepartureAirportId, s.ArrivalAirportId
FROM ScheduledFlight s
    WHERE s.DepartureAirportId = @DepartureAirportId

SELECT Airport.Id, Airport.Code, Airport.City, Airport.ProvinceState, Airport.Country
FROM Airport
	WHERE Airport.Id = @DepartureAirportId
	   OR Airport.Id IN (SELECT s.ArrivalAirportId
	                                FROM ScheduledFlight s
									WHERE s.DepartureAirportId = @DepartureAirportId)";

                using (var multi = await connection.QueryMultipleAsync(query, new { FromCode = from }))
                {
                    scheduledFlights = multi.Read<ScheduledFlight>();
                    var airports = multi.Read<Airport>().ToDictionary(a => a.Id);
                    foreach (var flight in scheduledFlights)
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


        // POST api/scheduledflight
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] ScheduledFlight model)
        {
            int? newScheduledFlightId = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var transaction = connection.BeginTransaction();

                try
                {
                    var insertScheduledFlightSql = @"
INSERT INTO [dbo].[ScheduledFlight]
           ([FlightNumber]
           ,[DepartureAirportId]
           ,[DepartureHour]
           ,[DepartureMinute]
           ,[ArrivalAirportId]
           ,[ArrivalHour]
           ,[ArrivalMinute]
           ,[IsSundayFlight]
           ,[IsMondayFlight]
           ,[IsTuesdayFlight]
           ,[IsWednesdayFlight]
           ,[IsThursdayFlight]
           ,[IsFridayFlight]
           ,[IsSaturdayFlight])
     VALUES
           (@FlightNumber
           ,@DepartureAirportId
           ,@DepartureHour
           ,@DepartureMinute
           ,@ArrivalAirportId
           ,@ArrivalHour
           ,@ArrivalMinute
           ,@IsSundayFlight
           ,@IsMondayFlight
           ,@IsTuesdayFlight
           ,@IsWednesdayFlight
           ,@IsThursdayFlight
           ,@IsFridayFlight
           ,@IsSaturdayFlight);
SELECT CAST(SCOPE_IDENTITY() as int)";
                    newScheduledFlightId = await connection.ExecuteScalarAsync<int>(insertScheduledFlightSql, model, transaction);

                    model.Id = newScheduledFlightId.Value;
                    var flights = model.GenerateFlights(DateTime.Now, DateTime.Now.AddMonths(12));

                    var insertFlightsSql = @"INSERT INTO [dbo].[Flight]
           ([ScheduledFlightId]
           ,[Day]
           ,[ScheduledDeparture]
           ,[ActualDeparture]
           ,[ScheduledArrival]
           ,[ActualArrival])
     VALUES
           (@ScheduledFlightId
           ,@Day
           ,@ScheduledDeparture
           ,@ActualDeparture
           ,@ScheduledArrival
           ,@ActualArrival)";

                    await connection.ExecuteAsync(insertFlightsSql, flights, transaction);
                    throw new Exception("OH NOES!");
                    transaction.Commit();
                }
                catch (Exception ex)
                { 
                    //Log the exception (ex)
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // Handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        // Log the exception ex2
                    }
                    return StatusCode(500);
                }
            }
            return Ok(newScheduledFlightId);
        }


    }
}
