using System.Net.Mime;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using PrimitiveClash.Backend.DTOs.LoadTest.Responses;

namespace PrimitiveClash.Backend.Controllers
{
    [ApiController]
    [Route("api/loadtest")]
    [Produces(MediaTypeNames.Application.Json)]

    public class LoadTestController() : ControllerBase
    {
        // Endpoint 1: Trabajo CPU intensivo
        [HttpGet("cpu")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CpuResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CpuIntensive()
        {
            var random = new Random();

            // Cálculos matemáticos pesados
            double result = 0;
            for (int i = 0; i < 200000; i++)
            {
                result += Math.Sqrt(i) * Math.Sin(i) * Math.Cos(i) * Math.Log(i + 1);
            }

            // Hash criptográfico (muy pesado)
            var data = new byte[1024 * 50]; // 50KB
            random.NextBytes(data);

            byte[] hash = data;
            for (int i = 0; i < 30; i++)
            {
                hash = SHA256.HashData(hash);
            }

            return Ok(new CpuResponse(
                Status: "ok",
                Result: result,
                Hash: Convert.ToBase64String(hash),
                Server: Environment.MachineName,
                Timestamp: DateTime.UtcNow
            ));
        }

        // Endpoint 2: Simulación de procesamiento de datos
        [HttpPost("process")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProcessResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ProcessData()
        {
            var random = new Random();

            // Simula procesamiento de muchos datos
            var results = new List<object>();
            for (int i = 0; i < 20; i++)
            {
                var data = Enumerable.Range(0, 10000)
                    .Select(x => new
                    {
                        Id = x,
                        Value = Math.Sqrt(x) * random.NextDouble(),
                        Hash = Guid.NewGuid().ToString()
                    })
                    .OrderByDescending(x => x.Value)
                    .Take(50)
                    .ToList();

                results.Add(new { Batch = i, ProcessedCount = data.Count });
            }

            return Ok(new ProcessResponse(
                Processed: true,
                Batches: results.Count,
                Server: Environment.MachineName
            ));
        }
    }
}