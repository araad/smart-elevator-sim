using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using common_lib;
using Microsoft.Extensions.Logging;

public interface ICallPanel
{
    Task<int> CallElevator(TripRequest trip);
}

public class CallPanelService : ICallPanel
{
    private readonly ILogger<CallPanelService> _logger;
    private readonly IHttpClientFactory _clientFactory;

    public CallPanelService(ILogger<CallPanelService> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _clientFactory = clientFactory;
    }
    public async Task<int> CallElevator(TripRequest trip)
    {
        var tripJson = new StringContent(
                JsonSerializer.Serialize(trip),
                Encoding.UTF8,
                "application/json");

        var client = _clientFactory.CreateClient();

        var response = await client.PostAsync("http://localhost:5000/api/schedule", tripJson);

        var result = await response.Content.ReadAsStringAsync();

        return int.Parse(result);
    }
}