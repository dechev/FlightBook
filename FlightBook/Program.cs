using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading.Tasks;

namespace FlightBook
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();
            await host.StartAsync();
            await host.WaitForShutdownAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging
                        .ClearProviders()
                        .AddSerilog(
                            new LoggerConfiguration()
                            .ReadFrom.Configuration(hostingContext.Configuration)
                            .CreateLogger(),
                            true);
#if DEBUG
                    logging.AddDebug();
#endif
                });
    }
}
