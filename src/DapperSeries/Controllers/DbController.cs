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
            using(var connection = new SqlConnection(builder.ConnectionString))
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
                createTablesCommand.CommandText = System.IO.File.ReadAllText("./Scripts/CreateTables.sql");
                await createTablesCommand.ExecuteNonQueryAsync();
            }
            return Ok();
        }
    }
}
