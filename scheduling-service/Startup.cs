using common_lib.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using scheduling_service.Configuration;
using scheduling_service.Hubs;
using scheduling_service.Services;

namespace scheduling_service
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

            var elevatorConfig = new ElevatorConfiguration();
            Configuration.Bind("Elevator", elevatorConfig);
            services.AddSingleton(elevatorConfig);

            services.AddTransient<IElevatorService>(s =>
               new ElevatorService(logger:
                   s.GetRequiredService<ILogger<ElevatorService>>(),
                   hub: s.GetRequiredService<IHubContext<ElevatorTrackingHub>>(),
                   buildingConfiguration: buildingConfig,
                   elevatorConfiguration: elevatorConfig));

            services.AddHostedService<SchedulingService>();
            services.AddControllers();
            services.AddSignalR();
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
                endpoints.MapHub<ElevatorTrackingHub>("/elevator-tracking");
            });
        }
    }
}