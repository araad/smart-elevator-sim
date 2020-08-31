using System;
using System.Threading.Tasks;
using call_panel_service.Services;
using common_lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace call_panel_service.Controllers
{
    [ApiController]
    [Route("/api/call-panel")]
    public class CallPanelController : ControllerBase
    {
        private readonly ILogger<CallPanelController> _logger;
        private readonly ICallPanelService _callPanelService;

        public CallPanelController(ILogger<CallPanelController> logger, ICallPanelService callPanelService)
        {
            _logger = logger;
            _callPanelService = callPanelService;
        }

        [HttpPost]
        public async Task<ActionResult> CallElevator(TripRequest trip)
        {
            try
            {
                int response = await _callPanelService.CallElevator(trip);
                return Ok(response);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error from CallPanelService.CallElevator -  origin: {trip.origin} \tdestination: {trip.destination}");
                return StatusCode(503);
            }
        }
    }
}