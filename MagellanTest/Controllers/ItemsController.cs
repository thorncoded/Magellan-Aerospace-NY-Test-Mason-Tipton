using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json.Serialization;
using static MagellanTest.Controllers.ItemsController;
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


        //test connection, personal use, was in usage but comment out for final submission
       /* [HttpGet("testconnection")]
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
        } */


        // Model classes to represent the data received in the JSON payload
        public class ItemData
        {
            public string ItemName { get; set; }
            public int? ParentItem { get; set; }
            public int Cost { get; set; }
            public DateTime ReqDate { get; set; }
        }

        public class ReadItemData
        {
            public int Id { get; set; }
            public string ItemName { get; set; }
            public int? ParentItem { get; set; }
            public int Cost { get; set; }
            public DateTime ReqDate { get; set; }
        }


        //Create a new record in the item table. User will supply the values for item_name, parent_item, cost, and req_date in json. The endpoint will return the id of the newly created record.
        [HttpPost("create")]
        public IActionResult CreateItem([FromBody] ItemData itemData)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Create a new record in the item table
                    using (var cmd = new NpgsqlCommand(
                        "INSERT INTO public.item (item_name, parent_item, cost, req_date) VALUES (@itemName, @parentItem, @cost, @reqDate) RETURNING id",
                        connection))
                    {
                        cmd.Parameters.AddWithValue("itemName", itemData.ItemName);
                        cmd.Parameters.AddWithValue("parentItem", (object)itemData.ParentItem ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("cost", itemData.Cost);
                        cmd.Parameters.AddWithValue("reqDate", itemData.ReqDate);

                        var newItemId = cmd.ExecuteScalar();

                        // Return the ID of the newly created record
                        return Ok(newItemId);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to create item: {ex.Message}");
            }
        }



        //Query the item table by supplying the id of the record. The endpoint will return the id, item_name, parent_item, cost, and req_date in json if record exists.
        [HttpGet("get")]
        public IActionResult GetById(int id)
        {
            try
            {

                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (var cmd = new NpgsqlCommand(
                                            "SELECT id, item_name, parent_item, cost, req_date FROM public.item WHERE id = @id",
                                            connection))
                    {
                        cmd.Parameters.AddWithValue("id", id);
                        //use reader to loop through line data
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var lineDetails = new ReadItemData
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                                    ItemName = reader.GetString(reader.GetOrdinal("item_name")),
                                    ParentItem = reader.IsDBNull(reader.GetOrdinal("parent_item")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("parent_item")),
                                    Cost = reader.GetInt32(reader.GetOrdinal("cost")),
                                    ReqDate = reader.GetDateTime(reader.GetOrdinal("req_date"))
                                };

                                return Ok(lineDetails);
                            }

                            else
                            {
                                return NotFound("Could not find.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to retrieve item: {ex.Message}");
            }
        }


        //create an endpoint that calls the Get_Total_Cost function in previous step. User will supply the item_name for the function. The endpoint will return the value returned by the function.
        [HttpGet("totalcost")]
        public IActionResult GetTotalCost(String name)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Create a new record in the item table
                    using (var cmd = new NpgsqlCommand(
                        "SELECT Get_Total_Cost(@itemName)",connection))
                    {
                        cmd.Parameters.AddWithValue("itemName", name);

                        var totalCost = (int)cmd.ExecuteScalar();

                        return Ok(totalCost);
                    }

                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to get the total cost: {ex.Message}");
            }
        }
    }
}
