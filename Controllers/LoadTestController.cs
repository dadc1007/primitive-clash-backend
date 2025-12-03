using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.LoadTest.Responses;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/loadtest")]
    [Produces(MediaTypeNames.Application.Json)]

    public class LoadTestController() : ControllerBase
    {
        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PingResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Ping()
        {
            // Simula trabajo real (consulta rápida, cálculo, etc)
            var random = new Random();
            var delay = random.Next(20, 100);
            Thread.Sleep(delay);

            var response = new PingResponse(
                Status: "ok",
                Server: Environment.MachineName,
                Timestamp: DateTime.UtcNow,
                Delay: delay
            );

            return Ok(response);
        }

        [HttpPost("work")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DoWork()
        {
            // Simula procesamiento
            Thread.Sleep(50);
            var response = new WorkResponse(Processed: true);

            return Ok(response);
        }
    }
}