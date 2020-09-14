using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using common_lib;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace call_panel_service.Services
{
    public class CallPanelService : ICallPanelService
    {
        private readonly ILogger<CallPanelService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public CallPanelService(ILogger<CallPanelService> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }
        public async Task<int> CallElevator(TripRequest tripRequest)
        {
            var tripJson = new StringContent(
                    JsonSerializer.Serialize(tripRequest),
                    Encoding.UTF8,
                    "application/json");

            var client = _httpClientFactory.CreateClient();

            var str = _config["SchedulingServiceUrl"];

            _logger.LogInformation($"calling {str}");

            var response = await client.PostAsync(str, tripJson);

            var result = await response.Content.ReadAsStringAsync();

            return int.Parse(result);
        }
    }
}