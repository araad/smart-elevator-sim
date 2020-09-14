namespace call_panel_service.Configuration
{
    public class CallPanelConfiguration
    {
        public bool CallPanelSimulationEnabled { get; set; }
        public int MinDelayBetweenSimulatedCalls { get; set; }
        public int MaxDelayBetweenSimulatedCalls { get; set; }
    }
}