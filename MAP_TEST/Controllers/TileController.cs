using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;


namespace MAP_TEST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TileController : ControllerBase
    {
        private readonly string _connectionString;

        public TileController()
        {
            _connectionString = "Data Source=mydatabase.sqlite";
        }

        [HttpGet("{zoom}/{x}/{y}")]
        public IActionResult GetTile(int zoom, int x, int y)
        {
            byte[] tileData = GetTileFromDB(zoom, x, y);

            if (tileData == null)
            {
                return NotFound();
            }

            return File(tileData, "image/png");
        }

        private byte[] GetTileFromDB(int zoom, int x, int y)
        {
            byte[] tileData = null;

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    var command = connection.CreateCommand();

                    command.CommandText = "SELECT tile_data FROM maptiles WHERE zoom_level = $zoom AND tile_column = $x AND tile_row = $y";
                    command.Parameters.AddWithValue("$zoom", zoom);
                    command.Parameters.AddWithValue("$x", x);
                    command.Parameters.AddWithValue("$y", y);

                    var reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        tileData = (byte[])reader["tile_data"];
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            return tileData;
        }
    }
}