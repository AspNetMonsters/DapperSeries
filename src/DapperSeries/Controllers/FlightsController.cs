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
    [Route("api/flights")]
    public class FlightsController : Controller
    {
        private string _connectionString;

        public FlightsController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        //GET api/flights
        [HttpGet]
        public async Task<PagedResults<Flight>> Get(string airportCode, int page=1, int pageSize=10)
        {
            var results = new PagedResults<Flight>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
SELECT f.*, sf.*
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.ArrivalAirportId = a.Id
INNER JOIN Airport d ON sf.DepartureAirportId = d.Id
WHERE a.Code = @AirportCode OR d.Code = @AirportCode
ORDER BY f.Day, sf.FlightNumber
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY;

SELECT COUNT(*)
FROM Flight f
INNER JOIN ScheduledFlight sf ON f.ScheduledFlightId = sf.Id
INNER JOIN Airport a ON sf.ArrivalAirportId = a.Id
INNER JOIN Airport d ON sf.DepartureAirportId = d.Id
WHERE a.Code = @AirportCode OR d.Code = @AirportCode
";

               
                using (var multi = await connection.QueryMultipleAsync(query,
                            new { AirportCode = airportCode,
                                  Offset = (page - 1) * pageSize,
                                  PageSize = pageSize }))
                {
                    results.Items = multi.Read<Flight, ScheduledFlight, Flight>((f, sf) =>
                        {
                            f.ScheduledFlight = sf;
                            return f;
                        }).ToList();

                    results.TotalCount = multi.ReadFirst<int>();
                }
            }

            return results;
        }
    }
}
