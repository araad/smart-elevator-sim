using System;
using System.Threading.Tasks;
using common_lib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("/api/call-panel")]
public class CallPanelController : ControllerBase
{
    private readonly ILogger<CallPanelController> _logger;
    private readonly ICallPanel _panelService;

    public CallPanelController(ILogger<CallPanelController> logger, ICallPanel panelService)
    {
        _logger = logger;
        _panelService = panelService;
    }

    [HttpPost]
    public async Task<ActionResult> callElevator(TripRequest trip)
    {
        try
        {
            int response = await _panelService.CallElevator(trip);
            return Ok(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error from CallPanelService.CallElevator -  origin: {trip.origin} \tdestination: {trip.destination}");
            return StatusCode(503);
        }
    }
}