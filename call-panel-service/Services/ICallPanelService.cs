using System.Threading.Tasks;
using common_lib;

namespace call_panel_service.Services
{
    public interface ICallPanelService
    {
        Task<int> CallElevator(TripRequest tripRequest);
    }
}