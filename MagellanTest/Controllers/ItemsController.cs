using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
namespace MagellanTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemsController : ControllerBase
    {

        //would typically use ioptions pattern for this but this is fine
        private readonly string connectionString;
        public ItemsController(IConfiguration config)
        {
            connectionString = config.GetConnectionString("DefaultConnection");
        }


        //test connection, personal use, delete later.
        [HttpGet("testconnection")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Connection succeeded, return OK
                    return Ok("Database connection successful.");
                }
            }
            catch (Exception ex)
            {
                //tell me that it failed
                return StatusCode(500, $"Database connection failed: {ex.Message}");
            }
        }



    }
}

