using library;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace web.Controllers
{
    [ApiController]
    [Route("api/travelPlan")]
    public class TravelPlannerController : ControllerBase
    {
        private static HttpClient HttpClient =
            new HttpClient() { BaseAddress = new Uri("https://cddataexchange.blob.core.windows.net") };

        private readonly ILogger<TravelPlannerController> logger;

        public TravelPlannerController(ILogger<TravelPlannerController> logger)
        {
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] string from, [FromQuery] string to, [FromQuery] string start)
        {
            // Check for errors
            if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(start))
            {
                logger.LogWarning("Invalid parameters.");
                return BadRequest();
            }

            // Get all routes
            var routesResponse = await HttpClient.GetAsync("/data-exchange/htl-homework/travelPlan.json");
            routesResponse.EnsureSuccessStatusCode();
            var responseBody = await routesResponse.Content.ReadAsStringAsync();
            var routes = JsonSerializer.Deserialize<Route[]>(responseBody);

            // Find the connection
            var finder = new ConnectionFinder(routes);
            var trip = finder.FindConnection(from, to, start);
            if (trip == null)
            {
                logger.LogWarning("No connection found.");
                return NotFound();
            }

            return Ok(trip);
        }
    }
}
