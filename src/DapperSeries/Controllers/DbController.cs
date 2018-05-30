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
    [Route("api/db")]
    public class DbController : Controller
    {
        private string _connectionString;

        public DbController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        [HttpPost("init")]
        public async Task<IActionResult> Init()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            var databaseToCreate = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = string.Format(@"IF EXISTS(select * from sys.databases where name='{0}')
                        DROP DATABASE [{0}]
                        CREATE DATABASE [{0}]", databaseToCreate);
                await command.ExecuteNonQueryAsync();


            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var createTablesCommand = connection.CreateCommand();
                createTablesCommand.CommandTimeout = 600000;
                createTablesCommand.CommandText = System.IO.File.ReadAllText("./Scripts/CreateTables.sql");
                await createTablesCommand.ExecuteNonQueryAsync();

                var createProcsCommand = connection.CreateCommand();
                createProcsCommand.CommandText = System.IO.File.ReadAllText("./Scripts/CreateStoredProcedures.sql");
                await createProcsCommand.ExecuteNonQueryAsync();

                //Create a set of future flights based on the scheduled flights
                var scheduledFlights = await connection.QueryAsync<ScheduledFlight>("SELECT * FROM ScheduledFlight");
                foreach (var scheduledFlight in scheduledFlights)
                {
                    var flights = new List<Flight>();
                    for (int i = 1; i < 60; i++)
                    {
                        var currentDate = DateTime.Today.AddDays(i);
                        if (IsFlightOnDay(currentDate, scheduledFlight))
                        {
                            var flight = new Flight()
                            {
                                ScheduledFlightId = scheduledFlight.Id,
                                Day = currentDate,
                                ScheduledDeparture = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, scheduledFlight.DepartureHour, scheduledFlight.DepartureMinute, 0),
                                ScheduledArrival = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, scheduledFlight.ArrivalHour, scheduledFlight.ArrivalMinute, 0)
                            };
                            flights.Add(flight);
                        }
                    }
                    await connection.ExecuteAsync("INSERT INTO Flight(ScheduledFlightId, Day, ScheduledDeparture, ScheduledArrival) VALUES(@ScheduledFlightId, @Day, @ScheduledDeparture, @ScheduledArrival)", flights);
                }
            }
            return Ok();
        }

        private bool IsFlightOnDay(DateTime currentDate, ScheduledFlight scheduledFlight)
        {
            return (currentDate.DayOfWeek == DayOfWeek.Sunday && scheduledFlight.IsSundayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Monday && scheduledFlight.IsMondayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Tuesday && scheduledFlight.IsTuesdayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Wednesday && scheduledFlight.IsWednesdayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Thursday && scheduledFlight.IsThursdayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Friday && scheduledFlight.IsFridayFlight)
                || (currentDate.DayOfWeek == DayOfWeek.Saturday && scheduledFlight.IsSaturdayFlight);
        }
    }
}
