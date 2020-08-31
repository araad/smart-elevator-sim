using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using common_lib;
using Microsoft.Extensions.Logging;

namespace call_panel_service.Services
{
    public class CallPanelService : ICallPanelService
    {
        private readonly ILogger<CallPanelService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CallPanelService(ILogger<CallPanelService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<int> CallElevator(TripRequest tripRequest)
        {
            var tripJson = new StringContent(
                    JsonSerializer.Serialize(tripRequest),
                    Encoding.UTF8,
                    "application/json");

            var client = _httpClientFactory.CreateClient();

            var response = await client.PostAsync("http://localhost:5000/api/schedule", tripJson);

            var result = await response.Content.ReadAsStringAsync();

            return int.Parse(result);
        }
    }
}