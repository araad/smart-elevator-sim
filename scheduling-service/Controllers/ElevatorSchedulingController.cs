using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using common_lib;
using scheduling_service.Services;

namespace scheduling_service.Controllers
{
    [ApiController]
    [Route("/api/schedule")]
    public class ElevatorSchedulingController : ControllerBase
    {
        private readonly ILogger<ElevatorSchedulingController> _logger;
        private readonly SchedulingService _schedulingSrv = null;

        public ElevatorSchedulingController(
            ILogger<ElevatorSchedulingController> logger,
            [FromServices] IEnumerable<IHostedService> services)
        {
            _logger = logger;
            _schedulingSrv = services.FirstOrDefault(x => x is SchedulingService) as SchedulingService;
        }

        [HttpPost]
        public ActionResult<int> ScheduleTrip(TripRequest trip)
        {
            _logger.LogInformation("scheduleTrip request: {request}", JsonSerializer.Serialize(trip));

            if (_schedulingSrv == null)
            {
                return StatusCode(503, "Service temporarily down. Please try again later.");
            }

            int elevatorId = _schedulingSrv.ScheduleTrip(trip.origin, trip.destination);

            return Ok(elevatorId);
        }

        [HttpGet("floor-count")]
        public ActionResult<int> GetFloorCount()
        {
            _logger.LogInformation("getFloorCount request");

            return Ok(Config.FloorCount);
        }

        [HttpGet("elevators")]
        public ActionResult<List<Elevator>> GetElevators()
        {
            _logger.LogInformation("getElevators request");

            if (_schedulingSrv == null)
            {
                return StatusCode(503, "Service temporarily down. Please try again later.");
            }

            return Ok(_schedulingSrv.GetElevators());
        }
    }

}