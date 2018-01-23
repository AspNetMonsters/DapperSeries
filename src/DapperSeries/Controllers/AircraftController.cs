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

namespace DapperSeries.Controllers
{
    [Route("api/aircraft")]
    public class AircraftController : Controller
    {
        private string _connectionString;

        public AircraftController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        
        //GET api/aircraft
        [HttpGet]
        public async Task<IEnumerable<Aircraft>> Get()
        {
            IEnumerable<Aircraft> aircraft;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
SELECT 
       Id
      ,Manufacturer
      ,Model
      ,RegistrationNumber
      ,FirstClassCapacity
      ,RegularClassCapacity
      ,CrewCapacity
      ,ManufactureDate
      ,NumberOfEngines
      ,EmptyWeight
      ,MaxTakeoffWeight
  FROM Aircraft";
                aircraft = await connection.QueryAsync<Aircraft>(query);
            }
            return aircraft;
        }

        // GET api/aircraft/125
        [HttpGet("{id}")]
        public async Task<Aircraft> Get(int id)
        {
            
            Aircraft aircraft;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
SELECT 
       Id
      ,Manufacturer
      ,Model
      ,RegistrationNumber
      ,FirstClassCapacity
      ,RegularClassCapacity
      ,CrewCapacity
      ,ManufactureDate
      ,NumberOfEngines
      ,EmptyWeight
      ,MaxTakeoffWeight
  FROM Aircraft WHERE Id = @Id";
                aircraft = await connection.QuerySingleAsync<Aircraft>(query, new {Id = id});
            }
            return aircraft;
        }
    }
}
