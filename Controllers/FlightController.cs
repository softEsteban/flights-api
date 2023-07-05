using Microsoft.AspNetCore.Mvc;
using FlightApi.Models;
using FlightApi.Services;
using Microsoft.Extensions.Configuration;

namespace FlightApi.Controllers.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightController : ControllerBase
    {
        private readonly FlightService _flightService;

        public FlightController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpGet("get-all-flights")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights()
        {
            var flights = await _flightService.GetFlights();
            return Ok(flights);
        }

        [HttpGet("search-flights")]
        public async Task<ActionResult<IEnumerable<Flight>>> SearchFlights(
            string origin,
            string destination
        )
        {
            var flights = await _flightService.SearchFlights(origin, destination);
            return Ok(flights);
        }
    }
}
