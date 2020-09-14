using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace call_panel_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var configDir = Path.GetFullPath(Path.Combine("..", "config"));

                    config.AddJsonFile(Path.Combine(configDir, "building.config.json"),
                        optional: false,
                        reloadOnChange: true);

                    config.AddJsonFile(Path.Combine(configDir, "call-panel.config.json"),
                        optional: false,
                        reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
