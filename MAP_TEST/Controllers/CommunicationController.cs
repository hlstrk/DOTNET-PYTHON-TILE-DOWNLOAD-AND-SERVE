using MAP_TEST.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CommunicationController : ControllerBase
{
    [HttpGet("GetAllyPlane")]
    public async Task<IActionResult> GetAllyPlane()
    {
        var url = new Uri("ws://localhost:56781/websocket/server");
        var buffer = new byte[1024];
        var sb = new StringBuilder();

        using (var webSocket = new ClientWebSocket())
        {
            try
            {
                await webSocket.ConnectAsync(url, CancellationToken.None);

                // Receive data from the WebSocket
                WebSocketReceiveResult result;
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                } while (!result.EndOfMessage);

                // Deserialize the received JSON string into a Plane object
                var json = sb.ToString();
                var plane = JsonConvert.DeserializeObject<Plane>(json);
                Console.WriteLine(plane);
                return Ok(plane);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}