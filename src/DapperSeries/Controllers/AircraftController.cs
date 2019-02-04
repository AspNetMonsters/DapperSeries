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
        public async Task<IEnumerable<Aircraft>> Get(string model)
        {
            IEnumerable<Aircraft> aircraft;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                if (string.IsNullOrWhiteSpace(model))
                {
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
                else
                {
                    aircraft = await connection.QueryAsync<Aircraft>("GetAircraftByModel",
                                            new {Model = model}, 
                                            commandType: CommandType.StoredProcedure);
                }

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

        // POST api/aircraft
        [HttpPost()]
        public async Task<IActionResult> Post([FromBody] Aircraft model)
        {
            int newAircraftId;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
INSERT INTO Aircraft 
       (Manufacturer
      ,Model
      ,RegistrationNumber
      ,FirstClassCapacity
      ,RegularClassCapacity
      ,CrewCapacity
      ,ManufactureDate
      ,NumberOfEngines
      ,EmptyWeight
      ,MaxTakeoffWeight)
VALUES (@Manufacturer
      ,@Model
      ,@RegistrationNumber
      ,@FirstClassCapacity
      ,@RegularClassCapacity
      ,@CrewCapacity
      ,@ManufactureDate
      ,@NumberOfEngines
      ,@EmptyWeight
      ,@MaxTakeoffWeight);
      
SELECT CAST(SCOPE_IDENTITY() as int)";
                newAircraftId = await connection.ExecuteScalarAsync<int>(query, model);
            }
            return Ok(newAircraftId);
        }

        // PUT api/aircraft/id
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Aircraft model)
        {
            if (id != model.Id) 
            {
                return BadRequest();
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
UPDATE Aircraft 
  SET  Manufacturer = @Manufacturer
      ,Model = @Model
      ,RegistrationNumber = @RegistrationNumber 
      ,FirstClassCapacity = @FirstClassCapacity
      ,RegularClassCapacity = @RegularClassCapacity
      ,CrewCapacity = @CrewCapacity
      ,ManufactureDate = @ManufactureDate
      ,NumberOfEngines = @NumberOfEngines
      ,EmptyWeight = @EmptyWeight
      ,MaxTakeoffWeight = @MaxTakeoffWeight
WHERE Id = @Id";
                await connection.ExecuteAsync(query, model);
            }
            return Ok();
        }

        // DELETE api/aircraft/id
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
        
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE Aircraft WHERE Id = @Id";
                await connection.ExecuteAsync(query, new {Id = id});
            }
            return Ok();
        }
    }
}
