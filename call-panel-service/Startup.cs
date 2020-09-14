using call_panel_service.Configuration;
using call_panel_service.Services;
using common_lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace call_panel_service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var buildingConfig = new BuildingConfiguration();
            Configuration.Bind("Building", buildingConfig);
            services.AddSingleton(buildingConfig);

            var callPanelConfig = new CallPanelConfiguration();
            Configuration.Bind("CallPanel", callPanelConfig);
            services.AddSingleton(callPanelConfig);

            services.AddHttpClient();
            services.AddTransient(typeof(ICallPanelService), typeof(CallPanelService));

            if (callPanelConfig.CallPanelSimulationEnabled)
            {
                services.AddHostedService<CallSimulatorService>();
            }

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
